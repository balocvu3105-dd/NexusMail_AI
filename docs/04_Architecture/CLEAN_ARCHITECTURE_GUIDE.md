# CLEAN_ARCHITECTURE_GUIDE.md

> NexusMail AI Clean Architecture Specification

Version: 1.0.0

Status: Production

Priority: CRITICAL

Document Type: Software Architecture

Applies To

- Backend
- AI Services
- Shared Libraries

---

# Purpose

This document defines the official Clean Architecture implementation used throughout NexusMail AI.

Every AI Coding Agent MUST follow this document before generating any code.

---

# Architecture Philosophy

The architecture must be:

- Independent of frameworks
- Independent of databases
- Independent of UI
- Independent of AI providers
- Independent of cloud vendors

Business survives technology changes.

Technology must never dictate architecture.

---

# Architecture Layers

```
+------------------------------------------------------+
|                  Presentation Layer                  |
| Controllers / Minimal API / SignalR / Swagger        |
+------------------------------------------------------+
                       ↓
+------------------------------------------------------+
|                 Application Layer                    |
| CQRS / Use Cases / Validators / DTO / Pipelines      |
+------------------------------------------------------+
                       ↓
+------------------------------------------------------+
|                    Domain Layer                      |
| Entities / ValueObjects / Events / Rules             |
+------------------------------------------------------+
                       ↓
+------------------------------------------------------+
|                Infrastructure Layer                  |
| EF Core / Redis / RabbitMQ / OpenSearch              |
+------------------------------------------------------+
```

Only downward dependencies are allowed.

---

# Dependency Rule

Presentation

↓

Application

↓

Domain

↓

Infrastructure

Never reverse dependencies.

Forbidden

Infrastructure → Application

Infrastructure → Presentation

Presentation → Infrastructure (direct access)

Domain → EF Core

Domain → ASP.NET Core

Domain → Redis

Domain → RabbitMQ

---

# Layer Responsibilities

## Presentation

Purpose

Expose the application to external consumers.

Contains

- Controllers
- Minimal APIs
- SignalR Hubs
- Middleware
- Authentication
- Authorization
- Filters
- Swagger
- Dependency Injection bootstrap

Forbidden

Business Rules

Database Access

AI Logic

Repository Calls

---

## Application

Purpose

Execute use cases.

Contains

- Commands
- Queries
- Handlers
- DTOs
- Validators
- Behaviors
- Interfaces
- Mappers
- Transactions
- Use Cases

Responsibilities

- Coordinate workflows
- Publish events
- Call repositories
- Call AI Gateway
- Return DTOs

Forbidden

EF Core

SQL

Redis

RabbitMQ

Business persistence

---

## Domain

Purpose

Represent business.

Contains

- Entities
- Value Objects
- Aggregates
- Domain Events
- Specifications
- Policies
- Factories
- Exceptions
- Repository Interfaces

The Domain Layer is the heart of the system.

Forbidden

HTTP

Database

Frameworks

Logging

Serialization

Caching

Message Queue

AI SDK

Cloud SDK

---

## Infrastructure

Purpose

Implement external technologies.

Contains

- EF Core
- Repository Implementations
- Redis
- RabbitMQ
- OpenSearch
- SMTP
- OAuth Providers
- File Storage
- Logging Providers
- Email Providers
- AI Gateway Clients

Infrastructure may depend on every lower abstraction but owns no business rules.

---

# Dependency Inversion Principle

High-level modules must not depend on low-level modules.

Both depend on abstractions.

Example

Application

↓

IEmailRepository

↓

Infrastructure

EmailRepository

Never

Application

↓

EmailRepository

---

# Data Flow

HTTP Request

↓

Controller

↓

Mediator

↓

Command

↓

Handler

↓

Repository Interface

↓

Repository Implementation

↓

Database

↓

Response DTO

↓

HTTP Response

---

# Read Flow

