# CQRS_GUIDE.md

> NexusMail AI CQRS (Command Query Responsibility Segregation) Guide

Version: 1.0.0

Status: Production

Priority: CRITICAL

Document Type: Architecture Standard

Applies To

- Backend
- All Application Modules
- All Business Features

---

# Purpose

This document defines the official CQRS implementation used in NexusMail AI.

Every Application Module MUST follow this guide.

CQRS is mandatory.

CRUD architecture is NOT allowed for business modules.

---

# What is CQRS

CQRS separates

Read

and

Write

into different execution pipelines.

Write Side

↓

Commands

↓

Business Rules

↓

Transaction

↓

Domain Events

↓

Persistence

------------------------------------

Read Side

↓

Queries

↓

Projection

↓

DTO

↓

Response

The Read Model and Write Model are independent.

---

# Why CQRS

Advantages

- Better scalability
- Better maintainability
- Better testability
- Better performance
- Better security
- Better separation of concerns

---

# Folder Structure

Every feature MUST use

```
Feature/

Commands/

Queries/

DTO/

Validators/

Mappings/

Events/

Behaviors/

Handlers/

README.md

```

Example

```
Email/

Commands/

CreateEmail/

ArchiveEmail/

DeleteEmail/

Queries/

GetEmail/

SearchEmail/

GetInbox/

```

---

# Command

Purpose

Modify system state.

Commands

Never return Entity.

Return

Result

Result<T>

Guid

Boolean

Never return collections.

---

# Command Naming

Correct

CreateEmailCommand

DeleteEmailCommand

ArchiveEmailCommand

UpdatePriorityCommand

Wrong

EmailCommand

SaveCommand

UpdateCommand

ProcessCommand

---

# Command Handler

One command

↓

One handler

Example

CreateEmailCommand

↓

CreateEmailCommandHandler

---

# Command Responsibilities

Validate

Load Aggregate

Execute Business Rules

Persist Changes

Publish Events

Return Result

Nothing else.

---

# Query

Purpose

Read data.

Queries NEVER modify state.

---

# Query Naming

Correct

GetInboxQuery

SearchEmailsQuery

GetDashboardQuery

GetEmailDetailQuery

Wrong

EmailQuery

Search

Dashboard

---

# Query Handler

Responsibilities

Read

Project

Map DTO

Return

No transaction.

No write.

No business mutation.

---

# DTO Rules

Separate DTOs for

Command

Query

Response

Never reuse Entity.

---

# Validation

Every Command

Must have

Validator

Examples

CreateEmailValidator

UpdateTagValidator

DeleteWorkspaceValidator

Queries may also have validators.

---

# MediatR Pipeline

Every request flows through

Request

↓

LoggingBehavior

↓

ValidationBehavior

↓

AuthorizationBehavior

↓

PerformanceBehavior

↓

TransactionBehavior (Commands Only)

↓

Handler

↓

Response

---

# Behaviors

Logging

Validation

Authorization

Caching (Queries)

Performance

Transaction

Metrics

Retry

Each behavior has one responsibility.

---

# Read Model

Optimized for reading.

May use

Projection

View

Materialized View

OpenSearch

Never load unnecessary Entities.

---

# Write Model

Uses

Aggregate

Domain Rules

Repository

Unit Of Work

Events

Always transactional.

---

# Domain Events

Commands may publish events.

Example

CreateEmailCommand

↓

EmailCreatedEvent

↓

Notification

↓

Analytics

↓

Search Index

↓

Audit Log

---

# Idempotency

Commands must support idempotency when appropriate.

Example

SyncEmailCommand

Retry must not create duplicate emails.

---

# Transactions

Only Commands

Open transactions.

Queries never use transactions unless explicitly required.

---

# Folder Example

```
CreateEmail/

CreateEmailCommand.cs

CreateEmailCommandHandler.cs

CreateEmailValidator.cs

CreateEmailRequest.cs

CreateEmailResponse.cs

README.md

```

---

# Query Example

```
GetInbox/

GetInboxQuery.cs

GetInboxHandler.cs

InboxDto.cs

README.md

```

---

# Mapping

Preferred

Mapster

Alternative

AutoMapper

Mapping belongs to Application Layer.

---

# Error Handling

Handlers throw

Business Exceptions

Infrastructure Exceptions

Application Exceptions

Controllers return

ProblemDetails

---

# Repository Usage

Commands

↓

Repository

↓

Save

Queries

↓

Repository

↓

Projection

↓

DTO

Repositories never return API models.

---

# Caching

Only Queries may be cached.

Never cache Commands.

Cache invalidation occurs after successful Commands.

---

# Authorization

Commands

Permission Check

Queries

Read Permission Check

Authorization handled through pipeline behaviors.

---

# Logging

Every Command

Information

Start

Success

Failure

Duration

CorrelationId

Every Query

Duration

Result Count

Cache Hit/Miss

---

# Metrics

Collect

Command Execution Time

Query Execution Time

Validation Failures

Retry Count

Cache Hit Ratio

Average Response Time

---

# Naming Convention

Commands

Verb + Entity + Command

Queries

Get/Search/List + Entity + Query

Handlers

<Name>Handler

Validators

<Name>Validator

DTO

<Name>Dto

---

# Testing

Every Command

Unit Tests

Integration Tests

Every Query

Unit Tests

Integration Tests

Target Coverage

Commands

90%+

Queries

80%+

---

# Common Mistakes

❌ Command returns Entity

❌ Query modifies state

❌ Business Logic inside Controller

❌ SQL inside Handler

❌ Validation inside Controller

❌ Reusing DTO for Commands and Queries

❌ Repository containing Business Rules

---

# AI Coding Checklist

Before generating any feature verify

✔ Command exists

✔ CommandHandler exists

✔ Validator exists

✔ Query exists

✔ QueryHandler exists

✔ DTOs created

✔ Mapping configured

✔ Tests generated

✔ Logging enabled

✔ Authorization configured

✔ Documentation updated

---

# Definition of Done

A CQRS feature is complete when

✔ Commands implemented

✔ Queries implemented

✔ Validators added

✔ Pipeline Behaviors configured

✔ Repository used correctly

✔ Domain Events published

✔ Transactions handled

✔ DTOs mapped

✔ Tests pass

✔ Documentation updated

---

END OF DOCUMENT