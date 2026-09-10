# NexusMail AI
# Email Sync Implementation Document

Version:
1.0.0

Status:
Implementation Ready

Document Type:
Email Synchronization Implementation

Technology:
.NET 8 Worker Service
Gmail API
Microsoft Graph API
IMAP
Entity Framework Core
RabbitMQ

---

# 1. Implementation Goal

This document defines:
- Email provider connection
- Sync service
- Email normalization
- Message processing
- Event publishing
- Error handling

---

# 2. Project Location

Worker:
backend/src/NexusMail.Worker.EmailSync

Infrastructure:
backend/src/NexusMail.Infrastructure.Email

---

# 3. Email Sync Architecture

Email Provider
   |
   v
IEmailProvider
   |
   v
EmailSyncService
   |
   v
EmailNormalizer
   |
   v
EmailRepository
   |
   v
Domain Event Publisher

---

# 4. Provider Abstraction

All providers implement:
```csharp
public interface IEmailProvider
{
    string ProviderName { get; }
    Task<SyncResult> SyncAsync(EmailAccount account, CancellationToken cancellationToken);
}
```

---

# 5. Provider Implementations

Structure:
EmailProviders/
├── Gmail/
├── Microsoft/
└── Imap/

---

# 6. Gmail Provider

Class:
GmailEmailProvider

Uses:
Google.Apis.Gmail.v1

Responsibilities:
Authenticate
Fetch messages
Download content
Handle history cursor

---

# 7. Gmail Authentication

Required scopes:
gmail.readonly

Never request:
gmail.modify
unless feature requires.

---

# 8. Microsoft Provider

Class:
MicrosoftGraphEmailProvider

Uses:
Microsoft.Graph SDK

Capabilities:
Mail folders
Messages
Delta sync

---

# 9. IMAP Provider

Class:
ImapEmailProvider

Library:
MailKit

Supports:
Gmail IMAP
Custom mail server
Exchange IMAP

---

# 10. Email Account Entity

Domain:
EmailAccount

Properties:
Id
WorkspaceId
Provider
EmailAddress
Status
LastSyncAt

---

# 11. Provider Credential Storage

Entity:
EmailProviderCredential

Fields:
Id
EmailAccountId
EncryptedAccessToken
EncryptedRefreshToken
ExpiresAt

---

# 12. Token Encryption

Rule:
Never store: access_token, refresh_token raw.
Use: AES-256
Key source: Environment Secret, Vault

---

# 13. Sync Service

Interface:
```csharp
public interface IEmailSyncService
{
    Task ExecuteSyncAsync(Guid emailAccountId, CancellationToken token);
}
```

---

# 14. Sync Flow

Implementation:

Load EmailAccount
        |
Validate Credential
        |
Get Provider Adapter
        |
Sync Messages
        |
Normalize
        |
Check Duplicate
        |
Save Email
        |
Publish Event

---

# 15. Email Normalizer

Interface:
IEmailNormalizer

Purpose:
Convert: Gmail Message, Outlook Message, IMAP Message
into: NexusMail.Email

---

# 16. Normalized Email Model

```csharp
public class EmailMessageDto
{
    public string ProviderId {get;set;}
    public string Subject {get;set;}
    public string Body {get;set;}
    public string Sender {get;set;}
    public DateTime ReceivedAt {get;set;}
}
```

---

# 17. Duplicate Detection

Before insert:
Check: EmailAccountId + ProviderMessageId
Database: Unique Index.

---

# 18. Repository Implementation

Interface:
IEmailRepository

Methods:
```csharp
Task<bool> ExistsAsync(string providerId);
Task AddAsync(Email email);
```

---

# 19. Sync Cursor

Entity:
EmailSyncState

Properties:
EmailAccountId
Cursor
LastSyncTime

---

# 20. Gmail Cursor

Store:
HistoryId

---

# 21. Microsoft Cursor

Store:
DeltaToken

---

# 22. IMAP Cursor

Store:
UID

---

# 23. Worker Implementation

Class:
EmailSyncWorker

Inheritance:
BackgroundService

---

# 24. Queue Consumer

Queue:
email.sync.request

Message:
```json
{
 "EmailAccountId": "uuid"
}
```

---

# 25. Sync Job Handler

Flow:
Receive Message
       |
Execute Sync
       |
Save Result
       |
Acknowledge

---

# 26. Error Handling

Errors:
TokenExpired
RateLimit
NetworkFailure
PermissionDenied

---

# 27. Retry Policy

Using:
Polly

Strategy:
Retry
Exponential Backoff
Circuit Breaker

---

# 28. Rate Limit Handling

Provider limits:
Handled by:
Queue Delay
Provider Throttle

---

# 29. Email Attachment Processing

Rules:
Database: Metadata only.
Storage: S3, Azure Blob, MinIO

---

# 30. Email Threading

Future:
Support: ConversationId, InReplyTo, References

---

# 31. Domain Event Publishing

After save:
Publish: EmailReceivedEvent

Message:
```json
{
"EmailId": "",
"WorkspaceId": ""
}
```

---

# 32. Logging

Required:
EmailAccountId
Provider
MessageCount
Duration
Result

Never log:
Email Body
Attachment Content

---

# 33. Testing

Unit:
Parser
Normalizer
Duplicate checker

Integration:
Gmail sandbox
Microsoft test tenant
IMAP server

---

# 34. Performance Target

Initial sync:
1000 emails/minute

Incremental:
< 3 seconds

---

# 35. Security Checklist

[x] OAuth encrypted
[x] Least privilege scope
[x] No email content logging
[x] Tenant isolation
[x] Audit sync activity

---

# 36. Completion Checklist

[x] Provider abstraction
[x] Gmail adapter
[x] Microsoft adapter
[x] IMAP adapter
[x] Sync worker
[x] Repository
[x] Event publishing
[x] Retry handling

---

# 37. Final Rules

Rule 1: Every provider uses adapter.
Rule 2: Sync is incremental.
Rule 3: Duplicate email is impossible.
Rule 4: Email events trigger intelligence.
Rule 5: Provider failure never breaks system.

---

# End Of Document
