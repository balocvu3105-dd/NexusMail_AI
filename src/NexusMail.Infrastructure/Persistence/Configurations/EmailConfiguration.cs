using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexusMail.Domain.Email.Entities;

namespace NexusMail.Infrastructure.Persistence.Configurations;

public class EmailConfiguration : IEntityTypeConfiguration<NexusMail.Domain.Email.Entities.Email>
{
    public void Configure(EntityTypeBuilder<NexusMail.Domain.Email.Entities.Email> builder)
    {
        builder.ToTable("Emails");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.MessageId).IsRequired().HasMaxLength(255);
        builder.Property(x => x.Sender).IsRequired().HasMaxLength(255);
        builder.Property(x => x.Subject).HasMaxLength(1000);
        builder.Property(x => x.LifecycleState).HasMaxLength(50);
        
        builder.HasIndex(x => x.AccountId);
        builder.HasIndex(x => new { x.AccountId, x.MessageId }).IsUnique();
    }
}
