using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexusMail.Domain.Identity.Entities;

namespace NexusMail.Infrastructure.Persistence.Configurations;

public sealed class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.ToTable("UserSessions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.RefreshTokenHash)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasIndex(x => x.RefreshTokenHash)
            .IsUnique();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.ExpiresAtUtc)
            .IsRequired();

        builder.Property(x => x.DeviceName)
            .HasMaxLength(255);

        builder.Property(x => x.IpAddress)
            .HasMaxLength(45); // IPv6 max length

        builder.Property(x => x.UserAgent)
            .HasMaxLength(1000);
            
        // Setup foreign key constraint (UserSession is its own aggregate, but points to User)
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
