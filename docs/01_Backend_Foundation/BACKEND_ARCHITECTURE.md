# NexusMail AI
# Backend Architecture Document

Version:
1.0.0

Status:
Approved

Document Type:
Backend Implementation Architecture

Technology:
.NET 8 LTS

Architecture Style:
Clean Architecture
+
DDD
+
CQRS
+
Event Driven Architecture

---

# 1. Backend Overview

NexusMail AI backend is built using:

.NET 8 LTS
ASP.NET Core Web API
Entity Framework Core
PostgreSQL
Redis
RabbitMQ
OpenSearch

Backend responsibilities:

- Business logic
- API exposure
- Data persistence
- Email synchronization
- Event publishing
- Background processing
- Security enforcement

---

# 2. Solution Structure

backend/
NexusMail.sln
src/
├── NexusMail.API
├── NexusMail.Application
├── NexusMail.Domain
├── NexusMail.Infrastructure
├── NexusMail.Shared
workers/
├── NexusMail.Worker.EmailSync
├── NexusMail.Worker.AI
├── NexusMail.Worker.Notification
tests/
├── NexusMail.UnitTests
├── NexusMail.IntegrationTests
├── NexusMail.ArchitectureTests

---

# 3. Project Dependency

Dependency direction:

API
|
v
Application
|
v
Domain
Infrastructure
|
+---- Application
|
+---- Domain

Rules:

Domain knows nothing.
Application owns use cases.
Infrastructure implements details.
API only exposes endpoints.

---

# 4. Domain Project

Project:
NexusMail.Domain

Purpose:
Contains enterprise business rules.

Contains:

Domain/
├── Common
│
├── Entities
│
├── Aggregates
│
├── ValueObjects
│
├── Events
│
├── Exceptions
│
├── Enums
│
└── Specifications

---

# 5. Domain Rules

Domain cannot reference:

Forbidden:

Entity Framework
ASP.NET Core
JSON
RabbitMQ
Redis
AI SDK

Allowed:

.NET Standard Library
Domain primitives

---

# 6. Entity Pattern

Example:

public class Email : AggregateRoot
{
public EmailId Id {get;}
public WorkspaceId WorkspaceId {get;}
public string Subject {get;}

public void Archive()
{
    AddDomainEvent(
      new EmailArchivedEvent(Id)
    );
}
}

Rules:

Entity controls behavior.
No public setters.
No anemic model.

---

# 7. Aggregate Rules

Aggregate:

- Protects consistency.
- Owns child entities.
- Publishes domain events.

Example:

Email
|
+-- Attachment
+-- Participant

Cannot:

attachment.Update()

Must:

email.AddAttachment()

---

# 8. Value Object Strategy

Value objects:

EmailAddress
Money
Priority
Language
TenantId

Characteristics:

- Immutable
- No identity
- Self validation

---

# 9. Application Layer

Project:

NexusMail.Application

Responsibilities:

- Use cases
- CQRS
- Validation
- DTO
- Interfaces

Structure:

Application/
├── Common
├── Features
│
├── Identity
│
├── Workspace
│
├── Email
│
├── AI
│
├── Search
│
├── Automation
│
└── Billing

---

# 10. CQRS Pattern

Commands:

Change system state.

Example:

ConnectEmailAccountCommand
CreateRuleCommand
ArchiveEmailCommand

Queries:

Read data.

Example:

GetInboxQuery
SearchEmailQuery
GetSummaryQuery

---

# 11. MediatR Usage

All application requests use:

IRequest<T>

Pipeline:

Controller
|
v
MediatR
|
+----------------+
|                |
Validation    Logging
|
v
Handler

---

# 12. Application Service Rules

Application services:

DO:
- Coordinate workflow
- Call repositories
- Publish events

DO NOT:
- Contain business rules
- Access database directly

---

# 13. Repository Pattern

Interfaces live in:

Application

Example:

IEmailRepository

Implementation:

Infrastructure
EmailRepository

---

# 14. Infrastructure Layer

Project:

NexusMail.Infrastructure

Structure:

Infrastructure/
├── Persistence
│
├── Identity
│
├── EmailProviders
│
├── AI
│
├── Messaging
│
├── Search
│
├── Cache
│
└── Storage

---

# 15. Database Architecture

Technology:

PostgreSQL

ORM:

Entity Framework Core 8

Structure:

Persistence/
├── AppDbContext
├── Configurations
├── Migrations
└── Repositories

---

# 16. EF Core Rules

Required:

- Fluent API
- Migration only
- No auto migration production
- Explicit relationships

Forbidden:

DataAnnotations everywhere

---

# 17. API Layer

Project:

NexusMail.API

Structure:

API/
├── Controllers
├── Middleware
├── Authentication
├── Filters
├── Extensions
└── Swagger

---

# 18. Controller Rules

Controller:
Only handles:

- HTTP Request
- Authentication
- Response

Example:

POST /emails/archive
Controller
|
ArchiveEmailCommand

No:
- Database calls
- Business logic

---

# 19. Dependency Injection

All services registered through:

DependencyInjection.cs

Example:

services.AddApplication();
services.AddInfrastructure();

---

# 20. Configuration Management

Configuration:

appsettings.json
appsettings.Development.json
Environment Variables
Secret Manager

Never:

Commit secrets.

---

# 21. Background Workers

Separate projects:

Worker.EmailSync
Worker.AI
Worker.Notification

Technology:

.NET Worker Service

Responsibilities:

Email synchronization
AI processing
Notification delivery

---

# 22. Event Publishing

Technology:

RabbitMQ

Flow:

Application
|
Domain Event
|
Event Publisher
|
RabbitMQ

---

# 23. Logging

Technology:

Serilog

Every request:

Contains:
- TraceId
- UserId
- WorkspaceId

---

# 24. Exception Handling

Global middleware:

ExceptionMiddleware

Response:

{
code,
message,
traceId
}

---

# 25. Validation

Technology:

FluentValidation

Pipeline:

Request
|
Validator
|
Handler

---

# 26. Testing Architecture

Unit Test:

Domain rules

Integration Test:

Application + Database

Architecture Test:

Dependency validation

---

# 27. Quality Gates

Every Pull Request:

Required:

Build
Unit Test
Integration Test
Architecture Test
Security Scan

---

# 28. Coding Rules

Required:

SOLID
Clean Code
Nullable Reference Enabled
Async Everywhere
CancellationToken Support

---

# 29. Naming Convention

Projects:

NexusMail.{Layer}

Namespaces:

NexusMail.Domain.Email
NexusMail.Application.Email.Commands
NexusMail.Infrastructure.Persistence

---

# 30. Golden Rules

Rule 1: Domain is the source of business truth.
Rule 2: Application orchestrates.
Rule 3: Infrastructure provides technology.
Rule 4: API exposes contracts.
Rule 5: Workers execute background processes.
Rule 6: Events connect independent modules.
Rule 7: Every dependency must be replaceable.

---

# End Of Document
