using FinanceSap.Application.UseCases.RequestLoan;
using Microsoft.AspNetCore.Mvc;

namespace FinanceSap.Api.Controllers;

[ApiController]
[Consumes("application/json")]
[Route("api/[controller]")]
public sealed class LoansController(RequestLoanHandler handler) : ControllerBase
{
    /// <summary>
    /// Solicita um novo empréstimo.
    /// </summary>
    /// <param name="command">Dados da solicitação de empréstimo.</param>
    /// <param name="ct">Token de cancelamento.</param>
    /// <returns>ID do empréstimo criado ou erro de validação.</returns>
    [HttpPost]
    public async Task<IActionResult> RequestLoan(
        [FromBody] RequestLoanCommand command,
        CancellationToken ct = default)
    {
        var result = await handler.HandleAsync(command, ct);

        if (!result.IsSuccess)
        {
            return result.ErrorType switch
            {
                Domain.Common.ErrorType.NotFound => NotFound(new { error = result.Error }),
                Domain.Common.ErrorType.Validation => BadRequest(new { error = result.Error }),
                _ => BadRequest(new { error = result.Error })
            };
        }

        return CreatedAtAction(
            nameof(GetLoan),
            new { id = result.Value },
            new { loanId = result.Value });
    }

    /// <summary>
    /// Placeholder para obter empréstimo por ID (implementar futuramente).
    /// </summary>
    [HttpGet("{id:guid}")]
    public IActionResult GetLoan(Guid id)
    {
        return Ok(new { message = "GetLoan endpoint - to be implemented", loanId = id });
    }
}