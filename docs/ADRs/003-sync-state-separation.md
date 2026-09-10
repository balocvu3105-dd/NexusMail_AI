# ADR-003: EmailSynchronizationState Separation

**Status:** Approved  
**Date:** 2026-08-03  
**Sprint:** 3.1 – Synchronization Infrastructure  
**Deciders:** Engineering Lead

---

## Context

The initial `EmailAccount` entity design included synchronization tracking fields directly on the aggregate — properties such as `LastSyncAt`, `LastError`, `SyncEnabled`, and `Status`. This is natural for a first iteration, but it introduces a serious architectural problem as the system evolves.

### The Problem

`EmailAccount` has two distinct identities and lifecycles:

1. **Account Identity** — Who owns the account, which workspace it belongs to, what email address, which OAuth provider, what token, when was it created. This changes rarely and is managed by user actions (connect, update settings, disconnect).

2. **Synchronization Runtime** — Is the sync currently running? What stage is it at? When did it last attempt? What was the error? How many retries? What's the next retry time? This changes frequently and is driven by the background worker.

When both concerns live in the same entity, **every sync cycle creates a write conflict with user-facing operations**. In a high-concurrency scenario, a worker updating `LastSyncAt` on `EmailAccount` will conflict with an API request updating `SyncEnabled`. Both touch the same row, and PostgreSQL's row-level locking (or EF Core's optimistic concurrency via `xmin`) will force one to retry or fail.

Furthermore, the synchronization state has a fundamentally different read/write pattern:
- Account data: low write frequency, high read frequency
- Sync state: **very high write frequency** (updated on every stage transition), relatively low read frequency (mainly for monitoring/debugging)

Mixing them violates the **Single Responsibility Principle** at the data model level.

---

## Decision

We separate synchronization state into a dedicated aggregate: `EmailSynchronizationState`.

### Entity Design

```csharp
public sealed class EmailSynchronizationState : EntityBase<Guid>
{
    public Guid EmailAccountId { get; private set; }   // FK to EmailAccount
    public EmailSyncStatus Status { get; private set; }
    public EmailSyncStage CurrentStage { get; private set; }
    public int RetryCount { get; private set; }
    public DateTime? NextRetryAt { get; private set; }
    public DateTime? LastAttemptAt { get; private set; }
    public DateTime? LastSuccessAt { get; private set; }
    public string? LastError { get; private set; }
    public string? CorrelationId { get; private set; }
    public uint Version { get; private set; }          // Optimistic concurrency
}
```

### Ownership and Access Rules

| Operation | Actor | Table Written |
|---|---|---|
| Connect account | HTTP Request (User) | `EmailAccounts` |
| Update settings | HTTP Request (User) | `EmailAccounts` |
| Disconnect account | HTTP Request (User) | `EmailAccounts` |
| Begin sync attempt | Background Worker | `EmailSynchronizationStates` |
| Update sync stage | Background Worker | `EmailSynchronizationStates` |
| Record sync success | Background Worker | `EmailSynchronizationStates` |
| Record sync failure / retry | Background Worker | `EmailSynchronizationStates` |

The two tables are **never written to in the same transaction by the same actor**. This eliminates the write conflict entirely.

### Database Schema

```sql
-- EmailAccounts: identity, credentials, settings
CREATE TABLE "EmailAccounts" (
    "Id" UUID PRIMARY KEY,
    "WorkspaceId" UUID NOT NULL,
    "EmailAddress" TEXT NOT NULL,
    "Provider" INTEGER NOT NULL,
    "EncryptedAccessToken" TEXT NOT NULL,
    "EncryptedRefreshToken" TEXT,
    "Status" INTEGER NOT NULL,
    "SyncEnabled" BOOLEAN NOT NULL DEFAULT TRUE,
    "LastSyncAt" TIMESTAMP WITH TIME ZONE,         -- last known successful sync
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL,
    "UpdatedAt" TIMESTAMP WITH TIME ZONE NOT NULL
);

-- EmailSynchronizationStates: runtime sync tracking (worker-owned)
CREATE TABLE "EmailSynchronizationStates" (
    "Id" UUID PRIMARY KEY,
    "EmailAccountId" UUID NOT NULL REFERENCES "EmailAccounts"("Id") ON DELETE CASCADE,
    "Status" INTEGER NOT NULL,
    "CurrentStage" INTEGER NOT NULL,
    "RetryCount" INTEGER NOT NULL DEFAULT 0,
    "NextRetryAt" TIMESTAMP WITH TIME ZONE,
    "LastAttemptAt" TIMESTAMP WITH TIME ZONE,
    "LastSuccessAt" TIMESTAMP WITH TIME ZONE,
    "LastError" TEXT,
    "CorrelationId" TEXT,
    "xmin" OID NOT NULL                            -- PostgreSQL system column for optimistic concurrency
);
```

