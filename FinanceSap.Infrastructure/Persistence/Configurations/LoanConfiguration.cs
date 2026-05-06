using FinanceSap.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceSap.Infrastructure.Persistence.Configurations;

public sealed class LoanConfiguration : IEntityTypeConfiguration<Loan>
{
    public void Configure(EntityTypeBuilder<Loan> builder)
    {
        builder.ToTable("loans");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.CustomerId)
               .HasColumnName("customer_id")
               .IsRequired();

        builder.Property(x => x.PrincipalAmount)
               .HasColumnName("principal_amount")
               .HasColumnType("DECIMAL(18,2)")
               .IsRequired();

        builder.Property(x => x.InterestRate)
               .HasColumnName("interest_rate")
               .HasColumnType("DECIMAL(5,2)")
               .IsRequired();

        builder.Property(x => x.Installments)
               .HasColumnName("installments")
               .IsRequired();

        builder.Property(x => x.MonthlyPaymentAmount)
               .HasColumnName("monthly_payment_amount")
               .HasColumnType("DECIMAL(18,2)")
               .IsRequired();

        builder.Property(x => x.TotalToPay)
               .HasColumnName("total_to_pay")
               .HasColumnType("DECIMAL(18,2)")
               .IsRequired();

        builder.Property(x => x.Status)
               .HasColumnName("status")
               .HasColumnType("VARCHAR(20)")
               .HasMaxLength(20)
               .HasConversion<string>()
               .IsRequired();

        builder.Property(x => x.CreatedAt)
               .HasColumnName("created_at")
               .IsRequired();

        builder.Property(x => x.UpdatedAt)
               .HasColumnName("updated_at")
               .IsRequired(false);

        builder.HasIndex(x => x.CustomerId)
               .HasDatabaseName("IX_loans_customer_id");

        builder.HasIndex(x => x.Status)
               .HasDatabaseName("IX_loans_status");
    }
}
