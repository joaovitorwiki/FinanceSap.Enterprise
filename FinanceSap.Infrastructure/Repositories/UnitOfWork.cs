using FinanceSap.Domain.Common;
using FinanceSap.Domain.Interfaces;
using FinanceSap.Infrastructure.Persistence;
using MediatR;

namespace FinanceSap.Infrastructure.Repositories;

// Garante atomicidade: persiste mudanças e publica Domain Events no mesmo ciclo.
// Ordem: SaveChanges primeiro → eventos publicados após confirmação no banco.
public sealed class UnitOfWork(ApplicationDbContext context, IPublisher publisher) : IUnitOfWork
{
    public async Task<int> CommitAsync(CancellationToken ct = default)
    {
        // Coleta eventos antes de salvar (entidades podem ser detached após SaveChanges).
        var events = context.ChangeTracker
            .Entries<BaseEntity>()
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();

        var result = await context.SaveChangesAsync(ct);

        // Publica eventos após persistência bem-sucedida — garante consistência.
        foreach (var domainEvent in events)
            await publisher.Publish(domainEvent, ct);

        return result;
    }
}
