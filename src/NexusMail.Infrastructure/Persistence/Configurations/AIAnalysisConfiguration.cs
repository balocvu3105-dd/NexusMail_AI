using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexusMail.Domain.AI.Entities;

namespace NexusMail.Infrastructure.Persistence.Configurations;

public class AIAnalysisConfiguration : IEntityTypeConfiguration<AIAnalysis>
{
    public void Configure(EntityTypeBuilder<AIAnalysis> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.EmailId)
               .IsRequired();

        builder.HasIndex(a => a.EmailId)
               .IsUnique();

        builder.Property<uint>("Version")
               .IsRowVersion();

        builder.Property(a => a.Category)
               .IsRequired(false);

        builder.Property(a => a.Language)
               .IsRequired(false);

        builder.Property(a => a.Summary)
               .IsRequired(false);

        builder.Property(a => a.Priority)
               .IsRequired(false);

        builder.Property(a => a.SummaryStatus)
               .HasConversion<string>()
               .IsRequired();
               
        builder.Property(a => a.PriorityStatus)
               .HasConversion<string>()
               .IsRequired();
               
        builder.Property(a => a.ClassificationStatus)
               .HasConversion<string>()
               .IsRequired();
               
        builder.Property(a => a.EmbeddingStatus)
               .HasConversion<string>()
               .IsRequired();

        builder.Property(a => a.Confidence);
        
        builder.Property(a => a.PriorityScore);

        builder.PrimitiveCollection(a => a.Tags)
               .HasColumnType("text[]");
    }
}
