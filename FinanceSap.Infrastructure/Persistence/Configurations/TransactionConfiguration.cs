using FinanceSap.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceSap.Infrastructure.Persistence.Configurations;

public sealed class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("transactions");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Amount)
               .HasColumnType("decimal(18,2)")
               .IsRequired();

        builder.Property(x => x.TransactionType)
               .HasConversion<string>()
               .HasMaxLength(20)
               .IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasOne(x => x.Account)
               .WithMany()
               .HasForeignKey(x => x.AccountId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.AccountId)
               .HasDatabaseName("IX_transactions_account_id");

        builder.HasIndex(x => x.CreatedAt)
               .HasDatabaseName("IX_transactions_created_at");
    }
}
