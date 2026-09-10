using Microsoft.EntityFrameworkCore;
using NexusMail.Domain.Search.Entities;

namespace NexusMail.Infrastructure.Search.Persistence;

public class SearchDbContext : DbContext
{
    public SearchDbContext(DbContextOptions<SearchDbContext> options) : base(options)
    {
    }

    public DbSet<EmailSearchIndex> EmailSearchIndices => Set<EmailSearchIndex>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("vector");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SearchDbContext).Assembly, type => type.Namespace?.Contains("Search.Persistence.Configurations") == true);
        base.OnModelCreating(modelBuilder);
    }
}
