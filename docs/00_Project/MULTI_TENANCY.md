# NexusMail AI
# Multi Tenancy Architecture

Version:
1.0.0

Status:
Approved

Document Type:
Architecture Specification

Priority:
Critical

---

# 1. Overview

NexusMail AI uses a multi-tenant SaaS architecture.

The platform supports:
- Individual users
- Professional users
- Teams
- Enterprise organizations

Each tenant has isolated:
- Data
- Users
- Permissions
- Configuration
- Billing
- Analytics

---

# 2. Multi Tenant Model

NexusMail AI uses:
Workspace Based Multi Tenancy

Hierarchy:

Organization
  |
  |
  v
Workspace
  |
  |
  v
Users
  |
  |
  v
Email Accounts
  |
  |
  v
Emails

---

# 3. Tenant Definition

## Tenant
A tenant represents an isolated customer environment.

Tenant types:

## Personal Tenant

Example:
Single user using Free / Pro plan.

Structure:

User
|
Workspace

---

## Business Tenant

Example:
Small team.

Structure:

Organization
|
Workspace
|
Members

---

## Enterprise Tenant

Example:
Large company.

Structure:

Organization
|
Multiple Workspaces
|
Thousands of Users

---

# 4. Core Tenant Entities

## Organization

Represents company or enterprise.

Properties:
OrganizationId
Name
OwnerId
CreatedAt
Status

Responsibilities:
- Billing ownership
- Enterprise policies
- User management

---

## Workspace

Main security boundary.

Properties:
WorkspaceId
OrganizationId
Name
Plan
Settings
CreatedAt

Responsibilities:
- Data ownership
- Permission boundary
- Configuration

---

## Membership

Connects user and workspace.

Properties:
MembershipId
WorkspaceId
UserId
RoleId
Status

---

# 5. Tenant Isolation Rules

## Rule 1
Every tenant-owned entity MUST contain:
WorkspaceId

Examples:
Email

Email
Id
WorkspaceId
Subject

Rule:
No WorkspaceId = Invalid Entity

---

## Rule 2
Application Layer MUST validate tenant access.

Example:
Request:
GET /emails/123

Process:
Authenticate User
    |
    v
Get Workspace Context
    |
    v
Validate Permission
    |
    v
Execute Query

---

# 6. Data Isolation Strategy

## Current Strategy
Logical Isolation

Meaning:
Same database.
Different WorkspaceId.

Example:
emails table
Workspace A
Email 1
Email 2
Workspace B
Email 3
Email 4

Queries always include:
WHERE WorkspaceId = CurrentWorkspace

---

# 7. Future Enterprise Isolation

Large enterprise customers may require:
Database Isolation

Example:
Enterprise A
Database A

Enterprise B
Database B

Migration path:
Shared Database
    |
    |
    v
Dedicated Database
    |
    |
    v
Dedicated Infrastructure

---

# 8. Tenant Context

Every request has tenant context.

Example:
TenantContext
{
UserId,
OrganizationId,
WorkspaceId,
Role,
Permissions
}

Available through:
ITenantContext

---

# 9. Authorization Model

NexusMail AI uses:
RBAC (Role Based Access Control)

Hierarchy:

Organization Owner
    |
    v
Workspace Admin
    |
    v
Manager
    |
    v
Member
    |
    v
Guest

---

# 10. Roles

## Organization Owner
Permissions:
- Manage billing
- Manage organization
- Create workspace
- Manage admins

---

## Workspace Admin
Permissions:
- Manage members
- Configure rules
- Manage integrations

---

## Manager
Permissions:
- Manage shared resources

---

## Member
Permissions:
- Use email features

---

## Guest
Permissions:
- Limited access

---

# 11. Permission Model

Permissions are granular.

Examples:
EMAIL.READ
EMAIL.WRITE
EMAIL.DELETE
RULE.CREATE
RULE.UPDATE
USER.INVITE
BILLING.READ
AUDIT.READ

---

# 12. Feature Isolation

Features depend on subscription plan.

Example:

Free:
1 Email Account
Basic AI

Pro:
Unlimited Email
Semantic Search
Automation

Enterprise:
SSO
Audit
API
Plugin

Authorization checks:
Permission
Subscription Feature

---

# 13. Tenant Configuration

Workspace settings:
WorkspaceSettings
{
Timezone,
Language,
NotificationPreference,
AISetting,
RetentionPolicy
}

---

# 14. Tenant Aware Services

All services must support tenant scope.

Example:

Email Service:

Bad:
GetEmails()

Correct:
GetEmails(WorkspaceId)

---

# 15. Background Worker Tenant Rules

Workers process events with tenant information.

Every event contains:
TenantId
WorkspaceId

Worker:
Receive Event
    |
Validate Tenant
    |
Process Data

---

# 16. Cache Isolation

Redis keys MUST include tenant.

Wrong:
user_permission

Correct:
tenant:{WorkspaceId}:user:{UserId}:permission

---

# 17. Search Isolation

OpenSearch documents MUST contain:
WorkspaceId

Every query applies filter:
WorkspaceId = CurrentWorkspace

No global search without permission.

---

# 18. Audit Isolation

Every audit record contains:
WorkspaceId
UserId
Action
Timestamp

Enterprise requires complete traceability.

---

# 19. Security Requirements

Forbidden:
Cross tenant data access

Example:
User A cannot access Workspace B emails

Required:
- Tenant validation middleware
- Authorization policy
- Database filtering
- Audit logging

---

# 20. Implementation Rules

Backend:

Every Aggregate:
Contains WorkspaceId if tenant owned.

Every Repository:
Requires tenant scope.

Every Query:
Must filter tenant.

Every Command:
Must validate ownership.

Every Event:
Contains tenant information.

---

# 21. Future Scaling

Scale path:

Stage 1:
Single database
10K users

Stage 2:
Database optimization
100K users

Stage 3:
Tenant sharding
1M users

Stage 4:
Enterprise dedicated environment

---

# End Of Document
