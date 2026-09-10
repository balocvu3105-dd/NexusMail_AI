# PROJECT_CONTEXT.md

> NexusMail AI - Project Context

Version: 1.0.0

Status: Architecture Design

Phase: Foundation

Priority: Critical

Document Type: Project Context

---

# Project Overview

Project Name

NexusMail AI

Description

NexusMail AI là nền tảng AI Email Management thế hệ mới.

Hệ thống không phải Email Client.

Hệ thống là AI Layer nằm trên tất cả Email Provider.

AI sẽ

• Hiểu Email

• Phân loại

• Gắn Tag

• Chấm điểm

• Tự động hóa

• Tìm kiếm

• Trả lời

• Phân tích

---

# Vision

One Inbox.

Unlimited Intelligence.

---

# Mission

Giúp người dùng không cần đọc toàn bộ Email.

AI sẽ xử lý trước.

Người dùng chỉ xử lý Email thật sự quan trọng.

---

# Product Position

KHÔNG phải

Gmail Clone

KHÔNG phải

Outlook Clone

KHÔNG phải

Mail Client

Là

AI Productivity Platform.

---

# Business Model

Free

1 Email Account

Basic AI

----------------------

Pro

Unlimited Email

AI Summary

Semantic Search

Automation

----------------------

Enterprise

Workspace

Organization

Analytics

Audit

API

Plugin

SSO

---

# Business Domains

Identity

Workspace

Email

AI

Automation

Notification

Analytics

Administration

Billing

Search

Security

Plugin

---

# Bounded Context

Identity Context

Workspace Context

Email Context

AI Context

Notification Context

Analytics Context

Search Context

Automation Context

Security Context

Administration Context

Billing Context

Plugin Context

---

# Core Modules

Authentication

Authorization

User

Workspace

Email Account

Inbox

Folders

Categories

Tags

Rules

AI

Notification

Analytics

Dashboard

Audit

Settings

Search

Plugin

Billing

API

---

# User Roles

Guest

User

Premium User

Workspace Admin

Organization Owner

System Admin

Developer

Support

AI Service

---

# Main Actors

End User

Administrator

AI Engine

Email Provider

Notification Service

Payment Provider

Search Engine

Monitoring System

---

# Supported Email Providers

Gmail

Outlook

Yahoo

Exchange

IMAP

Office365

Future

Proton

Zoho

Apple Mail

---

# Email Lifecycle

Connect Account

↓

Synchronize

↓

Download

↓

Store

↓

Analyze

↓

AI Classification

↓

AI Summary

↓

Priority Score

↓

Tag

↓

Notification

↓

Search Index

↓

Archive

↓

History

---

# AI Lifecycle

Receive Email

↓

Normalize

↓

Extract Metadata

↓

Language Detection

↓

Embedding

↓

Classification

↓

Tag Generation

↓

Priority

↓

Spam

↓

Phishing

↓

Summary

↓

Recommendation

↓

Save Result

---

# Notification Lifecycle

Email Arrived

↓

Rule Evaluation

↓

Priority

↓

Notification

↓

Desktop

↓

Mobile

↓

Discord

↓

Slack

↓

Teams

↓

History

---

# Search Lifecycle

Keyword

↓

Vector Search

↓

Hybrid Search

↓

Ranking

↓

Result

---

# Automation Lifecycle

Trigger

↓

Condition

↓

Rule

↓

Action

↓

Notification

↓

History

---

# Main Entities

User

Workspace

Organization

Role

Permission

EmailProvider

EmailAccount

Email

Attachment

Folder

Category

Tag

Rule

Notification

Reminder

AIResult

Summary

PriorityScore

SearchIndex

AuditLog

ApiKey

Webhook

Plugin

Subscription

Invoice

Payment

---

# Aggregate Roots

User

Workspace

Email

Rule

Notification

Subscription

Plugin

---

# Value Objects

EmailAddress

PhoneNumber

Money

Priority

Language

TimeZone

TagName

CategoryName

RuleCondition

RuleAction

---

# Domain Events

UserRegistered

WorkspaceCreated

EmailReceived

EmailDeleted

EmailArchived

EmailTagged

RuleExecuted

NotificationSent

ReminderTriggered

SubscriptionActivated

PluginInstalled

---

# External Services

Google OAuth

Microsoft OAuth

GitHub OAuth

OpenAI Compatible

