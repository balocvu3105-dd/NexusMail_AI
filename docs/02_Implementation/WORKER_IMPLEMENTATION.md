# NexusMail AI
# Worker Implementation Document

Version:
1.0.0

Status:
Implementation Ready

Document Type:
Background Processing Implementation

Technology:
.NET 8 Worker Service
BackgroundService
RabbitMQ
Quartz.NET
Polly
OpenTelemetry

---

# 1. Worker Implementation Goal

This document defines:
- Worker architecture
- Queue processing
- Background jobs
- Retry strategy
- Scheduling
- Monitoring
- Graceful shutdown

---

# 2. Worker Projects

Location:
backend/src/
├── NexusMail.Worker.EmailSync
├── NexusMail.Worker.AI
├── NexusMail.Worker.Notification
└── NexusMail.Worker.Automation

---

# 3. Worker Philosophy

API:
Handles requests.

Worker:
Handles long-running tasks.

Forbidden:
Controller
|
Send Email
|
Call AI
|
Wait 30 seconds

Required:
API
|
Queue
|
Worker

---

# 4. Worker Base Architecture

RabbitMQ
|
v
Message Consumer
|
v
Job Handler
|
v
Application Service
|
v
Domain Action

---

# 5. Worker Base Project Structure

Worker/
├── Consumers/
├── Handlers/
├── Jobs/
├── Services/
├── Health/
└── Configuration/

---

# 6. Worker Host Setup

Create:
```bash
dotnet new worker
```

Example:
```csharp
public class EmailSyncWorker : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while(!stoppingToken.IsCancellationRequested)
        {
            await ProcessAsync();
        }
    }
}
```

---

# 7. Worker Lifecycle

Startup:
Load Configuration
      |
Connect Dependencies
      |
Connect Queue
      |
Start Consumer

Shutdown:
Stop Consume
      |
Finish Current Job
      |
Release Resources

---

# 8. RabbitMQ Architecture

Exchange:
nexusmail.events

Queues:
email.sync.request
ai.email.process
notification.send
automation.execute

---

# 9. Message Contract

Location:
Shared/Messaging

Example:
```csharp
public record EmailSyncRequested
(
    Guid EmailAccountId
);
```

---

# 10. Message Flow

Example Email Sync:
Scheduler
 |
email.sync.request
 |
EmailSyncWorker
 |
EmailSyncService
 |
EmailReceivedEvent

---

# 11. RabbitMQ Consumer

Interface:
```csharp
public interface IMessageConsumer<T>
{
    Task ConsumeAsync(T message, CancellationToken token);
}
```

---

# 12. Consumer Implementation

Example:
```csharp
public class EmailSyncConsumer : IMessageConsumer<EmailSyncRequested>
{
    public async Task ConsumeAsync(EmailSyncRequested message, CancellationToken token)
    {
        await syncService.ExecuteSyncAsync(message.EmailAccountId, token);
    }
}
```

---

# 13. Retry Strategy

Library:
Polly

Policies:
Retry
Exponential Backoff
Circuit Breaker
Timeout

---

# 14. Retry Example

Attempt 1: 5 seconds
Attempt 2: 30 seconds
Attempt 3: 5 minutes

---

# 15. Dead Letter Queue

Failed messages:
main queue
      |
      v
dead-letter queue

Purpose:
Investigation
Replay
Debug

---

# 16. Job Scheduler

Technology:
Quartz.NET

Used for:
Daily sync
Cleanup
Reports
Billing

---

# 17. Scheduled Jobs

Examples:
EmailFullSyncJob
DailyAnalyticsJob
CleanupJob
SubscriptionCheckJob

---

# 18. Email Sync Schedule

Example:
Every 5 minutes

Process:
Find Active Accounts
        |
Create Sync Messages
        |
Worker Executes

---

# 19. AI Queue Processing

Queue:
ai.email.process

Consumer:
AIWorker

Flow:
EmailReceived
 |
Queue Message
 |
AI Processing
 |
AICompleted

---

# 20. Notification Worker

Queue:
notification.send

Responsibilities:
Desktop notification
Mobile push
Discord
Slack
Teams

---

# 21. Automation Worker

Queue:
automation.execute

Responsibilities:
Evaluate rule
Execute action
Record history

---

# 22. Worker Dependency Injection

Example:
```csharp
services.AddHostedService<EmailSyncWorker>();
```

---

# 23. Configuration

Example:
```json
{
  "RabbitMQ": {
    "Host": "localhost",
    "Port": 5672
  }
}
```

---

# 24. Health Check

Every worker exposes:
/health

Checks:
Database
RabbitMQ
Redis
External Provider

---

# 25. Logging

Required:
JobId
MessageId
TenantId
Duration
Result

Never log:
Email Body
Token
Password

---

# 26. Distributed Lock

Problem:
Multiple workers process same job.

Solution:
Redis Lock.

Example:
sync:user@email.com

---

# 27. Idempotency

Every job must support:
Execute once
or
Execute many times safely

Example:
Email insert: Unique ProviderMessageId

---

# 28. Graceful Shutdown

Worker must:
Stop receiving messages
Complete active tasks
Close connections

---

# 29. Scaling Strategy

10 users:
1 Worker

10000 users:
Multiple Worker Instances
RabbitMQ Load Balance

---

# 30. Monitoring

Metrics:
worker_jobs_total
worker_duration
worker_failure
queue_length

---

# 31. OpenTelemetry

Collect:
Trace
Metric
Log

Flow:
API
 |
RabbitMQ
 |
Worker
 |
Database

---

# 32. Testing

Unit:
Handler
Retry policy
Scheduler

Integration:
RabbitMQ Test Container
Worker execution

---

# 33. Security

Worker:
Must use:
Service Account
Least Privilege
Secret Management

---

# 34. Completion Checklist

[x] Worker projects
[x] Queue system
[x] Consumers
[x] Retry
[x] Scheduler
[x] DLQ
[x] Monitoring
[x] Graceful shutdown

---

# 35. Final Worker Rules

Rule 1: Workers never contain business rules.
Rule 2: Workers execute Application Services.
Rule 3: Every message is traceable.
Rule 4: Every job is retryable.
Rule 5: Failures must be recoverable.

---

# End Of Document
