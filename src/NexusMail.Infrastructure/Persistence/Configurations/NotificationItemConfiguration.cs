using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexusMail.Notification.Domain;

namespace NexusMail.Infrastructure.Persistence.Configurations;

public sealed class NotificationItemConfiguration : IEntityTypeConfiguration<NotificationItem>
{
    public void Configure(EntityTypeBuilder<NotificationItem> builder)
    {
        builder.ToTable("Notifications");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Title)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(n => n.Message)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(n => n.ActionUrl)
            .HasMaxLength(500);

        builder.Property(n => n.PayloadJson)
            .HasColumnType("jsonb"); // Works well with PostgreSQL

        builder.Property(n => n.SourceEventId)
            .IsRequired()
            .HasMaxLength(100);

        // Idempotency guardrail: unique constraint on WorkspaceId + SourceEventId
        builder.HasIndex(n => new { n.WorkspaceId, n.SourceEventId })
            .IsUnique();

        builder.HasIndex(n => n.WorkspaceId);
    }
}
