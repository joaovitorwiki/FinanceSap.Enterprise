using FinanceSap.Domain.Entities;
using FinanceSap.Domain.Enums;
using FinanceSap.Domain.Interfaces;
using MediatR;

namespace FinanceSap.Application.UseCases.Deposit;

public sealed class DepositHandler(
    IAccountRepository accountRepository,
    ITransactionRepository transactionRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DepositCommand, Result>
{
    public async Task<Result> Handle(DepositCommand request, CancellationToken ct)
    {
        if (request.Amount <= 0)
            return Result.Failure("O valor do depósito deve ser positivo.");

        var account = await accountRepository.GetByIdTrackedAsync(request.AccountId, ct);
        if (account is null)
            return Result.Failure("Conta não encontrada.", ErrorType.NotFound);

        account.Credit(request.Amount);

        var transaction = Transaction.Create(account.Id, request.Amount, TransactionType.Deposit);
        await transactionRepository.AddAsync(transaction, ct);

        await unitOfWork.CommitAsync(ct);
        return Result.Success();
    }
}