### Relationship

`EmailSynchronizationState` has a 1:1 relationship with `EmailAccount`, created when the account is connected and owned exclusively by the background worker thereafter. The API layer reads `EmailAccount` (for user-facing data) and optionally joins `EmailSynchronizationState` for monitoring/status displays.

---

## Stage Lifecycle

```
Pending
  └─▶ RefreshingToken
        └─▶ FetchingProfile
              └─▶ FetchingLabels
                    └─▶ FetchingMessages
                          └─▶ Mapping
                                └─▶ Persisting
                                      └─▶ Completed ✅
                                          or
                                          Failed ❌
```

The `CurrentStage` field provides granular observability into where in the pipeline a synchronization failure occurred, which is critical for:
- Debugging production issues without needing full log traces
- Implementing targeted retry strategies per stage
- Building a monitoring dashboard in future sprints

---

## Alternatives Considered

| Alternative | Why Rejected |
|---|---|
| **Keep sync state in `EmailAccount`** | Creates writer conflict between HTTP requests and background workers; violates SRP; harder to apply different concurrency strategies per concern |
| **Use a separate `SyncLog` table (append-only)** | Useful for audit trails, but doesn't replace mutable state needed for retry logic and current status queries |
| **Use Redis for sync state** | Adds infrastructure dependency; loses durability; sync state should survive worker restarts |
| **Use EF Core shadow properties on `EmailAccount`** | No real isolation — the same row is still written to by both actors |

---

## Consequences

### Pros
- **No write conflicts** between user requests and background workers
- **Independent concurrency strategies** per concern:
  - `EmailAccount` uses `xmin` optimistic concurrency for infrequent user updates
  - `EmailSynchronizationState` uses `xmin` for the worker's own version-gated updates
- **Clearer ownership** — API layer owns account identity; Worker owns sync runtime
- **Scalability** — `EmailSynchronizationStates` can be sharded, archived, or moved to a separate store independently
- **Granular observability** — `CurrentStage` enables precise failure attribution

### Cons
- Additional database table and EF Core aggregate to maintain
- Queries that need both account info and sync status require a JOIN
- `EmailSynchronizationState` must be created when `EmailAccount` is created (currently via `EmailAccountConnectedEventHandler`) — this coupling must be maintained

---

## Non-Goals

- **Sync state does not drive business logic** — it is observational only; the worker drives its own state machine independently
- **No user-facing mutations on `EmailSynchronizationState`** — users can enable/disable sync via `EmailAccount.SyncEnabled`, but the state itself is entirely worker-managed
- **No history / time-series** — `EmailSynchronizationState` is a mutable snapshot, not an event log. Historical sync logs are deferred to a future observability sprint

---

## Future Evolution

1. **Monitoring Dashboard** — `EmailSynchronizationState.CurrentStage` and `Status` can directly power a real-time sync monitoring UI without any schema changes
2. **Per-stage retry strategies** — When `Stage = RefreshingToken` and it fails, we may want to immediately mark the account as expired and notify the user, rather than applying generic exponential backoff
3. **Sync history table** — An append-only `EmailSyncHistory` table can be added alongside `EmailSynchronizationState` for historical analytics without disrupting the current mutable state design
4. **Multi-provider sync isolation** — If a single account is synced from multiple providers in the future, the 1:1 relationship can be relaxed to 1:N
