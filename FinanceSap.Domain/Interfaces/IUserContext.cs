namespace FinanceSap.Domain.Interfaces;

// Abstração de contexto do usuário autenticado.
// Permite que a Application acesse UserId e CustomerId sem depender de Identity (Clean Architecture).
public interface IUserContext
{
    Task<Guid?> GetCustomerIdByUserIdAsync(Guid userId, CancellationToken ct = default);
}
