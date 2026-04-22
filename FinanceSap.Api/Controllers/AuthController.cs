using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FinanceSap.Application.EventHandlers;
using FinanceSap.Domain.Entities;
using FinanceSap.Domain.Interfaces;
using FinanceSap.Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace FinanceSap.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ICustomerRepository customerRepository,
    IUnitOfWork unitOfWork,
    IMediator mediator,
    IConfiguration configuration) : ControllerBase
{
    // POST /api/auth/register
    // Cria User (Identity) + Customer (Domain) e dispara evento CustomerCreated.
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
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return Unauthorized(new { message = "Credenciais inválidas." });

        var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
                return Unauthorized(new { message = "Conta bloqueada temporariamente." });

            return Unauthorized(new { message = "Credenciais inválidas." });
        }

        var token = GenerateJwtToken(user);
        return Ok(new { token, expiresIn = 900 }); // 15 min em segundos
    }

    private string GenerateJwtToken(ApplicationUser user)
    {
        var key     = configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key não configurada.");
        var issuer  = configuration["Jwt:Issuer"] ?? "FinanceSap";
        var audience = configuration["Jwt:Audience"] ?? "FinanceSap";

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("customerId", user.CustomerId?.ToString() ?? string.Empty)
        };

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer:   issuer,
            audience: audience,
            claims:   claims,
            expires:  DateTime.UtcNow.AddMinutes(15), // Token de curta duração
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public sealed record RegisterRequest(string Email, string Password, string Document, string FullName);
public sealed record LoginRequest(string Email, string Password);
