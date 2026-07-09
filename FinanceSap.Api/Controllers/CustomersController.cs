using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FinanceSap.Api.Extensions;
using FinanceSap.Application.Queries;
using FinanceSap.Application.UseCases.CreateCustomer;
using FinanceSap.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FinanceSap.Api.Controllers;

[ApiController]
[Consumes("application/json")]
[Route("api/[controller]")]
public sealed class CustomersController(
    CreateCustomerHandler handler,
    IMediator mediator) : ControllerBase
{
    [HttpPost]
    [EnableRateLimiting(ApiServiceExtensions.CustomersRateLimitPolicy)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCustomerCommand command,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(command, ct);

        if (!result.IsSuccess)
        {
            var (statusCode, title) = result.ErrorType switch
            {
                ErrorType.Conflict   => (StatusCodes.Status409Conflict,           "Conflito de recurso."),
                ErrorType.NotFound   => (StatusCodes.Status404NotFound,           "Recurso não encontrado."),
                ErrorType.Validation => (StatusCodes.Status400BadRequest,         "Requisição inválida."),
                _                    => (StatusCodes.Status500InternalServerError, "Erro interno.")
            };

            return Problem(detail: result.Error, title: title, statusCode: statusCode);
        }

        return CreatedAtAction(
            actionName:  nameof(GetById),
            routeValues: new { id = result.Value },
            value:       new { id = result.Value }
        );
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var customer = await mediator.Send(new GetCustomerByIdQuery(id, userId.Value), ct);
        return customer is null ? NotFound() : Ok(customer);
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}
