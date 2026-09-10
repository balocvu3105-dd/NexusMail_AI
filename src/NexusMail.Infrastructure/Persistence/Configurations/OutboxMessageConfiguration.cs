using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexusMail.Application.Abstractions.Outbox;

namespace NexusMail.Infrastructure.Persistence.Configurations;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");

        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Type).IsRequired().HasMaxLength(255);
        
        builder.Property(x => x.Content).IsRequired();
        
        builder.Property(x => x.LockId).HasMaxLength(100);
        
        builder.Property(x => x.CorrelationId).HasMaxLength(100);

        builder.Property(x => x.FailureReason).HasConversion<int>();

        builder.HasIndex(x => new { x.ProcessedOnUtc, x.LockedUntilUtc })
               .HasFilter("\"ProcessedOnUtc\" IS NULL AND \"DeadLetteredAt\" IS NULL");
    }
}
