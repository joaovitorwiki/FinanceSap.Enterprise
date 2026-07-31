using FinanceSap.Domain.Entities;
using FinanceSap.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace FinanceSap.Infrastructure.Persistence;

// DbContext principal — herda de IdentityDbContext para incluir tabelas do ASP.NET Core Identity.
// ApplyConfigurationsFromAssembly auto-descobre todos os IEntityTypeConfiguration<T>.
public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<LoanApplication> LoanApplications => Set<LoanApplication>();
    public DbSet<Customer>        Customers         => Set<Customer>();
    public DbSet<Account>         Accounts          => Set<Account>();
    public DbSet<Loan>            Loans             => Set<Loan>();
    public DbSet<Transaction>     Transactions      => Set<Transaction>();
    public DbSet<RefreshToken>    RefreshTokens     => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Suprime o warning de pending migrations — os testes aplicam MigrateAsync() no InitializeAsync.
        optionsBuilder.ConfigureWarnings(w =>
            w.Ignore(RelationalEventId.PendingModelChangesWarning));
    }
}
