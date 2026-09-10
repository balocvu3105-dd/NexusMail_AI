# NexusMail AI
# Code Generation Guide

Version:
1.0.0

Status:
Implementation Standard

Document Type:
AI Assisted Development Guide

Technology:
.NET 8
ASP.NET Core
Clean Architecture
Next.js
TypeScript
PostgreSQL
RabbitMQ
OpenTelemetry

---

# 1. Purpose

This document defines:
- AI coding workflow
- Code generation rules
- Review standards
- Module implementation order
- Definition of Done

---

# 2. Core Principle

AI is a coding assistant.

AI must:
Generate code.
Explain decisions.
Create tests.
Follow architecture.

AI must NOT:
Change architecture.
Remove security.
Skip tests.
Create shortcuts.

---

# 3. Development Workflow

Every feature follows:

Requirement
  |
Architecture Review
  |
Domain Design
  |
Implementation
  |
Testing
  |
Code Review
  |
Merge

---

# 4. Implementation Order

Never code randomly.

Required order:
Domain Layer
Application Layer
Infrastructure Layer
API Layer
Worker Layer
Frontend
Tests

---

# 5. Repository Understanding

Before generating code:

AI must read:
PROJECT_CONTEXT.md
SYSTEM_ARCHITECTURE.md
DOMAIN_MODEL.md
SOLUTION_STRUCTURE.md
CODING_STANDARD.md

---

# 6. Architecture Rules

Follow:
Clean Architecture

Dependency:
API
|
Application
|
Domain
Infrastructure
|
Application

---

# 7. Domain Rules

Domain layer:

Allowed:
Entities
Value Objects
Enums
Domain Events
Business Rules

Forbidden:
Database
HTTP
External API
Framework Dependency

---

# 8. Entity Generation Rules

Every entity requires:
Id
CreatedAt
UpdatedAt
Domain Behavior
Validation

Example:

Bad:
```csharp
public string Status {get;set;}
```

Good:
```csharp
public EmailStatus Status {get;private set;}
```

---

# 9. Application Layer Rules

Contains:
Use Cases
Commands
Queries
Handlers
Interfaces
DTOs

No:
Business Rules
SQL
External Calls

---

# 10. Infrastructure Rules

Contains:
Database
External APIs
File Storage
Message Queue
AI Providers

---

# 11. API Rules

Controllers:

Only:
Receive Request
Validate
Call Application
Return Response

Never:
Business Logic
Database Query
AI Processing

---

# 12. Frontend Rules

Frontend:

Contains:
UI
State
User Interaction

Never:
Business Decision
Permission Logic
AI Processing

---

# 13. Feature Development Template

Every feature requires:
Feature Name
Purpose
Domain Impact
Database Change
API Change
Frontend Change
Tests
Monitoring
Security Review

---

# 14. AI Prompt Template

Use:
You are implementing NexusMail AI.

Read:
[Documents]

Task:
[Feature]

Constraints:
[Rules]

Generate:
[Code]

Include:
Tests

Explain:
Architecture Decision

---

# 15. Example Feature Request

Example:
Implement Email Priority Scoring.

Read:
DOMAIN_MODEL.md
AI_PIPELINE_ARCHITECTURE.md

Create:
Domain Model
Service Interface
Implementation
Tests
API Endpoint

---

# 16. Database Generation Rules

AI must create:
Entity
Configuration
Migration
Index
Tests

Required:
TenantId
Audit Fields
Indexes
Constraints

---

# 17. API Generation Rules

Every API:

Requires:
Endpoint
Request DTO
Response DTO
Validation
Authorization
Tests
OpenAPI Documentation

---

# 18. Worker Generation Rules

Background Worker:

Requires:
Queue Consumer
Retry Policy
Error Handling
Logging
Metrics
Health Check

---

# 19. AI Feature Generation Rules

Every AI feature:

Requires:
Prompt Version
Model Selection
Input Validation
Output Schema
Confidence
Cost Tracking

---

# 20. Testing Requirements

Every feature:

Must include:
Unit Test
Integration Test

If UI:
E2E Test

---

# 21. Test Coverage Goal

Minimum:
Domain: 90%
Application: 80%
Infrastructure: 70%

---

# 22. Code Quality Rules

Required:
SOLID
DRY
Clean Code
Async Best Practice
Dependency Injection

---

# 23. Forbidden Code Patterns

Never generate:
God Class: 1000+ lines service
Static Service: Global helper abuse
Hidden Dependency: new Service()
Magic String: "admin"

---

# 24. Error Handling

Use:
Result Pattern
Exception Middleware
ProblemDetails

Never:
try catch everywhere

---

# 25. Logging Rules

Every important action:

Log:
UserId
WorkspaceId
TraceId
Operation
Result

Never log:
Password
Token
Email Content

---

# 26. Security Review

Before merge:

Check:
Authentication
Authorization
Input Validation
Tenant Isolation
Secret Handling

---

# 27. AI Generated Code Review

Reviewer checks:

Architecture:
Does it respect boundaries?

Security:
Any leakage?

Performance:
Any unnecessary query?

Maintainability:
Can another developer understand?

---

# 28. Git Workflow

Branch:
feature/
bugfix/
hotfix/

Commit:
Example:
feat(email): add gmail sync service

---

# 29. Pull Request Requirements

PR must include:
Description
Architecture Impact
Testing Result
Screenshots
Migration Notes

---

# 30. Definition of Done

Feature completed when:
[x] Requirement implemented
[x] Tests passed
[x] Security checked
[x] Logging added
[x] Metrics added
[x] Documentation updated

---

# 31. AI Agent Development Sequence

Order:
Foundation
        |
Identity
        |
Workspace
        |
Email
        |
AI Pipeline
        |
Search
        |
Automation
        |
Billing
        |
Enterprise Features

---

# 32. Human Developer Responsibility

Human decides:
Architecture
Business Rules
Security
Trade-offs

AI assists:
Implementation
Refactoring
Testing
Documentation

---

# 33. Final AI Coding Rules

Rule 1: Understand before coding.
Rule 2: Architecture before implementation.
Rule 3: Every feature needs tests.
Rule 4: Security cannot be compromised.
Rule 5: Simple code beats clever code.
Rule 6: Never sacrifice maintainability for speed.

---

# End Of Document
