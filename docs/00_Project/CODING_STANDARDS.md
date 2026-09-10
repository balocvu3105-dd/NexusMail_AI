# CODING_STANDARDS.md

> NexusMail AI Engineering Coding Standards

Version: 1.0.0

Status: Production

Priority: Critical

Applies To

- Backend
- AI
- Frontend
- Infrastructure
- Tests

---

# 1. General Principles

The entire project follows

- Clean Architecture
- SOLID
- DDD
- CQRS
- DRY
- KISS
- YAGNI
- Dependency Injection
- Event Driven Design

Code must be

- Readable
- Testable
- Maintainable
- Extensible
- Secure

Code is written for humans first.

---

# 2. General Rules

Never optimize prematurely.

Never duplicate business logic.

Never hardcode configuration.

Never bypass architecture.

Never write code that only works "for now".

Always think long term.

---

# 3. Backend Language

Language

C#

Version

Latest .NET LTS

Nullable

Enabled

Implicit Usings

Enabled

Treat Warnings As Errors

Enabled

---

# 4. C# Style Rules

Always use

file-scoped namespace

Example

namespace NexusMail.Modules.Email;

Never

namespace
{

}

---

Use PascalCase

Classes

Methods

Properties

Enums

Records

---

Use camelCase

Variables

Parameters

Private Fields

---

Private fields

Prefix

_

Example

_emailRepository

_logger

_aiGateway

---

Constants

UPPER_SNAKE_CASE

Example

MAX_RETRY

CACHE_DURATION

DEFAULT_TIMEOUT

---

# 5. XML Documentation

Every public member

Must contain XML documentation.

Example

/// <summary>
/// Creates new email.
/// </summary>

Never skip.

---

# 6. Async Rules

Everything should be async.

Always

Task

ValueTask

CancellationToken

Never

Thread.Sleep()

Task.Wait()

.Result

---

Correct

Task<User>

Wrong

User

---

# 7. Dependency Injection

Never

new Service()

Always

Constructor Injection

Never

Service Locator

---

# 8. DTO Rules

Entities

Never exposed.

Always create DTO.

Request DTO

Response DTO

Update DTO

Create DTO

Summary DTO

---

# 9. Entity Rules

Entities

Represent business.

Never serialization models.

Never API models.

Never ViewModels.

---

# 10. Repository Rules

Repositories only

Read

Write

Delete

Exists

Count

Specification

Nothing else.

No business logic.

---

# 11. Service Rules

Application Service

Coordinates use cases.

Never contains infrastructure logic.

Never accesses database directly.

---

# 12. Controller Rules

Controller

↓

Validate

↓

Mediator

↓

Return Result

No business logic.

Maximum 30 lines.

---

# 13. MediatR Rules

Every feature

↓

Command

↓

Command Handler

↓

Query

↓

Query Handler

↓

Validator

↓

Tests

---

# 14. Validation

Framework

FluentValidation

Validation never inside Controller.

Validation never inside Entity.

---

# 15. Exceptions

Create custom exceptions.

Examples

WorkspaceNotFoundException

EmailNotFoundException

AttachmentTooLargeException

UnauthorizedWorkspaceException

Never throw generic Exception.

---

# 16. Logging

ILogger<T>

Only.

Never Console.WriteLine()

Log Levels

Trace

Debug

Information

Warning

Error

Critical

---

# 17. EF Core Rules

Always

AsNoTracking()

For read-only.

Always

SplitQuery()

When appropriate.

Always

Projection

Instead of loading full entities.

Never

Select *

Never

Lazy Loading

---

# 18. LINQ Rules

Prefer

Select

Where

OrderBy

ThenBy

GroupBy

Projection

Avoid

Nested LINQ

Repeated enumeration

Multiple ToList()

---

# 19. SQL Rules

Never

SELECT *

Always specify columns.

Every searchable field

Must have index.

---

# 20. API Rules

RESTful.

Use nouns.

Correct

/api/emails

/api/users

/api/workspaces

Wrong

/getEmails

/createUser

/deleteEmail

---

# 21. HTTP Rules

GET

Read

POST

Create

PUT

Replace

PATCH

Update

DELETE

Delete

---

# 22. Response Rules

Never return Entity.

Always

ApiResponse<T>

or

ProblemDetails

---

# 23. Pagination

Every list endpoint

Must support

Page

PageSize

Sort

Order

Filter

Search

---

# 24. Date Rules

Always UTC.

DateTimeOffset

Never local server time.

---

# 25. File Upload

Validate

MimeType

