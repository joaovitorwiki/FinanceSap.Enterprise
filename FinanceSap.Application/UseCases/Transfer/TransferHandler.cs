using FinanceSap.Domain.Entities;
using FinanceSap.Domain.Enums;
using FinanceSap.Domain.Interfaces;
using MediatR;

namespace FinanceSap.Application.UseCases.Transfer;

public sealed class TransferHandler(
    IAccountRepository accountRepository,
    ITransactionRepository transactionRepository,
    IUnitOfWork unitOfWork,
    IUserContext userContext)
    : IRequestHandler<TransferCommand, Result>
{
    public async Task<Result> Handle(TransferCommand request, CancellationToken ct)
    {
        if (request.Amount <= 0)
            return Result.Failure("O valor da transferência deve ser positivo.");

        if (request.SourceAccountId == request.DestinationAccountId)
            return Result.Failure("Conta de origem e destino não podem ser iguais.");

        var source = await accountRepository.GetByIdTrackedAsync(request.SourceAccountId, ct);
        if (source is null)
            return Result.Failure("Conta de origem não encontrada.", ErrorType.NotFound);

        // IDOR: valida que o UserId do JWT é dono da conta de origem.
        var ownerCustomerId = await userContext.GetCustomerIdByUserIdAsync(request.UserId, ct);
        if (ownerCustomerId is null || ownerCustomerId.Value != source.CustomerId)
            return Result.Failure("Conta de origem não encontrada.", ErrorType.NotFound);

        var destination = await accountRepository.GetByIdTrackedAsync(request.DestinationAccountId, ct);
        if (destination is null)
            return Result.Failure("Conta de destino não encontrada.", ErrorType.NotFound);

        var debitResult = source.Debit(request.Amount);
        if (!debitResult.IsSuccess)
            return debitResult;

        destination.Credit(request.Amount);

        // Dois registros de ledger — um para cada lado da transferência.
        await transactionRepository.AddAsync(
            Transaction.Create(source.Id, request.Amount, TransactionType.Transfer), ct);
        await transactionRepository.AddAsync(
            Transaction.Create(destination.Id, request.Amount, TransactionType.Transfer), ct);

        await unitOfWork.CommitAsync(ct);
        return Result.Success();
    }
}
