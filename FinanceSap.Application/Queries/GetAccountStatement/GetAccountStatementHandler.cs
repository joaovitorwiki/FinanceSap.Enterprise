using FinanceSap.Domain.Interfaces;
using MediatR;

namespace FinanceSap.Application.Queries.GetAccountStatement;

public sealed class GetAccountStatementHandler(
    IAccountRepository accountRepository,
    ITransactionRepository transactionRepository,
    IUserContext userContext)
    : IRequestHandler<GetAccountStatementQuery, Result<AccountStatementResult>>
{
    public async Task<Result<AccountStatementResult>> Handle(
        GetAccountStatementQuery request, CancellationToken ct)
    {
        if (request.Page < 1 || request.PageSize < 1 || request.PageSize > 100)
            return Result<AccountStatementResult>.Failure("Parâmetros de paginação inválidos.");

        var account = await accountRepository.GetByIdAsync(request.AccountId, ct);
        if (account is null)
            return Result<AccountStatementResult>.Failure("Conta não encontrada.", ErrorType.NotFound);

        // IDOR: valida que o UserId do JWT é dono desta conta.
        var ownerCustomerId = await userContext.GetCustomerIdByUserIdAsync(request.UserId, ct);
        if (ownerCustomerId is null || ownerCustomerId.Value != account.CustomerId)
            return Result<AccountStatementResult>.Failure("Conta não encontrada.", ErrorType.NotFound);

        var transactions = await transactionRepository.GetByAccountIdAsync(
            request.AccountId, request.Page, request.PageSize, ct);

        var totalCount = await transactionRepository.CountByAccountIdAsync(request.AccountId, ct);

        return Result<AccountStatementResult>.Success(new AccountStatementResult(
            account.Id,
            account.Balance,
            totalCount,
            request.Page,
            request.PageSize,
            transactions));
    }
}
