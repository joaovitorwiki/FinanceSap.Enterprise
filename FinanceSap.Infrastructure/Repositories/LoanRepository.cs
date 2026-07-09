using FinanceSap.Domain.Entities;
using FinanceSap.Domain.Interfaces;
using FinanceSap.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinanceSap.Infrastructure.Repositories;

public sealed class LoanRepository(ApplicationDbContext context) : ILoanRepository
{
    public async Task AddAsync(Loan loan, CancellationToken ct = default)
        => await context.Loans.AddAsync(loan, ct);

    public async Task<Loan?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await context.Loans
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<Loan?> GetByIdTrackedAsync(Guid id, CancellationToken ct = default)
        => await context.Loans
                        .FirstOrDefaultAsync(x => x.Id == id, ct);
}
