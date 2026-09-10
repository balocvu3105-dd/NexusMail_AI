# NexusMail AI
# Coding Standard Document

Version:
1.0.0

Status:
Approved

Document Type:
Software Engineering Standard

Technology:
C# 12
.NET 8 LTS

---

# 1. Coding Philosophy

NexusMail AI follows:
Clean Code
SOLID
DDD Principles
Explicit Design
Readable First

Priority order:

Correctness
  >
Maintainability
  >
Performance
  >
Optimization

Code must be:
- Easy to understand
- Easy to test
- Easy to replace
- Easy to extend

---

# 2. Language Standard

Required:
C# 12
.NET 8 LTS

Enable:
Nullable Reference Types
Implicit Usings
Global Using

---

# 3. Project Configuration

Every project uses:

<TargetFramework>net8.0</TargetFramework>
<Nullable>enable</Nullable>
<ImplicitUsings>enable</ImplicitUsings>

---

# 4. Naming Convention

## Classes
PascalCase

Example:
```csharp
EmailService
UserRepository
CreateEmailCommand
```

## Interfaces
Prefix: I

Example:
IEmailRepository
IAIService

## Methods
PascalCase

Example:
GetEmailAsync()
CreateWorkspaceAsync()

## Variables
camelCase

Example:
emailRepository
workspaceId

## Constants
PascalCase

Example:
DefaultPageSize

---

# 5. File Organization

One public type per file.

Example:
Email.cs
EmailRepository.cs
CreateEmailCommand.cs

Filename must match type name.

---

# 6. Namespace Convention

Format:
NexusMail.{Layer}.{Feature}

Examples:
NexusMail.Domain.Email
NexusMail.Application.Email.Commands
NexusMail.Infrastructure.Persistence
NexusMail.API.Controllers

---

# 7. Class Design Rules

Classes should have:
Single responsibility
Small public surface
Explicit dependencies

Bad:
public class EmailManager
{
    // 2000 lines
}

Good:
public class EmailSyncService
{
}

---

# 8. Dependency Injection Rules

Forbidden:
new EmailRepository();

Required:
private readonly IEmailRepository repository;

public EmailService(IEmailRepository repository)
{
    this.repository = repository;
}

---

# 9. Constructor Rules

Use:
Primary constructor
or
Explicit constructor injection

Avoid:
Service Locator

---

# 10. Async Programming Rules

All I/O operations must be async.

Required:
Task
ValueTask
async/await
CancellationToken

Example:
public async Task<Email> GetAsync(Guid id, CancellationToken cancellationToken)
{
}

---

# 11. CancellationToken Rules

Every async application method accepts:
CancellationToken cancellationToken

Example:
Task ProcessAsync(CancellationToken cancellationToken)

---

# 12. ConfigureAwait Rule

Library code should use:
ConfigureAwait(false)

Application code:
Not required.

---

# 13. LINQ Rules

Allowed:
Readable LINQ.

Example:
var unreadEmails = emails.Where(x => x.IsUnread).ToList();

Avoid:
Complex chained LINQ.

Bad:
.Where().Select().GroupBy().SelectMany()...
Move logic into methods.

---

# 14. Exception Handling

Do not use:
catch(Exception)
{
}
without handling.

Use domain exceptions:
Example:
throw new EmailNotFoundException(id);

---

# 15. Exception Layer Rules

Domain: Business exceptions.
Application: Use case exceptions.
Infrastructure: Technical exceptions.
API: Convert to HTTP response.

---

# 16. Logging Standard

Technology:
Serilog

Every log should include:
TraceId
UserId
WorkspaceId
Operation

Example:
logger.LogInformation("Email synchronized {EmailId}", emailId);

---

# 17. Sensitive Data Logging

Never log:
Password
OAuth Token
Email Body
Attachment
API Key

---

# 18. DTO Rules

Never expose:
Domain Entity

Wrong:
return email;

Correct:
return EmailDtoMapper.Map(email);

---

# 19. Entity Rules

Entities:
Must protect state.

Avoid:
public string Subject {get;set;}

Prefer:
public string Subject {get; private set;}

---

# 20. Repository Rules

Repository responsibilities:

Allowed:
Query database
Save entity
Load aggregate

Forbidden:
Business logic
Authorization
External API calls

---

# 21. Service Rules

Application Service: Coordinates workflow.
Domain Service: Contains complex business rules.
Infrastructure Service: Handles external technology.

---

# 22. Validation Rules

Technology:
FluentValidation

Every command requires validation.

Example:
CreateWorkspaceCommandValidator

---

# 23. Comments Rules

Code should explain itself.

Avoid:
// Loop emails
foreach(...)

Use comments only for:
Why
Business decision
Complex algorithm

---

# 24. Magic Number Rule

Forbidden:
if(score > 80)

Use:
HighPriorityThreshold

---

# 25. Configuration Rules

Never:
const string apiKey="xxx";

Use:
IOptions<T>

---

# 26. Database Rules

Required:
EF Fluent API
Migration
Repository abstraction

Avoid:
Direct DbContext usage everywhere.

---

# 27. API Controller Rules

Controllers must be thin.

Allowed:
return await mediator.Send(command);

Forbidden:
Database logic.

---

# 28. Unit Testing Rules

Every business rule requires tests.

Test naming:
Method_State_ExpectedResult

Example:
ArchiveEmail_WhenEmailExists_ShouldArchive

---

# 29. Architecture Testing

Must verify:
Domain cannot reference Infrastructure.
Application cannot reference API.

---

# 30. Git Convention

Commit format:
type(scope): message

Examples:
feat(email): add gmail sync
fix(auth): refresh token issue
docs(api): update contract

---

# 31. Pull Request Rules

Every PR requires:
Description
Testing result
Architecture impact
Security impact

---

# 32. AI Generated Code Rules

AI generated code must:
Follow existing patterns
Include tests
Respect dependency rules
Not introduce new libraries without approval

AI cannot:
Modify architecture silently
Add random packages
Duplicate existing services

---

# 33. Performance Rules

Avoid premature optimization.

Required:
Async I/O
Pagination
Caching where needed
Database indexing

---

# 34. Security Rules

Every feature review:

Must answer:
Authentication?
Authorization?
Tenant isolation?
Sensitive data?
Audit requirement?

---

# 35. Final Engineering Rules

Rule 1: Readable code beats clever code.
Rule 2: Business logic belongs in Domain.
Rule 3: External systems belong in Infrastructure.
Rule 4: Every dependency must be injectable.
Rule 5: Every feature must be testable.
Rule 6: Every change must preserve architecture.

---

# End Of Document
