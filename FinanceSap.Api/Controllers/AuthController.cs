using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FinanceSap.Api.Extensions;
using FinanceSap.Api.Services;
using FinanceSap.Application.EventHandlers;
using FinanceSap.Domain.Entities;
using FinanceSap.Domain.Interfaces;
using FinanceSap.Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;

namespace FinanceSap.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting(ApiServiceExtensions.AuthRateLimitPolicy)]
public sealed class AuthController(
    IAuthService authService,
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ICustomerRepository customerRepository,
    IUnitOfWork unitOfWork,
    IMediator mediator) : ControllerBase
{
    // POST /api/auth/register
    // Cria User (Identity) + Customer (Domain) e dispara evento CustomerCreated.
    [HttpPost("refresh-token")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var authResult = await authService.RefreshTokenAsync(request.RefreshToken, ipAddress);

        if (!authResult.Success)
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Refresh token inválido",
                Detail = authResult.ErrorMessage
            });

        return Ok(new
        {
            token = authResult.JwtToken,
            refreshToken = authResult.RefreshToken,
            expiresIn = 900 // 15 min em segundos
        });
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        // 1. Valida e cria Customer no Domain.
        var customerResult = Customer.Create(request.Document, request.FullName);
        if (!customerResult.IsSuccess)
            return BadRequest(new { message = customerResult.Error });

        // 2. Cria ApplicationUser no Identity.
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email    = request.Email
        };

        var identityResult = await userManager.CreateAsync(user, request.Password);
        if (!identityResult.Succeeded)
        {
            var errors = string.Join("; ", identityResult.Errors.Select(e => e.Description));
            return BadRequest(new { message = errors });
        }

        // 3. Persiste Customer e linka ao User.
        await customerRepository.AddAsync(customerResult.Value!, default);
        await unitOfWork.CommitAsync();

        user.CustomerId = customerResult.Value!.Id;
        await userManager.UpdateAsync(user);

        // 4. Dispara evento CustomerCreated — trigger assíncrono de CreateAccountCommand.
        await mediator.Publish(new CustomerCreatedNotification(
            customerResult.Value.Id,
            customerResult.Value.FullName
        ));

        return CreatedAtAction(
            nameof(Login),
            new { id = user.Id },
            new { userId = user.Id, customerId = customerResult.Value.Id }
        );
    }

    // POST /api/auth/login
    // Valida credenciais e retorna JWT de curta duração (15 min).
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var authResult = await authService.AuthenticateAsync(request.Email, request.Password, ipAddress);

        if (!authResult.Success)
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Autenticação falhou",
                Detail = authResult.ErrorMessage
            });

        return Ok(new
        {
            token = authResult.JwtToken,
            refreshToken = authResult.RefreshToken,
            expiresIn = 900 // 15 min em segundos
        });
    }

}

public sealed record RegisterRequest(string Email, string Password, string Document, string FullName);
public sealed record LoginRequest(string Email, string Password);
public sealed record RefreshTokenRequest(string RefreshToken);
