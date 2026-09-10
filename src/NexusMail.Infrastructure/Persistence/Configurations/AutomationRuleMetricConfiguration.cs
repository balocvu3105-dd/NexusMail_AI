using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexusMail.Domain.Automation.Entities;

namespace NexusMail.Infrastructure.Persistence.Configurations;

public sealed class AutomationRuleMetricConfiguration : IEntityTypeConfiguration<AutomationRuleMetric>
{
    public void Configure(EntityTypeBuilder<AutomationRuleMetric> builder)
    {
        builder.ToTable("AutomationRuleMetrics");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Result).IsRequired().HasMaxLength(50);
        builder.Property(x => x.ErrorMessage).HasMaxLength(2000);
        
        builder.HasIndex(x => x.RuleId);
        builder.HasIndex(x => x.EmailId);
    }
}
