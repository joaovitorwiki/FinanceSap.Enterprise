namespace FinanceSap.Application.UseCases.CreateLoanApplication;

// DTO de entrada imutável. Expõe apenas os campos necessários para o caso de uso.
public sealed record CreateLoanApplicationCommand(
    string Document,
    string FullName,
    decimal Amount,
    int TermInMonths
);
