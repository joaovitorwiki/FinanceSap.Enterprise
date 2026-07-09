using FinanceSap.Domain.Entities;

namespace FinanceSap.Domain.Interfaces;

public interface ILoanRepository
{
    Task AddAsync(Loan loan, CancellationToken ct = default);
    Task<Loan?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Loan?> GetByIdTrackedAsync(Guid id, CancellationToken ct = default);
}
