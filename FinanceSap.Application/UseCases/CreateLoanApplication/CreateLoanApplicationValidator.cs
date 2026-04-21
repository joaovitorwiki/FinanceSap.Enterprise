using FluentValidation;

namespace FinanceSap.Application.UseCases.CreateLoanApplication;

// Valida o Command antes de chegar ao Use Case, prevenindo dados inválidos e Mass Assignment.
public sealed class CreateLoanApplicationValidator : AbstractValidator<CreateLoanApplicationCommand>
{
    public CreateLoanApplicationValidator()
    {
        RuleFor(x => x.Document)
            .NotEmpty()
            .Length(11).WithMessage("CPF deve conter 11 dígitos.");

        RuleFor(x => x.FullName)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("O valor deve ser maior que zero.");

        RuleFor(x => x.TermInMonths)
            .GreaterThan(0).WithMessage("O prazo deve ser maior que zero.");
    }
}
