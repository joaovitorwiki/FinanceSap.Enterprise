using FinanceSap.Domain.Common;
using FinanceSap.Domain.Entities;
using FinanceSap.Domain.Interfaces;
using MediatR;

namespace FinanceSap.Application.Queries.GetAccountByCustomer;

/// <summary>
/// Handler for GetAccountByCustomerQuery.
/// Returns the primary account for the authenticated customer.
/// </summary>
public sealed class GetAccountByCustomerHandler(
    IAccountRepository accountRepository,
    IUserContext userContext)
    : IRequestHandler<GetAccountByCustomerQuery, Result<Account>>
{
    public async Task<Result<Account>> Handle(
        GetAccountByCustomerQuery request,
        CancellationToken ct)
    {
        // 1. Get the CustomerId by UserId from JWT - used for ownership validation (IDOR prevention)
        var customerId = await userContext.GetCustomerIdByUserIdAsync(request.UserId, ct);
        if (customerId is null)
            return Result<Account>.Failure("Usuário não encontrado.", ErrorType.NotFound);

        // 2. Get the Account by CustomerId linked to the User
        var account = await accountRepository.GetByCustomerIdAsync(customerId.Value, ct);
        if (account is null)
            return Result<Account>.Failure("Conta não encontrada.", ErrorType.NotFound);

        // 3. Ownership validated - return the account
        return Result<Account>.Success(account);
    }
}