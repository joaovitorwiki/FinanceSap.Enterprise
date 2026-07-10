using FinanceSap.Domain.Entities;
using FinanceSap.Domain.Enums;
using FinanceSap.Domain.Events;
using FinanceSap.Domain.Interfaces;
using MediatR;

namespace FinanceSap.Application.UseCases.LoanDisbursal;

public sealed class LoanApprovedEventHandler(
    IAccountRepository accountRepository,
    ITransactionRepository transactionRepository,
    IUnitOfWork unitOfWork)
    : INotificationHandler<LoanApprovedEvent>
{
    public async Task Handle(LoanApprovedEvent notification, CancellationToken ct)
    {
        var account = await accountRepository.GetByCustomerIdTrackedAsync(notification.CustomerId, ct);
        if (account is null)
            return;

        account.Credit(notification.Amount);

        var transaction = Transaction.Create(account.Id, notification.Amount, TransactionType.LoanDisbursal);
        await transactionRepository.AddAsync(transaction, ct);

        await unitOfWork.CommitAsync(ct);
    }
}
