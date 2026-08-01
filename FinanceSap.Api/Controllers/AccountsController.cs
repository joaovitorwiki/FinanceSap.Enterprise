using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FinanceSap.Api.Extensions;
using FinanceSap.Application.Queries.GetAccountBalance;
using FinanceSap.Application.Queries.GetAccountByCustomer;
using FinanceSap.Application.Queries.GetAccountStatement;
using FinanceSap.Application.UseCases.Deposit;
using FinanceSap.Application.UseCases.Transfer;
using FinanceSap.Application.UseCases.Withdraw;
using FinanceSap.Domain.Common;
using FinanceSap.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FinanceSap.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting(ApiServiceExtensions.GlobalRateLimitPolicy)]
[Consumes("application/json")]
public sealed class AccountsController(IMediator mediator) : ControllerBase
{
    private Guid? ExtractUserId() =>
        Guid.TryParse(
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value,
            out var id) ? id : null;

    private IActionResult MapError(string error, ErrorType errorType) =>
        errorType switch
        {
            ErrorType.NotFound   => Problem(detail: error, statusCode: StatusCodes.Status404NotFound),
            ErrorType.Conflict   => Problem(detail: error, statusCode: StatusCodes.Status409Conflict),
            _                    => Problem(detail: error, statusCode: StatusCodes.Status400BadRequest)
        };

    // GET /api/accounts/balance
    [HttpGet("balance")]
    [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBalance()
    {
        var userId = ExtractUserId();
        if (userId is null) return Unauthorized(new { message = "Token inválido." });

        var result = await mediator.Send(new GetAccountBalanceQuery(userId.Value));
        if (!result.IsSuccess) return MapError(result.Error!, result.ErrorType);

        return Ok(new { balance = result.Value });
    }

    // POST /api/accounts/{id}/deposit
    [HttpPost("{id:guid}/deposit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deposit(Guid id, [FromBody] MoneyRequest request)
    {
        var result = await mediator.Send(new DepositCommand(id, request.Amount));
        if (!result.IsSuccess) return MapError(result.Error!, result.ErrorType);

        return Ok(new { message = "Depósito realizado com sucesso." });
    }

    // POST /api/accounts/{id}/withdraw
    [HttpPost("{id:guid}/withdraw")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Withdraw(Guid id, [FromBody] MoneyRequest request)
    {
        var userId = ExtractUserId();
        if (userId is null) return Unauthorized(new { message = "Token inválido." });

        var result = await mediator.Send(new WithdrawCommand(id, request.Amount, userId.Value));
        if (!result.IsSuccess) return MapError(result.Error!, result.ErrorType);

        return Ok(new { message = "Saque realizado com sucesso." });
    }

    // POST /api/accounts/{id}/transfer
    [HttpPost("{id:guid}/transfer")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Transfer(Guid id, [FromBody] TransferRequest request)
    {
        var userId = ExtractUserId();
        if (userId is null) return Unauthorized(new { message = "Token inválido." });

        var result = await mediator.Send(new TransferCommand(id, request.DestinationAccountId, request.Amount, userId.Value));
        if (!result.IsSuccess) return MapError(result.Error!, result.ErrorType);

        return Ok(new { message = "Transferência realizada com sucesso." });
    }

    // GET /api/accounts/{id}/statement?page=1&pageSize=20
    [HttpGet("{id:guid}/statement")]
    [ProducesResponseType(typeof(AccountStatementResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStatement(
        Guid id,
        [FromQuery] int page     = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = ExtractUserId();
        if (userId is null) return Unauthorized(new { message = "Token inválido." });

        var result = await mediator.Send(new GetAccountStatementQuery(id, userId.Value, page, pageSize));
        if (!result.IsSuccess) return MapError(result.Error!, result.ErrorType);

        return Ok(result.Value);
    }

    // GET /api/accounts/primary
    [HttpGet("primary")]
    [ProducesResponseType(typeof(Account), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPrimaryAccount()
    {
        var userId = ExtractUserId();
        if (userId is null) return Unauthorized(new { message = "Token inválido." });

        var result = await mediator.Send(new GetAccountByCustomerQuery(userId.Value));
        if (!result.IsSuccess) return MapError(result.Error!, result.ErrorType);

        return Ok(result.Value);
    }
}

// Request DTOs — mantidos no controller por serem simples value objects de entrada.
public sealed record MoneyRequest(decimal Amount);
public sealed record TransferRequest(Guid DestinationAccountId, decimal Amount);
