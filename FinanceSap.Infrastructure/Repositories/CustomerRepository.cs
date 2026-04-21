using FinanceSap.Domain.Entities;
using FinanceSap.Domain.Interfaces;
using FinanceSap.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinanceSap.Infrastructure.Repositories;

// Implementação concreta de ICustomerRepository.
// Todas as queries são parametrizadas pelo EF Core — sem risco de SQL Injection.
public sealed class CustomerRepository(ApplicationDbContext context) : ICustomerRepository
{
    public async Task AddAsync(Customer customer, CancellationToken ct = default)
        => await context.Customers.AddAsync(customer, ct);

    public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await context.Customers
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<bool> ExistsByDocumentAsync(string document, CancellationToken ct = default)
        => await context.Customers
                        .AsNoTracking()
                        .AnyAsync(x => x.Document == document, ct);
}
