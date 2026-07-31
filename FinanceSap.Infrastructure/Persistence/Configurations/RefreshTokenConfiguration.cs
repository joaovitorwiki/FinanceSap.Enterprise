using FinanceSap.Domain.Entities;
using FinanceSap.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceSap.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Configuration for RefreshToken entity.
/// </summary>
public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    /// <summary>
    /// Configures the RefreshToken entity.
    /// </summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        // Table configuration
        builder.ToTable("refresh_tokens");

        // Primary key
        builder.HasKey(rt => rt.Id);

        // Properties configuration
        builder.Property(rt => rt.Id)
            .HasColumnName("Id")
            .HasColumnType("char(36)")
            .IsRequired();

        builder.Property(rt => rt.UserId)
            .HasColumnName("UserId")
            .HasColumnType("char(36)")
            .IsRequired();

        builder.Property(rt => rt.Token)
            .HasColumnName("Token")
            .HasColumnType("varchar(255)")
            .IsRequired();

        builder.Property(rt => rt.JwtTokenId)
            .HasColumnName("JwtTokenId")
            .HasColumnType("varchar(255)")
            .IsRequired();

        builder.Property(rt => rt.ExpiresAt)
            .HasColumnName("ExpiresAt")
            .HasColumnType("datetime(6)")
            .IsRequired();

        builder.Property(rt => rt.CreatedAt)
            .HasColumnName("CreatedAt")
            .HasColumnType("datetime(6)")
            .IsRequired();

        builder.Property(rt => rt.CreatedByIp)
            .HasColumnName("CreatedByIp")
            .HasColumnType("varchar(45)") // IPv6 max length
            .IsRequired();

        builder.Property(rt => rt.RevokedAt)
            .HasColumnName("RevokedAt")
            .HasColumnType("datetime(6)")
            .IsRequired(false);

        builder.Property(rt => rt.RevokedByIp)
            .HasColumnName("RevokedByIp")
            .HasColumnType("varchar(45)") // IPv6 max length
            .IsRequired(false);

        builder.Property(rt => rt.RevocationReason)
            .HasColumnName("RevocationReason")
            .HasColumnType("varchar(255)")
            .IsRequired(false);

        // Indexes
        builder.HasIndex(rt => rt.UserId, "IX_refresh_tokens_UserId");
        builder.HasIndex(rt => rt.Token, "IX_refresh_tokens_Token").IsUnique();
        builder.HasIndex(rt => rt.ExpiresAt, "IX_refresh_tokens_ExpiresAt");

        // Relationships
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}