# REPOSITORY_STRUCTURE.md

> NexusMail AI Repository Structure Specification

Version: 1.0.0

Status: Production

Priority: CRITICAL

Document Type: Repository Architecture

---

# Purpose

This document defines the complete repository structure.

Every AI Coding Agent MUST follow this structure.

No folders may be created outside this specification unless approved by an Architecture Decision Record (ADR).

---

# Repository Layout

NexusMail-AI/

```
.
├── README.md
├── LICENSE
├── CHANGELOG.md
├── CONTRIBUTING.md
├── AGENTS.md
├── ANTIGRAVITY.md
├── PROJECT_CONTEXT.md
├── SYSTEM_RULES.md
├── CODING_STANDARDS.md
│
├── docs/
├── architecture/
├── decisions/
├── diagrams/
├── prompts/
├── tasks/
├── scripts/
├── tools/
├── deployment/
├── database/
├── tests/
│
├── backend/
├── frontend/
├── ai/
└── shared/
```

---

# backend/

Responsible for all .NET services.

```
backend/

src/

tests/

build/

docker/

```

---

# backend/src

```
src/

Gateway/

Identity/

Email/

Notification/

Search/

Analytics/

Automation/

Administration/

Billing/

Shared/

```

Each module is an independent application module.

---

# Module Layout

Every module MUST follow the same structure.

Example

```
Email/

Email.Api/

Email.Application/

Email.Domain/

Email.Infrastructure/

Email.Contracts/

Email.Tests/

README.md

Architecture.md

CHANGELOG.md

```

---

# Email.Api

Contains

```
Controllers/

Endpoints/

Filters/

Middlewares/

Swagger/

Extensions/

DependencyInjection/

Program.cs

```

Forbidden

Business Logic

Database

Entity Framework

---

# Email.Application

Contains

```
Commands/

Queries/

Handlers/

DTO/

Validators/

Mappings/

Behaviors/

Interfaces/

Events/

Services/

Pipelines/

```

Responsibilities

Application orchestration.

CQRS.

Validation.

Transactions.

---

# Email.Domain

Contains

```
Entities/

Aggregates/

ValueObjects/

Enums/

Events/

Policies/

Factories/

Specifications/

Repositories/

Services/

Exceptions/

```

Forbidden

EF Core

Redis

RabbitMQ

ASP.NET

OpenSearch

Logging Framework

---

# Email.Infrastructure

Contains

```
Persistence/

Repositories/

Configurations/

Redis/

Messaging/

Search/

Providers/

OAuth/

BackgroundJobs/

Caching/

Logging/

DependencyInjection/

```

Responsibilities

Infrastructure only.

---

# Email.Contracts

Contains

```
Requests/

Responses/

Events/

Messages/

Enums/

Constants/

```

Purpose

Communication between services.

---

# Email.Tests

```
Unit/

Integration/

Performance/

Fixtures/

Builders/

Mocks/

```

Coverage Target

80%+

---

# frontend/

```
frontend/

apps/

packages/

shared/

```

---

# React Application

```
apps/web/

src/

components/

pages/

layouts/

hooks/

services/

api/

types/

stores/

utils/

assets/

styles/

tests/

```

---

# Components

```
components/

common/

layout/

email/

notification/

dashboard/

search/

charts/

forms/

modals/

```

Rule

Reusable only.

---

# Pages

```
pages/

Dashboard/

Inbox/

Email/

Search/

Analytics/

Settings/

Workspace/

Admin/

```

---

# Hooks

```
hooks/

useAuth

useEmail

useNotification

useSearch

useAI

useWorkspace

```

---

# Stores

```
stores/

auth

email

notification

workspace

search

settings

```

---

# ai/

```
ai/

app/

api/

core/

models/

providers/

prompts/

embeddings/

vector/

ocr/

security/

tests/

```

---

# AI Providers

```
providers/

OpenAI/

Azure/

Ollama/

Gemini/

Anthropic/

Mock/

```

Each provider implements

```
IAIProvider
```

---

# Prompt Library

```
prompts/

classification/

summary/

reply/

tagging/

translation/

ocr/

security/

```

---

# Vector Engine

```
vector/

embeddings/

index/

search/

ranking/

cache/

```

---

# shared/

Contains shared libraries.

```
shared/

Kernel/

BuildingBlocks/

Common/

Utilities/

Contracts/

Observability/

Security/

```

---

# database/

```
database/

migrations/

seed/

schema/

views/

functions/

indexes/

backup/

```

---

# scripts/

```
scripts/

build/

deploy/

migration/

cleanup/

seed/

```

---

# deployment/

```
deployment/

docker/

kubernetes/

helm/

terraform/

nginx/

```

---

# architecture/

Contains

```
C4/

DDD/

CQRS/

ERD/

Sequence/

Activity/

Deployment/

```

---

# decisions/

Architecture Decision Records

```
ADR-0001.md

ADR-0002.md

ADR-0003.md

...
```

---

# diagrams/

Contains

```
Mermaid/

PlantUML/

DrawIO/

PNG/

SVG/

```

---

# tests/

Global testing.

```
tests/

e2e/

performance/

load/

security/

chaos/

```

---

# tools/

Developer tools.

```
tools/

codegen/

analyzers/

benchmark/

lint/

```

---

# Naming Rules

Folder

PascalCase

Namespace

Matches folder

Project

Matches folder

Solution

NexusMail.sln

---

# Module Independence

Every module must be deployable independently.

Communication

Preferred

Events

REST

gRPC

Forbidden

Direct database access between modules.

---

# Shared Kernel Rules

Only place code in Shared if

- Used by at least two modules.
- Has no business-specific behavior.
- Is framework-agnostic where practical.

Do NOT move business logic into Shared simply to avoid duplication.

---

# Maximum Dependencies

Presentation

↓

Application

↓

Domain

↓

Infrastructure

Never reverse dependencies.

---

# Build Rules

Every module must

Build independently.

Run tests independently.

Generate OpenAPI independently.

Publish Docker Image independently.

---

# Repository Goals

Scalable

Modular

Cloud Native

Microservice Ready

AI Friendly

Enterprise Ready

Long-term Maintainable

---

END OF DOCUMENT