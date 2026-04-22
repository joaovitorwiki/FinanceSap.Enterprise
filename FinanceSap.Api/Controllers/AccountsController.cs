using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FinanceSap.Application.Queries.GetAccountBalance;
using FinanceSap.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceSap.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Todos os endpoints exigem autenticação JWT.
public sealed class AccountsController(IMediator mediator) : ControllerBase
{
    // GET /api/accounts/balance
    // Retorna o saldo da conta do usuário autenticado.
    // IDOR Prevention: o handler valida que o UserId do JWT corresponde ao CustomerId da Account.
    [HttpGet("balance")]
    [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBalance()
    {
        // Extrai UserId do JWT claim "sub".
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Token inválido." });

        var query  = new GetAccountBalanceQuery(userId);
        var result = await mediator.Send(query);

        if (!result.IsSuccess)
        {
            var statusCode = result.ErrorType switch
            {
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                _                  => StatusCodes.Status400BadRequest
            };

            return Problem(detail: result.Error, statusCode: statusCode);
        }

        return Ok(new { balance = result.Value });
    }
}
