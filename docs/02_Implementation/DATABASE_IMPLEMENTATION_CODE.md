# NexusMail AI
# Database Implementation Code Document

Version:
1.0.0

Status:
Implementation Ready

Document Type:
Persistence Layer Implementation

Technology:
PostgreSQL 16
Entity Framework Core 8
Npgsql
Redis

---

# 1. Database Architecture Goal

This document defines:
- DbContext implementation
- Entity mapping
- Database conventions
- Migration strategy
- Repository pattern
- Tenant isolation
- Audit system

---

# 2. Database Project Location

backend/src/NexusMail.Infrastructure

Database folder:

Infrastructure/
├── Persistence/
│   ├── Configurations/
│   ├── Migrations/
│   ├── Repositories/
│   └── Interceptors/

---

# 3. Database Provider

Production:
PostgreSQL 16

Reason:
- Open source
- JSON support
- pgvector support
- Enterprise ready

---

# 4. EF Core Setup

Package:
Npgsql.EntityFrameworkCore.PostgreSQL

---

# 5. DbContext

Location:
Persistence/NexusMailDbContext.cs

Implementation:
```csharp
public class NexusMailDbContext : DbContext
{
    public DbSet<User> Users {get;set;}
    public DbSet<Workspace> Workspaces {get;set;}
    public DbSet<Email> Emails {get;set;}

    public NexusMailDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(NexusMailDbContext).Assembly);
    }
}
```

---

# 6. Entity Configuration Pattern

Never configure inside Entity.

Use:
IEntityTypeConfiguration<T>

Example:
Configurations/
UserConfiguration.cs
EmailConfiguration.cs
WorkspaceConfiguration.cs

---

# 7. User Mapping

Example:
```csharp
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Email)
            .HasMaxLength(256)
            .IsRequired();
    }
}
```

---

# 8. Workspace Mapping

Table: workspaces

Columns:
Id
Name
OwnerId
CreatedAt

Index:
OwnerId

---

# 9. Email Mapping

Email is large entity.

Table: emails

Columns:
Id
WorkspaceId
Subject
Body
Sender
ReceivedAt
Status

---

# 10. Email Index Strategy

Required indexes:
IX_Email_WorkspaceId
IX_Email_ReceivedAt
IX_Email_Status
IX_Email_ProviderMessageId

Purpose:
Fast inbox query.

---

# 11. Value Object Mapping

Example:
EmailAddress:
Stored as: varchar

Implementation:
```csharp
builder
    .Property(x => x.Email)
    .HasConversion(
        x => x.Value,
        x => new EmailAddress(x)
    );
```

---

# 12. Enumeration Mapping

Store enum as:
INTEGER

Example:
EmailStatus
0 Received
1 Processed
2 Archived

---

# 13. DateTime Convention

All timestamps:
UTC only.

Columns:
CreatedAt
UpdatedAt

---

# 14. Base Entity Audit

Interface:
IAuditableEntity

Contains:
CreatedAt
UpdatedAt
CreatedBy

---

# 15. Audit Save Interceptor

Location:
Interceptors/AuditInterceptor.cs

Responsibilities:
Before Save:
Set CreatedAt
Update UpdatedAt
Track User

---

# 16. Soft Delete

Entities: Support IsDeleted

Example:
```csharp
public bool IsDeleted { get; private set; }
```

---

# 17. Global Query Filter

Example:
```csharp
builder.HasQueryFilter(x => !x.IsDeleted);
```

Deleted data:
Still exists.
Not visible.

---

# 18. Multi Tenant Database Isolation

Critical.
Every tenant resource contains: WorkspaceId

Example: Email:
Email
{
    Id
    WorkspaceId
    Subject
}

---

# 19. Tenant Query Filter

Current tenant:
IWorkspaceContext

Filter:
```csharp
builder.HasQueryFilter(x => x.WorkspaceId == workspaceContext.Id);
```

---

# 20. Repository Pattern

Location:
Repositories/

Interface:
```csharp
public interface IRepository<T>
{
    Task<T?> GetAsync(Guid id);
    Task AddAsync(T entity);
}
```

---

# 21. Repository Implementation

Example:
```csharp
public class Repository<T> : IRepository<T> where T:Entity
{
    private readonly DbContext _context;

    public async Task AddAsync(T entity)
    {
        await _context.Set<T>().AddAsync(entity);
    }
}
```

---

# 22. Unit Of Work

Interface:
IUnitOfWork

Responsibilities:
Commit transaction
Publish domain events

---

# 23. Transaction Strategy

Default:
One application command: One transaction.

Example:
Create Rule
|
Database Transaction
|
Commit

---

# 24. Domain Event Dispatcher

After SaveChanges:

Flow:
DbContext
 |
Collect Domain Events
 |
Publish
 |
Clear Events

---

# 25. Migration Strategy

Tool:
dotnet ef migrations add

Example:
dotnet ef migrations add InitialCreate

Apply:
dotnet ef database update

---

# 26. Migration Rules

Never:
Modify existing migration.

Always:
Create new migration.

---

# 27. Database Naming Convention

Tables: snake_case
Example: email_accounts, refresh_tokens, audit_logs

Columns: created_at, updated_at

---

# 28. Database Schema Groups

Schemas:
identity
email
ai
automation
billing
audit

---

# 29. Initial Tables

Phase 1:
Identity: users, roles, permissions, refresh_tokens
Email: email_accounts, emails, attachments
AI: ai_results, embeddings

---

# 30. Database Seed

Development only:
Seed: Admin role, Default permissions, Test workspace

Never:
Production password.

---

# 31. Connection Configuration

Example:
```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Database=nexusmail"
  }
}
```

---

# 32. Performance Optimization

Required:
Indexing
Pagination
No Tracking Query
Projection
Batch Operations

---

# 33. Read Optimization

Queries:
Use: AsNoTracking()

For:
Inbox
Search
Dashboard

---

# 34. Backup Strategy

Production:
Daily backup.
Retention: 30-90 days.

---

# 35. Database Testing

Required:
Test: Migration, Repository, Transaction, Tenant isolation
Using: Testcontainers PostgreSQL

---

# 36. Completion Checklist

[x] DbContext created
[x] Entity mapping
[x] Repository
[x] Migration strategy
[x] Audit
[x] Soft delete
[x] Tenant isolation
[x] Testing strategy

---

# 37. Final Database Rules

Rule 1: Database is Infrastructure.
Rule 2: Domain does not know EF Core.
Rule 3: Every tenant query is isolated.
Rule 4: All changes are audited.
Rule 5: Migration is version controlled.

---

# End Of Document