Ollama

SMTP

Redis

RabbitMQ

OpenSearch

Prometheus

Grafana

Stripe

PayPal

---

# Security Principles

Least Privilege

Zero Trust

Encryption At Rest

Encryption In Transit

MFA

JWT

OAuth2

OpenID Connect

Audit Logging

Secret Rotation

---

# Performance Goals

API < 200ms

Search < 100ms

AI Summary < 5s

Notification < 3s

Dashboard < 2s

Email Sync Parallel

Cache Hit > 80%

---

# Scalability Goals

10 Users

↓

100 Users

↓

1,000 Users

↓

10,000 Users

↓

100,000 Users

↓

1 Million Users

Without Architecture Changes.

---

# Availability Goals

99.9%

Future

99.99%

---

# Quality Attributes

Maintainable

Scalable

Secure

Observable

Testable

Extensible

Plugin Ready

Cloud Native

AI Native

---

# Coding Philosophy

Every Feature Independent

Every Module Replaceable

Every Service Testable

Every Dependency Injectable

Every API Versioned

Every Event Observable

Every Configuration Externalized

---

# Future Modules

Calendar

Task

CRM

Meeting

Drive

Knowledge Base

Document AI

Voice Assistant

Chat

Workflow Designer

Marketplace

SDK

Mobile

Desktop

Browser Extension

---

# Out Of Scope (Version 1)

Video Meeting

Instant Messaging

Social Network

Blockchain

Cryptocurrency

IoT

---

# Success Metrics

Classification Accuracy

Summary Accuracy

Search Accuracy

Spam Detection Accuracy

Phishing Detection Accuracy

Notification Delay

API Latency

CPU Usage

Memory Usage

User Satisfaction

Retention

---

# Repository Structure

/backend

/frontend

/ai

/shared

/docs

/database

/api

/devops

/prompts

/tests

/tools

/scripts

---

# Project Status

Phase

Foundation

Status

In Progress

---

END OF DOCUMENT
---

# High Level Architecture

`	ext
                Users
                  |
                  |
          Web / Desktop / Mobile
                  |
                  |
              API Gateway
                  |
        -----------------------
        |          |          |
        v          v          v
 Identity     Email Core    AI Core
        |          |          |
        -----------------------
                  |
          Event Bus
              RabbitMQ
                  |
        ---------------------
        |         |         |
    Worker    Search    Notification
                  |
        ---------------------
        PostgreSQL
        Redis
        OpenSearch
`

---

# Multi Tenancy

NexusMail AI supports multi tenant architecture.

Tenant isolation:
- Logical isolation
- Workspace based authorization
- Data access policy enforcement

Every business entity must contain:
- TenantId
- WorkspaceId
- CreatedAt
- UpdatedAt

VĂ­ dá»¥ Email table:
`
Email
Id
WorkspaceId
AccountId
Sender
Subject
Content
CreatedAt
`

KhĂ´ng cĂ³ WorkspaceId sáº½ khĂ³ scale Enterprise.

---

# Data Ownership

User owns:
- Personal mailbox

Workspace owns:
- Shared mailbox
- Rules
- Analytics
- Audit

Organization owns:
- Billing
- Policies
- Members

---

# AI Governance

AI never:
- Sends email automatically without permission
- Deletes user data
- Changes security settings

AI actions Must have:
- Confidence score
- Audit trail
- User approval

VĂ­ dá»¥ AI Ä‘á» xuáº¥t:
"Reply: TĂ´i sáº½ tham gia cuá»™c há»p"

NhÆ°ng:
AI Suggestion -> User Approval -> Send

---

# Email Privacy

Principles:
- User owns email data
- Encryption required
- Minimal data exposure
- AI processing transparency

AI processing modes:
- Cloud AI
- Local AI
- Hybrid AI

---

# Domain Events

- EmailReceived
- EmailDeleted
- EmailArchived
- EmailSyncStarted
- EmailSyncCompleted
- EmailAnalyzed
- EmailEmbeddingCreated
- EmailClassificationCompleted
- EmailPriorityCalculated
- PhishingDetected
- SpamDetected
- SearchIndexUpdated
- AutomationExecuted
- AIApprovalRequired

---

# Reliability Requirements

Email Sync:
- At least once delivery

Event Processing:
- Retry enabled

Worker:
- Idempotent

Failure:
- No data loss

