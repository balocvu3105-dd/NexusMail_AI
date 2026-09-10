# LOGGING_GUIDE.md

> NexusMail AI Logging & Observability Guide

Version: 1.0.0

Status: Production

Priority: CRITICAL

Document Type: Observability Standard

Applies To

- Backend
- AI Services
- Frontend
- Infrastructure
- Background Services

---

# Purpose

This document defines the official logging and observability strategy used by NexusMail AI.

Every generated feature must produce meaningful, structured, searchable logs.

Logging is mandatory.

---

# Philosophy

Logs exist to answer

What happened?

When?

Where?

Who?

Why?

How long?

What failed?

Never log for debugging only.

Log for production operations.

---

# Three Pillars of Observability

Observability consists of

Logs

↓

Metrics

↓

Distributed Tracing

Every module must support all three.

---

# Logging Framework

Backend

Serilog

Provider

OpenTelemetry

Console

Seq

Elastic

Grafana Loki

File (Development only)

---

# Log Format

Structured JSON

Never plain text.

Example

{
  "timestamp": "...",
  "level": "Information",
  "message": "...",
  "requestId": "...",
  "correlationId": "...",
  "workspaceId": "...",
  "userId": "...",
  "durationMs": 24
}

---

# Log Levels

Trace

Detailed diagnostics

Debug

Development only

Information

Normal business events

Warning

Unexpected but recoverable

Error

Failed operation

Critical

System unavailable

Fatal

Application termination

---

# Required Context

Every log entry MUST include

Timestamp (UTC)

Application

Environment

Module

RequestId

CorrelationId

WorkspaceId (if available)

UserId (if available)

MachineName

ServiceVersion

---

# Correlation ID

Every incoming request

↓

Generate or reuse

↓

Propagate

↓

Return to client

Every downstream service receives the same CorrelationId.

---

# Request Logging

Log

HTTP Method

Path

Query

Status Code

Duration

Remote IP

User Agent

Authenticated User

Workspace

Do not log request bodies containing secrets.

---

# Response Logging

Log

Status Code

Execution Time

Payload Size

Exception (if any)

Never log sensitive response data.

---

# Authentication Logging

Log

Login Success

Login Failure

Logout

Refresh Token

Token Revocation

MFA Verification

Provider Used

Never log passwords or tokens.

---

# Authorization Logging

Log

Permission Denied

Policy Failure

Resource Access

Privilege Escalation Attempt

---

# Business Logging

Examples

Workspace Created

Email Received

Email Archived

Email Tagged

Summary Generated

Rule Executed

Notification Sent

Subscription Activated

Billing Completed

These are Information level events.

---

# AI Logging

Log

Prompt Template ID

Provider

Model

Latency

Token Usage

Retry Count

Fallback Triggered

Confidence Score

Do NOT log

Raw prompts containing sensitive user data.

Full LLM responses if confidential.

---

# Email Synchronization Logging

Log

Provider

Mailbox

Email Count

Duration

Sync Status

Failures

Retry Attempts

---

# Background Jobs

Every job logs

Started

Completed

Duration

Items Processed

Errors

Retry Count

Cancellation

---

# Database Logging

Log

Slow Queries

Migration Execution

Connection Failures

Deadlocks

Timeouts

Never log full SQL with secrets.

---

# Cache Logging

Log

Cache Hit

Cache Miss

Eviction

Refresh

TTL

---

# Search Logging

Log

Query

Duration

Result Count

Provider

Vector Search

Hybrid Search

Ranking Time

---

# Notification Logging

Log

Channel

Recipient

Priority

Delivery Status

Retry Count

Failure Reason

---

# External Provider Logging

Every integration logs

Request

Response Status

Latency

Retry

Circuit Breaker

Timeout

Provider Name

Never expose secrets.

---

# Exception Logging

Always include

Exception Type

Message

Stack Trace (Internal)

CorrelationId

Module

User Context

Return sanitized messages to clients.

---

# Sensitive Data Policy

Never log

Passwords

JWT Tokens

Refresh Tokens

OAuth Secrets

API Keys

Credit Card Data

CVV

Private Keys

Access Tokens

Mask

Email addresses (optional policy)

Phone numbers

Personal identifiers where required.

---

# Log Retention

Development

7 Days

Staging

30 Days

Production

90 Days

Audit Logs

1–7 Years (configurable)

---

# Log Rotation

Daily

or

100 MB

Compressed

Archived

Encrypted if required.

---

# Metrics

Every module publishes

Request Count

Request Duration

Error Count

Cache Hit Ratio

Database Latency

AI Latency

Queue Length

Memory Usage

CPU Usage

---

# Distributed Tracing

Use OpenTelemetry.

Trace

API

↓

Database

↓

Redis

↓

RabbitMQ

↓

AI Gateway

↓

OpenSearch

Single trace across services.

---

# Dashboards

Grafana dashboards include

API Performance

AI Performance

Email Sync

Notification Delivery

Search Performance

Database Health

Queue Health

Error Rate

---

# Alerting

Critical alerts

API unavailable

Database unavailable

AI unavailable

Queue backlog

High error rate

Memory exhaustion

Disk full

Authentication failures

---

# Logging Performance

Logging must not significantly impact application performance.

Prefer asynchronous sinks.

Batch where supported.

---

# Development Rules

Debug logs

Enabled only in Development.

Production

Minimum level

Information

---

# Audit Logging

Separate from application logs.

Immutable.

Tracks

Who

What

When

Where

Before

After

Reason (if applicable)

---

# AI Coding Checklist

Before completing a feature verify

✔ Structured logging added

✔ CorrelationId propagated

✔ Sensitive data masked

✔ Exceptions logged

✔ Metrics emitted

✔ Tracing enabled

✔ Dashboard metrics available

✔ Audit events generated where required

---

# Definition of Done

A feature is observability-complete when

✔ Structured logs generated

✔ Metrics published

✔ Distributed tracing supported

✔ Sensitive data protected

✔ Dashboard visibility available

✔ Alerts configured where applicable

✔ Audit logs implemented

✔ Documentation updated

---

END OF DOCUMENT