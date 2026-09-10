using Microsoft.EntityFrameworkCore;
using NexusMail.Application.Abstractions.Persistence;
using NexusMail.Domain.Workspace.Entities;
using NexusMail.Application.Abstractions.Outbox;
using NexusMail.Domain.Email.Entities;
using NexusMail.Domain.Identity.Entities;
using NexusMail.Domain.Automation.Entities;
using NexusMail.Notification.Domain;
using System.Reflection;

namespace NexusMail.Infrastructure.Persistence;

public sealed class ApplicationDbContext : DbContext, IUnitOfWork
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<UserSession> UserSessions { get; set; }
    public DbSet<Workspace> Workspaces { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
    public DbSet<EmailAccount> EmailAccounts { get; set; }
    public DbSet<NexusMail.Domain.Email.Entities.Email> Emails { get; set; }
    public DbSet<EmailSynchronizationState> EmailSynchronizationStates { get; set; }
    public DbSet<AutomationRule> AutomationRules { get; set; }
    public DbSet<AutomationRuleMetric> AutomationRuleMetrics { get; set; }
    public DbSet<AutomationAudit> AutomationAudits { get; set; }
    public DbSet<AutomationExecution> AutomationExecutions { get; set; }
    public DbSet<AutomationActionExecution> AutomationActionExecutions { get; set; }
    public DbSet<NexusMail.Domain.AI.Entities.AIAnalysis> AIAnalyses { get; set; }
    public DbSet<NotificationItem> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            Assembly.GetExecutingAssembly(),
            type => type.Namespace?.Contains("Search.Persistence.Configurations") == false
        );
        base.OnModelCreating(modelBuilder);
    }
}
