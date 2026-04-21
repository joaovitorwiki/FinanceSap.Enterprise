using FinanceSap.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceSap.Infrastructure.Persistence.Configurations;

// Mapeamento de LoanApplication (Aggregate Root).
// Customer referenciado via FK — cada um tem sua própria tabela e ciclo de vida.
public sealed class LoanApplicationConfiguration : IEntityTypeConfiguration<LoanApplication>
{
    public void Configure(EntityTypeBuilder<LoanApplication> builder)
    {
        builder.ToTable("loan_applications");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Amount)
               .HasColumnType("decimal(18,2)")
               .IsRequired();

        builder.Property(x => x.TermInMonths).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().IsRequired();

        // FK para Customer — um Customer pode ter múltiplas LoanApplications.
        builder.Property(x => x.CustomerId).IsRequired();

        builder.HasOne(x => x.Customer)
               .WithMany()
               .HasForeignKey(x => x.CustomerId)
               .OnDelete(DeleteBehavior.Restrict); // não apaga Customer ao deletar LoanApplication
    }
}
