using FinanceSap.Domain.Entities;

namespace FinanceSap.Domain.Interfaces;

public interface IAccountRepository
{
    Task AddAsync(Account account, CancellationToken ct = default);
    Task<Account?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Account?> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default);
    Task<bool> ExistsByCustomerIdAsync(Guid customerId, CancellationToken ct = default);
}
