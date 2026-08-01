using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FinanceSap.Api.Extensions;
using FinanceSap.Application.Queries;
using FinanceSap.Application.Queries.GetAccountByCustomer;
using FinanceSap.Application.Queries.GetAccountStatement;
using FinanceSap.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FinanceSap.Api.Controllers;

/// <summary>
/// Controller para operações relacionadas a transações financeiras.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting(ApiServiceExtensions.GlobalRateLimitPolicy)]
public sealed class TransactionsController(IMediator mediator) : ControllerBase
{
    private Guid? ExtractUserId() =>
        Guid.TryParse(
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value,
            out var id) ? id : null;

    /// <summary>
    /// Retrieves the transaction history for the authenticated customer's primary account.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(AccountStatementResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTransactions()
    {
        var userId = ExtractUserId();
        if (userId is null) return Unauthorized(new { message = "Token inválido." });

        // 1. Obter a conta principal do cliente
        var accountResult = await mediator.Send(new GetAccountByCustomerQuery(userId.Value));
        if (!accountResult.IsSuccess) return NotFound(new { message = "Conta não encontrada." });

        var account = accountResult.Value;

        // 2. Obter o extrato da conta
        var statementResult = await mediator.Send(new GetAccountStatementQuery(
            account.Id,
            userId.Value,
            1,  // page
            100 // pageSize - retornar todas as transações por padrão
        ));

        if (!statementResult.IsSuccess)
        {
            return statementResult.ErrorType switch
            {
                ErrorType.NotFound => NotFound(new { message = statementResult.Error }),
                ErrorType.Validation => BadRequest(new { message = statementResult.Error }),
                _ => StatusCode(500, new { message = "Erro interno do servidor." })
            };
        }

        return Ok(statementResult.Value);
    }
}