Extension

Hash

Virus Scan

Size

Owner

Permission

---

# 26. Configuration

Everything configurable.

Never hardcode.

Use

appsettings

Environment Variables

Secret Manager

---

# 27. Security

JWT

OAuth2

Refresh Token

HTTPS

Encryption

MFA Ready

---

# 28. AI Coding Rules

AI never edits generated files blindly.

Always

Read

Understand

Plan

Modify

Test

Document

---

# 29. Python Standards

Version

3.12+

Formatter

Black

Import Order

isort

Linter

Ruff

Typing

Required

Framework

FastAPI

Validation

Pydantic

ORM

SQLAlchemy (if needed)

Package Manager

uv (preferred)

---

# 30. AI Python Structure

ai/

├── api/

├── services/

├── models/

├── prompts/

├── providers/

├── embeddings/

├── vector/

├── security/

├── tests/

└── main.py

---

# 31. TypeScript Standards

Strict Mode

Enabled

ESLint

Required

Prettier

Required

No "any"

Prefer interfaces

Use React Hooks

Functional Components Only

---

# 32. React Standards

Never Class Components.

Prefer

Composition

Small Components

Reusable Components

Feature-based folders

---

# 33. CSS Standards

TailwindCSS only.

No inline styles.

No duplicated utility classes.

Reusable components preferred.

---

# 34. Testing Standards

Backend

xUnit

Moq

FluentAssertions

Frontend

Vitest

React Testing Library

Python

pytest

Coverage

Backend

>= 80%

Domain Layer

>= 95%

---

# 35. Git Standards

Branch

feature/email

feature/ai-summary

fix/login

refactor/search

Commit

feat:

fix:

refactor:

docs:

test:

perf:

build:

ci:

---

# 36. Code Review Checklist

Before merge verify

Architecture

Naming

Logging

Validation

Security

Performance

Tests

Documentation

No dead code

No duplicated code

No warnings

---

# 37. Forbidden

Never use

#region

dynamic

goto

Thread.Abort

async void

Console.WriteLine()

Magic Strings

Magic Numbers

Global State

Static Service Locator

Hardcoded Secrets

---

# 38. Definition of Excellent Code

Excellent code is

Readable

Predictable

Deterministic

Secure

Observable

Documented

Tested

Performant

Replaceable

Extensible

---

END OF DOCUMENT# CODING_STANDARDS.md

> NexusMail AI Engineering Coding Standards

Version: 1.0.0

Status: Production

Priority: Critical

Applies To

- Backend
- AI
- Frontend
- Infrastructure
- Tests

---

# 1. General Principles

The entire project follows

- Clean Architecture
- SOLID
- DDD
- CQRS
- DRY
- KISS
- YAGNI
- Dependency Injection
- Event Driven Design

Code must be

- Readable
- Testable
- Maintainable
- Extensible
- Secure

Code is written for humans first.

---

# 2. General Rules

Never optimize prematurely.

Never duplicate business logic.

Never hardcode configuration.

Never bypass architecture.

Never write code that only works "for now".

Always think long term.

---

# 3. Backend Language

Language

C#

Version

Latest .NET LTS

Nullable

Enabled

Implicit Usings

Enabled

Treat Warnings As Errors

Enabled

---

# 4. C# Style Rules

Always use

file-scoped namespace

Example

namespace NexusMail.Modules.Email;

Never

namespace
{

}

---

Use PascalCase

Classes

Methods

Properties

Enums

Records

---

Use camelCase

Variables

Parameters

Private Fields

---

Private fields

Prefix

_

Example

_emailRepository

_logger

_aiGateway

---

Constants

UPPER_SNAKE_CASE

Example

MAX_RETRY

CACHE_DURATION

DEFAULT_TIMEOUT

---

# 5. XML Documentation

Every public member

Must contain XML documentation.

Example

/// <summary>
/// Creates new email.
/// </summary>

Never skip.

---

# 6. Async Rules

Everything should be async.

Always

Task

ValueTask

CancellationToken

Never

Thread.Sleep()

Task.Wait()

.Result

---

Correct

Task<User>

Wrong

User

---

# 7. Dependency Injection

Never

new Service()

Always

Constructor Injection

Never

Service Locator

---

# 8. DTO Rules

Entities

Never exposed.

Always create DTO.

Request DTO

Response DTO

Update DTO

Create DTO

Summary DTO

---

# 9. Entity Rules

Entities

Represent business.

Never serialization models.

Never API models.

Never ViewModels.

---

# 10. Repository Rules

Repositories only

