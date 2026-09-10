# NAMING_CONVENTIONS.md

> NexusMail AI Naming Convention Standard

Version: 1.0.0

Status: Production

Priority: CRITICAL

Document Type: Engineering Standard

Applies To

- Backend (.NET)
- Python AI Service
- Frontend (React)
- Database
- Infrastructure
- DevOps
- Documentation

---

# Purpose

This document defines the official naming conventions used across NexusMail AI.

Consistent naming improves readability, maintainability, discoverability, and AI-assisted code generation.

Every AI Coding Agent MUST follow these rules.

---

# General Principles

Names must be

- Clear
- Descriptive
- Consistent
- Predictable
- Business-oriented

Avoid abbreviations unless industry standard.

Bad

```
Mgr

Tmp

Util

Data

Obj

Info

Stuff

Misc
```

Good

```
EmailClassificationService

NotificationDispatcher

WorkspaceRepository

AttachmentMetadata
```

---

# C# Naming

## Namespace

PascalCase

```
NexusMail.Email.Domain

NexusMail.Search.Application

NexusMail.Notification.Infrastructure
```

---

## Class

PascalCase

```
EmailService

EmailRepository

SummaryGenerator

NotificationHub
```

---

## Interface

Prefix

I

```
IEmailRepository

IAIGateway

INotificationProvider
```

---

## Record

PascalCase

```
CreateEmailCommand

EmailSummaryDto
```

---

## Enum

PascalCase

```
EmailPriority

NotificationType

ProviderStatus
```

Enum Values

PascalCase

```
High

Medium

Low
```

---

## Method

PascalCase

Verb first

```
CreateEmail()

Archive()

GenerateSummary()

SendNotification()
```

Never

```
Email()

Summary()

Notification()
```

---

## Property

PascalCase

```
CreatedUtc

WorkspaceId

EmailAddress

RetryCount
```

---

## Private Field

CamelCase

Prefix

_

```
_emailRepository

_logger

_configuration

_aiGateway
```

---

## Local Variable

camelCase

```
email

summary

workspace

provider
```

---

## Constant

PascalCase

```
DefaultTimeout

MaxRetryCount

DefaultLanguage
```

---

## Static Readonly

PascalCase

```
SupportedProviders

AllowedMimeTypes
```

---

# Async Methods

Every asynchronous method ends with

Async

Correct

```
GetEmailAsync()

SaveChangesAsync()

GenerateSummaryAsync()
```

Wrong

```
GetEmail()

Load()

Save()
```

when asynchronous.

---

# Commands

Pattern

```
Verb + Entity + Command
```

Examples

```
CreateEmailCommand

ArchiveEmailCommand

UpdateTagCommand

DeleteWorkspaceCommand
```

---

# Queries

Pattern

```
Get/Search/List + Entity + Query
```

Examples

```
GetInboxQuery

SearchEmailsQuery

ListNotificationsQuery
```

---

# Handlers

Pattern

```
<Name>Handler
```

Examples

```
CreateEmailCommandHandler

SearchEmailsQueryHandler
```

---

# Validators

Pattern

```
<Name>Validator
```

Examples

```
CreateWorkspaceValidator

GenerateReplyValidator
```

---

# DTO

Suffix

Dto

Examples

```
EmailDto

WorkspaceSummaryDto

NotificationResponseDto
```

---

# Events

Past tense

Examples

```
EmailCreated

EmailArchived

SummaryGenerated

NotificationSent
```

Never

```
CreateEmail

Archive

SendNotification
```

---

# Exceptions

Suffix

Exception

Examples

```
InvalidEmailException

WorkspaceLimitExceededException

DuplicateTagException
```

---

# Repository

Suffix

Repository

Examples

```
EmailRepository

WorkspaceRepository

SearchRepository
```

---

# Services

Suffix

Service

Examples

```
EmailSyncService

SummaryService

NotificationService
```

