# NexusMail AI
# Project Initialization Document

Version:
1.0.0

Status:
Implementation Ready

Document Type:
Development Environment Setup

Technology:
.NET 8 LTS
C# 12
PostgreSQL 16
Redis
RabbitMQ
Docker
GitHub Actions

---

# 1. Initialization Goal

This document defines:
- Solution creation
- Project structure
- Dependencies
- Development environment
- Infrastructure setup

After completion:

Developer can:
- Build solution
- Run API
- Run Worker
- Connect Database
- Execute Tests

---

# 2. Repository Root Structure

Final:

NexusMail/
├── backend/
│
├── frontend/
│
├── ai/
│
├── shared/
│
├── database/
│
├── docs/
│
├── devops/
│
├── prompts/
│
├── tests/
│
├── tools/
│
├── scripts/
│
└── docker-compose.yml

---

# 3. Backend Solution Structure

Location:
/backend

Solution:
NexusMail.sln

---

# 4. Backend Projects

Create:

src/
├── NexusMail.API
├── NexusMail.Application
├── NexusMail.Domain
├── NexusMail.Infrastructure
├── NexusMail.Shared
├── NexusMail.Identity
├── NexusMail.Worker.EmailSync
├── NexusMail.Worker.AI
├── NexusMail.Worker.Notification
└── NexusMail.Worker.Automation

---

# 5. Test Projects

tests/
├── NexusMail.UnitTests
├── NexusMail.IntegrationTests
├── NexusMail.ArchitectureTests
├── NexusMail.ApiTests
└── NexusMail.WorkerTests

---

# 6. Create Solution

Command:
```bash
dotnet new sln -n NexusMail
```

---

# 7. Create Projects

API
```bash
dotnet new webapi -n NexusMail.API
```

Domain
```bash
dotnet new classlib -n NexusMail.Domain
```

Application
```bash
dotnet new classlib -n NexusMail.Application
```

Infrastructure
```bash
dotnet new classlib -n NexusMail.Infrastructure
```

---

# 8. Add Projects To Solution

Example:
```bash
dotnet sln add src/NexusMail.API
```

All projects must belong to solution.

---

# 9. Project Reference Rules

Dependency:

API
 |
Application
 |
Domain

Infrastructure
 |
Application
 |
Domain

Rules:
Domain references nothing.
Application references Domain.
Infrastructure references Application + Domain.
API references Application + Infrastructure.

---

# 10. Directory.Build.props

Create:
backend/Directory.Build.props

Purpose:
Global compiler settings.

Example:
```xml
<Project>
<PropertyGroup>
<TargetFramework>net8.0</TargetFramework>
<Nullable>enable</Nullable>
<ImplicitUsings>enable</ImplicitUsings>
<LangVersion>latest</LangVersion>
</PropertyGroup>
</Project>
```

---

# 11. Central Package Management

Enable:
Directory.Packages.props

Purpose:
Single package version control.

Example:
```xml
<Project>
<PropertyGroup>
<ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
</PropertyGroup>
</Project>
```

---

# 12. Required NuGet Packages

API
Microsoft.AspNetCore.Authentication.JwtBearer
Swashbuckle.AspNetCore
Serilog.AspNetCore

Application
MediatR
FluentValidation

Infrastructure
Microsoft.EntityFrameworkCore
Npgsql.EntityFrameworkCore.PostgreSQL
StackExchange.Redis
RabbitMQ.Client

Testing
xUnit
Moq
FluentAssertions
Testcontainers
NetArchTest

---

# 13. Development Environment

Required:
.NET SDK 8
Docker Desktop
PostgreSQL Client
Git
IDE

Recommended:
JetBrains Rider
Visual Studio 2022
VS Code

---

# 14. Docker Infrastructure

File:
docker-compose.yml

Services:
postgres
redis
rabbitmq

---

# 15. PostgreSQL Container

Configuration:
Database: nexusmail
User: nexusmail
Password: development_only

---

# 16. Redis Container

Purpose:
Cache
Distributed lock
Session data

---

# 17. RabbitMQ Container

Purpose:
Event messaging
Worker communication

Enable:
Management Plugin

---

# 18. Environment Configuration

Never store secrets:
appsettings.json

Use:
appsettings.Development.json
Environment Variables
Secret Manager

---

# 19. Application Configuration

Structure:
Configuration/
DatabaseOptions
JwtOptions
AIOptions
RabbitMqOptions
StorageOptions

---

# 20. Initial Database Setup

Install:
```bash
dotnet tool install --global dotnet-ef
```

Verify:
```bash
dotnet ef --version
```

---

# 21. First Build Validation

Command:
```bash
dotnet build
```

Requirement:
0 errors.

---

# 22. Initial Test Validation

Command:
```bash
dotnet test
```

Requirement:
All tests pass.

---

# 23. Git Initialization

Commands:
```bash
git init
git branch -M main
```

---

# 24. Initial Repository Files

Required:
.gitignore
README.md
LICENSE
.editorconfig
Directory.Build.props
Directory.Packages.props

---

# 25. EditorConfig

Rules:
Enable:
UTF-8
Spaces
Formatting
Nullable warnings

---

# 26. CI Pipeline Initial Setup

Location:
.github/workflows/

Create:
build.yml

Pipeline:
Checkout
        |
Setup .NET
        |
Restore
        |
Build
        |
Test

---

# 27. Docker Development Flow

Start:
```bash
docker compose up -d
```

Stop:
```bash
docker compose down
```

---

# 28. Local Development Flow

Developer:
Clone Repository
        |
Install SDK
        |
Start Docker
        |
Run Migration
        |
Run API
        |
Run Workers

---

# 29. First Milestone

After initialization:

System can:
[x] Compile
[x] Run API
[x] Connect Database
[x] Run Worker Host
[x] Execute Tests

---

# 30. Forbidden Actions

Before architecture approval:
Do NOT:
Add random packages
Create random projects
Put business logic in API
Skip tests
Bypass dependency rules

---

# 31. Completion Checklist

Repository:
[x] Solution created
[x] Projects created
[x] References configured
[x] Packages configured
[x] Docker ready
[x] CI skeleton ready
[x] Test structure ready

---

# End Of Document
