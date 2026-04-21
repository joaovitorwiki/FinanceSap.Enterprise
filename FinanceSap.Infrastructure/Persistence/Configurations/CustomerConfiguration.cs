using FinanceSap.Domain.Entities;
using FinanceSap.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FinanceSap.Infrastructure.Persistence.Configurations;

// Mapeamento de Customer como Aggregate Root independente.
// ValueConverter<Cpf, string> explícito: necessário para EF Core inspecionar o tipo
// em tempo de design (Add-Migration) e para queries LINQ funcionarem corretamente.
public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        var cpfConverter = new ValueConverter<Cpf, string>(
            cpf => cpf.Value,
            raw => Cpf.Create(raw).Value
        );

        builder.Property(x => x.Document)
               .HasColumnName("document")
               .HasColumnType("VARCHAR(11)")
               .HasMaxLength(11)
               .IsRequired()
               .HasConversion(cpfConverter);

        // Índice único garante integridade no nível do banco — complementa a verificação
        // ExistsByDocumentAsync que previne a race condition na maioria dos casos.
        builder.HasIndex(x => x.Document)
               .IsUnique()
               .HasDatabaseName("IX_customers_document");

        builder.Property(x => x.FullName)
               .HasColumnName("full_name")
               .HasColumnType("VARCHAR(150)")
               .HasMaxLength(150)
               .IsRequired();
    }
}
