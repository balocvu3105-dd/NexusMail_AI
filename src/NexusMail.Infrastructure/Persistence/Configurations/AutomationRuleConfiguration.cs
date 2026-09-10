using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexusMail.Domain.Automation.Entities;

namespace NexusMail.Infrastructure.Persistence.Configurations;

public sealed class AutomationRuleConfiguration : IEntityTypeConfiguration<AutomationRule>
{
    public void Configure(EntityTypeBuilder<AutomationRule> builder)
    {
        builder.ToTable("AutomationRules");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(255);
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.ConditionsJson).HasColumnType("jsonb");
        builder.Property(x => x.ActionsJson).HasColumnType("jsonb");
        
        builder.HasIndex(x => x.WorkspaceId);
    }
}
