using FinanceSap.Domain.Entities;

namespace FinanceSap.Domain.Interfaces;

// Contrato de persistência para o aggregate LoanApplication.
// Mantido no Domain para que a Application dependa apenas de abstrações.
public interface ILoanApplicationRepository
{
    Task AddAsync(LoanApplication loanApplication, CancellationToken ct = default);
    Task<LoanApplication?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<LoanApplication>> GetAllAsync(CancellationToken ct = default);
}
