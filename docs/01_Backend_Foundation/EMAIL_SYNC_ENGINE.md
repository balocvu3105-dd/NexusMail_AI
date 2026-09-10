# NexusMail AI
# Email Sync Engine Architecture

Version:
1.0.0

Status:
Approved

Document Type:
Email Synchronization Design

Technology:
.NET 8 Worker Service
OAuth 2.0
REST API
IMAP
RabbitMQ

---

# 1. Overview

Email Sync Engine is responsible for:
- Connecting Email Providers
- Synchronizing emails
- Normalizing email data
- Storing email metadata
- Detecting changes
- Publishing events

It is the entry point of Email Intelligence Pipeline.

---

# 2. Supported Providers

Phase 1:
Gmail
Microsoft Outlook
Office365
IMAP

Future:
Yahoo
Proton
Zoho
Apple Mail

---

# 3. Architecture Overview

Email Provider
  |
  |
  v
Provider Adapter
  |
  |
  v
Sync Engine
  |
  |
  v
Email Normalizer
  |
  |
  v
Database
  |
  |
  v
Domain Event
  |
  |
  v
RabbitMQ
  |
  +------------+
  |            |
  v            v
AI Worker Search Worker

---

# 4. Design Principles

Rules:
Provider independent
Incremental sync
Idempotent processing
Event driven
Fault tolerant
Retry capable

---

# 5. Sync Components

EmailSyncService
├── Provider Connector
├── Sync Scheduler
├── Sync State Manager
├── Email Downloader
├── Email Parser
├── Normalizer
├── Deduplicator
└── Event Publisher

---

# 6. Provider Adapter Pattern

Every provider implements:
```csharp
IEmailProvider
```

Interface:
```csharp
public interface IEmailProvider
{
    Task<SyncResult> SyncAsync(
        EmailAccount account,
        CancellationToken token);
}
```

---

# 7. Gmail Adapter

Implementation:
GmailProvider

Uses:
Google Gmail API

Capabilities:
List messages
Get message
History API
Push notification

---

# 8. Microsoft Adapter

Implementation:
MicrosoftGraphProvider

Uses:
Microsoft Graph API

Capabilities:
Mail sync
Delta query
Webhook subscription

---

# 9. IMAP Adapter

Implementation:
ImapProvider

Uses:
IMAP Protocol

Capabilities:
Folder sync
Message download

---

# 10. Sync Workflow

Full flow:

Connect Account
        ↓
Validate OAuth
        ↓
Create EmailAccount
        ↓
Start Initial Sync
        ↓
Download Messages
        ↓
Normalize
        ↓
Save Database
        ↓
Publish EmailReceived Event

---

# 11. Incremental Sync

Never download everything repeatedly.

Use:
Provider sync token

Examples:
Gmail: HistoryId
Microsoft: DeltaToken
IMAP: UID

---

# 12. Sync State Entity

Entity:
EmailSyncState

Properties:
Id
EmailAccountId
Provider
Cursor
LastSyncTime
Status

---

# 13. Sync Status

Enum:
Idle
Running
Failed
Paused

---

# 14. Scheduler Design

Technology:
.NET Background Worker

Flow:

Scheduler
 |
Find accounts needing sync
 |
Queue Sync Job
 |
Worker executes

---

# 15. Worker Architecture

Project:
NexusMail.Worker.EmailSync

Contains:
Workers/
EmailSyncWorker
Jobs/
SyncEmailJob
Services/
SyncService

---

# 16. Queue Architecture

RabbitMQ queues:
email.sync.request
email.received
email.failed

---

# 17. Sync Job Message

Example:
```json
{
 "EmailAccountId": "uuid",
 "Provider": "Gmail"
}
```

---

# 18. Email Normalization

Different providers return different formats.

Normalize into:
Email
Subject
Sender
Recipients
Body
Attachments
ReceivedTime
ConversationId

---

# 19. Email Entity Ownership

Email belongs to:
Workspace
    |
EmailAccount
    |
Email

---

# 20. Duplicate Detection

Required checks:
Provider Message ID
Account ID

Example:
Gmail MessageId + EmailAccountId

Unique constraint:
IX_Email_ProviderMessageId

---

# 21. Attachment Handling

Rules:
Do not store huge files in database.

Store:
Metadata: Database
Binary: Object Storage

Example:
S3
Azure Blob
MinIO

---

# 22. Email Processing Pipeline

After save:

EmailReceived Event
        |
        v
AI Processing
        |
        v
Search Indexing
        |
        v
Notification

---

# 23. Domain Events

Events:
EmailReceived
EmailUpdated
EmailDeleted
AttachmentReceived

---

# 24. Error Handling

Provider errors:

Examples:
TokenExpired
RateLimited
NetworkFailure
InvalidPermission

Actions:
Retry
Refresh Token
Pause Sync
Notify User

---

# 25. Retry Policy

Technology:
Polly

Strategies:
Retry
Exponential Backoff
Circuit Breaker

---

# 26. Rate Limit Handling

Provider limits:
Gmail API quota
Microsoft throttling

Solution:
Queue throttling

---

# 27. Security

Required:
OAuth tokens encrypted
Least privilege permission
Audit synchronization
No email content logging

---

# 28. Sync Performance Target

Goals:
Initial sync: 1000 emails/minute
Incremental sync: < 3 seconds
Memory: Stable under worker load

---

# 29. Scaling Strategy

Stage 1:
Single Email Worker

Stage 2:
Multiple workers
RabbitMQ partitioning

Stage 3:
Provider-specific workers

---

# 30. Monitoring

Metrics:
sync_success_total
sync_failed_total
sync_duration
emails_processed
provider_latency

---

# 31. Testing Strategy

Unit test:
Parser
Normalizer
Deduplicator

Integration:
Gmail sandbox
Microsoft test tenant

---

# 32. Failure Recovery

System must handle:
Worker crash
Network failure
Provider outage
Database failure

Resume from:
SyncState cursor

---

# 33. Future Improvements

Add:
AI pre-filtering
Smart sync priority
Real-time push sync
Email threading intelligence

---

# 34. Final Rules

Rule 1: Never depend directly on provider API.
Rule 2: Every provider uses adapter.
Rule 3: Sync must be idempotent.
Rule 4: Email events drive AI processing.
Rule 5: Sync state enables recovery.
Rule 6: Email content is protected data.

---

# End Of Document
