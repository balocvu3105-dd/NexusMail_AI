# NexusMail AI
# Background Worker Design Document

Version:
1.0.0

Status:
Approved

Document Type:
Background Processing Architecture

Technology:
.NET 8 Worker Service
RabbitMQ
Quartz.NET
Redis
Docker

---

# 1. Overview

Background Worker system handles asynchronous processing.

Responsibilities:
- Email synchronization
- AI processing
- Notification delivery
- Automation execution
- Scheduled tasks

---

# 2. Architecture Overview

            API
             |
             |
             v
          RabbitMQ
             |
    +--------+--------+
    |        |        |
    v        v        v
Email AI Notification
Worker Worker Worker

---

# 3. Worker Projects

Solution:
workers/
├── NexusMail.Worker.EmailSync
├── NexusMail.Worker.AI
├── NexusMail.Worker.Notification
└── NexusMail.Worker.Automation

---

# 4. Worker Responsibilities

## EmailSync Worker
Handles:
- Provider synchronization
- Token refresh
- Email download
- Email normalization

Consumes:
email.sync.request

---

## AI Worker
Handles:
- Classification
- Summary
- Embedding
- Risk analysis

Consumes:
ai.email.process

---

## Notification Worker
Handles:
- Push notification
- Discord
- Slack
- Teams

Consumes:
notification.send

---

# 5. Worker Lifecycle

Every worker follows:

Start
|
Initialize Dependency
|
Connect Queue
|
Health Check
|
Consume Message
|
Process
|
Acknowledge
|
Repeat
|
Shutdown Gracefully

---

# 6. Worker Base Architecture

Structure:
Worker Project/
├── Workers/
├── Consumers/
├── Jobs/
├── Services/
├── Handlers/
├── HealthChecks/
└── DependencyInjection.cs

---

# 7. .NET Worker Implementation

Technology:
BackgroundService

Example:
```csharp
public class EmailSyncWorker : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
    }
}
```

---

# 8. Message Queue Architecture

Technology:
RabbitMQ

Exchange:
nexusmail.events

---

# 9. Queue Naming Convention

Pattern:
{module}.{action}

Examples:
email.sync.request
email.received
ai.process.email
notification.send
automation.execute

---

# 10. Message Contract

All messages contain:
```json
{
 "MessageId": "uuid",
 "EventType": "EmailReceived",
 "OccurredAt": "datetime",
 "WorkspaceId": "uuid",
 "Payload": {}
}
```

---

# 11. Message Processing Rules

Every consumer must:
1. Validate message
2. Process business action
3. Save state
4. Publish result
5. Acknowledge

---

# 12. Idempotency Requirement

Messages can be delivered multiple times.
Workers must handle duplicate messages.

Example:
MessageId
    |
Check ProcessedMessages
    |
Already exists?
    |
Skip

---

# 13. Processed Message Storage

Table:
ProcessedMessage

Fields:
Id
MessageId
ProcessedAt

---

# 14. Retry Strategy

Technology:
Polly

Retry:
Failure
|
Retry 1
|
Retry 2
|
Retry 3
|
Dead Letter Queue

---

# 15. Dead Letter Queue

Purpose:
Store failed messages.

Queues:
email.failed
ai.failed
notification.failed

---

# 16. Failure Classification

Temporary:
- Network error
- Provider timeout
- Database unavailable
Retry.

Permanent:
- Invalid data
- Permission denied
- Corrupted message
Move to DLQ.

---

# 17. Scheduled Jobs

Technology:
Quartz.NET

Used for:
- Daily sync
- Cleanup
- Reports
- Billing tasks

---

# 18. Scheduler Architecture

Quartz Scheduler
    |
    v
Job Queue
    |
    v
Worker

---

# 19. Email Sync Scheduling

Example:
Every account:
LastSyncTime
SyncInterval

Generate:
email.sync.request

---

# 20. AI Processing Scheduling

Normal:
Event driven.

Additional:
Batch processing.

Example:
Reprocess failed AI jobs.

---

# 21. Notification Processing

Flow:
Business Event
    |
    v
Notification Queue
    |
    v
Notification Worker
    |
    +---- Discord
    +---- Slack
    +---- Mobile

---

# 22. Graceful Shutdown

Worker shutdown:
Receive stop signal
    |
Stop consuming
    |
Finish current job
    |
Close connection
    |
Exit

---

# 23. Health Check

Every worker exposes:
/health

Checks:
- RabbitMQ connection
- Database
- External API

---

# 24. Worker Observability

Metrics:
jobs_processed_total
jobs_failed_total
job_duration
queue_length
retry_count

---

# 25. Logging Standard

Every job log:

Required:
JobId
MessageId
WorkspaceId
Duration

---

# 26. Distributed Tracing

Support:
OpenTelemetry

Trace:
API Request
|
Event Publish
|
Worker Consume
|
AI Processing

---

# 27. Resource Management

Worker must:
- Limit concurrency
- Respect provider rate limit
- Avoid memory growth

---

# 28. Concurrency Strategy

Configuration:
MaxConcurrentJobs

Example:
AI Worker:
10 jobs

---

# 29. Worker Scaling

Stage 1: Single instance
Stage 2: Multiple replicas
Stage 3: Kubernetes Horizontal Scaling

---

# 30. Container Deployment

Each worker: Own Docker container.

Example:
nexusmail-api
nexusmail-email-worker
nexusmail-ai-worker
nexusmail-notification-worker

---

# 31. Security

Workers require:
- Service identity
- Minimum database permission
- Secret management
- Encrypted communication

---

# 32. Testing Strategy

Unit Test:
- Handler
- Retry logic
- Message validation

Integration Test:
- RabbitMQ
- Database
- External provider

---

# 33. Future Worker Types

Possible additions:
Calendar Worker
Document AI Worker
CRM Worker
Agent Worker

---

# 34. Final Rules

Rule 1: Never execute heavy jobs inside API.
Rule 2: Every async process uses queue.
Rule 3: Every message is idempotent.
Rule 4: Every worker is independently scalable.
Rule 5: Failures must be recoverable.
Rule 6: Everything must be observable.

---

# End Of Document
