# DDD_GUIDE.md

> NexusMail AI Domain Driven Design Guide

Version: 1.0.0

Status: Production

Priority: CRITICAL

Document Type: Domain Driven Design Standard

Applies To

- Backend
- AI Services
- Domain Layer
- All Future Modules

---

# Purpose

This document defines how Domain Driven Design (DDD) is applied throughout NexusMail AI.

Every AI Coding Agent must follow these rules before generating any domain model.

---

# DDD Philosophy

Software must model the business.

Database does not define the Domain.

UI does not define the Domain.

Framework does not define the Domain.

Business defines the Domain.

---

# Strategic Design

NexusMail AI consists of the following Bounded Contexts.

Identity

Workspace

Email

AI

Notification

Search

Analytics

Automation

Administration

Billing

Plugin

Security

Each Bounded Context owns its own business rules.

Never place business logic across contexts.

---

# Context Relationships

Identity

↓

Workspace

↓

Email

↓

AI

↓

Notification

↓

Analytics

↓

Automation

Search is shared by Email and AI.

Billing is isolated.

Plugin communicates only through contracts.

---

# Ubiquitous Language

All engineers and AI agents must use the same terminology.

Correct

Email

Attachment

Workspace

Organization

Tag

Category

Priority

Summary

Reminder

Notification

Rule

Avoid synonyms.

Never mix

Mail

Message

Letter

Inbox Item

All documentation and code use the same vocabulary.

---

# Entity

Entity has identity.

Examples

User

Workspace

Email

Attachment

Rule

Notification

Subscription

Plugin

Identity never changes.

Properties may change.

---

# Value Object

Value Objects have no identity.

Immutable.

Examples

EmailAddress

Money

Priority

Language

TimeZone

Coordinates

TagName

CategoryName

RuleCondition

RuleAction

Always compare by value.

---

# Aggregate

Aggregate controls consistency.

Only Aggregate Root is publicly accessible.

Example

Email

↓

Attachments

↓

Tags

↓

Classification

↓

Summary

External modules communicate only with Email Aggregate.

Never modify child entities directly.

---

# Aggregate Roots

Identity

User

Workspace

Email

Notification

Subscription

Plugin

AutomationRule

---

# Domain Services

Use only when business logic cannot naturally belong to an Entity or Value Object.

Examples

EmailClassificationService

SpamDetectionService

ReminderSchedulingService

PriorityCalculationService

Never place persistence logic here.

---

# Repository

Repositories belong to the Domain.

Interfaces live in Domain.

Implementations live in Infrastructure.

Example

IEmailRepository

EmailRepository

---

# Factory

Factories create complex aggregates.

Example

EmailFactory

WorkspaceFactory

NotificationFactory

Factories validate construction rules.

---

# Specification Pattern

Used for reusable business conditions.

Examples

UnreadEmailSpecification

HighPrioritySpecification

SpamSpecification

PremiumWorkspaceSpecification

Never duplicate conditional logic.

---

# Domain Events

Represent something that already happened.

Examples

EmailReceived

EmailArchived

EmailTagged

SummaryGenerated

NotificationSent

WorkspaceCreated

UserInvited

Never use Domain Events to represent commands.

---

# Application Services

Responsible for orchestration.

Allowed

Transactions

Calling multiple repositories

Publishing events

Calling AI Gateway

Calling Notification Service

Forbidden

Business rules.

---

# Domain Purity

Domain Layer must never reference

ASP.NET Core

EF Core

Redis

RabbitMQ

SignalR

OpenSearch

FastAPI

HTTP

Logging frameworks

Cloud SDKs

---

# Email Aggregate Example

Email

├── Subject

├── Sender

├── Recipients

├── Body

├── Attachments

├── Tags

├── AI Summary

├── Priority

├── Status

└── Metadata

Email is the Aggregate Root.

Attachments cannot exist without Email.

---

# Workspace Aggregate

Workspace

├── Members

├── Roles

├── Email Accounts

├── Settings

├── Subscription

└── Organization

---

# Notification Aggregate

Notification

├── Channels

├── Delivery History

├── Priority

└── Retry Policy

---

# Aggregate Rules

Each Aggregate

Owns its data.

Controls its invariants.

Maintains consistency.

Never expose internal collections for modification.

---

# Business Invariants

Examples

Email must belong to one Workspace.

Attachment must belong to one Email.

Tag names are unique per Workspace.

Workspace Owner cannot be removed while active.

Deleted Emails cannot receive new Attachments.

These rules live in Domain.

---

# Anti-Corruption Layer

External providers

Google

Microsoft

Yahoo

IMAP

↓

Adapter

↓

Domain

Never expose external SDK models inside Domain.

---

# Shared Kernel

Contains only

Primitive Value Objects

Base Entities

Base Events

Common Interfaces

Do not place business rules inside Shared.

---

# Module Communication

Preferred

Domain Events

Application Events

REST APIs

gRPC

Forbidden

Shared database tables

Direct Entity references

Cross-module repositories

---

# Rich Domain Model

Entities should contain behavior.

Example

Email.MarkAsRead()

Email.AddTag()

Email.Archive()

Avoid Anemic Domain Models.

---

# Domain Exceptions

Use explicit exceptions.

Examples

DuplicateTagException

InvalidPriorityException

WorkspaceLimitExceededException

Never throw generic Exception.

---

# Testing Strategy

Every Entity

Unit Tested.

Every Aggregate

Unit Tested.

Every Specification

Unit Tested.

Every Domain Service

Unit Tested.

Target Coverage

95%+

---

# AI Coding Rules

Before generating any feature

AI must identify

Aggregate Root

Entities

Value Objects

Domain Events

Repositories

Factories

Specifications

Business Rules

If unclear

Stop and request clarification.

Never invent business rules.

---

# Definition of Done

DDD implementation complete when

✔ Aggregate Root identified

✔ Entities defined

✔ Value Objects immutable

✔ Domain Events implemented

✔ Specifications created

✔ Repositories abstracted

✔ Factories added where needed

✔ Business invariants enforced

✔ Unit tests added

✔ Documentation updated

---

END OF DOCUMENT