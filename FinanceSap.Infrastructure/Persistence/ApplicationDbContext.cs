using FinanceSap.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanceSap.Infrastructure.Persistence;

// DbContext principal da aplicação.
// ApplyConfigurationsFromAssembly auto-descobre todos os IEntityTypeConfiguration<T>
// desta assembly — nenhum registro manual necessário.
public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options)
{
    public DbSet<LoanApplication> LoanApplications => Set<LoanApplication>();
    public DbSet<Customer>        Customers         => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
