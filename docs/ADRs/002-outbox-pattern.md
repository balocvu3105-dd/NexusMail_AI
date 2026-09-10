# ADR-002: Transactional Outbox Pattern

**Status:** Approved  
**Date:** 2026-08-03  
**Sprint:** 3.1 – Synchronization Infrastructure  
**Deciders:** Engineering Lead

---

## Context

When an `EmailAccount` is connected (via `ConnectEmailAccountCommand`), a domain event (`EmailAccountConnectedEvent`) is raised. This event must trigger a background synchronization process.

A naïve approach — dispatching the event inline within the HTTP request — introduces a **dual-write problem**: the database transaction may commit while the message dispatch fails (or vice versa), leaving the system in an inconsistent, hard-to-recover state.

Furthermore, the synchronization process itself involves external API calls (Gmail API, Microsoft Graph) that are slow and failure-prone. These must never block the user-facing HTTP response.

---

## Decision

We implement a **Custom Transactional Outbox Pattern** with the following guarantees:

### Write Path (Transactional)

Domain events are intercepted by `ConvertDomainEventsToOutboxMessagesInterceptor` (an EF Core `SaveChangesInterceptor`) and persisted into the `OutboxMessages` table **within the same database transaction** as the domain entity change. This eliminates the dual-write problem.

```
HTTP Request → Domain.EmailAccount.Connect() → Raises DomainEvent
     ↓
EF Core SaveChanges
     ↓ (intercepted by ConvertDomainEventsToOutboxMessagesInterceptor)
BEGIN TRANSACTION
  INSERT INTO EmailAccounts ...
  INSERT INTO OutboxMessages ...
COMMIT
```

### Dispatch Path (Background Worker)

A dedicated `OutboxWorker` (hosted service) polls `OutboxMessages` on a configurable interval. The dispatcher implements a **Claim-Process-Complete** lease mechanism:

1. **Claim** – `UPDATE ... SET LockId = <workerId>, LockedUntilUtc = NOW() + LeaseTimeout WHERE ... FOR UPDATE SKIP LOCKED` — atomic, supports horizontal scaling
2. **Process** – Deserialize the event and dispatch via `IPublisher` (MediatR)
3. **Complete** – On success: mark `ProcessedOnUtc`. On failure: increment `Attempts`, optionally dead-letter after `MaxAttempts`

### Failure Handling

| Condition | Behavior |
|---|---|
| Handler exception | Increment `Attempts`, record `Error` + `FailureReason = HandlerException` |
| Deserialization failure | Mark `FailureReason = JsonSerialization`, skip re-dispatch |
| `Attempts >= MaxAttempts` | Set `DeadLetteredAt`, stop retry |
| Expired lease (crash recovery) | Next worker run re-claims the message |

---

## Alternatives Considered

| Alternative | Why Rejected |
|---|---|
| **Direct inline MediatR dispatch** | Blocks HTTP request; vulnerable to external API downtime; no retry |
| **MassTransit Transactional Outbox** | Couples Application Layer to messaging framework; strong vendor lock-in; harder to debug in early stages |
| **RabbitMQ / Azure Service Bus** | Premature infrastructure complexity; adds operational overhead before product-market fit |

The custom implementation provides clean separation: `IOutboxStore` and `IOutboxDispatcher` are Application-layer abstractions. The PostgreSQL + EF Core implementation is Infrastructure detail. This makes a future migration to MassTransit seamless.

---

## Consequences

### Pros
- **At-least-once delivery** — eliminates dual-write problem
- **Horizontal scalability** — multiple workers via `FOR UPDATE SKIP LOCKED`
- **Crash recovery** — expired leases are automatically reclaimed
- **Dead letter support** — exhausted messages are quarantined, not silently dropped
- **Strict Clean Architecture** — messaging framework is an implementation detail
- **Observable** — `CorrelationId`, `FailureReason`, `Attempts`, `Error` provide full audit trail

### Cons
- Requires custom polling logic (mitigated by clean abstraction)
- At-least-once delivery means **all event handlers must be strictly idempotent**
- Polling introduces a small delivery latency (configurable, default 5s)

---

## Non-Goals

The following are **explicitly out of scope** for this sprint and should not be implemented before these decisions are revisited:

- **Exactly-once delivery** — not achievable without distributed transactions; idempotency is the correct solution
- **Priority queues** — all messages are treated as equal priority; ordering is by `OccurredOnUtc`
- **Fan-out / pub-sub topology** — each `OutboxMessage` maps to exactly one event type; multi-subscriber routing is not implemented
- **Message schema versioning** — not required yet; type is stored as `AssemblyQualifiedName`; will need a versioning strategy before introducing breaking event schema changes
- **Metrics / alerting on dead letters** — monitoring integration (Prometheus, OpenTelemetry) is deferred to Sprint 4 (Observability)

---

## Future Evolution

The current design is deliberately **framework-agnostic**. The migration path to a full messaging infrastructure is:

```
Current (Sprint 3.1)          Future (Sprint N)
─────────────────────         ─────────────────────────────────────────
IOutboxDispatcher             → IMessageBus (MassTransit / Rebus)
OutboxWorker (polling)        → Event-driven consumer (RabbitMQ / ASB)
PostgreSQL OutboxMessages     → Broker-native deduplication / idempotency
MediatR IPublisher            → ISendEndpoint / IPublishEndpoint
```

Key migration principles:
1. Replace `IOutboxDispatcher` implementation — Application Layer is unchanged
2. Retain the Write Path (EF Core interceptor) — the transactional guarantee remains
3. Introduce broker-level idempotency keys before retiring the custom dead-letter logic
4. Keep `OutboxMessages` table as an audit trail even after migrating dispatch to broker

---

## Test Coverage

The following integration tests verify the production hardening guarantees:

| Test | Scenario | Verified |
|---|---|---|
| `ConcurrentWorkers_ShouldProcessMessageOnlyOnce` | Two workers race for the same message | ✅ |
| `CrashRecovery_ShouldReclaimExpiredLeases` | Message with expired lease is re-processed | ✅ |
| `DeadLetter_ShouldMarkAsDeadLetter_AfterMaxAttempts` | Message exhausted retries is dead-lettered | ✅ |
| `Idempotency_ShouldNotProcessSameEventTwice` | Handler deduplication via `EventId` | ✅ |
| `BatchProcessing_ShouldNotMissMessages` | 100 messages processed in 5 batches of 20 | ✅ |
