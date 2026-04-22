using Microsoft.AspNetCore.Identity;

namespace FinanceSap.Infrastructure.Identity;

// ApplicationUser — estende IdentityUser com CustomerId.
// Link 1:1 entre autenticação (Identity) e domínio (Customer).
// Permite queries de ownership sem joins complexos.
public sealed class ApplicationUser : IdentityUser<Guid>
{
    public Guid? CustomerId { get; set; }
}
