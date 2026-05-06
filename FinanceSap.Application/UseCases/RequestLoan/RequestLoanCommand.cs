namespace FinanceSap.Application.UseCases.RequestLoan;

public sealed record RequestLoanCommand(
    Guid CustomerId,
    decimal Amount,
    int Installments,
    decimal AnnualRate
);
