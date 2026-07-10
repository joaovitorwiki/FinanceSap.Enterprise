using FinanceSap.Domain.Entities;

namespace FinanceSap.Domain.Interfaces;

public interface ITransactionRepository
{
    Task AddAsync(Transaction transaction, CancellationToken ct = default);
    Task<IReadOnlyList<Transaction>> GetByAccountIdAsync(Guid accountId, int page, int pageSize, CancellationToken ct = default);
    Task<int> CountByAccountIdAsync(Guid accountId, CancellationToken ct = default);
}
