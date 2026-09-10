# NexusMail AI
# API Implementation Document

Version:
1.0.0

Status:
Implementation Ready

Document Type:
REST API Implementation

Technology:
ASP.NET Core 8
Minimal API / Controllers
MediatR
FluentValidation
Swagger/OpenAPI
JWT Bearer
OpenTelemetry

---

# 1. API Implementation Goal

This document defines:
- API architecture
- Endpoint structure
- Request pipeline
- Authentication
- Authorization
- Validation
- Error handling
- Documentation

---

# 2. API Project

Location:
backend/src/NexusMail.API

Responsibilities:
HTTP Handling
Authentication
Authorization
Request Mapping
Response Mapping
API Documentation

---

# 3. API Architecture

Client
|
HTTPS
|
API Gateway
|
Middleware Pipeline
|
Controllers
|
Application Commands
|
Domain Logic

---

# 4. API Rules

API MUST NOT:
- Access DbContext directly
- Modify Entity directly
- Execute AI call directly
- Send email directly

API MUST:
- Validate request
- Check authorization
- Call Application Service

---

# 5. Project Structure

NexusMail.API/
├── Controllers/
├── Endpoints/
├── Middleware/
├── Filters/
├── Contracts/
├── Extensions/
├── Configuration/
└── OpenAPI/

---

# 6. API Style Decision

Architecture:
REST API

Reason:
- Enterprise compatible
- Mobile friendly
- External integration ready

---

# 7. API Versioning

Format:
/api/v1/{resource}

Example:
GET /api/v1/emails

Future:
/api/v2/emails

---

# 8. API Response Standard

All responses:
```json
{
"success": true,
"data": {},
"error": null,
"timestamp": ""
}
```

---

# 9. Error Response

Example:
```json
{
"success": false,
"error": {
  "code": "EMAIL_NOT_FOUND",
  "message": "Email does not exist"
}
}
```

---

# 10. Controller Structure

Example:
Controllers/
AuthController
EmailController
WorkspaceController
RuleController
SearchController
BillingController

---

# 11. Authentication Controller

Route:
/api/v1/auth

Endpoints:
POST /register
POST /login
POST /refresh
POST /logout

---

# 12. Email Controller

Route:
/api/v1/emails

Endpoints:
GET /
GET /{id}
POST /archive
POST /tag
DELETE /{id}

---

# 13. Workspace Controller

Route:
/api/v1/workspaces

Endpoints:
GET /current
GET /members
POST /invite
DELETE /member/{id}

---

# 14. Search Controller

Route:
/api/v1/search

Endpoints:
GET /emails?q=keyword

Supports:
Keyword search
Semantic search
Hybrid search

---

# 15. AI Controller

Route:
/api/v1/ai

Endpoints:
GET /emails/{id}/summary
GET /emails/{id}/analysis

---

# 16. Rule Controller

Route:
/api/v1/rules

Endpoints:
GET
POST
PUT
DELETE

---

# 17. CQRS Pattern

Use:
MediatR

Flow:
Controller
 |
Command
 |
Handler
 |
Application Service
 |
Domain

---

# 18. Command Example

Request:
ArchiveEmailCommand
{
EmailId
}

Handler:
ArchiveEmailCommandHandler

---

# 19. Query Example

Example:
GetInboxQuery

Handler:
GetInboxQueryHandler

---

# 20. Validation Pipeline

Library:
FluentValidation

Pipeline:
Request
 |
Validator
 |
Handler

---

# 21. Validation Example

```csharp
public class LoginValidator : AbstractValidator<LoginRequest>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email).EmailAddress();
    }
}
```

---

# 22. Exception Middleware

Location:
Middleware/ExceptionMiddleware.cs

Responsibilities:
Catch:
DomainException
ValidationException
UnauthorizedException
InternalException

---

# 23. HTTP Status Mapping

Rules:
200 Success
201 Created
400 Validation Error
401 Unauthorized
403 Forbidden
404 Not Found
500 Server Error

---

# 24. Authentication Middleware

Pipeline:
Request
 |
Exception
 |
Logging
 |
Authentication
 |
Authorization
 |
Endpoint

---

# 25. Permission Authorization

Example:
```csharp
[RequirePermission("EMAIL.READ")]
```

---

# 26. Rate Limiting

ASP.NET Core:
Use: RateLimiter

Limits:
Free: 100 requests/minute
Pro: 1000 requests/minute
Enterprise: Custom

---

# 27. API Security

Required:
[x] HTTPS
[x] JWT
[x] CORS
[x] Rate Limit
[x] Input Validation
[x] Audit Logging

---

# 28. CORS Policy

Allowed:
Production: Specific domains only.
Never: AllowAnyOrigin()

---

# 29. OpenAPI Documentation

Swagger:
Available: /swagger

Includes:
JWT Authentication
Request schemas
Response schemas

---

# 30. API Health Endpoint

Routes:
GET /health
GET /health/ready
GET /health/live

Checks:
Database
Redis
RabbitMQ

---

# 31. API Logging

Log:
RequestId
UserId
WorkspaceId
Endpoint
Duration
StatusCode

Never log:
Password
Token
Email Body

---

# 32. API Telemetry

OpenTelemetry:

Collect:
HTTP latency
Error rate
Request count
Dependency calls

---

# 33. API Testing

Unit:
Validators
Handlers

Integration:
Authentication
Authorization
Endpoint behavior

Tools:
WebApplicationFactory
TestContainers

---

# 34. API Deployment

Runs as:
Docker Container

Behind:
Nginx
or
Cloud Load Balancer

---

# 35. API Performance Goals

Target:
Latency: <200ms
Search: <100ms

---

# 36. Completion Checklist

[x] API structure
[x] Versioning
[x] Controllers
[x] CQRS
[x] Validation
[x] Authentication
[x] Authorization
[x] Swagger
[x] Monitoring
[x] Testing

---

# 37. Final API Rules

Rule 1: API contains no business logic.
Rule 2: All operations go through Application Layer.
Rule 3: Every endpoint requires authorization.
Rule 4: Every request is observable.
Rule 5: Every response follows contract.

---

# End Of Document
