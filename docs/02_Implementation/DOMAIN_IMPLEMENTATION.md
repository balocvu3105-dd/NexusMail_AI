# NexusMail AI
# Domain Implementation Document

Version:
1.0.0

Status:
Implementation Ready

Document Type:
DDD Domain Layer Implementation

Technology:
C# 12
.NET 8 LTS

---

# 1. Domain Implementation Goal

This document defines:
- Entity implementation
- Aggregate roots
- Value objects
- Domain events
- Domain exceptions
- Domain rules

---

# 2. Domain Project Location

backend/src/NexusMail.Domain

Domain contains:

NexusMail.Domain/
├── Common/
├── Entities/
├── Aggregates/
├── ValueObjects/
├── Events/
├── Exceptions/
├── Enums/
└── Interfaces/

---

# 3. Domain Dependency Rules

Domain:

References:
NONE

Allowed:
System libraries only.

Forbidden:
EntityFrameworkCore
ASP.NET Core
RabbitMQ
Redis
AI SDK

---

# 4. Base Entity

Location:
Common/Entity.cs

Purpose:
All entities share:
- Identity
- Equality
- Domain events

Implementation:

```csharp
public abstract class Entity
{
    private readonly List<IDomainEvent> _events = new();

    public Guid Id { get; protected set; }

    public IReadOnlyCollection<IDomainEvent> DomainEvents
        => _events.AsReadOnly();

    protected Entity()
    {
        Id = Guid.NewGuid();
    }

    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _events.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _events.Clear();
    }
}
```

---

# 5. Aggregate Root

Location:
Common/AggregateRoot.cs

Implementation:

```csharp
public abstract class AggregateRoot : Entity
{
}
```

All business aggregates inherit:
AggregateRoot

---

# 6. User Aggregate

Location:
Aggregates/User

Structure:
User.cs
UserStatus.cs

Responsibilities:
Identity ownership
Profile management
Workspace membership

---

# 7. User Entity

Example:

```csharp
public class User : AggregateRoot
{
    public string Email { get; private set; }
    public string DisplayName { get; private set; }

    private User()
    {
    }

    public User(string email, string displayName)
    {
        Email = email;
        DisplayName = displayName;
    }
}
```

---

# 8. Workspace Aggregate

Location:
Aggregates/Workspace

Responsibilities:
Tenant boundary
Member management
Settings

Entity:
Workspace

Properties:
Id
Name
OwnerId
CreatedAt

---

# 9. Email Aggregate

Most important aggregate.

Location:
Aggregates/Email

Responsibilities:
Email lifecycle
State changes
Archive
Delete
Tagging

---

# 10. Email Entity

Example:

```csharp
public class Email : AggregateRoot
{
    public Guid WorkspaceId {get; private set;}
    public string Subject {get; private set;}
    public EmailStatus Status {get; private set;}

    public void Archive()
    {
        Status = EmailStatus.Archived;
        AddDomainEvent(new EmailArchivedEvent(Id));
    }
}
```

---

# 11. Entity Behavior Rule

Entities protect state.

Forbidden:
email.Status = Deleted;

Required:
email.Delete();

Business action belongs inside entity.

---

# 12. Value Objects

Value objects:
EmailAddress
Money
Priority
Language
TagName
RuleCondition

Characteristics:
Immutable
No identity
Equality by value

---

# 13. EmailAddress Value Object

Example:

```csharp
public sealed class EmailAddress
{
    public string Value {get;}

    private EmailAddress(string value)
    {
        if(string.IsNullOrWhiteSpace(value))
            throw new DomainException();

        Value = value;
    }
}
```

---

# 14. TagName Value Object

Rules:
Maximum length
Cannot empty
Normalize text

Example:
Important
Finance
Work

---

# 15. Domain Enumerations

Location:
Enums/

Examples:
EmailStatus
PriorityLevel
UserStatus
SubscriptionStatus

---

# 16. EmailStatus

Example:

```csharp
public enum EmailStatus
{
    Received,
    Processed,
    Archived,
    Deleted
}
```

---

# 17. Domain Events

Location:
Events/

Interface:
```csharp
public interface IDomainEvent
{
    DateTime OccurredAt {get;}
}
```

---

# 18. Core Domain Events

Required:
UserRegistered
WorkspaceCreated
EmailReceived
EmailArchived
EmailTagged
RuleExecuted
SubscriptionActivated

---

# 19. EmailReceived Event

Example:

```csharp
public record EmailReceivedEvent(Guid EmailId) : IDomainEvent
{
    public DateTime OccurredAt => DateTime.UtcNow;
}
```

---

# 20. Domain Exception

Location:
Exceptions/

Base:
```csharp
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}
```

---

# 21. Business Rule Example

Rule:
Email cannot archive when deleted.

Implementation:

```csharp
public void Archive()
{
    if(Status == EmailStatus.Deleted)
        throw new DomainException("Deleted email cannot archive");

    Status = EmailStatus.Archived;
}
```

---

# 22. Aggregate Rules

Aggregate controls:
State transition
Validation
Domain events

Aggregate does NOT:
Save database
Call API
Send email

---

# 23. Factory Pattern

Complex creation:
Use factory.

Example:
EmailFactory
WorkspaceFactory

---

# 24. Domain Services

Used when:
Logic does not belong to one entity.

Example:
EmailPriorityCalculator

Interface:
```csharp
public interface IEmailPriorityService
{
    Priority Calculate(Email email);
}
```

---

# 25. Domain Interfaces

Allowed:
Repository abstraction

Example:
```csharp
public interface IEmailRepository
{
    Task<Email?> GetAsync(Guid id);
}
```

Implementation:
NOT HERE.

---

# 26. Domain Testing Requirement

Every entity requires:
Creation test
State transition test
Invalid operation test
Event test

---

# 27. Domain Folder Final Structure

NexusMail.Domain
├── Common
├── Aggregates
├── Entities
├── ValueObjects
├── Events
├── Exceptions
├── Enums
└── Interfaces

---

# 28. First Implemented Aggregates

Priority:
User
Workspace
Email
Rule
Notification
Subscription

---

# 29. Domain Completion Checklist

[x] Entity base
[x] Aggregate root
[x] Value objects
[x] Domain events
[x] Exceptions
[x] Business rules
[x] Folder structure

---

# 30. Final Domain Rules

Rule 1: Domain owns business truth.
Rule 2: Entities protect their state.
Rule 3: No infrastructure dependency.
Rule 4: Events communicate changes.
Rule 5: Application coordinates, Domain decides.

---

# End Of Document
