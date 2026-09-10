# NexusMail AI
# Dependency Graph Document

Version:
1.0.0

Status:
Approved

Document Type:
Architecture Dependency Specification

Priority:
Critical

---

# 1. Dependency Philosophy

NexusMail AI follows:
Clean Architecture
+
Domain Driven Design
+
Modular Architecture

Main rule:
Dependencies point inward.
External systems depend on internal business logic.

---

# 2. High Level Dependency Flow

             API Layer
                |
                v
      Application Layer
                |
                v
          Domain Layer
                ^
                |
                |
    Infrastructure Layer

Meaning:
Domain: No dependency
Application: Depends on Domain
Infrastructure: Implements Domain contracts
API: Depends on Application

---

# 3. Backend Project Structure

Backend
/src
|
+-- NexusMail.API
|
+-- NexusMail.Application
|
+-- NexusMail.Domain
|
+-- NexusMail.Infrastructure
|
+-- NexusMail.Shared
/tests
|
+-- UnitTests
+-- IntegrationTests
+-- ArchitectureTests

---

# 4. Project Dependency Rules

## NexusMail.Domain

Can reference:
NONE

Allowed:
- Entities
- Value Objects
- Domain Events
- Business Rules

Cannot reference:
Database
EF Core
HTTP
AI
RabbitMQ

---

# 5. NexusMail.Application

Depends on:
Domain

Contains:
- Use Cases
- Commands
- Queries
- DTO
- Interfaces

Can define:
IEmailRepository
IAIService
INotificationService

---

# 6. NexusMail.Infrastructure

Depends on:
Application
Domain

Contains:
Database
EF Core
Redis
RabbitMQ
Email Providers
AI Clients

Examples:
EmailRepository
OpenAIClient
GmailProvider
RedisCache

---

# 7. NexusMail.API

Depends on:
Application
Infrastructure

Contains:
Controllers
Middleware
Authentication
Swagger
Filters

---

# 8. Shared Project

Purpose:
Common contracts.

Contains:
- Constants
- Event Contracts
- Common DTO
- Error Models

Dependency:
Domain
Application
Infrastructure
API

---

# 9. Module Dependency Graph

             Identity
                |
                v
           Workspace
                |
                v
              Email
         /      |       \
        /       |        \
       v        v         v
      AI     Search   Automation
                |
                v
         Notification
                |
                v
            Billing

---

# 10. Bounded Context Dependency Rules

## Identity
Can depend: Shared
Cannot depend: Email, AI, Billing

## Workspace
Can depend: Identity Contract
Cannot depend: Email Data

## Email
Can depend: Workspace Identity
Cannot depend: Billing

## AI
Can depend: Email Event Contract
Cannot modify: Email Aggregate directly

## Search
Can depend: Email Events
Owns: Search Index

## Automation
Can consume: Domain Events

## Notification
Consumes: Events
Does not own: Business data

---

# 11. Event Dependency Model

Communication:

Email Service
   |
   |
   v
RabbitMQ
   |
   |
   +-------------+
   |             |
   v             v
AI Worker Search Worker

Rules:
No direct service reference.

Wrong:
EmailService calls AIService

Correct:
EmailReceived Event

---

# 12. AI Service Boundary

AI is separated.

Architecture:

.NET Backend
 |
 |
RabbitMQ
 |
 |
Python AI Service

AI Service owns:
- Models
- Prompt
- Embedding
- Inference

Backend owns:
- Business rules
- User permission
- Data ownership

---

# 13. Database Dependency Rules

Only Infrastructure accesses database.

Wrong:
Controller
|
Database

Correct:
Controller
|
Application
|
Repository
|
Database

---

# 14. External Service Dependencies

## Gmail
Used by: Email Infrastructure

## Microsoft Graph
Used by: Email Infrastructure

## OpenAI Compatible API
Used by: AI Infrastructure

## Stripe
Used by: Billing Infrastructure

---

# 15. Dependency Injection Rules

All external dependencies use interfaces.

Example:

Application
IEmailSender
    ^
Infrastructure
EmailSender

Registered through:
DependencyInjection.cs

---

# 16. Circular Dependency Prevention

Forbidden:
Email -> AI -> Email

Solution:
Use:
- Events
- Interfaces
- Contracts

---

# 17. Worker Dependency Model

Workers:

EmailSyncWorker
  |
  v
Application Services

AI Worker:

AI Worker
  |
  v
AI Application

---

# 18. Testing Dependency

Unit Test: Domain only
Integration Test: Application + Infrastructure
API Test: Full stack
Architecture Test: Verify dependency rules

---

# 19. Architecture Validation

Tools:
NetArchTest
Roslyn Analyzer
CI Pipeline

Example:
Rule: Domain cannot reference Infrastructure

---

# 20. CI Dependency Check

Every Pull Request:

Run:
Build
Unit Test
Architecture Test
Security Scan

---

# 21. Final Dependency Graph

                     API
                      |
                      v
                Application
                      |
                      v
                  Domain
                      ^
                      |
              Infrastructure

External:

             Infrastructure
                  |
   --------------------------------
   |              |               |
PostgreSQL RabbitMQ AI Service

---

# 22. Golden Rules

Rule 1: Domain owns business truth.
Rule 2: Application orchestrates.
Rule 3: Infrastructure implements.
Rule 4: API exposes.
Rule 5: Events connect independent systems.
Rule 6: No module accesses another module database.
Rule 7: Every dependency must be replaceable.

---

# End Of Document
