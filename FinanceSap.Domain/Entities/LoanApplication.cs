using FinanceSap.Domain.Enums;
using FinanceSap.Domain.ValueObjects;

namespace FinanceSap.Domain.Entities;

// Aggregate Root do módulo de empréstimos.
// Encapsula invariantes de negócio diretamente no construtor — sem setters públicos.
public sealed class LoanApplication
{
    public Guid     Id             { get; private set; }
    public Guid     CustomerId     { get; private set; }  // FK para Customer (Aggregate Root)
    public Customer Customer       { get; private set; }
    public decimal  Amount         { get; private set; }
    public int      TermInMonths   { get; private set; }
    public DateTime CreatedAt      { get; private set; }
    public LoanStatus Status       { get; private set; }

    // Construtor protegido para uso exclusivo do EF Core (sem validação).
    private LoanApplication() { Customer = null!; }

    public LoanApplication(Customer customer, decimal amount, int termInMonths)
    {
        if (customer is null)
            throw new ArgumentNullException(nameof(customer));
        if (amount <= 0)
            throw new ArgumentException("O valor do empréstimo deve ser maior que zero.", nameof(amount));
        if (termInMonths <= 0)
            throw new ArgumentException("O prazo deve ser maior que zero.", nameof(termInMonths));

        Id           = Guid.NewGuid();
        CustomerId   = customer.Id;
        Customer     = customer;
        Amount       = amount;
        TermInMonths = termInMonths;
        CreatedAt    = DateTime.UtcNow;
        Status       = LoanStatus.Pending;
    }

    public void Approve() => Status = LoanStatus.Approved;
    public void Reject()  => Status = LoanStatus.Rejected;
}
