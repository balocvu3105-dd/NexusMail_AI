using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexusMail.Domain.Workspace.Entities;

namespace NexusMail.Infrastructure.Persistence.Configurations;

public sealed class WorkspaceConfiguration : IEntityTypeConfiguration<Workspace>
{
    public void Configure(EntityTypeBuilder<Workspace> builder)
    {
        builder.ToTable("Workspaces");
        builder.HasKey(w => w.Id);
        
        builder.Property(w => w.Name).IsRequired().HasMaxLength(200);
        builder.Property(w => w.Plan).IsRequired().HasMaxLength(50);
        
        builder.HasMany(w => w.Members)
               .WithOne()
               .HasForeignKey(m => m.WorkspaceId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
