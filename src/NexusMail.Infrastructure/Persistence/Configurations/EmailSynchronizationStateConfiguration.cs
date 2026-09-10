using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexusMail.Domain.Email.Entities;

namespace NexusMail.Infrastructure.Persistence.Configurations;

public sealed class EmailSynchronizationStateConfiguration : IEntityTypeConfiguration<EmailSynchronizationState>
{
    public void Configure(EntityTypeBuilder<EmailSynchronizationState> builder)
    {
        builder.ToTable("EmailSynchronizationStates");

        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.EmailAccountId).IsRequired();
        
        builder.Property(x => x.Status).IsRequired().HasConversion<int>();
        
        builder.Property(x => x.LastError).HasMaxLength(2000);
        
        builder.Property(x => x.CorrelationId).HasMaxLength(100);
        
        builder.Property(x => x.Version)
               .IsRowVersion()
               .HasColumnName("xmin");
               
        builder.HasIndex(x => x.EmailAccountId).IsUnique();
        builder.HasIndex(x => x.Status);
        
        builder.HasOne<EmailAccount>()
               .WithOne(e => e.State)
               .HasForeignKey<EmailSynchronizationState>(x => x.EmailAccountId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
