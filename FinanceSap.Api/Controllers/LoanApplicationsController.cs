using FinanceSap.Application.UseCases.CreateLoanApplication;
using Microsoft.AspNetCore.Mvc;

namespace FinanceSap.Api.Controllers;

// Controller REST para o módulo de solicitações de empréstimo.
// Delega toda a lógica ao Use Case — sem regras de negócio no controller.
[ApiController]
[Route("api/[controller]")]
public sealed class LoanApplicationsController(CreateLoanApplicationUseCase useCase) : ControllerBase
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
    [ProducesResponseType(typeof(CreateLoanApplicationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id)
    {
        // Placeholder para o Use Case de consulta — extensível sem quebrar o contrato.
        return NotFound();
    }
}
