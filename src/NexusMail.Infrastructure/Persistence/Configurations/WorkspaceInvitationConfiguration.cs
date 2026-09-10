using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexusMail.Domain.Workspace.Entities;

namespace NexusMail.Infrastructure.Persistence.Configurations;

public sealed class WorkspaceInvitationConfiguration : IEntityTypeConfiguration<WorkspaceInvitation>
{
    public void Configure(EntityTypeBuilder<WorkspaceInvitation> builder)
    {
        builder.ToTable("WorkspaceInvitations");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.WorkspaceId).IsRequired();
        builder.Property(i => i.Email).IsRequired().HasMaxLength(255);
        builder.Property(i => i.Role).IsRequired();
        builder.Property(i => i.Token).IsRequired().HasMaxLength(500);
        builder.Property(i => i.Status).IsRequired();
        builder.Property(i => i.InvitedAtUtc).IsRequired();
        builder.Property(i => i.ExpiresAtUtc).IsRequired();

        builder.HasIndex(i => i.WorkspaceId);
        builder.HasIndex(i => i.Email);
    }
}
