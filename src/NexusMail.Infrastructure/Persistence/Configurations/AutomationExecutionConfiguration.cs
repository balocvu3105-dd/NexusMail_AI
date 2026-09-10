using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexusMail.Domain.Automation.Entities;

namespace NexusMail.Infrastructure.Persistence.Configurations;

public sealed class AutomationExecutionConfiguration : IEntityTypeConfiguration<AutomationExecution>
{
    public void Configure(EntityTypeBuilder<AutomationExecution> builder)
    {
        builder.ToTable("AutomationExecutions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => new { x.RuleId, x.EmailId })
            .IsUnique();
    }
}
