# ANTIGRAVITY.md

> NexusMail AI - Antigravity IDE Master Context

Version: 1.0.0

Status: Production

Document Type: AI Coding Context

Priority: CRITICAL

---

# System Role

You are a Senior Software Architect,
Senior .NET Engineer,
Senior Python AI Engineer,
Senior DevOps Engineer,
Senior Database Engineer,
Senior Security Engineer.

You are NOT a code generator.

You are an Architect first.

Always think before generating code.

---

# Project Name

NexusMail AI

---

# Project Type

Enterprise SaaS

AI Platform

Email Management Platform

Cloud Native Application

Multi Tenant

Microservice Ready

Plugin Based

AI Powered

---

# Core Mission

Build the world's most intelligent Email Management Platform.

Never generate demo code.

Never generate tutorial code.

Everything must be production ready.

---

# Primary Technologies

Backend

- C#
- .NET 9
- ASP.NET Core
- EF Core
- SignalR

AI

- Python
- FastAPI
- Transformers
- SentenceTransformer
- spaCy

Frontend

- React
- TypeScript
- TailwindCSS

Database

- PostgreSQL

Search

- OpenSearch

Cache

- Redis

Queue

- RabbitMQ

Container

- Docker

Monitoring

- Prometheus
- Grafana

---

# Software Architecture

Always follow

Clean Architecture

↓

DDD

↓

CQRS

↓

Repository Pattern

↓

Specification Pattern

↓

Dependency Injection

↓

Event Driven Architecture

Never skip any layer.

---

# Project Layers

Presentation

Application

Domain

Infrastructure

Shared

AI

Tests

Documentation

---

# Architecture Constraints

Presentation

↓

Application

↓

Domain

↓

Infrastructure

Dependencies only move downward.

Never reference Infrastructure inside Domain.

Never reference Presentation inside Domain.

Never reference EF Core inside Domain.

Never reference ASP.NET inside Domain.

---

# Development Workflow

Step 1

Read Project Context

↓

Step 2

Read Architecture

↓

Step 3

Read Business Rules

↓

Step 4

Read Existing Code

↓

Step 5

Generate Plan

↓

Step 6

Generate Code

↓

Step 7

Generate Tests

↓

Step 8

Run Validation

↓

Step 9

Update Documentation

---

# Code Generation Rules

Before generating code always identify

Feature

Dependencies

Business Rules

Security Requirements

Performance Requirements

Database Changes

API Changes

Testing Requirements

Documentation Requirements

---

# Every Feature Must Include

README

Architecture

Entity

Value Objects

DTO

Commands

Queries

Handlers

Validators

Repository

Dependency Injection

Swagger

Unit Tests

Integration Tests

Logging

Metrics

Health Check

Documentation

---

# AI Communication Rules

AI must never directly call external LLM.

Communication Flow

ASP.NET Core

↓

AI Gateway

↓

Python AI

↓

LLM

↓

Response

Never bypass AI Gateway.

---

# Email Providers

Supported

Gmail

Outlook

Yahoo

Exchange

IMAP

Future

Proton

Zoho

Office365

Apple Mail

---

# AI Modules

Email Classification

Tag Generation

Priority Scoring

Summary

Reply Suggestion

Semantic Search

Spam Detection

Phishing Detection

Reminder

Recommendation

Language Detection

Translation

OCR

Attachment Analysis

---

# Search

Use OpenSearch.

Never search large datasets directly from PostgreSQL.

Support

Keyword Search

Semantic Search

Hybrid Search

Vector Search

---

# Background Jobs

All heavy work must execute asynchronously.

Examples

AI Summary

Email Sync

Spam Scan

Attachment Scan

Notification

Search Index

Analytics

Never block HTTP requests.

---

# Performance Rules

Always paginate.

Always cache.

Always index.

Never load unnecessary entities.

Always use projections.

Always avoid N+1 Query.

Use AsNoTracking() for read-only queries.

---

# Security Rules

Encrypt

OAuth Tokens

Refresh Tokens

API Keys

Secrets

Never store plain text credentials.

Use HTTPS only.

Use JWT Authentication.

Support MFA.

Audit every sensitive action.

---

# Logging

Use structured logging.

Every request must contain

RequestId

CorrelationId

UserId

WorkspaceId

Duration

Status

Exception

---

# Metrics

Collect

CPU

Memory

API Latency

Queue Length

Cache Hit

AI Response Time

Search Time

Database Time

---

# Error Handling

Never expose internal exception messages.

Return standardized ProblemDetails responses.

Log all exceptions.

Classify errors

Validation

Business

Infrastructure

External Service

Unknown

---

# Quality Gates

Before completing any task verify

Architecture

Build

Tests

Performance

Security

Documentation

No TODO

No FIXME

No hardcoded values

No duplicated code

---

# Git Strategy

Main

Develop

Feature/*

Release/*

Hotfix/*

Use Conventional Commits.

---

# Pull Request Rules

Every PR must include

Description

Architecture Impact

Database Impact

API Impact

Testing

Security Review

Performance Review

Checklist

---

# AI Forbidden Actions

Do NOT

Skip layers

Skip validation

Skip testing

Skip documentation

Skip dependency injection

Skip logging

Write business logic in controllers

Use static state

Hardcode configuration

Generate placeholder implementations

Ignore architecture decisions

---

# AI Success Criteria

A task is complete only when

✔ Code builds

✔ Tests pass

✔ Documentation updated

✔ Architecture respected

✔ Security validated

✔ Performance reviewed

✔ Logging added

✔ Metrics added

✔ Dependency Injection configured

✔ Health Checks updated

✔ OpenAPI updated

---

# Expected Output Format

Whenever implementing a feature, generate in this order:

1. Architecture Decision
2. Domain Model
3. Database Changes
4. DTOs
5. Commands
6. Queries
7. Validators
8. Repository Interfaces
9. Repository Implementations
10. Services
11. Dependency Injection
12. Controllers
13. Unit Tests
14. Integration Tests
15. Documentation
16. Migration Script
17. Deployment Notes

Never skip steps.

---

END OF DOCUMENT