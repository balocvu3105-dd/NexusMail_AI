# ADR 0008: Authentication Architecture

## Status
Accepted

## Context
As NexusMail AI is an enterprise-oriented multi-tenant SaaS application, robust authentication and authorization mechanisms are critical. In Sprint 2, we moved away from simple CRUD towards a resilient, extensible architecture for Identity and Session Management. We need a system that supports standard Token-based authentication while retaining the ability to forcefully invalidate sessions, implement rotation, and handle multi-device logouts effectively.

The typical pattern of only storing basic user information and a long-lived JWT is insufficient because we cannot revoke JWTs before they expire without a complex blacklist implementation. Furthermore, we need to track device/IP/UserAgent metadata for security auditing.

## Decision
We have decided to implement a dual-token (Access + Refresh Token) Authentication Architecture with the following key components and decisions:

### 1. `UserSession` as an Independent Aggregate Root
- Instead of modeling `UserSession` as an Entity within the `User` Aggregate, we have designed `UserSession` as a completely independent Aggregate Root.
- **Reasoning**: `User` and `UserSession` change for fundamentally different reasons (Single Responsibility Principle at the Domain level). User lifecycle events involve profile changes or account locks, whereas UserSession lifecycle events involve logins, token refreshes, and logouts.
- A User can have multiple active sessions simultaneously. `UserSession` stores a reference to `UserId`.

### 2. Refresh Token Storage & Rotation
- Refresh tokens are issued as opaque cryptographically secure random strings.
- **Security**: The plaintext refresh token is returned to the client once. We store only the **SHA-256 Hash** of the refresh token in the `UserSession` table.
- **Rotation**: On every successful refresh, a new refresh token is issued, and the session's hash is updated (Refresh Token Rotation). This limits the lifespan of a stolen refresh token.

### 3. JWT and Transport Agnostic Application
- JWTs are strictly handled in the `Infrastructure` and `API` layers. The `Application` layer does not know about "JWTs", "Headers", or "Cookies".
- `ICurrentUser` abstracts away identity resolution (`UserId`, `IsAuthenticated`).
- `IWorkspaceContext` abstracts away the active workspace resolution (e.g., from the `X-Workspace-Id` header).
- **Reasoning**: This protects our Application layer from changes in transport mechanisms. If we switch from headers to subdomain-based routing (e.g., `tenantA.nexusmail.ai`) in the future, the core domain logic will remain untouched.

### 4. Application Rate Limiting
- We have introduced `IdentityRateLimitOptions` using the Options Pattern to configure rate limits for sensitive endpoints (`/api/v1/identity/login` and `/api/v1/identity/refresh-token`).
- In-memory rate limiting is used initially, but abstraction is kept clean to allow a seamless migration to Redis in a distributed deployment.

## Consequences
- **Positive**: We gain full control over session lifecycles, enabling features like "Log out of all devices".
- **Positive**: Hardened security against token theft via refresh token hashing and rotation.
- **Positive**: Clear separation of concerns; Application layer tests do not require complex HTTP context mocks.
- **Negative**: Increased complexity in the authentication flow and the need for database round-trips during token refreshes.

## References
- [OAuth 2.0 Security Best Current Practice (Refresh Token Rotation)](https://datatracker.ietf.org/doc/html/draft-ietf-oauth-security-topics-15)
