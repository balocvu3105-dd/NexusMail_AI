# NexusMail AI
# Event Catalog

Version:
1.0.0

Status:
Approved

Document Type:
Event Architecture Specification

Priority:
Critical

---

# 1. Event Architecture Overview

NexusMail AI uses Event Driven Architecture.

Events represent:
"Something important happened in the business."

Example:
EmailReceived

means:
"The system has successfully received a new email."

Events are immutable facts.
Events cannot be modified after publishing.

---

# 2. Event Flow

Producer
|
|
v
Message Broker
(RabbitMQ)
|
|
+----------------+
|                |
v                v
Consumer A Consumer B

---

# 3. Event Design Principles

## Immutable
Event cannot change after creation.

## Self Contained
Event contains enough information for consumer processing.

## Versioned
Events support evolution.

## Observable
Every event must have:
- EventId
- Timestamp
- CorrelationId
- TenantId

---

# 4. Base Event Contract

All events inherit:

EventBase
{
EventId
EventType
Version
CreatedAt
CorrelationId
TenantId
WorkspaceId
Producer
}

---

# 5. Identity Events

# UserRegistered

## Producer
Identity Service

## Consumers
Notification Service
Analytics Service

## Payload

{
UserId,
Email,
CreatedAt
}


Purpose:
Create user profile
Send welcome notification
Track registration

---

# UserLoggedIn

Producer:
Identity Service

Consumers:
Analytics

Payload:

{
UserId,
LoginMethod,
IpAddress,
Timestamp
}


---

# 6. Workspace Events

# WorkspaceCreated

Producer:
Workspace Service

Consumers:
Billing
Analytics

Payload:

{
WorkspaceId,
OwnerId,
Plan
}


---

# MemberInvited

Producer:
Workspace Service

Consumers:
Notification Service

Payload:

{
WorkspaceId,
Email,
Role
}


---

# 7. Email Events

This is the core event group.

---

# EmailAccountConnected

Producer:
Email Service

Consumers:
Sync Worker

Payload:

{
AccountId,
WorkspaceId,
Provider,
EmailAddress
}


---

# EmailSyncStarted

Producer:
Email Worker

Consumers:
Analytics

Payload:

{
AccountId,
SyncId,
StartedAt
}


---

# EmailSyncCompleted

Producer:
Email Worker

Consumers:
AI Worker
Search Worker

Payload:

{
AccountId,
TotalReceived,
CompletedAt
}


---

# EmailReceived

Producer:
Email Sync Worker

Consumers:
AI Worker
Search Worker
Notification Worker

Payload:

{
EmailId,
AccountId,
Sender,
Subject,
ReceivedAt
}


Purpose:
Trigger email intelligence pipeline.

Flow:
EmailReceived
↓
AI Analysis
↓
Index
↓
Notification

---

# EmailUpdated

Producer:
Email Service

Consumers:
Search Worker

Payload:

{
EmailId,
ChangedFields
}


---

# EmailArchived

Producer:
Email Service

Consumers:
Analytics

Payload:

{
EmailId,
UserId
}


---

# EmailDeleted

Producer:
Email Service

Consumers:
Search Worker

Payload:

{
EmailId
}


---

# 8. AI Events

# EmailAnalysisStarted

Producer:
AI Worker

Consumers:
Analytics

Payload:

{
EmailId,
Model,
StartedAt
}


---

# EmailAnalyzed

Producer:
AI Service

Consumers:
Email Service
Search Worker

Payload:

{
EmailId,
Language,
Category,
Confidence
}


---

# SummaryGenerated

Producer:
AI Service

Consumers:
Email Service
Notification Service

Payload:

{
EmailId,
Summary,
Model,
Confidence
}


---

# ClassificationCompleted

Producer:
AI Service

Consumers:
Automation Engine

Payload:

{
EmailId,
Category,
Confidence
}


---

# PhishingDetected

Producer:
AI Security Module

Consumers:
Notification Service
Security Service

Payload:

{
EmailId,
RiskScore,
Reason
}


---

# 9. Search Events

# SearchIndexCreated

Producer:
Search Worker

Consumers:
Analytics

Payload:

{
EmailId,
IndexId
}


---

# SearchIndexUpdated

Producer:
Search Worker

Consumers:
Analytics

Payload:

{
EmailId,
UpdatedAt
}


---

# 10. Automation Events

# RuleCreated

Producer:
Automation Service

Consumers:
Analytics

Payload:

{
RuleId,
WorkspaceId
}


---

# RuleExecuted

Producer:
Automation Engine

Consumers:
Notification Service
Audit Service

Payload:

{
RuleId,
TriggerEvent,
Action,
ExecutedAt
}


---

# RuleFailed

Producer:
Automation Engine

Consumers:
Admin Service

Payload:

{
RuleId,
Error,
Timestamp
}


---

# 11. Notification Events

# NotificationCreated

Producer:
Notification Service

Consumers:
Delivery Worker

Payload:

{
NotificationId,
UserId,
Channel
}


---

# NotificationSent

Producer:
Delivery Worker

Consumers:
Analytics

Payload:

{
NotificationId,
Channel,
SentAt
}


---

# NotificationFailed

Producer:
Delivery Worker

Consumers:
Retry Worker

Payload:

{
NotificationId,
Reason
}


---

# 12. Billing Events

# SubscriptionActivated

Producer:
Billing Service

Consumers:
Authorization Service

Payload:

{
SubscriptionId,
UserId,
Plan
}


---

# PaymentCompleted

Producer:
Billing Service

Consumers:
Analytics

Payload:

{
PaymentId,
Amount,
Currency
}


---

# 13. Plugin Events

# PluginInstalled

Producer:
Plugin Service

Consumers:
Audit Service

Payload:

{
PluginId,
WorkspaceId,
Version
}


---

# 14. Dead Letter Queue Events

Failed events go to:
DLQ

Reasons:
- Invalid payload
- Consumer failure
- Timeout
- Processing exception

DLQ message contains:

{
OriginalEventId,
Error,
RetryCount,
FailedAt
}


---

# 15. Event Naming Convention

Format:

<Entity><ActionPastTense>

Correct:
EmailReceived
UserRegistered

Wrong:
ReceiveEmail
CreateUserNow

---

# 16. Event Versioning

Example:
EmailReceived.v1

Future:
EmailReceived.v2

Rules:
Breaking changes require new version.

---

# 17. Idempotency Rules

Consumers must handle duplicate events.

Every consumer stores:
EventId
Processing status

Example:
If EmailReceived event arrives twice:

First:
Process

Second:
Ignore

---

# 18. Event Security

Events must not contain:
- Password
- Access Token
- Secret Key

Sensitive data:
Use reference ID.

---

# 19. Monitoring

Track:
Event latency
Queue size
Failed events
Retry count
Consumer health

---

# End Of Document
