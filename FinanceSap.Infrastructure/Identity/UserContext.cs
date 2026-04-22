using FinanceSap.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace FinanceSap.Infrastructure.Identity;

// Implementação de IUserContext usando ASP.NET Core Identity.
// Adapta UserManager para a interface do Domain (Adapter Pattern).
public sealed class UserContext(UserManager<ApplicationUser> userManager) : IUserContext
{
    public async Task<Guid?> GetCustomerIdByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        return user?.CustomerId;
    }
}
