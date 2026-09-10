# TESTING_GUIDE.md

> NexusMail AI Testing Engineering Guide

Version: 1.0.0

Status: Production

Priority: CRITICAL

Document Type: Testing Standard

Applies To

- Backend
- Frontend
- AI Services
- Infrastructure
- Database
- DevOps

---

# Purpose

This document defines the official testing strategy for NexusMail AI.

Every feature must include automated tests.

No feature is considered complete without testing.

---

# Testing Philosophy

Testing is a first-class engineering activity.

Testing is written alongside production code.

Testing prevents regressions.

Testing documents expected behavior.

---

# Testing Pyramid

```
                Manual Testing
                     ▲
             End-to-End Tests
                   ▲
          Integration Tests
                ▲
          Component Tests
              ▲
          Unit Tests
```

Priority

1. Unit Tests

2. Integration Tests

3. End-to-End Tests

Manual testing is only the final verification.

---

# Test Coverage Targets

| Layer | Minimum Coverage |
|--------|------------------|
| Domain | 95% |
| Application | 90% |
| Infrastructure | 80% |
| API | 80% |
| Frontend | 75% |
| AI Services | 80% |

Overall project target

≥ 85%

---

# Backend Testing Stack

Framework

xUnit

Assertions

FluentAssertions

Mocking

Moq

Test Data

Bogus

Architecture Tests

NetArchTest

Snapshot Tests

Verify

---

# Frontend Testing Stack

Framework

Vitest

React Testing Library

Playwright

MSW

---

# Python Testing Stack

Framework

pytest

Coverage

pytest-cov

Mocking

pytest-mock

Async

pytest-asyncio

---

# Types of Tests

Unit Tests

Integration Tests

Contract Tests

API Tests

Component Tests

Performance Tests

Load Tests

Stress Tests

Chaos Tests

Security Tests

End-to-End Tests

Smoke Tests

Regression Tests

---

# Unit Tests

Purpose

Test one class in isolation.

Dependencies must be mocked.

Never access

Database

Redis

RabbitMQ

Network

Filesystem

---

# Unit Test Naming

Pattern

MethodName_State_ExpectedResult

Example

```
CreateEmail_WhenSenderMissing_ShouldReturnValidationError

ArchiveEmail_WhenAlreadyArchived_ShouldThrowException

MarkAsRead_WhenUnread_ShouldChangeStatus
```

---

# Domain Testing

Every Entity

Every Aggregate

Every Value Object

Every Specification

Every Factory

Every Domain Service

Must have unit tests.

Coverage

95%+

---

# Application Testing

Test

Commands

Queries

Validators

Pipeline Behaviors

Mappings

Application Services

Coverage

90%+

---

# Repository Testing

Repository implementations require

Integration Tests

against a real PostgreSQL instance.

Never mock EF Core DbContext for repository tests.

---

# Integration Testing

Purpose

Verify interaction between components.

Examples

API ↔ Database

API ↔ Redis

API ↔ RabbitMQ

API ↔ AI Gateway

API ↔ OpenSearch

---

# Test Database

Dedicated database only.

Never use production database.

Database recreated automatically.

Seed deterministic test data.

---

# API Testing

Test

Authentication

Authorization

Validation

Pagination

Filtering

Sorting

Rate Limiting

Error Handling

OpenAPI contracts

---

# AI Testing

Test

Classification

Summarization

Prompt Templates

Fallback Logic

Timeout Handling

Provider Failover

Embedding Generation

Vector Search

Never rely on live LLM responses for deterministic tests.

Use mocked providers where appropriate.

---

# Contract Testing

Verify

REST Contracts

gRPC Contracts

Webhook Contracts

Message Queue Contracts

Provider Adapters

---

# End-to-End Testing

Framework

Playwright

Scenarios

Login

Connect Gmail

Sync Email

AI Summary

Search

Notification

Logout

Critical user journeys only.

---

# Performance Testing

Tools

k6

NBomber

Scenarios

API Load

Search

AI Gateway

Database

Queue

Concurrent Login

---

# Load Testing Targets

Concurrent Users

100

500

1000

5000

Response Targets

GET

<100ms

POST

<200ms

Search

<150ms

---

# Chaos Testing

Simulate

Redis Down

RabbitMQ Down

Database Failover

AI Timeout

Provider Failure

Network Latency

Application must degrade gracefully.

---

# Security Testing

Include

Authentication Tests

Authorization Tests

SQL Injection Tests

XSS Tests

CSRF Tests

Rate Limit Tests

JWT Validation Tests

Secret Leakage Tests

---

# Test Data

Never use production data.

Generate using

Builders

Factories

Bogus

Random data must remain deterministic when required.

---

# Test Builders

Example

```
EmailBuilder

WorkspaceBuilder

UserBuilder

NotificationBuilder
```

Promote readability.

---

# Fixtures

Reusable setup only.

Examples

DatabaseFixture

ApiFixture

RedisFixture

SearchFixture

---

# Mocks

Allowed

Repositories

External Providers

SMTP

OAuth

AI Providers

Forbidden

Domain Entities

Value Objects

Business Rules

---

# CI/CD Testing

Every Pull Request executes

Build

↓

Static Analysis

↓

Unit Tests

↓

Integration Tests

↓

Coverage

↓

Security Scan

↓

Package Validation

Merge blocked on failure.

---

# Coverage Rules

Do not chase 100% blindly.

Test business behavior.

Test edge cases.

Test failure scenarios.

Test security-sensitive paths.

---

# Regression Testing

Every bug fix

Must include a regression test.

Bug must never reappear unnoticed.

---

# Naming Convention

Test Project

Module.Tests

Test Class

EmailServiceTests

Test Method

Action_State_Result

---

# Test Reports

Generate

Coverage Report

Performance Report

Security Report

Integration Report

Publish artifacts in CI.

---

# AI Coding Checklist

Before completing a feature verify

✔ Unit Tests added

✔ Integration Tests added

✔ Validation tested

✔ Authorization tested

✔ Error scenarios tested

✔ Performance considered

✔ Coverage target achieved

✔ CI pipeline updated

✔ Documentation updated

---

# Definition of Done

A feature is test-complete when

✔ Unit Tests pass

✔ Integration Tests pass

✔ API Tests pass

✔ Security Tests pass

✔ Performance Tests acceptable

✔ Coverage target achieved

✔ CI pipeline green

✔ No flaky tests

✔ Test documentation updated

---

END OF DOCUMENT