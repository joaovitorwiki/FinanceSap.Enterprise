using FinanceSap.Domain.Enums;

namespace FinanceSap.Domain.Entities;

// Transaction — registro imutável de cada movimentação financeira.
// Nunca é modificada após criação — garante integridade do audit trail.
public sealed class Transaction
{
    public Guid            Id              { get; private set; }
    public Guid            AccountId       { get; private set; }
    public decimal         Amount          { get; private set; }
    public TransactionType TransactionType { get; private set; }
    public DateTime        CreatedAt       { get; private set; }

    // Navegação opcional — para queries de extrato.
    public Account Account { get; private set; } = null!;

    private Transaction() { } // EF Core

    public static Transaction Create(Guid accountId, decimal amount, TransactionType type)
        => new()
        {
            Id              = Guid.NewGuid(),
            AccountId       = accountId,
            Amount          = amount,
            TransactionType = type,
            CreatedAt       = DateTime.UtcNow
        };
}
