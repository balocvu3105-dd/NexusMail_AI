# NexusMail AI
# Domain Model Document

Version:
1.0.0

Status:
Approved

Document Type:
Domain Design

Priority:
Critical

---

# 1. Domain Overview

NexusMail AI uses Domain Driven Design (DDD).

The system is divided into bounded contexts.
Each context owns:
- Business rules
- Entities
- Aggregates
- Domain events

No context can directly access another context's internal data.

Communication:
Domain Event
or
Application Service

---

# 2. Bounded Context Map

                Identity Context
                       |
                       v

              Workspace Context
                       |
                       v
+------------------------------------------------+
                Email Context
+------------------------------------------------+
    |                 |                |
    v                 v                v

AI Context      Search Context   Automation Context

    |
    v
Notification Context
    |
    v
Billing Context
    |
    v
Administration Context

---

# 3. Identity Context

## Responsibility
Manage user identity and authentication.

## Aggregate Roots
User

## Entities
User
Role
Permission
OAuthAccount
Session

## Value Objects
EmailAddress
PhoneNumber
UserName
TimeZone

## Business Rules
A user must have unique identity.
A user can belong to multiple workspaces.
Authentication provider cannot create duplicate accounts.

## Domain Events
UserRegistered
UserLoggedIn
UserPasswordChanged
OAuthConnected

---

# 4. Workspace Context

## Responsibility
Multi tenant management.

## Aggregate Roots
Workspace

## Entities
Workspace
Organization
Membership
Invitation
RoleAssignment

## Value Objects
WorkspaceName
TenantId
PermissionSet

## Business Rules
Every workspace has owner.
Members require permission.
Data cannot cross workspace boundary.

## Domain Events
WorkspaceCreated
MemberInvited
MemberJoined
RoleChanged

---

# 5. Email Context

## Responsibility
Core email domain.
Manages:
- Email account
- Synchronization
- Messages
- Conversations
- Attachments

## Aggregate Roots
EmailAccount
Email

---

# EmailAccount Aggregate

Entities:
EmailAccount

Properties:
Id
WorkspaceId
Provider
Address
AccessTokenReference
SyncStatus

Business Rules:
One account belongs to one workspace.
Provider credentials cannot be stored plaintext.

Domain Events:
EmailAccountConnected
EmailSyncStarted
EmailSyncCompleted

---

# Email Aggregate

Entities:
Email
Attachment
EmailParticipant
Conversation

Value Objects:
Subject
EmailAddress
EmailContent
MessageId

Business Rules:
Email belongs to one account.
Email lifecycle must be tracked.

Lifecycle:
Received
Processed
Classified
Indexed
Archived

Domain Events:
EmailReceived
EmailUpdated
EmailArchived
EmailDeleted

---

# 6. AI Context

## Responsibility
Artificial Intelligence processing.

## Aggregate Roots
AIAnalysis

## Entities
AIResult
Summary
Classification
Embedding
PriorityScore

## Value Objects
ConfidenceScore
Language
AIModelVersion

## Business Rules
AI result must have confidence score.
AI cannot execute destructive actions.

## Domain Events
EmailAnalyzed
SummaryGenerated
ClassificationCompleted
PhishingDetected

---

# 7. Search Context

## Responsibility
Email discovery and retrieval.

## Aggregate Roots
SearchIndex

## Entities
SearchDocument
EmbeddingVector

## Value Objects
SearchQuery
RankingScore

## Business Rules
Search index is not source of truth.
Database remains authoritative.

## Domain Events
SearchIndexCreated
SearchIndexUpdated
SearchIndexRemoved

---

# 8. Automation Context

## Responsibility
User defined workflows.

## Aggregate Roots
Rule

## Entities
Rule
Condition
Action
ExecutionHistory

## Value Objects
RuleName
RuleCondition
RuleAction

## Business Rules
Disabled rules cannot execute.
Every execution must be logged.

## Example

Trigger:
EmailReceived

Condition:
Sender contains customer.com

Action:
Create notification

## Domain Events
RuleCreated
RuleExecuted
RuleFailed

---

# 9. Notification Context

## Responsibility
User notification management.

## Aggregate Roots
Notification

## Entities
Notification
NotificationPreference
DeliveryAttempt

## Value Objects
NotificationChannel
NotificationPriority

## Channels
Desktop
Mobile
Discord
Slack
Teams

## Business Rules
Notification respects user preferences.
Failed delivery must retry.

## Domain Events
NotificationCreated
NotificationSent
NotificationFailed

---

# 10. Billing Context

## Responsibility
Subscription management.

## Aggregate Roots
Subscription

## Entities
Plan
Subscription
Invoice
Payment

## Value Objects
Money
Currency
BillingPeriod

## Business Rules
Subscription controls feature access.
Payment status must be auditable.

## Domain Events
SubscriptionActivated
PaymentCompleted
SubscriptionCancelled

---

# 11. Plugin Context

## Responsibility
Enterprise extensibility.

## Aggregate Roots
Plugin

## Entities
Plugin
PluginVersion
PluginPermission

## Value Objects
PluginIdentifier
VersionNumber

## Business Rules
Plugin requires permission declaration.
Plugin installation must be audited.

## Domain Events
PluginInstalled
PluginUpdated
PluginRemoved

---

# 12. Administration Context

## Responsibility
System management.

## Aggregate Roots
AuditLog

## Entities
AuditLog
SystemSetting
ApiKey

## Value Objects
ApiKeyValue
ConfigurationKey

## Domain Events
AuditCreated
ApiKeyGenerated

---

# 13. Aggregate Relationship

User
|
|
v
Workspace
|
|
+----------------+
|                |
v                v
EmailAccount Subscription
|
|
v
Email
|
|
+-------------+
|             |
v             v
AIResult SearchIndex

Email
|
|
v
Automation Rule
|
|
v
Notification

---

# 14. Domain Event Catalog

## User Events
UserRegistered

## Workspace Events
WorkspaceCreated

## Email Events
EmailReceived
EmailArchived
EmailDeleted

## AI Events
EmailAnalyzed
SummaryGenerated

## Automation Events
RuleExecuted

## Notification Events
NotificationSent

## Billing Events
SubscriptionActivated

## Plugin Events
PluginInstalled

---

# 15. Domain Rules

## Rule 1
Domain cannot depend on Infrastructure.

## Rule 2
Aggregate controls its own consistency.

## Rule 3
External communication happens through Application Layer.

## Rule 4
Events represent business facts.

## Rule 5
AI output is treated as domain data, not business authority.

---

# 16. Backend Mapping

Future structure:

Backend
src
|
+-- Domain
| |
| +-- Identity
| +-- Workspace
| +-- Email
| +-- AI
| +-- Search
| +-- Automation
| +-- Notification
| +-- Billing
|
+-- Application
|
+-- Infrastructure
|
+-- API

---

# End Of Document
