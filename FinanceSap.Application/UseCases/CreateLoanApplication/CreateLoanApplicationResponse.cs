namespace FinanceSap.Application.UseCases.CreateLoanApplication;

// DTO de saída — expõe apenas os dados relevantes ao cliente, sem vazar detalhes internos.
public sealed record CreateLoanApplicationResponse(
    Guid   Id,
    string Status,
    decimal Amount,
    int    TermInMonths,
    DateTime CreatedAt
);