---

# Factories

Suffix

Factory

Examples

```
EmailFactory

WorkspaceFactory
```

---

# Specifications

Suffix

Specification

Examples

```
UnreadEmailSpecification

PremiumWorkspaceSpecification
```

---

# Python Naming

Files

snake_case.py

```
summary_service.py

embedding_provider.py

vector_search.py
```

Classes

PascalCase

Functions

snake_case

Variables

snake_case

Constants

UPPER_CASE

---

# React Naming

Components

PascalCase

```
InboxPage.tsx

NotificationCard.tsx

WorkspaceSettings.tsx
```

Hooks

Prefix

use

```
useEmails()

useNotifications()

useWorkspace()
```

Contexts

Suffix

Context

```
AuthContext

WorkspaceContext
```

Providers

Suffix

Provider

```
ThemeProvider

AuthProvider
```

---

# CSS

Prefer

Tailwind

If CSS Modules

```
Inbox.module.css
```

---

# API Routes

Lowercase

Plural

Correct

```
/api/v1/emails

/api/v1/workspaces

/api/v1/tags
```

Wrong

```
/Emails

/GetEmail

/emailList
```

---

# Database Naming

Tables

snake_case

```
emails

workspaces

notifications
```

Columns

snake_case

```
workspace_id

created_utc

is_deleted
```

Primary Key

id

Foreign Keys

```
workspace_id

email_id

user_id
```

Indexes

```
idx_email_created

idx_workspace_name
```

Constraints

```
pk_emails

fk_email_workspace

uq_workspace_name
```

---

# Docker

Images

```
nexusmail-api

nexusmail-ai

nexusmail-web
```

Containers

Lowercase

Hyphen separated

---

# Kubernetes

Deployment

```
email-api

ai-service

notification-worker
```

Service

```
email-service
```

ConfigMap

```
email-config
```

Secret

```
email-secret
```

---

# Environment Variables

UPPER_CASE

```
DATABASE_CONNECTION

REDIS_CONNECTION

JWT_SECRET

OPENAI_API_KEY

AI_TIMEOUT_SECONDS
```

---

# Git Branches

```
feature/email-sync

feature/semantic-search

bugfix/login-timeout

hotfix/token-refresh

release/v1.2.0
```

---

# Markdown Files

UPPER_CASE

Examples

```
README.md

SECURITY_GUIDE.md

TESTING_GUIDE.md

PROJECT_CONTEXT.md
```

---

# Folder Naming

Feature folders

PascalCase

```
Email

Notification

Workspace

AI
```

Infrastructure folders

PascalCase

```
Repositories

Persistence

Providers

Messaging
```

---

# File Naming

One public class

↓

One file

File name equals class name.

---

# AI Prompt Files

```
SummarizeEmail.prompt.md

GenerateReply.prompt.md

Translate.prompt.md

ClassifyEmail.prompt.md
```

---

# Logging Event Names

Past tense

```
EmailCreated

NotificationSent

WorkspaceDeleted

SummaryGenerated
```

---

# Metrics

Use dot notation

```
email.created

email.deleted

ai.summary.duration

notification.sent

search.duration
```

---

# Anti-Patterns

Avoid

```
Manager

Helper

Utility

Misc

Common

Data

Info

BaseHelper
```

Prefer meaningful domain names.

---

# AI Coding Checklist

Before generating code verify

✔ Naming follows conventions

✔ Async methods end with Async

✔ Commands/Queries named correctly

✔ DTO suffix applied

✔ Repository suffix applied

✔ Services named consistently

✔ Database objects follow snake_case

✔ API routes use plural nouns

✔ Documentation updated

---

# Definition of Done

Naming is compliant when

✔ Consistent across solution

✔ Business-oriented

✔ Predictable

✔ Matches architecture

✔ No ambiguous abbreviations

✔ Documentation aligned

---

END OF DOCUMENT