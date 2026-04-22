using FinanceSap.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceSap.Infrastructure.Persistence.Configurations;

// Mapeamento de Account — Aggregate Root do módulo financeiro.
// Relacionamento 1:1 com Customer via CustomerId (FK).
public sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("accounts");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.AccountNumber)
               .HasColumnType("VARCHAR(10)")
               .HasMaxLength(10)
               .IsRequired();

        // Índice único em AccountNumber — garante unicidade no banco.
        builder.HasIndex(x => x.AccountNumber)
               .IsUnique()
               .HasDatabaseName("IX_accounts_account_number");

        builder.Property(x => x.Balance)
               .HasColumnType("decimal(18,2)")
               .IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired();

        // Relacionamento 1:1 com Customer.
        // OnDelete Restrict: não permite deletar Customer se Account existir.
        builder.HasOne(x => x.Customer)
               .WithOne()
               .HasForeignKey<Account>(x => x.CustomerId)
               .OnDelete(DeleteBehavior.Restrict);

        // Índice único em CustomerId — garante 1:1 no nível do banco.
        builder.HasIndex(x => x.CustomerId)
               .IsUnique()
               .HasDatabaseName("IX_accounts_customer_id");
    }
}
