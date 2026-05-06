using FinanceSap.Domain.Common;
using FinanceSap.Domain.Entities;
using FinanceSap.Domain.Interfaces;
using FinanceSap.Domain.Services;
using FluentValidation;

namespace FinanceSap.Application.UseCases.RequestLoan;

public sealed class RequestLoanHandler(
    ICustomerRepository customerRepository,
    ILoanRepository loanRepository,
    IUnitOfWork unitOfWork,
    IValidator<RequestLoanCommand> validator)
{
    public async Task<Result<Guid>> HandleAsync(
        RequestLoanCommand command,
        CancellationToken ct = default)
    {
        var validation = await validator.ValidateAsync(command, ct);
        if (!validation.IsValid)
        {
            var errors = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage));
            return Result<Guid>.Failure(errors);
        }

        var customer = await customerRepository.GetByIdAsync(command.CustomerId, ct);
        if (customer is null)
            return Result<Guid>.Failure("Cliente não encontrado.", ErrorType.NotFound);

        var calculator = new CompoundInterestCalculator();
        var loanResult = Loan.Create(
            command.CustomerId,
            command.Amount,
            command.AnnualRate,
            command.Installments,
            calculator);

        if (!loanResult.IsSuccess)
            return Result<Guid>.Failure(loanResult.Error!);

        await loanRepository.AddAsync(loanResult.Value!, ct);
        await unitOfWork.CommitAsync(ct);

        return Result<Guid>.Success(loanResult.Value!.Id);
    }
}
