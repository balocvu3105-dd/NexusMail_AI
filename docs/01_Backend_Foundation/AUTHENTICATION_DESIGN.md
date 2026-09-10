# NexusMail AI
# Authentication Design Document

Version:
1.0.0

Status:
Approved

Document Type:
Identity & Access Architecture

Technology:
ASP.NET Core Identity
OAuth 2.0
OpenID Connect
JWT

---

# 1. Authentication Overview

NexusMail AI uses a hybrid identity architecture.

Supported authentication:

Phase 1:
- Google OAuth
- Microsoft OAuth
- JWT Session

Future:
- GitHub OAuth
- Enterprise SSO
- SAML
- LDAP
- MFA

---

# 2. Identity Responsibilities

Identity system manages:
- User account
- Login provider
- Session
- Token
- Roles
- Permissions
- Workspace membership

Identity does NOT manage:
- Email provider token
- Email synchronization
- AI permission

---

# 3. Authentication Architecture

Client
|
|
v
NexusMail API
|
|
v
Identity Service
|
+----------------+
|                |
v                v
OAuth Provider Database

---

# 4. Identity Components

NexusMail.Identity
├── Authentication
├── Authorization
├── OAuth
├── Tokens
├── Sessions
├── Permissions
└── Security

---

# 5. User Identity Model

Entity:
User

Properties:
Id
Email
DisplayName
AvatarUrl
Status
CreatedAt

---

# 6. External Login Model

Entity:
OAuthAccount

Properties:
Id
UserId
Provider
ProviderUserId
CreatedAt

Example:
User: Bá Lộc
Connected: Google, Microsoft

---

# 7. Login Flow

Example Google Login:

User
|
v
GET /auth/google/login
|
v
Google OAuth
|
v
Callback
|
v
Validate Identity
|
v
Create / Update User
|
v
Issue JWT

---

# 8. OAuth Rules

Never store:
- OAuth password
- Client secret in database

Store:
Provider information only.

Secrets:
Environment / Secret Manager

---

# 9. JWT Architecture

JWT contains:
UserId
WorkspaceId
Role
PermissionScope
IssuedAt
Expiration

---

# 10. Access Token

Purpose:
API authentication.

Lifetime:
Short.

Recommended:
15-30 minutes

Stored:
Client memory

---

# 11. Refresh Token

Purpose:
Generate new access token.

Lifetime:
Long.

Example:
30 days

Stored:
Database hashed form.

---

# 12. Refresh Token Entity

Table:
refresh_tokens

Fields:
Id
UserId
TokenHash
ExpiresAt
CreatedAt
RevokedAt

---

# 13. Refresh Token Rotation

Every refresh:

Old Token
  |
  v
Invalidate
  |
  v
Create New Token

Purpose:
Prevent token theft.

---

# 14. Token Revocation

Token revoked when:
- Logout
- Password change
- Suspicious activity
- Admin action

---

# 15. Logout Flow

Client
|
Send logout
|
API
|
Revoke refresh token
|
Session invalid

---

# 16. Authorization Architecture

Authorization uses:
RBAC
+
Permission Based Access Control

---

# 17. Role Model

Roles:
Organization Owner
Workspace Admin
Manager
Member
Guest

---

# 18. Permission Model

Permission examples:

User:
USER.READ
USER.INVITE
USER.REMOVE

Email:
EMAIL.READ
EMAIL.DELETE
EMAIL.ARCHIVE

Automation:
RULE.CREATE
RULE.UPDATE

Administration:
AUDIT.READ
APIKEY.CREATE

---

# 19. Authorization Flow

Every request:

JWT
|
v
User
|
v
Workspace
|
v
Permission Check
|
v
Resource Access

---

# 20. Workspace Context

Every request creates:

WorkspaceContext
{
UserId
WorkspaceId
Role
Permissions
}

Available through:
IWorkspaceContext

---

# 21. Authentication Middleware Pipeline

Order:

Exception Middleware
    ↓
Logging Middleware
    ↓
Authentication Middleware
    ↓
Authorization Middleware
    ↓
Controller

---

# 22. Email Provider Authentication

Separate from user authentication.

Example:
User Login: Google OAuth
Email Access: Gmail OAuth Permission

---

# 23. Email OAuth Token Storage

Entity:
EmailProviderCredential

Fields:
Id
EmailAccountId
EncryptedAccessToken
EncryptedRefreshToken
ExpiresAt

Rules:
Encrypted only.

---

# 24. Encryption Strategy

Sensitive tokens:
Encrypted at rest.

Encryption key:
External secret manager.

Never:
Plain database storage.

---

# 25. MFA Roadmap

Future support:

Methods:
- TOTP
- Authenticator App
- Hardware Key

---

# 26. Enterprise SSO Roadmap

Future:

Supported:
- SAML 2.0
- OpenID Connect
- Azure AD

---

# 27. Security Requirements

Must implement:
[x] JWT validation
[x] Token expiration
[x] Refresh rotation
[x] Permission check
[x] Tenant isolation
[x] Audit login events

---

# 28. Login Audit Events

Events:
UserLoggedIn
UserLoggedOut
RefreshTokenCreated
RefreshTokenRevoked
FailedLoginAttempt

---

# 29. Failed Login Protection

Required:
- Rate limit
- Monitoring
- Temporary lock

---

# 30. API Security Rules

Protected endpoint:
[Authorize]

Permission:
[RequirePermission("EMAIL.READ")]

---

# 31. Identity Testing

Required tests:

Authentication:
- Login success
- Login failure
- Token expiration
- Refresh rotation

Authorization:
- Role validation
- Permission validation
- Tenant isolation

---

# 32. Implementation Package

Recommended:
ASP.NET Core Identity
Microsoft Authentication Library
JWT Bearer Authentication

---

# 33. Final Authentication Rules

Rule 1: Authentication identifies user.
Rule 2: Authorization protects resources.
Rule 3: Workspace is the security boundary.
Rule 4: Refresh tokens are never stored raw.
Rule 5: Email OAuth tokens are separated from login tokens.
Rule 6: Every security action is audited.

---

# End Of Document
