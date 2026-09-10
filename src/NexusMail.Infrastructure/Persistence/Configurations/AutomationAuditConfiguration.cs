using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexusMail.Domain.Automation.Entities;

namespace NexusMail.Infrastructure.Persistence.Configurations;

internal sealed class AutomationAuditConfiguration : IEntityTypeConfiguration<AutomationAudit>
{
    public void Configure(EntityTypeBuilder<AutomationAudit> builder)
    {
        builder.ToTable("AutomationAudits");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Executor)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Result)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(a => a.EmailId);
        builder.HasIndex(a => a.RuleId);
    }
}
