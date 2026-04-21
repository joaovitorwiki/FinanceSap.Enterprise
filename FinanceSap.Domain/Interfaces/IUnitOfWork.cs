namespace FinanceSap.Domain.Interfaces;

// Abstrai a transação de banco de dados, garantindo atomicidade entre múltiplos repositórios.
public interface IUnitOfWork
{
    Task<int> CommitAsync(CancellationToken ct = default);
}
