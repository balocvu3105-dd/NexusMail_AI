# NexusMail AI
# Identity Implementation Document

Version:
1.0.0

Status:
Implementation Ready

Document Type:
Identity & Security Implementation

Technology:
ASP.NET Core Identity
JWT Bearer
OAuth 2.0
OpenID Connect
Entity Framework Core

---

# 1. Identity Implementation Goal

This document defines:
- User authentication
- OAuth integration
- JWT token system
- Refresh token system
- Role management
- Permission authorization

---

# 2. Identity Project

Location:
backend/src/NexusMail.Identity

Responsibilities:
Authentication
Authorization
Token Management
OAuth
Security

---

# 3. Identity Architecture

Client
|
v
NexusMail.API
|
v
Identity Service
|
+----------------+
|                |
v                v
Database    OAuth Provider

---

# 4. Required Packages

Install:
Microsoft.AspNetCore.Identity.EntityFrameworkCore
Microsoft.AspNetCore.Authentication.JwtBearer
Microsoft.IdentityModel.Tokens
Microsoft.AspNetCore.Authentication.Google
Microsoft.AspNetCore.Authentication.MicrosoftAccount

---

# 5. Identity User Model

Location:
Identity/Entities/ApplicationUser.cs

Implementation:
```csharp
public class ApplicationUser : IdentityUser<Guid>
{
    public string DisplayName { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

---

# 6. Identity Database Context

Location:
IdentityDbContext.cs

Implementation:
```csharp
public class IdentityDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public IdentityDbContext(DbContextOptions options) : base(options)
    {
    }
}
```

---

# 7. Identity Configuration

Program.cs:
```csharp
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole<Guid>>()
    .AddEntityFrameworkStores<IdentityDbContext>()
    .AddDefaultTokenProviders();
```

---

# 8. User Registration Flow

Flow:
Register Request
        |
Validate Email
        |
Create User
        |
Assign Default Role
        |
Create Workspace
        |
Generate Token

---

# 9. Register DTO

Location:
Contracts/Auth/RegisterRequest.cs

Example:
```csharp
public record RegisterRequest
(
    string Email,
    string Password,
    string DisplayName
);
```

---

# 10. Registration Service

Interface:
IAuthenticationService

Methods:
RegisterAsync()
LoginAsync()
RefreshTokenAsync()
LogoutAsync()

---

# 11. Login Flow

User
 |
Email Password
 |
Identity Validation
 |
Generate JWT
 |
Return Tokens

---

# 12. JWT Architecture

Access Token:
Contains:
{
"userId": "",
"workspaceId": "",
"roles": [],
"permissions": [],
"exp": 1234567890
}

---

# 13. JWT Configuration

Options:
```csharp
public class JwtOptions
{
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public string SecretKey { get; set; }
    public int ExpirationMinutes { get; set; }
}
```

---

# 14. JWT Service

Location:
Services/JwtTokenService.cs

Interface:
IJwtTokenService

Methods:
GenerateAccessToken()
GenerateRefreshToken()

---

# 15. Refresh Token Model

Entity:
RefreshToken

Properties:
Id
UserId
TokenHash
ExpiresAt
CreatedAt
RevokedAt

---

# 16. Refresh Token Security

Rules:
Never store: Raw Token
Store: SHA256 Hash

---

# 17. Refresh Flow

Refresh Token
 |
Hash Token
 |
Compare Database
 |
Generate New JWT
 |
Rotate Token

---

# 18. Logout Implementation

Action:
Invalidate Refresh Token
Remove Session
Audit Event

---

# 19. OAuth Architecture

Supported:

Phase 1:
Google
Microsoft

Future:
GitHub
Azure AD
SAML

---

# 20. Google OAuth Setup

Configuration:
```json
{
  "Google": {
    "ClientId": "",
    "ClientSecret": ""
  }
}
```

---

# 21. OAuth Login Flow

User
 |
Google Login
 |
Callback
 |
Find External Account
 |
Create User If Needed
 |
Generate JWT

---

# 22. External Login Entity

Table:
external_logins

Columns:
Id
UserId
Provider
ProviderKey

---

# 23. Role System

Roles:
SystemAdmin
OrganizationOwner
WorkspaceAdmin
Member
Guest

---

# 24. Role Seed

Development seed:
SystemAdmin
WorkspaceAdmin
Member

---

# 25. Permission System

Permission Entity:
Permission

Example:
EMAIL.READ
EMAIL.DELETE
RULE.CREATE
AUDIT.READ
APIKEY.CREATE

---

# 26. User Permission Flow

JWT
 |
User
 |
Roles
 |
Permissions
 |
Authorize

---

# 27. Permission Attribute

Example:
```csharp
[RequirePermission("EMAIL.READ")]
public IActionResult GetEmails()
```

---

# 28. Authorization Handler

Location:
Authorization/PermissionHandler.cs

Responsibilities:
Read permission requirement
Validate user claims
Allow/Deny

---

# 29. Workspace Context

Interface:
IWorkspaceContext

Contains:
UserId
WorkspaceId
Roles
Permissions

---

# 30. Authentication Middleware Pipeline

Order:
Exception
 ↓
Authentication
 ↓
Authorization
 ↓
Controller

---

# 31. Security Rules

Required:
[x] Password hashing
[x] JWT validation
[x] Refresh rotation
[x] OAuth validation
[x] Permission checking
[x] Audit logging

---

# 32. Password Policy

Default:
Minimum: 8 characters
Require: Uppercase, Lowercase, Number

---

# 33. Account Security

Implement:
Login attempt tracking
Rate limiting
Account lockout

---

# 34. Audit Events

Events:
UserRegistered
UserLoggedIn
UserLoggedOut
FailedLoginAttempt
TokenRevoked

---

# 35. Identity Testing

Unit:
Token generation
Permission validation
Password rules

Integration:
Register
Login
OAuth callback
Refresh token

---

# 36. Environment Configuration

Secrets:
Never store: appsettings.json
Use: Environment Variable, Secret Manager, Vault

---

# 37. Completion Checklist

[x] Identity User
[x] Identity Database
[x] JWT
[x] Refresh Token
[x] OAuth
[x] RBAC
[x] Permission
[x] Workspace Context
[x] Tests

---

# 38. Final Identity Rules

Rule 1: Never expose password.
Rule 2: Access token is short lived.
Rule 3: Refresh token is rotated.
Rule 4: Authorization happens after authentication.
Rule 5: Workspace boundary is mandatory.
Rule 6: Every security action is audited.

---

# End Of Document
