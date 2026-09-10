# NexusMail AI
# Solution Structure Document

Version:
1.0.0

Status:
Approved

Document Type:
.NET Solution Architecture

Technology:
.NET 8 LTS

---

# 1. Solution Overview

Solution Name:
NexusMail

Purpose:
Define the complete backend code organization.

Architecture:
Clean Architecture
+
DDD
+
Modular Monolith

---

# 2. Repository Structure

Root:

NexusMail/
├── backend/
├── frontend/
├── ai/
├── shared/
├── docs/
├── database/
├── devops/
├── prompts/
├── tests/
└── tools/

---

# 3. Backend Structure

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
└── NexusMail.ArchitectureTests

---

# 4. Project List

## NexusMail.Domain

Type:
Class Library

Purpose:
Business core.

Contains:
- Entities
- Aggregates
- Value Objects
- Domain Events
- Domain Exceptions

Reference:
NONE

---

## NexusMail.Application

Type:
Class Library

Purpose:
Application business flow.

Contains:
- Commands
- Queries
- Handlers
- DTO
- Interfaces
- Validators

References:
NexusMail.Domain
NexusMail.Shared

---

## NexusMail.Infrastructure

Type:
Class Library

Purpose:
External implementations.

Contains:
- Database
- EF Core
- Email Provider
- AI Client
- Redis
- RabbitMQ
- Storage

References:
NexusMail.Domain
NexusMail.Application
NexusMail.Shared

---

## NexusMail.API

Type:
ASP.NET Core Web API

Purpose:
HTTP Interface.

Contains:
- Controllers
- Middleware
- Authentication
- Swagger
- API Configuration

References:
NexusMail.Application
NexusMail.Infrastructure
NexusMail.Shared

---

# 5. Worker Projects

## NexusMail.Worker.EmailSync

Type:
.NET Worker Service

Purpose:
Synchronize email providers.

Responsibilities:
- Gmail Sync
- Outlook Sync
- IMAP Sync
- Queue publishing

References:
Application
Infrastructure
Shared

---

## NexusMail.Worker.AI

Purpose:
AI processing pipeline.

Responsibilities:
- Consume EmailReceived event
- Call AI service
- Store AI result

References:
Application
Infrastructure
Shared

---

## NexusMail.Worker.Notification

Purpose:
Notification delivery.

Responsibilities:
- Desktop notification
- Mobile push
- Discord
- Slack
- Teams

References:
Application
Infrastructure
Shared

---

# 6. Complete Dependency Graph

              NexusMail.API
                   |
                   v
          NexusMail.Application
                   |
                   v
           NexusMail.Domain

                   ^
                   |
      NexusMail.Infrastructure

Workers:

Worker
|
+---- Application
|
+---- Infrastructure
|
+---- Shared

---

# 7. Folder Convention

Every project follows:

Project/
├── Features/
├── Common/
├── Extensions/
├── Configuration/
└── DependencyInjection.cs

---

# 8. Application Feature Structure

Example:

Application/
Features/
Email/
├── Commands/
│   │── ConnectAccount/
│   │── ArchiveEmail/
├── Queries/
│   │── GetInbox/
│   │── GetEmailDetail/
├── DTOs/
└── Validators/

---

# 9. Domain Feature Structure

Example:

Domain/
Modules/
Email/
├── Entities/
├── ValueObjects/
├── Events/
├── Exceptions/

---

# 10. Infrastructure Structure

Infrastructure/
Persistence/
├── AppDbContext.cs
├── Configurations/
├── Repositories/
Email/
├── Gmail/
├── Outlook/
├── IMAP/
AI/
├── OpenAI/
├── Ollama/
Messaging/
├── RabbitMQ/
Caching/
├── Redis/
Storage/
├── S3/

---

# 11. Namespace Convention

Pattern:
NexusMail.{Project}.{Module}

Examples:
NexusMail.Domain.Email
NexusMail.Application.Email.Commands
NexusMail.Infrastructure.Persistence
NexusMail.API.Controllers

---

# 12. File Naming Convention

Classes:
PascalCase

Example:
EmailSyncService.cs
EmailRepository.cs
CreateRuleCommand.cs

Interfaces:
Prefix: I

Example:
IEmailRepository.cs
IAIService.cs

---

# 13. Dependency Injection Convention

Every project exposes:
DependencyInjection.cs

Example:
ApplicationDependencyInjection
InfrastructureDependencyInjection

Usage:
builder.Services.AddApplication();
builder.Services.AddInfrastructure();

---

# 14. Configuration Structure

Configuration/
├── DatabaseOptions.cs
├── RedisOptions.cs
├── AIOptions.cs
├── EmailOptions.cs

---

# 15. Environment Structure

.env
.env.development
.env.production

Secrets:
Never committed.

---

# 16. Test Structure

tests/
NexusMail.UnitTests/
├── Domain/
├── Application/
NexusMail.IntegrationTests/
├── API/
├── Database/
├── Messaging/
NexusMail.ArchitectureTests/
├── DependencyTests/

---

# 17. Build Configuration

Root files:
Directory.Build.props
Directory.Build.targets
.editorconfig
global.json

Purpose:
Centralize:
- Compiler settings
- Nullable
- Warning rules
- Package versions

---

# 18. Package Management

Use:
Central Package Management

File:
Directory.Packages.props

Example:
MediatR
EntityFrameworkCore
FluentValidation
Serilog

---

# 19. Source Control Structure

Git:
main
develop
feature/*
bugfix/*

---

# 20. Docker Structure

docker/
├── api/
├── worker-email/
├── worker-ai/
├── worker-notification/

---

# 21. Future Module Expansion

New module example:
Calendar

Add:
Domain.Calendar
Application.Calendar
Infrastructure.Calendar
API.Calendar

No existing module modification required.

---

# 22. Architecture Rules

Rule 1: Projects must follow dependency direction.
Rule 2: Domain has zero external dependency.
Rule 3: Feature folders preferred over technical folders.
Rule 4: Every module owns its logic.
Rule 5: No shared database access between modules.
Rule 6: Communication through contracts/events.

---

# 23. Final Solution Tree

NexusMail
backend
├── NexusMail.sln
├── src
│   ├── NexusMail.API
│   ├── NexusMail.Application
│   ├── NexusMail.Domain
│   ├── NexusMail.Infrastructure
│   ├── NexusMail.Shared
├── workers
│   ├── Worker.EmailSync
│   ├── Worker.AI
│   └── Worker.Notification
└── tests
    ├── UnitTests
    ├── IntegrationTests
    └── ArchitectureTests

---

# End Of Document
