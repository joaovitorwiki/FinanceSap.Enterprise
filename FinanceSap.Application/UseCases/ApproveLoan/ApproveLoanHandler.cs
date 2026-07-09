using FinanceSap.Domain.Common;
using FinanceSap.Domain.Interfaces;
using MediatR;

namespace FinanceSap.Application.UseCases.ApproveLoan;

public sealed class ApproveLoanHandler(
    ILoanRepository loanRepository,
    IUnitOfWork unitOfWork,
    IUserContext userContext)
    : IRequestHandler<ApproveLoanCommand, Result>
{
    public async Task<Result> Handle(ApproveLoanCommand request, CancellationToken ct)
    {
        var loan = await loanRepository.GetByIdTrackedAsync(request.LoanId, ct);
        if (loan is null)
            return Result.Failure("Empréstimo não encontrado.", ErrorType.NotFound);

        // IDOR: valida que o CustomerId do JWT é dono deste empréstimo.
        var ownerCustomerId = await userContext.GetCustomerIdByUserIdAsync(request.UserId, ct);
        if (ownerCustomerId is null || ownerCustomerId.Value != loan.CustomerId)
            return Result.Failure("Empréstimo não encontrado.", ErrorType.NotFound);

        var result = loan.Approve();
        if (!result.IsSuccess)
            return result;

        await unitOfWork.CommitAsync(ct);
        return Result.Success();
    }
}
