# NexusMail AI
# Database Implementation Document

Version:
1.0.0

Status:
Approved

Document Type:
Database Engineering Specification

Technology:

PostgreSQL 16
Entity Framework Core 8
.NET 8 LTS

---

# 1. Database Overview

NexusMail AI uses PostgreSQL as the primary transactional database.

Responsibilities:
- User data
- Workspace data
- Email metadata
- AI results
- Automation rules
- Billing
- Audit logs

PostgreSQL is the source of truth.

---

# 2. Database Architecture

Application
  |
  v
Repository Interface
  |
  v
EF Core Repository
  |
  v
PostgreSQL

---

# 3. Database Project Location

Database implementation belongs to:
NexusMail.Infrastructure
|
Persistence

Structure:

Persistence/
├── AppDbContext.cs
├── Configurations/
├── Repositories/
├── Interceptors/
├── Migrations/
└── Seed/

---

# 4. DbContext Design

Main context:
NexusMailDbContext

Responsibilities:
- Entity mapping
- Tracking
- Save changes
- Transactions
- Audit handling

Example:
```csharp
public class NexusMailDbContext : DbContext
{
}
```

---

# 5. DbContext Rules

DbContext:

Allowed:
Entity configuration
SaveChanges override
Transaction handling

Forbidden:
Business logic
External API calls
AI processing

---

# 6. Entity Configuration Strategy

Use:
EF Core Fluent API

Location:
Persistence/Configurations

Example:
EmailConfiguration.cs

Not allowed:
DataAnnotations everywhere

---

# 7. Base Entity Model

All entities inherit:
EntityBase

Contains:
Id
CreatedAt
UpdatedAt

Example:
```csharp
public abstract class EntityBase
{
    public Guid Id {get;protected set;}
    public DateTime CreatedAt {get;protected set;}
    public DateTime UpdatedAt {get;protected set;}
}
```

---

# 8. Auditable Entity

Sensitive entities use:
AuditableEntity

Contains:
CreatedBy
UpdatedBy

Used for:
User
Workspace
Billing
Security entities

---

# 9. Soft Delete Strategy

Entities supporting deletion contain:
IsDeleted
DeletedAt

Example:
Email
Attachment
Rule

---

# 10. Global Query Filter

EF Core global filter:

Example:
```csharp
builder.Entity<Email>()
    .HasQueryFilter(x => !x.IsDeleted);
```

Purpose:
Prevent accidental deleted data access.

---

# 11. Multi Tenant Implementation

Every tenant-owned entity contains:
WorkspaceId

Example:
Email
Id
WorkspaceId
Subject

---

# 12. Tenant Query Filter

All tenant data queries require:
WorkspaceContext

Example:
CurrentWorkspaceId

Repository automatically applies:
WHERE WorkspaceId = CurrentWorkspaceId

---

# 13. Primary Key Strategy

Use:
UUID

Example:
Guid

Reason:
Distributed system ready
Avoid predictable IDs
Better SaaS scaling

---

# 14. Entity Relationships

Use explicit relationships.

Example:
Email:

Workspace
    |
    |
    N
Email

---

# 15. Required Relationships

Important:
Workspace 1:N Email Account
Email Account 1:N Email
Email 1:N Attachment
Email 1:N AI Result

---

# 16. Enum Storage

Enums stored as:
String

Example:
Priority.High

Database:
"High"

Reason:
Better readability.

---

# 17. JSON Column Usage

PostgreSQL JSONB allowed for:
Dynamic settings
Metadata
AI configuration
Provider response

Example:
WorkspaceSettings JSONB

Not allowed for:
Core business data.

---

# 18. Index Strategy

Every important query requires index.

Email Indexes
Required:
WorkspaceId
AccountId
ReceivedAt
ConversationId

Example:
IX_Email_Workspace_ReceivedAt

User Indexes
Required:
Email

Unique:
IX_User_Email_Unique

Audit Indexes
Required:
WorkspaceId
CreatedAt

---

# 19. Unique Constraints

Examples:

User: Email unique
Workspace: Organization + Name
Email Account: Workspace + Provider + Address

---

# 20. Migration Strategy

Technology:
EF Core Migration

Commands:

Create:
dotnet ef migrations add InitialCreate

Apply:
dotnet ef database update

---

# 21. Production Migration Rules

Forbidden:
Automatic migration on startup.

Wrong:
Database.Migrate()

Production process:
CI Pipeline
      |
Migration Review
      |
Deploy

---

# 22. Seed Data

Only system data.

Examples:
Default roles
Permissions
System settings

Never seed:
User data
Email data

---

# 23. Transaction Strategy

Transactions required for:
Email import
Rule execution
Billing operation

Example:
Begin Transaction
Save Changes
Publish Event
Commit

---

# 24. Domain Event Integration

Database events flow:
Entity
 |
Domain Event
 |
SaveChangesInterceptor
 |
Event Publisher
 |
RabbitMQ

---

# 25. Repository Implementation

Repository location:
Infrastructure/Persistence/Repositories

Example:
Interface: IEmailRepository
Implementation: EmailRepository

---

# 26. Query Optimization

Required:
Projection
Pagination
AsNoTracking
Proper indexes

Example:
```csharp
db.Emails
  .AsNoTracking()
  .Select(x => new EmailDto())
```

---

# 27. Tracking Rules

Read operation: Use AsNoTracking()
Write operation: Use Tracked Entity

---

# 28. Pagination Rules

Never: Load all emails.

Wrong:
db.Emails.ToList()

Correct:
Skip()
Take()

---

# 29. Connection Management

Database connection:
Managed by EF Core Pooling

Configuration:
AddDbContextPool

---

# 30. Backup Strategy

Required:
Daily backup
Point-in-time recovery
Encrypted backup

---

# 31. Database Security

Required:
Encrypted connection
Least privilege account
No root database user
Secret rotation

---

# 32. Performance Target

Goals:
API query: <200ms
Inbox query: <100ms
Search: OpenSearch

---

# 33. Scaling Strategy

Stage 1: Single PostgreSQL
Stage 2: Read Replica
Stage 3: Partition Email tables
Stage 4: Tenant database isolation

---

# 34. Database Testing

Required:
Integration tests with Testcontainers PostgreSQL

Verify:
Migration
Repository
Query
Tenant isolation

---

# 35. Database Rules Summary

Rule 1: PostgreSQL is source of truth.
Rule 2: Infrastructure owns persistence.
Rule 3: All tenant entities contain WorkspaceId.
Rule 4: No direct DbContext access outside Infrastructure.
Rule 5: Every important query requires index.
Rule 6: Migration is controlled.
Rule 7: Database design follows Domain Model.

---

# End Of Document
