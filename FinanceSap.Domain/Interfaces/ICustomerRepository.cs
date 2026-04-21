using FinanceSap.Domain.Entities;

namespace FinanceSap.Domain.Interfaces;

// Contrato de persistência para o Aggregate Root Customer.
// Reside no Domain — a Application depende apenas desta abstração.
public interface ICustomerRepository
{
    Task AddAsync(Customer customer, CancellationToken ct = default);
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsByDocumentAsync(string document, CancellationToken ct = default);
}
