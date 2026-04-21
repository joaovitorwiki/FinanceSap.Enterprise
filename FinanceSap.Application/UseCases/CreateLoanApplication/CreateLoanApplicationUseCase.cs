using FinanceSap.Domain.Common;
using FinanceSap.Domain.Entities;
using FinanceSap.Domain.Interfaces;
using FluentValidation;

namespace FinanceSap.Application.UseCases.CreateLoanApplication;

// Orquestra a criação de uma solicitação de empréstimo.
// Primary Constructor (C# 13) para injeção de dependência.
public sealed class CreateLoanApplicationUseCase(
    ILoanApplicationRepository repository,
    IUnitOfWork unitOfWork,
    IValidator<CreateLoanApplicationCommand> validator)
{
    public async Task<Result<CreateLoanApplicationResponse>> ExecuteAsync(
        CreateLoanApplicationCommand command,
        CancellationToken ct = default)
    {
        var validation = await validator.ValidateAsync(command, ct);
        if (!validation.IsValid)
        {
            var errors = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage));
            return Result<CreateLoanApplicationResponse>.Failure(errors);
        }

        // Customer.Create() encapsula a validação do CPF (Módulo 11) e do nome.
        var customerResult = Customer.Create(command.Document, command.FullName);
        if (!customerResult.IsSuccess)
            return Result<CreateLoanApplicationResponse>.Failure(customerResult.Error!);

        var loan = new LoanApplication(customerResult.Value!, command.Amount, command.TermInMonths);

        await repository.AddAsync(loan, ct);
        await unitOfWork.CommitAsync(ct);

        return Result<CreateLoanApplicationResponse>.Success(new CreateLoanApplicationResponse(
            loan.Id,
            loan.Status.ToString(),
            loan.Amount,
            loan.TermInMonths,
            loan.CreatedAt
        ));
    }
}
