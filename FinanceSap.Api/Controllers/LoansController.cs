using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FinanceSap.Application.Queries;
using FinanceSap.Application.UseCases.ApproveLoan;
using FinanceSap.Application.UseCases.RejectLoan;
using FinanceSap.Application.UseCases.RequestLoan;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceSap.Api.Controllers;

[ApiController]
[Consumes("application/json")]
[Route("api/[controller]")]
public sealed class LoansController(
    RequestLoanHandler handler,
    IMediator mediator) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RequestLoan(
        [FromBody] RequestLoanCommand command,
        CancellationToken ct = default)
    {
        var result = await handler.HandleAsync(command, ct);

        if (!result.IsSuccess)
        {
            return result.ErrorType switch
            {
                Domain.Common.ErrorType.NotFound   => NotFound(new { error = result.Error }),
                Domain.Common.ErrorType.Validation => BadRequest(new { error = result.Error }),
                _                                  => BadRequest(new { error = result.Error })
            };
        }

        return CreatedAtAction(nameof(GetLoan), new { id = result.Value }, new { loanId = result.Value });
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLoan(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var loan = await mediator.Send(new GetLoanByIdQuery(id, userId.Value), ct);
        return loan is null ? NotFound() : Ok(loan);
    }

    // PUT /api/loans/{id}/approve
    [HttpPut("{id:guid}/approve")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Approve(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await mediator.Send(new ApproveLoanCommand(id, userId.Value), ct);

        if (!result.IsSuccess)
        {
            return result.ErrorType switch
            {
                Domain.Common.ErrorType.NotFound   => NotFound(new { error = result.Error }),
                Domain.Common.ErrorType.Validation => BadRequest(new { error = result.Error }),
                _                                  => BadRequest(new { error = result.Error })
            };
        }

        return NoContent();
    }

    // PUT /api/loans/{id}/reject
    [HttpPut("{id:guid}/reject")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectLoanRequest? body, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await mediator.Send(new RejectLoanCommand(id, userId.Value, body?.Reason), ct);

        if (!result.IsSuccess)
        {
            return result.ErrorType switch
            {
                Domain.Common.ErrorType.NotFound   => NotFound(new { error = result.Error }),
                Domain.Common.ErrorType.Validation => BadRequest(new { error = result.Error }),
                _                                  => BadRequest(new { error = result.Error })
            };
        }

        return NoContent();
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}

public sealed record RejectLoanRequest(string? Reason);
