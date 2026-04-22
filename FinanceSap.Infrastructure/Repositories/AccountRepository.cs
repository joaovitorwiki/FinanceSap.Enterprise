using FinanceSap.Domain.Entities;
using FinanceSap.Domain.Interfaces;
using FinanceSap.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinanceSap.Infrastructure.Repositories;

public sealed class AccountRepository(ApplicationDbContext context) : IAccountRepository
{
    public async Task AddAsync(Account account, CancellationToken ct = default)
        => await context.Accounts.AddAsync(account, ct);

    public async Task<Account?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await context.Accounts
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<Account?> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default)
        => await context.Accounts
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.CustomerId == customerId, ct);

    public async Task<bool> ExistsByCustomerIdAsync(Guid customerId, CancellationToken ct = default)
        => await context.Accounts
                        .AsNoTracking()
                        .AnyAsync(x => x.CustomerId == customerId, ct);
}
