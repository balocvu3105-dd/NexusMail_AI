# AGENTS.md

> AI Development Rules for NexusMail AI

Version: 1.0.0

Status: Draft

Document Type: AI Agent Specification

---

# Purpose

This document defines the mandatory rules that every AI Coding Agent must follow while working on the NexusMail AI project.

These rules override any default coding behaviors of the AI.

Supported AI Agents:

- Antigravity IDE
- OpenAI Codex
- Claude Code
- GitHub Copilot
- Gemini CLI
- Cursor AI
- Continue.dev
- Cline
- Roo Code

---

# Primary Objective

Your objective is NOT to generate code quickly.

Your objective is to generate:

- Production Ready Code
- Maintainable Code
- Secure Code
- Testable Code
- Enterprise Architecture

Always prefer quality over speed.

---

# Global Principles

Always follow:

- SOLID
- DRY
- KISS
- YAGNI
- Clean Architecture
- Domain Driven Design
- CQRS
- Event Driven Design

Never violate Architecture.

---

# Architecture Rules

The project contains four layers.

Presentation

↓

Application

↓

Domain

↓

Infrastructure

Dependencies only go downward.

Never break dependency direction.

---

# Layer Responsibilities

## Presentation

Allowed

Controllers

Authentication

Authorization

DTO

Validation

Swagger

SignalR

Forbidden

Business Logic

Database Access

SQL

Domain Rules

---

## Application

Allowed

Use Cases

CQRS

Commands

Queries

Handlers

DTO Mapping

Events

Forbidden

SQL

Controllers

HTTP Context

Entity Framework

---

## Domain

Allowed

Entities

Value Objects

Domain Events

Aggregates

Business Rules

Specifications

Interfaces

Forbidden

EF Core

ASP.NET

Redis

RabbitMQ

OpenAI

HTTP

Infrastructure Code

---

## Infrastructure

Allowed

EF Core

Redis

RabbitMQ

OpenSearch

SMTP

OAuth

Logging

Caching

Repositories

Forbidden

Business Rules

---

# Coding Style

Always

Use async

Use CancellationToken

Use ConfigureAwait(false) in library code when appropriate

Enable Nullable Reference Types

Use XML Documentation

Use File Scoped Namespace

Use Primary Constructor when appropriate

Prefer Immutable Objects

Prefer Record over Class for DTO

Prefer IReadOnlyCollection

Prefer DateTimeOffset

Prefer UTC

---

# Dependency Injection

Never instantiate services manually.

Wrong

new EmailService()

Correct

IEmailService

Constructor Injection

---

# Controllers

Controllers are thin.

Controller should only

Receive Request

Validate

Call MediatR

Return Response

Nothing else.

---

# CQRS

Every feature should contain

Command

Command Handler

Query

Query Handler

Validator

DTO

Mapping

Unit Test

Integration Test

---

# Validation

Always use

FluentValidation

Never validate manually inside Controller.

---

# Error Handling

Never

throw Exception()

Always

Use custom Exceptions.

Examples

UserNotFoundException

EmailAlreadyExistsException

AttachmentTooLargeException

InvalidOAuthTokenException

---

# Logging

Always use

ILogger<T>

Never use

Console.WriteLine()

---

# Database

Always use

Entity Framework Core

Never

Write SQL inside Controller

Never

Expose Entity directly

Always use DTO

---

# Security

Passwords

Never store plain text.

Always hash.

OAuth Tokens

Always encrypt.

API Keys

Always encrypt.

Secrets

Never hardcode.

JWT

Always validate.

---

# Background Jobs

Background tasks must use

BackgroundService

or Queue

Never block HTTP Request.

---

# AI Communication

Backend never directly calls LLM.

Backend

↓

AI Gateway

↓

Python Service

↓

LLM

Never bypass AI Gateway.

---

# Repository Rules

Repositories only perform persistence.

Repositories never contain Business Logic.

---

# Domain Rules

Business Rules belong only to Domain.

Never place Domain Logic inside

Controller

Repository

Service

Infrastructure

---

# Naming Convention

Interfaces

IEmailService

Classes

EmailService

DTO

CreateEmailDto

Command

CreateEmailCommand

Query

GetEmailQuery

Validator

CreateEmailValidator

Entity

Email

Repository

EmailRepository

---

# Folder Structure

Feature First

Example

Modules/

Email/

Commands/

Queries/

Validators/

Events/

DTO/

Domain/

Infrastructure/

Tests/

---

# Unit Testing

Framework

xUnit

Mock

Moq

Coverage

Minimum 80%

Every Business Rule must have tests.

---

# Performance

Never load unnecessary data.

Always paginate.

Always index searchable columns.

Avoid N+1 Query.

Always use AsNoTracking() for read-only queries.

---

# API Rules

RESTful

Use proper HTTP verbs.

GET

POST

PUT

PATCH

DELETE

Never

POST for Update

GET for Delete

---

# Transactions

Use Unit Of Work.

Never create nested transactions unless necessary.

---

# Event Driven Rules

Important operations should publish Domain Events.

Examples

EmailReceived

EmailDeleted

EmailArchived

EmailTagged

UserRegistered

---

# Documentation

Every Public API

Must contain

XML Documentation

Every Feature

Must contain README.md

Every Module

Must contain Architecture.md

---

# Git Rules

Small commits.

Meaningful commit messages.

Conventional Commit

Examples

feat(email): add tagging

fix(auth): refresh token

refactor(ai): improve summary pipeline

---

# AI Restrictions

AI MUST NEVER

Ignore Clean Architecture

Write Business Logic inside Controller

Skip Validation

Skip Unit Test

Skip Logging

Skip Error Handling

Skip DTO

Skip Dependency Injection

Generate duplicate code

Use magic strings

Hardcode secrets

---

# AI Expectations

Every generated feature should include

Entity

DTO

Command

Query

Handler

Validator

Repository Interface

Repository Implementation

Dependency Injection

Swagger

Unit Tests

Integration Tests

Documentation

---

# Acceptance Checklist

Before finishing any task AI must verify

[ ] Build passes

[ ] Unit Tests pass

[ ] Architecture respected

[ ] No warnings

[ ] No duplicated code

[ ] DTO used

[ ] Validation completed

[ ] Logging completed

[ ] Exception Handling completed

[ ] XML Documentation added

[ ] Dependency Injection configured

---

END OF DOCUMENT