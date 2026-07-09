using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FinanceSap.Application.Queries;
using FinanceSap.Application.UseCases.CreateLoanApplication;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceSap.Api.Controllers;

[ApiController]
[Consumes("application/json")]
[Route("api/[controller]")]
public sealed class LoanApplicationsController(
    CreateLoanApplicationUseCase useCase,
    IMediator mediator) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(CreateLoanApplicationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateLoanApplicationCommand command,
        CancellationToken ct)
    {
        var result = await useCase.ExecuteAsync(command, ct);

        if (!result.IsSuccess)
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title  = "Requisição inválida.",
                Detail = result.Error
            });

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(CreateLoanApplicationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var application = await mediator.Send(new GetLoanApplicationByIdQuery(id, userId.Value), ct);
        return application is null ? NotFound() : Ok(application);
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}