Read

Write

Delete

Exists

Count

Specification

Nothing else.

No business logic.

---

# 11. Service Rules

Application Service

Coordinates use cases.

Never contains infrastructure logic.

Never accesses database directly.

---

# 12. Controller Rules

Controller

↓

Validate

↓

Mediator

↓

Return Result

No business logic.

Maximum 30 lines.

---

# 13. MediatR Rules

Every feature

↓

Command

↓

Command Handler

↓

Query

↓

Query Handler

↓

Validator

↓

Tests

---

# 14. Validation

Framework

FluentValidation

Validation never inside Controller.

Validation never inside Entity.

---

# 15. Exceptions

Create custom exceptions.

Examples

WorkspaceNotFoundException

EmailNotFoundException

AttachmentTooLargeException

UnauthorizedWorkspaceException

Never throw generic Exception.

---

# 16. Logging

ILogger<T>

Only.

Never Console.WriteLine()

Log Levels

Trace

Debug

Information

Warning

Error

Critical

---

# 17. EF Core Rules

Always

AsNoTracking()

For read-only.

Always

SplitQuery()

When appropriate.

Always

Projection

Instead of loading full entities.

Never

Select *

Never

Lazy Loading

---

# 18. LINQ Rules

Prefer

Select

Where

OrderBy

ThenBy

GroupBy

Projection

Avoid

Nested LINQ

Repeated enumeration

Multiple ToList()

---

# 19. SQL Rules

Never

SELECT *

Always specify columns.

Every searchable field

Must have index.

---

# 20. API Rules

RESTful.

Use nouns.

Correct

/api/emails

/api/users

/api/workspaces

Wrong

/getEmails

/createUser

/deleteEmail

---

# 21. HTTP Rules

GET

Read

POST

Create

PUT

Replace

PATCH

Update

DELETE

Delete

---

# 22. Response Rules

Never return Entity.

Always

ApiResponse<T>

or

ProblemDetails

---

# 23. Pagination

Every list endpoint

Must support

Page

PageSize

Sort

Order

Filter

Search

---

# 24. Date Rules

Always UTC.

DateTimeOffset

Never local server time.

---

# 25. File Upload

Validate

MimeType

Extension

Hash

Virus Scan

Size

Owner

Permission

---

# 26. Configuration

Everything configurable.

Never hardcode.

Use

appsettings

Environment Variables

Secret Manager

---

# 27. Security

JWT

OAuth2

Refresh Token

HTTPS

Encryption

MFA Ready

---

# 28. AI Coding Rules

AI never edits generated files blindly.

Always

Read

Understand

Plan

Modify

Test

Document

---

# 29. Python Standards

Version

3.12+

Formatter

Black

Import Order

isort

Linter

Ruff

Typing

Required

Framework

FastAPI

Validation

Pydantic

ORM

SQLAlchemy (if needed)

Package Manager

uv (preferred)

---

# 30. AI Python Structure

ai/

├── api/

├── services/

├── models/

├── prompts/

├── providers/

├── embeddings/

├── vector/

├── security/

├── tests/

└── main.py

---

# 31. TypeScript Standards

Strict Mode

Enabled

ESLint

Required

Prettier

Required

No "any"

Prefer interfaces

Use React Hooks

Functional Components Only

---

# 32. React Standards

Never Class Components.

Prefer

Composition

Small Components

Reusable Components

Feature-based folders

---

# 33. CSS Standards

TailwindCSS only.

No inline styles.

No duplicated utility classes.

Reusable components preferred.

---

# 34. Testing Standards

Backend

xUnit

Moq

FluentAssertions

Frontend

Vitest

React Testing Library

Python

pytest

Coverage

Backend

>= 80%

Domain Layer

>= 95%

---

# 35. Git Standards

Branch

feature/email

feature/ai-summary

fix/login

refactor/search

Commit

feat:

fix:

refactor:

docs:

test:

perf:

build:

ci:

---

# 36. Code Review Checklist

Before merge verify

Architecture

Naming

Logging

Validation

Security

Performance

Tests

Documentation

No dead code

No duplicated code

No warnings

---

# 37. Forbidden

Never use

#region

dynamic

goto

Thread.Abort

async void

Console.WriteLine()

Magic Strings

Magic Numbers

Global State

Static Service Locator

Hardcoded Secrets

---

# 38. Definition of Excellent Code

Excellent code is

Readable

Predictable

Deterministic

Secure

Observable

Documented

Tested

Performant

Replaceable

Extensible

---

END OF DOCUMENT