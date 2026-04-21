using FinanceSap.Api.Extensions;
using FinanceSap.Application.UseCases.CreateCustomer;
using FinanceSap.Domain.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FinanceSap.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class CustomersController(CreateCustomerHandler handler) : ControllerBase
{
    // Rate Limiting aplicado apenas no POST — endpoint de escrita sensível a abuso e
    // enumeração de CPFs. O GET não é limitado aqui pois não expõe dados por enquanto.
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id) => NotFound();
}
