# DATABASE_STANDARDS.md

> NexusMail AI Database Standards

Version: 1.0.0

Status: Production

Priority: CRITICAL

Document Type: Database Engineering Standard

Applies To

- PostgreSQL
- Entity Framework Core
- AI Database
- Cache Metadata
- Audit Database

---

# Purpose

This document defines every database rule used throughout the project.

All AI Coding Agents MUST follow this specification.

---

# Database Philosophy

Database is the source of truth.

Business Rules belong to Domain.

Database stores state.

Database never contains business logic.

---

# Database Engine

Primary

PostgreSQL

Version

17+

Extensions

uuid-ossp

pg_trgm

vector

citext

unaccent

pgcrypto

Future

TimescaleDB

---

# Naming Convention

Database

snake_case

Table

snake_case

Column

snake_case

Index

idx_

Unique

uq_

Foreign Key

fk_

Primary Key

pk_

View

vw_

Function

fn_

Trigger

tr_

Sequence

seq_

---

# Primary Keys

Every table

Must use

UUID v7

Example

email_id

user_id

workspace_id

Never use

Identity

Auto Increment

---

# Required Columns

Every business table MUST contain

id

created_utc

created_by

updated_utc

updated_by

deleted_utc

deleted_by

is_deleted

row_version

---

# Audit Columns

created_utc

Timestamp

Not Null

updated_utc

Timestamp

Nullable

deleted_utc

Timestamp

Nullable

created_by

UUID

updated_by

UUID

deleted_by

UUID

---

# Soft Delete

Never delete business records.

Use

is_deleted

deleted_utc

deleted_by

Only administrators may permanently purge records.

---

# Row Version

Every mutable table

Must include

row_version

Purpose

Optimistic Concurrency

---

# Data Types

UUID

uuid

Boolean

boolean

Integer

integer

Long

bigint

Money

numeric(18,2)

Date

date

Timestamp

timestamptz

Json

jsonb

Binary

bytea

Vector

vector

---

# String Rules

Use

varchar

When length known.

Use

text

For unlimited content.

Never use

char

Unless fixed length.

---

# Email Fields

Always

citext

Purpose

Case insensitive search.

---

# JSON Rules

Allowed

Settings

Metadata

AI Output

Prompt Result

Provider Payload

Forbidden

Business Relations

---

# Foreign Keys

Every relationship

Must use FK.

Never store relation without constraint.

---

# Cascade Rules

Never cascade delete business data.

Allowed

Cascade

Lookup Tables

Reference Tables

Join Tables

---

# Index Rules

Every searchable field

Must have index.

Examples

email_address

sender

priority

created_utc

workspace_id

user_id

tag_id

category_id

---

# Composite Index

Example

workspace_id

created_utc

Example

user_id

priority

---

# Full Text Search

Never use SQL LIKE

For large datasets.

Use

OpenSearch

PostgreSQL only

Small datasets

Admin tools

---

# Partition Strategy

Email

Monthly Partition

Audit

Monthly Partition

Notification

Monthly Partition

Analytics

Monthly Partition

---

# Archive Strategy

Older than

365 Days

↓

Archive Storage

Never delete automatically.

---

# File Storage

Attachments

Never store in database.

Store

Object Storage

Database stores metadata only.

---

# Attachment Metadata

id

email_id

file_name

content_type

size

hash

storage_provider

storage_path

created_utc

---

# Encryption

Encrypt

OAuth Tokens

Refresh Tokens

API Keys

Secrets

Personally Identifiable Information

---

# Password Rules

Never store plain text.

Hash

Argon2id

or

BCrypt

---

# Transactions

Application Layer controls transaction.

Database never contains workflow.

---

# Isolation Level

Default

Read Committed

High Risk

Repeatable Read

Financial

Serializable

---

# Migrations

Framework

EF Core

Every migration

Small

Atomic

Reversible

Named correctly.

Example

AddEmailTagTable

CreateNotificationIndexes

---

# Seed Data

Separate project.

Never inside migration.

---

# Lookup Tables

countries

languages

timezones

currencies

permissions

roles

providers

---

# Workspace Isolation

Every business table

Must include

workspace_id

Multi Tenant

Required.

---

# User Isolation

Every user data

Must belong to workspace.

Never cross workspace.

---

# Audit Logging

Every sensitive action

Must create audit log.

Examples

Login

Delete

Permission Change

API Key

Billing

---

# Audit Table

audit_log

id

workspace_id

user_id

action

entity

entity_id

old_value

new_value

ip_address

user_agent

created_utc

---

# AI Tables

ai_summary

ai_embedding

ai_tag

ai_classification

ai_language

ai_translation

ai_reply

---

# Search Tables

search_index

search_keyword

search_vector

search_history

---

# Notification Tables

notification

notification_channel

notification_history

notification_template

---

# Billing Tables

subscription

invoice

payment

payment_history

license

---

# Security Tables

oauth_token

refresh_token

api_key

security_event

login_history

trusted_device

---

# Performance Targets

Simple Query

< 50 ms

Complex Query

< 200 ms

Insert

< 30 ms

Update

< 50 ms

Delete

< 30 ms

---

# Backup Strategy

Daily Incremental

Weekly Full

Monthly Archive

Encrypted

Geo Redundant

---

# Restore Strategy

Target

< 30 Minutes

Recovery Point

< 5 Minutes

---

# Monitoring

Monitor

Connections

Locks

Deadlocks

Slow Queries

Replication

CPU

Disk

Memory

Index Usage

---

# Database Health

Every deployment

Must verify

Migration

Indexes

Constraints

Views

Functions

Performance

---

# AI Restrictions

AI MUST NEVER

Drop table

Drop column

Remove FK

Remove Index

Without Architecture Decision Record (ADR)

---

# Definition of Done

Database task complete when

✔ Migration created

✔ Rollback tested

✔ Indexes reviewed

✔ Constraints added

✔ FK validated

✔ Audit fields added

✔ Soft Delete supported

✔ Performance reviewed

✔ Documentation updated

✔ ERD updated

---

END OF DOCUMENT