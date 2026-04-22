using FinanceSap.Domain.Common;

namespace FinanceSap.Domain.Entities;

// Account — Aggregate Root do módulo financeiro.
// Invariante crítica: Balance nunca pode ser negativo (garantida no construtor e em Debit).
public sealed class Account
{
    public Guid     Id            { get; private set; }
    public string   AccountNumber { get; private set; } = null!;
    public decimal  Balance       { get; private set; }
    public DateTime CreatedAt     { get; private set; }
    public Guid     CustomerId    { get; private set; }

    // Navegação para Customer — relacionamento 1:1.
    public Customer Customer { get; private set; } = null!;

    private Account() { } // EF Core

    private Account(string accountNumber, Guid customerId)
    {
        Id            = Guid.NewGuid();
        AccountNumber = accountNumber;
        Balance       = 0m; // Conta criada com saldo zero — invariante inicial.
        CreatedAt     = DateTime.UtcNow;
        CustomerId    = customerId;
    }

    public static Result<Account> Create(string accountNumber, Guid customerId)
    {
        if (string.IsNullOrWhiteSpace(accountNumber) || accountNumber.Length != 10)
            return Result<Account>.Failure("Número de conta inválido.");

        if (customerId == Guid.Empty)
            return Result<Account>.Failure("Cliente inválido.");

        return Result<Account>.Success(new Account(accountNumber, customerId));
    }

    // Crédito — sempre permitido, aumenta o saldo.
    public void Credit(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Valor de crédito deve ser positivo.", nameof(amount));

        Balance += amount;
    }

    // Débito — valida invariante: saldo não pode ficar negativo.
    public Result Debit(decimal amount)
    {
        if (amount <= 0)
            return Result.Failure("Valor de débito deve ser positivo.");

        if (Balance - amount < 0)
            return Result.Failure("Saldo insuficiente.", ErrorType.Validation);

        Balance -= amount;
        return Result.Success();
    }
}
