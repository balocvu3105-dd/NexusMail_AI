# GIT_WORKFLOW.md

> NexusMail AI Git Workflow & Collaboration Standard

Version: 1.0.0

Status: Production

Priority: CRITICAL

Document Type: Source Control Standard

Applies To

- Developers
- AI Coding Agents
- CI/CD
- Code Review
- Release Management

---

# Purpose

This document defines the official Git workflow for NexusMail AI.

Every change to the codebase must follow this workflow.

No direct development on the production branch.

---

# Git Philosophy

Git is the single source of truth.

Every change must be

- Traceable
- Reviewable
- Reproducible
- Reversible

Small commits are preferred.

Frequent integration reduces conflicts.

---

# Branch Strategy

Primary Branches

```
main
```

Production-ready code only.

```
develop
```

Integration branch.

Never deploy directly from develop.

---

# Supporting Branches

Feature

```
feature/<feature-name>
```

Examples

```
feature/email-sync

feature/semantic-search

feature/gmail-provider

feature/ai-summary
```

---

Bug Fix

```
bugfix/<issue-name>
```

Examples

```
bugfix/oauth-timeout

bugfix/search-ranking

bugfix/cache-refresh
```

---

Hotfix

```
hotfix/<issue-name>
```

Examples

```
hotfix/token-expired

hotfix/security-patch
```

Applied directly from main.

Merged back into develop.

---

Release

```
release/v1.2.0
```

Used for stabilization before production.

Only bug fixes allowed.

---

Experimental

```
experiment/<name>
```

Never merged directly into main.

For prototypes and research only.

---

# Branch Lifecycle

```
develop
      │
      ▼
feature/*
      │
      ▼
Pull Request
      │
      ▼
Code Review
      │
      ▼
Merge → develop
      │
      ▼
release/*
      │
      ▼
Testing
      │
      ▼
Merge → main
      │
      ▼
Production
```

---

# Commit Messages

Format

```
<type>: <short description>
```

Examples

```
feat: add gmail synchronization

fix: prevent duplicate notifications

refactor: simplify email aggregate

docs: update AI architecture

test: add integration tests

perf: optimize semantic search

ci: update GitHub Actions

build: upgrade .NET SDK

security: mask sensitive logs
```

---

# Commit Types

| Type | Purpose |
|-------|----------|
| feat | New feature |
| fix | Bug fix |
| docs | Documentation |
| refactor | Internal refactoring |
| perf | Performance improvement |
| style | Formatting only |
| test | Tests |
| build | Build system |
| ci | CI/CD |
| chore | Maintenance |
| security | Security fix |
| revert | Revert previous commit |

---

# Commit Rules

Each commit should

- Solve one problem
- Compile successfully
- Pass tests
- Be independently reversible

Avoid

```
update

fix

changes

misc

work

final
```

---

# Pull Requests

Every feature requires a Pull Request.

PR Template

```
Summary

Motivation

Implementation

Testing

Screenshots (if UI)

Breaking Changes

Checklist
```

---

# Pull Request Checklist

Before requesting review

✔ Build passes

✔ Tests pass

✔ Documentation updated

✔ No secrets committed

✔ Naming conventions followed

✔ Security reviewed

✔ Logging added

✔ Error handling implemented

✔ Performance considered

---

# Code Review

Minimum reviewers

2

Critical modules

Architecture review required.

---

# Review Focus

Review

Architecture

Business Rules

Security

Performance

Readability

Maintainability

Testing

Documentation

---

# Merge Strategy

Preferred

Squash Merge

Alternative

Rebase Merge

Avoid

Merge Commit (unless required)

---

# Protected Branches

Protected

main

develop

Rules

No force push

No direct commit

PR required

Status checks required

Approval required

---

# Tags

Semantic Versioning

Examples

```
v1.0.0

v1.1.0

v1.2.3
```

Annotated tags preferred.

---

# Releases

Every release includes

Release Notes

Migration Notes

Known Issues

Breaking Changes

Deployment Instructions

---

# GitHub Actions

Run on every Pull Request

Restore

Build

Static Analysis

Unit Tests

Integration Tests

Coverage

Security Scan

Artifact Upload

Deployment Preview (optional)

---

# Branch Cleanup

Delete merged feature branches.

Keep

main

develop

release/* (until released)

---

# Versioning

Semantic Versioning

```
MAJOR.MINOR.PATCH
```

MAJOR

Breaking change

MINOR

New backward-compatible feature

PATCH

Bug fix

---

# Reverting Changes

Use

```
git revert
```

Avoid

```
git reset --hard
```

on shared branches.

---

# Large Features

Split into

Small Pull Requests

Avoid PRs larger than ~500 lines where practical.

---

# Git Ignore

Must ignore

```
bin/

obj/

node_modules/

.env

.vscode/

.idea/

*.user

*.log

coverage/

TestResults/

artifacts/
```

---

# Secrets

Never commit

API Keys

JWT Secrets

OAuth Credentials

Certificates

Private Keys

Connection Strings

Use secret scanning in CI.

---

# Binary Files

Avoid committing

Large datasets

Generated artifacts

Temporary files

Store release artifacts separately.

---

# Documentation

Every significant feature updates

README

Architecture docs

API docs

Changelog

Migration guide (if applicable)

---

# AI Coding Agent Rules

Before creating a Pull Request verify

✔ Branch name follows convention

✔ Commit messages follow standard

✔ Tests pass

✔ Documentation updated

✔ No secrets committed

✔ CI pipeline green

✔ Code review checklist completed

---

# Definition of Done

A Git contribution is complete when

✔ Branch strategy followed

✔ Commit messages standardized

✔ Pull Request reviewed

✔ CI passed

✔ Tests passed

✔ Documentation updated

✔ Versioning respected

✔ Merge completed

✔ Branch cleaned up

---

END OF DOCUMENT