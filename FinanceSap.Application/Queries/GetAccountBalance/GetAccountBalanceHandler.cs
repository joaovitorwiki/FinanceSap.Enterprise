using FinanceSap.Domain.Common;
using FinanceSap.Domain.Interfaces;
using MediatR;

namespace FinanceSap.Application.Queries.GetAccountBalance;

// Handler — GetAccountBalance.
// IDOR Prevention: valida que o UserId autenticado corresponde ao CustomerId da Account.
// Se não corresponder, retorna NotFound (não Forbidden para não vazar existência do recurso).
public sealed class GetAccountBalanceHandler(
    IAccountRepository accountRepository,
    IUserContext userContext)
    : IRequestHandler<GetAccountBalanceQuery, Result<decimal>>
{
    public async Task<Result<decimal>> Handle(
        GetAccountBalanceQuery request,
        CancellationToken ct)
    {
        // 1. Busca o CustomerId pelo UserId do JWT.
        var customerId = await userContext.GetCustomerIdByUserIdAsync(request.UserId, ct);
        if (customerId is null)
            return Result<decimal>.Failure("Usuário não encontrado.", ErrorType.NotFound);

        // 2. Busca a Account pelo CustomerId linkado ao User.
        var account = await accountRepository.GetByCustomerIdAsync(customerId.Value, ct);
        if (account is null)
            return Result<decimal>.Failure("Conta não encontrada.", ErrorType.NotFound);

        // 3. Ownership validado — retorna o saldo.
        return Result<decimal>.Success(account.Balance);
    }
}
