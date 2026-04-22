using FinanceSap.Domain.Entities;
using FinanceSap.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FinanceSap.Infrastructure.Persistence;

// DbContext principal — herda de IdentityDbContext para incluir tabelas do ASP.NET Core Identity.
// ApplyConfigurationsFromAssembly auto-descobre todos os IEntityTypeConfiguration<T>.
public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<LoanApplication> LoanApplications => Set<LoanApplication>();
    public DbSet<Customer>        Customers         => Set<Customer>();
    public DbSet<Account>         Accounts          => Set<Account>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // Configura tabelas do Identity.

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
