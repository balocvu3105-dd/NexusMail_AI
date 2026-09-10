using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexusMail.Domain.Email.Entities;

namespace NexusMail.Infrastructure.Persistence.Configurations;

internal sealed class EmailAccountConfiguration : IEntityTypeConfiguration<EmailAccount>
{
    public void Configure(EntityTypeBuilder<EmailAccount> builder)
    {
        builder.ToTable("EmailAccounts");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.WorkspaceId)
            .IsRequired();

        builder.Property(e => e.Provider)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.EmailAddress)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(e => e.EncryptedAccessToken)
            .IsRequired();

        builder.Property(e => e.EncryptedRefreshToken)
            .IsRequired(false);

        builder.Property(e => e.EncryptionVersion)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        // PostgreSQL xmin concurrency token mapping
        builder.Property(e => e.Version)
            .IsRowVersion()
            .HasColumnName("xmin");

        builder.HasIndex(e => new { e.WorkspaceId, e.EmailAddress }).IsUnique();
        builder.HasIndex(e => new { e.WorkspaceId, e.Status });

        builder.HasOne<NexusMail.Domain.Workspace.Entities.Workspace>()
            .WithMany()
            .HasForeignKey(e => e.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

