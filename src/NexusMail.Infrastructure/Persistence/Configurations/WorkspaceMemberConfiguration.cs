using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexusMail.Domain.Workspace.Entities;

namespace NexusMail.Infrastructure.Persistence.Configurations;

public sealed class WorkspaceMemberConfiguration : IEntityTypeConfiguration<WorkspaceMember>
{
    public void Configure(EntityTypeBuilder<WorkspaceMember> builder)
    {
        builder.ToTable("WorkspaceMembers");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.WorkspaceId).IsRequired();
        builder.Property(m => m.UserId).IsRequired();
        builder.Property(m => m.Role).IsRequired();
        builder.Property(m => m.JoinedAtUtc).IsRequired();

        builder.HasIndex(m => new { m.WorkspaceId, m.UserId }).IsUnique();
    }
}
