# NexusMail AI
# Data Model Overview

Version:
1.0.0

Status:
Approved

Document Type:
Database Architecture Specification

Priority:
Critical

---

# 1. Data Architecture Overview

NexusMail AI uses a polyglot persistence approach.

Each storage has a specific responsibility.

              Application
                   |
      --------------------------------
      |              |              |
      v              v              v
 PostgreSQL      OpenSearch       Redis
 Transaction     Search          Cache
      |
      v
 Object Storage
 Attachments

---

# 2. Storage Responsibility

## PostgreSQL
Source of Truth.

Stores:
- Users
- Workspace
- Email metadata
- Rules
- Billing
- Audit

---

## OpenSearch
Search optimized storage.

Stores:
- Email content index
- Full text
- Embeddings
- Ranking data

Not source of truth.

---

## Redis
Temporary storage.

Stores:
- Cache
- Session
- Rate limit
- Processing state

---

## Object Storage
Stores:
- Attachments
- Large files
- AI generated files

Compatible:
S3
Azure Blob
MinIO

---

# 3. Database Principles

## Multi Tenant Required
Every tenant owned table must contain:

TenantId
WorkspaceId

---

## Audit Required
Important tables contain:

CreatedAt
UpdatedAt
CreatedBy
UpdatedBy

---

## Soft Delete
User data should not immediately disappear.

Required fields:

DeletedAt
IsDeleted

---

# 4. Identity Database Model

## Users
Table:
users

Purpose:
Store user accounts.

Columns:
Id
Email
DisplayName
AvatarUrl
Status
CreatedAt
UpdatedAt

Relationships:
User
1 -> N
WorkspaceMembership

---

## OAuth Accounts
Table:
oauth_accounts

Columns:
Id
UserId
Provider
ProviderUserId
CreatedAt

---

## Roles
Table:
roles

Columns:
Id
Name
Description

---

## Permissions
Table:
permissions

Columns:
Id
Code
Description

---

# 5. Workspace Model

## Workspaces
Table:
workspaces

Columns:
Id
OrganizationId
Name
Plan
OwnerId
CreatedAt

---

## Workspace Members
Table:
workspace_members

Columns:
Id
WorkspaceId
UserId
RoleId
JoinedAt

---

# 6. Email Database Model

## Email Accounts
Table:
email_accounts

Purpose:
Connected email providers.

Columns:
Id
WorkspaceId
Provider
EmailAddress
Status
LastSyncAt
CreatedAt

---

## Emails
Table:
emails

Core email entity.

Columns:
Id
WorkspaceId
AccountId
ExternalId
ConversationId
Sender
Subject
BodyPreview
ReceivedAt
Status
CreatedAt

Indexes:
WorkspaceId
AccountId
ReceivedAt
ConversationId

---

## Email Participants
Table:
email_participants

Columns:
Id
EmailId
Address
Name
Type

Types:
Sender
Receiver
CC
BCC

---

## Conversations
Table:
email_conversations

Columns:
Id
WorkspaceId
Subject
LastMessageAt

---

## Attachments
Table:
attachments

Columns:
Id
EmailId
FileName
Size
MimeType
StoragePath

---

# 7. AI Database Model

## AI Results
Table:
ai_results

Columns:
Id
EmailId
Model
Language
Confidence
CreatedAt

---

## AI Summaries
Table:
ai_summaries

Columns:
Id
EmailId
Summary
Model
Confidence

---

## Classification
Table:
email_classifications

Columns:
Id
EmailId
Category
Score

---

## Priority Score
Table:
email_priorities

Columns:
Id
EmailId
Score
Reason

---

# 8. Automation Database Model

## Rules
Table:
automation_rules

Columns:
Id
WorkspaceId
Name
Status
Trigger
CreatedAt

---

## Conditions
Table:
rule_conditions

Columns:
Id
RuleId
Field
Operator
Value

---

## Actions
Table:
rule_actions

Columns:
Id
RuleId
ActionType
Configuration

---

## Execution History
Table:
rule_executions

Columns:
Id
RuleId
Status
ExecutedAt
Error

---

# 9. Notification Database Model

## Notifications
Table:
notifications

Columns:
Id
WorkspaceId
UserId
Type
Channel
Status
CreatedAt

---

## Notification Delivery
Table:
notification_deliveries

Columns:
Id
NotificationId
Channel
Status
SentAt

---

# 10. Search Model

Search data is stored outside PostgreSQL.

OpenSearch Document:

EmailDocument
{
EmailId,
WorkspaceId,
Subject,
Content,
Sender,
Tags,
AIClassification,
EmbeddingVector
}

Vector dimension depends on AI model.

---

# 11. Billing Model

## Plans
Table:
plans

Columns:
Id
Name
Features
Price

---

## Subscriptions
Table:
subscriptions

Columns:
Id
WorkspaceId
PlanId
Status
StartDate
EndDate

---

## Payments
Table:
payments

Columns:
Id
SubscriptionId
Amount
Currency
Provider
Status

---

# 12. Audit Model

## Audit Logs
Table:
audit_logs

Columns:
Id
WorkspaceId
UserId
Action
Entity
EntityId
Metadata
CreatedAt

Used for:
- Security
- Compliance
- Enterprise audit

---

# 13. API Key Model

## API Keys
Table:
api_keys

Columns:
Id
WorkspaceId
Name
Hash
LastUsedAt
CreatedAt

Important:
Raw API key is never stored.

---

# 14. Database Relationship Overview

User
|
|
v
Workspace
|
+----------------------------+
|             |              |
v             v              v
Email       Rules       Subscription
|             |
+------------+
|             |
v             v
AI Result    Attachment
|             |
v
Search Index

---

# 15. Index Strategy

Important indexes:

## Email
(email_account_id)
(workspace_id, received_at)
(conversation_id)

## Search
Managed by OpenSearch.

## Audit
(workspace_id, created_at)

---

# 16. Partition Strategy

Future scale:
Emails table partitioned by:
WorkspaceId
or
ReceivedAt

---

# 17. Data Retention Policy

Free:
Limited history

Pro:
Extended history

Enterprise:
Custom retention policy

---

# 18. Backup Strategy

Database:
Daily backup
Point-in-time recovery

Object Storage:
Versioning enabled

Search:
Can be rebuilt from PostgreSQL

---

# 19. Migration Strategy

Database changes:
Must use migration system.

Technology:
EF Core Migration

Rules:
No manual production changes.

---

# 20. Data Security Rules

Never store:
Password
OAuth Token
Secret Key

Encryption required:
Sensitive columns

---

# End Of Document
