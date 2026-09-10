using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexusMail.Domain.Automation.Entities;

namespace NexusMail.Infrastructure.Persistence.Configurations;

public sealed class AutomationActionExecutionConfiguration : IEntityTypeConfiguration<AutomationActionExecution>
{
    public void Configure(EntityTypeBuilder<AutomationActionExecution> builder)
    {
        builder.ToTable("AutomationActionExecutions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ActionKey)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.ActionType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => new { x.ExecutionId, x.ActionKey })
            .IsUnique();
    }
}
