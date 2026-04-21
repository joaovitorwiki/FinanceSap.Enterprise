using FinanceSap.Domain.Entities;
using FinanceSap.Domain.Interfaces;
using FinanceSap.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinanceSap.Infrastructure.Repositories;

// Implementação concreta do repositório. Queries parametrizadas pelo EF Core nativamente.
public sealed class LoanApplicationRepository(ApplicationDbContext context)
    : ILoanApplicationRepository
{
    public async Task AddAsync(LoanApplication loanApplication, CancellationToken ct = default)
        => await context.LoanApplications.AddAsync(loanApplication, ct);

    public async Task<LoanApplication?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await context.LoanApplications
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<LoanApplication>> GetAllAsync(CancellationToken ct = default)
        => await context.LoanApplications
                        .AsNoTracking()
                        .ToListAsync(ct);
}
