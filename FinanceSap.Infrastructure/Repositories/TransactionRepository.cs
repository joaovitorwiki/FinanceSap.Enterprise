using FinanceSap.Domain.Entities;
using FinanceSap.Domain.Interfaces;
using FinanceSap.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinanceSap.Infrastructure.Repositories;

public sealed class TransactionRepository(ApplicationDbContext context) : ITransactionRepository
{
    public async Task AddAsync(Transaction transaction, CancellationToken ct = default)
        => await context.Transactions.AddAsync(transaction, ct);

    public async Task<IReadOnlyList<Transaction>> GetByAccountIdAsync(
        Guid accountId, int page, int pageSize, CancellationToken ct = default)
        => await context.Transactions
                        .AsNoTracking()
                        .Where(x => x.AccountId == accountId)
                        .OrderByDescending(x => x.CreatedAt)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync(ct);

    public async Task<int> CountByAccountIdAsync(Guid accountId, CancellationToken ct = default)
        => await context.Transactions
                        .AsNoTracking()
                        .CountAsync(x => x.AccountId == accountId, ct);
}