Browser

↓

GET /emails

↓

Controller

↓

Query

↓

Handler

↓

Repository

↓

Projection

↓

DTO

↓

JSON

---

# Write Flow

POST

↓

Controller

↓

Validator

↓

Command

↓

Handler

↓

Aggregate

↓

Repository

↓

Unit Of Work

↓

Commit

↓

Publish Domain Events

↓

Response

---

# Domain Events Flow

Email Received

↓

Aggregate

↓

Domain Event

↓

Application Event Handler

↓

Notification

↓

Search Index

↓

Analytics

↓

Audit Log

---

# Cross Module Communication

Preferred

- Domain Events
- Integration Events
- REST
- gRPC

Forbidden

Shared database

Cross-module Entity

Cross-module Repository

---

# Folder Layout

```
Email/

Email.Api/

Email.Application/

Email.Domain/

Email.Infrastructure/

Email.Contracts/

Email.Tests/
```

Every module follows the exact same layout.

---

# Shared Kernel

Contains only

- BaseEntity
- ValueObject
- Result<T>
- DomainEvent
- IEntity
- IRepository
- Guard Clauses

Never place business-specific code here.

---

# Application Pipelines

Every command/query passes through

Validation

↓

Authorization

↓

Logging

↓

Caching (Query)

↓

Transaction (Command)

↓

Execution

↓

Metrics

↓

Response

Implemented using MediatR Behaviors.

---

# Repository Pattern

Repositories provide persistence abstraction.

Allowed

GetByIdAsync

FindAsync

ExistsAsync

CountAsync

AddAsync

Update

Delete

Specification

Forbidden

Business Logic

EmailClassification()

SpamDetection()

PriorityCalculation()

---

# Unit Of Work

One Unit Of Work per request.

Commit only after successful execution.

Rollback on failure.

Never manually manage transactions inside Controllers.

---

# DTO Mapping

Entity

↓

DTO

↓

JSON

Mapping tools

Mapster (preferred)

or

AutoMapper

Never expose Entities directly.

---

# Error Handling

Errors propagate upward.

Infrastructure

↓

Application Exception

↓

ProblemDetails

Never leak stack traces.

---

# Configuration

Configuration belongs to Infrastructure.

Application receives configuration through abstractions.

Never access IConfiguration inside Domain.

---

# Logging

Structured Logging only.

Use ILogger<T>.

Every request must include

- CorrelationId
- RequestId
- UserId
- WorkspaceId

Never use Console.WriteLine().

---

# Caching

Caching implemented through decorators or MediatR behaviors.

Application layer should not know Redis exists.

---

# AI Integration

Correct Flow

Application

↓

IAIGateway

↓

Infrastructure

↓

Python Service

↓

LLM

Never call OpenAI directly from Application.

Never reference provider SDKs outside Infrastructure.

---

# Security

Authentication

Presentation

Authorization

Application

Business Permission

Domain

Encryption

Infrastructure

Each concern stays in its proper layer.

---

# Testing Strategy

Presentation

Integration Tests

Application

Unit Tests

Domain

Unit Tests (95%+)

Infrastructure

Integration Tests

---

# Architecture Validation Checklist

Before merging code verify

✔ Dependency direction respected

✔ No business logic in Controllers

✔ Domain has no framework references

✔ Infrastructure implements interfaces only

✔ DTOs returned from APIs

✔ Commands/Queries use MediatR

✔ Validation through FluentValidation

✔ Unit tests added

✔ Integration tests updated

✔ Documentation updated

---

# Definition of Done

A feature complies with Clean Architecture when

✔ Layers are respected

✔ Dependencies flow inward

✔ Business rules isolated

✔ Infrastructure replaceable

✔ UI replaceable

✔ Database replaceable

✔ AI provider replaceable

✔ Tests independent

✔ Modules loosely coupled

✔ Code review passed

---

END OF DOCUMENT