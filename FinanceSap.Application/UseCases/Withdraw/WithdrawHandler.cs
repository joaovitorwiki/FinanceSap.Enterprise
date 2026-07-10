using FinanceSap.Domain.Entities;
using FinanceSap.Domain.Enums;
using FinanceSap.Domain.Interfaces;
using MediatR;

namespace FinanceSap.Application.UseCases.Withdraw;

public sealed class WithdrawHandler(
    IAccountRepository accountRepository,
    ITransactionRepository transactionRepository,
    IUnitOfWork unitOfWork,
    IUserContext userContext)
    : IRequestHandler<WithdrawCommand, Result>
{
    public async Task<Result> Handle(WithdrawCommand request, CancellationToken ct)
    {
        if (request.Amount <= 0)
            return Result.Failure("O valor do saque deve ser positivo.");

        var account = await accountRepository.GetByIdTrackedAsync(request.AccountId, ct);
        if (account is null)
            return Result.Failure("Conta não encontrada.", ErrorType.NotFound);

        // IDOR: valida que o UserId do JWT é dono desta conta.
        var ownerCustomerId = await userContext.GetCustomerIdByUserIdAsync(request.UserId, ct);
        if (ownerCustomerId is null || ownerCustomerId.Value != account.CustomerId)
            return Result.Failure("Conta não encontrada.", ErrorType.NotFound);

        var debitResult = account.Debit(request.Amount);
        if (!debitResult.IsSuccess)
            return debitResult;

        var transaction = Transaction.Create(account.Id, request.Amount, TransactionType.Withdrawal);
        await transactionRepository.AddAsync(transaction, ct);

        await unitOfWork.CommitAsync(ct);
        return Result.Success();
    }
}
