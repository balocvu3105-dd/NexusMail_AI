# MODULE_TEMPLATE.md

> NexusMail AI Module Blueprint

Version: 1.0.0

Status: Production

Priority: CRITICAL

Document Type: Module Development Standard

Applies To

- Email Module
- AI Module
- Notification Module
- Search Module
- Workspace Module
- Billing Module
- Analytics Module
- Automation Module
- Every Future Module

---

# Purpose

This document defines the standard module architecture used throughout NexusMail AI.

Every module MUST follow this structure exactly.

Every AI Coding Agent MUST generate modules based on this template.

---

# Module Principles

Each module is

- Independent
- Loosely Coupled
- Highly Cohesive
- Deployable
- Testable
- Observable
- Replaceable

A module owns its own business logic.

No module may directly modify another module's database.

---

# Standard Module Layout

```
ModuleName/

README.md
Architecture.md
CHANGELOG.md

Module.Api/
Module.Application/
Module.Domain/
Module.Infrastructure/
Module.Contracts/
Module.Tests/
```

---

# Module.Api

Purpose

Expose HTTP API.

Contains

```
Controllers/

Endpoints/

Filters/

Middlewares/

Swagger/

DependencyInjection/

Extensions/

Program.cs
```

Responsibilities

- Authentication
- Authorization
- HTTP Request
- HTTP Response
- Model Binding
- OpenAPI

Forbidden

Business Logic

Database Access

Entity Framework

---

# Module.Application

Purpose

Application orchestration.

Folder Structure

```
Application/

Commands/

Queries/

DTO/

Validators/

Mappings/

Interfaces/

Behaviors/

Events/

Services/

Pipelines/

Authorization/

Caching/
```

Responsibilities

- CQRS
- Use Cases
- Transactions
- Mapping
- Validation
- Publish Events

Forbidden

EF Core

Redis

RabbitMQ

HTTP

---

# Commands

Example

```
Commands/

CreateEmail/

ArchiveEmail/

DeleteEmail/

RestoreEmail/

MoveEmail/

```

Each Command Folder

```
Command.cs

Handler.cs

Validator.cs

Request.cs

Response.cs

README.md
```

---

# Queries

Example

```
Queries/

GetInbox/

SearchEmails/

GetEmail/

GetDashboard/

```

Each Query Folder

```
Query.cs

Handler.cs

Response.cs

README.md
```

---

# DTO Structure

```
DTO/

Common/

Request/

Response/

Summary/

Projection/

Mapping/
```

DTOs are immutable.

Prefer records over classes where appropriate.

---

# Validators

One validator per request.

Framework

FluentValidation

Example

```
CreateEmailValidator

DeleteTagValidator

SearchEmailValidator
```

---

# Behaviors

```
LoggingBehavior

ValidationBehavior

AuthorizationBehavior

CachingBehavior

MetricsBehavior

PerformanceBehavior

TransactionBehavior
```

Implemented through MediatR.

---

# Application Services

Only orchestration.

Examples

```
EmailSyncService

ReminderScheduler

RuleExecutionService

AIWorkflowService
```

Never place business rules here.

---

# Module.Domain

Purpose

Business Core

Folder Structure

```
Entities/

Aggregates/

ValueObjects/

Events/

Factories/

Policies/

Specifications/

Enums/

Exceptions/

Repositories/

Services/

Constants/
```

---

# Entity Example

```
Email

Attachment

Tag

Folder

Reminder
```

Entities own business behavior.

Never expose setters unnecessarily.

---

# Aggregate Example

```
Email

├── Attachments

├── Tags

├── Metadata

├── AIResult

└── History
```

Aggregate Root

Email

---

# Value Objects

Examples

```
EmailAddress

Priority

Language

TagName

FolderName

RuleCondition
```

Immutable.

Equality by value.

---

# Domain Events

Examples

```
EmailCreated

EmailArchived

EmailDeleted

EmailTagged

PriorityChanged

SummaryGenerated
```

Published after successful transactions.

---

# Repository Interfaces

Located inside Domain.

Examples

```
IEmailRepository

ITagRepository

IFolderRepository
```

Persistence abstraction only.

---

# Module.Infrastructure

Purpose

Technology implementation.

Folder Structure

```
Persistence/

Repositories/

Configurations/

Providers/

Caching/

Messaging/

BackgroundJobs/

Search/

AI/

Email/

OAuth/

Storage/

Logging/

DependencyInjection/
```

---

# Persistence

Contains

```
DbContext

EntityConfigurations

Migrations

Interceptors

Seed
```

Never contains business rules.

---

# Providers

Examples

```
GmailProvider

OutlookProvider

YahooProvider

IMAPProvider
```

Every provider implements

```
IEmailProvider
```

---

# Background Jobs

Examples

```
EmailSyncJob

SummaryGenerationJob

SearchIndexJob

NotificationRetryJob
```

Hosted Services only.

Never block HTTP requests.

---

# Search Integration

Examples

```
EmailIndexer

SemanticSearch

VectorEmbedding

RankingService
```

---

# AI Integration

Folder

```
AI/

Classification/

Summary/

Reply/

Tagging/

Translation/

Gateway/
```

All communication goes through

```
IAIGateway
```

---

# Module.Contracts

Purpose

Contracts shared with other modules.

Folder

```
Requests/

Responses/

Events/

Enums/

Messages/
```

No business logic.

---

# Module.Tests

Folder Structure

```
Unit/

Integration/

Performance/

Fixtures/

Builders/

Mocks/
```

Coverage

80%+

Domain

95%+

---

# Documentation

Every module must include

README.md

Architecture.md

API.md

CHANGELOG.md

KnownIssues.md

MigrationGuide.md (when needed)

---

# Dependency Injection

Every module exposes

```
AddEmailModule()

AddNotificationModule()

AddSearchModule()
```

Never register services outside module boundaries.

---

# Configuration

Each module owns its own

```
appsettings.json

Options

Environment Variables
```

Use strongly typed Options pattern.

---

# Health Checks

Every module registers

```
Database

Redis

Queue

Search

AI Gateway

Email Provider
```

---

# Observability

Every module provides

Structured Logging

Metrics

Tracing

Health Checks

Audit Logs

---

# Security

Every module defines

Permissions

Policies

Roles

Resource Authorization

Sensitive Actions Audit

---

# Versioning

Each module maintains

Semantic Version

MAJOR.MINOR.PATCH

Independent changelog.

---

# Module Communication

Preferred

Domain Events

Integration Events

REST

gRPC

Forbidden

Shared database

Direct repository calls

Direct entity references

---

# AI Coding Checklist

Before generating a new module verify

✔ Folder structure created

✔ API layer added

✔ Application layer added

✔ Domain layer added

✔ Infrastructure layer added

✔ Contracts added

✔ Tests added

✔ README created

✔ Architecture documented

✔ DI configured

✔ Health Checks added

✔ Logging enabled

✔ Metrics enabled

✔ Security configured

✔ OpenAPI documented

✔ CI ready

---

# Definition of Done

A module is considered complete when

✔ Clean Architecture respected

✔ CQRS implemented

✔ DDD implemented

✔ Tests passing

✔ Documentation complete

✔ Security reviewed

✔ Performance validated

✔ Logging configured

✔ Monitoring enabled

✔ Health checks operational

✔ CI/CD compatible

✔ No architecture violations

---

END OF DOCUMENT