# SYSTEM_RULES.md

> NexusMail AI - Global Engineering Rules

Version: 1.0.0

Status: Production

Priority: CRITICAL

Document Type: Global Engineering Rules

Applies To

- Backend
- Frontend
- AI
- Database
- Infrastructure
- DevOps
- Documentation

---

# RULE 001

Architecture is Law.

Architecture cannot be violated.

If implementation conflicts with architecture,

Architecture always wins.

---

# RULE 002

Business Logic only exists inside Domain Layer.

Never place business logic inside

Controllers

Repositories

Middleware

Infrastructure

---

# RULE 003

Presentation Layer

Responsibilities

Receive Request

Authentication

Authorization

Validation

Return Response

Nothing Else.

---

# RULE 004

Application Layer

Responsibilities

Use Cases

Commands

Queries

Workflow

Transactions

Orchestration

DTO Mapping

No SQL

No Infrastructure Logic

---

# RULE 005

Domain Layer

Contains

Entities

Aggregates

Value Objects

Domain Events

Specifications

Business Rules

Interfaces

Factories

Policies

No external framework dependency.

---

# RULE 006

Infrastructure Layer

Contains

EF Core

Redis

RabbitMQ

SMTP

OpenSearch

OAuth

File Storage

Third Party APIs

No Business Rules allowed.

---

# RULE 007

Everything must be asynchronous.

Forbidden

Thread.Sleep

.Result

.Wait()

Preferred

async

await

CancellationToken

---

# RULE 008

Never expose Entity directly.

Always use

DTO

ViewModel

ResponseModel

---

# RULE 009

Always validate incoming requests.

Framework

FluentValidation

Never validate manually.

---

# RULE 010

Never trust client input.

Every request

Validate

Authorize

Sanitize

Log

Audit

---

# RULE 011

Passwords

Never store plain text.

Hash

Argon2id

or

BCrypt

Never MD5

Never SHA1

---

# RULE 012

Secrets

Never hardcode

API Keys

JWT Secrets

OAuth Secrets

Database Passwords

Connection Strings

Always use

Environment Variables

Secret Manager

Vault

---

# RULE 013

Database

Single source of truth

PostgreSQL

Every table

Primary Key

Indexes

Foreign Keys

Audit Fields

Soft Delete

CreatedUtc

UpdatedUtc

DeletedUtc

---

# RULE 014

Never delete business data permanently.

Use

Soft Delete

Unless legally required.

---

# RULE 015

Search

Never search millions of emails using SQL LIKE.

Always use

OpenSearch

Hybrid Search

Vector Search

---

# RULE 016

Caching

Redis

Cache

Dashboard

Summary

Statistics

Search

Session

Never cache sensitive information.

---

# RULE 017

Queue

RabbitMQ

Every heavy process

↓

Queue

Examples

AI Summary

Spam Scan

OCR

Search Index

Notification

Analytics

---

# RULE 018

Logging

Every request

↓

RequestId

CorrelationId

WorkspaceId

UserId

IP

Duration

StatusCode

Exception

---

# RULE 019

Exceptions

Never

throw Exception()

Always

Specific Exception

UserNotFoundException

EmailNotFoundException

InvalidRuleException

---

# RULE 020

Health Check

Every Service

Must expose

/health

/readiness

/liveness

---

# RULE 021

Metrics

Collect

CPU

Memory

Queue

Cache

Search

Database

API

AI

---

# RULE 022

Authentication

OAuth2

JWT

Refresh Token

OpenID Connect

Support MFA

Support SSO

---

# RULE 023

Authorization

Role Based

Policy Based

Permission Based

Never Role Only.

---

# RULE 024

Email Synchronization

Never block UI.

Always Background Worker.

---

# RULE 025

Notification

Priority

Critical

High

Medium

Low

Silent

Every notification must be categorized.

---

# RULE 026

AI

AI never changes user data automatically.

AI only

Recommend

Summarize

Tag

Prioritize

Unless user explicitly enables automation.

---

# RULE 027

AI Communication

Backend

↓

AI Gateway

↓

Python

↓

LLM

↓

Response

Never direct LLM call.

---

# RULE 028

OpenAI Compatibility

Every AI Provider must implement

IAIProvider

Supported

OpenAI

Azure OpenAI

Ollama

Anthropic

Gemini

Future providers.

---

# RULE 029

Email Provider

Every provider implements

IEmailProvider

Supported

Gmail

Outlook

Yahoo

Exchange

IMAP

---

# RULE 030

Repository Pattern

Repositories

Persistence Only.

Never Business Logic.

---

# RULE 031

CQRS

Every Feature

↓

Command

↓

CommandHandler

↓

Query

↓

QueryHandler

↓

Validator

↓

Tests

---

# RULE 032

Dependency Injection

Everything registered

through DI Container.

Never new Service().

---

# RULE 033

Background Services

Use

HostedService

BackgroundService

Never infinite loops without cancellation.

---

# RULE 034

API

RESTful

Versioned

/api/v1

Future

/api/v2

---

# RULE 035

HTTP Status

200

201

204

400

401

403

404

409

422

500

No custom status codes.

---

# RULE 036

OpenAPI

Every endpoint

Must appear in Swagger.

---

# RULE 037

Documentation

Every module

Must contain

README.md

Architecture.md

API.md

CHANGELOG.md

---

# RULE 038

Unit Tests

Minimum Coverage

80%

Business Rules

100%

---

# RULE 039

Integration Tests

Database

API

Authentication

Email Provider

AI Gateway

---

# RULE 040

Naming Convention

PascalCase

Classes

camelCase

Variables

UPPER_CASE

Constants

Interfaces

Prefix I

---

# RULE 041

Date Time

Always

UTC

DateTimeOffset

Never local server time.

---

# RULE 042

IDs

Default

UUID v7

Never auto-increment for business entities.

---

# RULE 043

Transactions

Application Layer controls transactions.

Never Controller.

---

# RULE 044

Configuration

Every configurable value

↓

Configuration

↓

Environment

↓

Database

Never hardcode.

---

# RULE 045

Monitoring

Prometheus

Grafana

OpenTelemetry

Structured Logging

Distributed Tracing

---

# RULE 046

Security Headers

HSTS

CSP

X-Frame-Options

X-Content-Type-Options

Referrer-Policy

---

# RULE 047

File Upload

Validate

Type

Size

Virus Scan

Hash

Storage

Audit

---

# RULE 048

AI Generated Code

Must follow

Architecture

Naming

Logging

Validation

Testing

Documentation

No exceptions.

---

# RULE 049

Technical Debt

Never leave TODO

Never leave FIXME

Never leave temporary code in main branch.

---

# RULE 050

Definition of Done

A task is complete only if

✔ Build passes

✔ Unit Tests pass

✔ Integration Tests pass

✔ Documentation updated

✔ Logging added

✔ Metrics added

✔ Validation added

✔ Authorization added

✔ Architecture respected

✔ Security reviewed

✔ Code reviewed

✔ Performance reviewed

✔ No critical warnings

✔ No duplicated code

✔ Swagger updated

✔ Migration created (if database changed)

✔ Changelog updated

---

END OF DOCUMENT