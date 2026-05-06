using FluentValidation;

namespace FinanceSap.Application.UseCases.RequestLoan;

public sealed class RequestLoanValidator : AbstractValidator<RequestLoanCommand>
{
    public RequestLoanValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Dados inválidos.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Dados inválidos.");

        RuleFor(x => x.Installments)
            .InclusiveBetween(1, 360).WithMessage("Dados inválidos.");

        RuleFor(x => x.AnnualRate)
            .InclusiveBetween(0, 0.50m).WithMessage("Dados inválidos.");
    }
}
