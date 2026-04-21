using FinanceSap.Domain.Interfaces;
using FinanceSap.Infrastructure.Persistence;

namespace FinanceSap.Infrastructure.Repositories;

// Implementação do Unit of Work: delega SaveChangesAsync ao DbContext.
// Garante que múltiplas operações de repositório sejam persistidas atomicamente.
public sealed class UnitOfWork(ApplicationDbContext context) : IUnitOfWork
{
    public Task<int> CommitAsync(CancellationToken ct = default)
        => context.SaveChangesAsync(ct);
}
