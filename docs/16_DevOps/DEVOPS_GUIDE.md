# DEVOPS_GUIDE.md

> NexusMail AI DevOps & Deployment Engineering Guide

Version: 1.0.0

Status: Production

Priority: CRITICAL

Document Type: DevOps Standard

Applies To

- Backend
- Frontend
- AI Services
- Database
- Infrastructure
- Cloud
- CI/CD

---

# Purpose

This document defines the official DevOps architecture and deployment standards for NexusMail AI.

Every AI Coding Agent must generate deployment-ready code.

Development must always assume cloud deployment.

---

# DevOps Philosophy

Build Once

↓

Test Once

↓

Deploy Anywhere

Infrastructure is code.

Automation first.

Manual deployment is prohibited except emergency recovery.

---

# Deployment Architecture

```
Internet
    │
    ▼
Load Balancer
    │
    ▼
API Gateway
    │
 ┌──┴───────────────┐
 │                  │
Backend API      AI Service
 │                  │
 ├──────┐           │
 │      │           │
 ▼      ▼           ▼
Redis RabbitMQ OpenSearch
 │
 ▼
PostgreSQL
 │
 ▼
Object Storage
```

---

# Infrastructure Components

Core

- ASP.NET Core API
- Python AI Service
- PostgreSQL
- Redis
- RabbitMQ
- OpenSearch

Observability

- Prometheus
- Grafana
- Loki
- Tempo
- OpenTelemetry

Ingress

- NGINX
- Traefik (optional)

Storage

- S3 Compatible Storage
- Azure Blob
- MinIO

---

# Environment Strategy

Supported

Development

Testing

Staging

Production

Each environment must have isolated

Database

Redis

RabbitMQ

Secrets

Storage

Logs

---

# Configuration

Never hardcode.

Configuration sources

Environment Variables

↓

Secret Manager

↓

Configuration Files

↓

Database

Priority follows the order above.

---

# Docker Standards

Every service

Must contain

Dockerfile

.dockerignore

Healthcheck

Non-root user

Minimal image

---

# Docker Base Images

Backend

mcr.microsoft.com/dotnet/aspnet

Build

mcr.microsoft.com/dotnet/sdk

AI

python:3.12-slim

Frontend

node:lts

NGINX

nginx:stable-alpine

---

# Dockerfile Rules

Use multi-stage builds.

Remove unnecessary packages.

Never expose secrets.

Use read-only filesystem where practical.

Run as non-root.

---

# Docker Compose

Development only.

Services

backend

frontend

ai

postgres

redis

rabbitmq

opensearch

minio

prometheus

grafana

loki

tempo

---

# Kubernetes

Production deployment target.

Resources

Deployment

Service

Ingress

ConfigMap

Secret

HorizontalPodAutoscaler

PersistentVolumeClaim

NetworkPolicy

---

# Resource Requests

Backend

CPU

500m

Memory

512Mi

AI Service

CPU

1000m

Memory

2Gi

Database

Dedicated node preferred.

---

# Horizontal Scaling

Stateless services only.

Backend

Horizontal

AI

Horizontal

Redis

Cluster (future)

RabbitMQ

Cluster

OpenSearch

Cluster

---

# Health Checks

Every service exposes

/health

/readiness

/liveness

Kubernetes probes required.

---

# CI/CD Pipeline

Pipeline

Checkout

↓

Restore

↓

Build

↓

Lint

↓

Unit Tests

↓

Integration Tests

↓

Security Scan

↓

Coverage

↓

Docker Build

↓

Image Scan

↓

Push Registry

↓

Deploy Staging

↓

Smoke Test

↓

Approval

↓

Deploy Production

---

# GitHub Actions

Primary CI platform.

Workflows

build.yml

test.yml

security.yml

deploy.yml

release.yml

---

# Static Analysis

Run

Roslyn Analyzers

SonarQube

CodeQL

Ruff

ESLint

Trivy

Dependabot

---

# Dependency Scanning

Automated

Daily

Weekly Reports

Block critical vulnerabilities.

---

# Container Registry

Supported

GitHub Container Registry

Azure Container Registry

Docker Hub

AWS ECR

---

# Secrets Management

Supported

Azure Key Vault

HashiCorp Vault

AWS Secrets Manager

Kubernetes Secrets

Never commit secrets.

---

# Infrastructure as Code

Preferred

Terraform

Alternative

Bicep

Helm for Kubernetes packages.

---

# Database Deployment

EF Core Migrations

Executed automatically in staging.

Production

Approval required.

Rollback plan mandatory.

---

# Backup Strategy

Database

Daily Incremental

Weekly Full

Object Storage

Versioning Enabled

Redis

Persistence Optional

Configuration

Backed up

---

# Disaster Recovery

Recovery Time Objective

30 Minutes

Recovery Point Objective

5 Minutes

Document recovery procedures.

---

# Monitoring

Prometheus

Metrics

Grafana

Dashboards

Loki

Logs

Tempo

Tracing

Alertmanager

Alerts

---

# Alerts

Critical

Database Down

AI Down

Queue Backlog

API Failure

Memory > 90%

Disk > 90%

Certificate Expiring

---

# Performance Targets

API

99th percentile

<500ms

AI Summary

<5 seconds

Search

<150ms

Dashboard

<300ms

---

# Release Strategy

Development

Continuous Deployment

Staging

Automatic

Production

Manual Approval

Blue/Green preferred.

Canary supported.

---

# Rollback

Every deployment must support rollback.

Rollback must be tested.

Never deploy without recovery plan.

---

# Versioning

Semantic Versioning

MAJOR.MINOR.PATCH

Git Tags

Required

Release Notes

Required

---

# Branch Strategy

main

Production

develop

Integration

feature/*

New Features

release/*

Release Preparation

hotfix/*

Production Fixes

---

# Artifact Management

Store

Build Artifacts

Coverage Reports

Test Reports

SBOM

Docker Images

Release Notes

---

# AI Service Deployment

Python AI

Independent deployment.

Own lifecycle.

Own scaling.

Own monitoring.

Communicates through secure API.

---

# Frontend Deployment

React

Static build

Served via CDN or NGINX.

Environment variables injected during build.

---

# Security

HTTPS only.

Signed container images preferred.

Image scanning mandatory.

Network policies enforced.

Least privilege IAM.

---

# AI Coding Checklist

Before completing infrastructure verify

✔ Dockerfile created

✔ Health checks added

✔ CI/CD configured

✔ Security scans enabled

✔ Secrets externalized

✔ Monitoring configured

✔ Alerts defined

✔ Rollback supported

✔ Documentation updated

---

# Definition of Done

Infrastructure is complete when

✔ Services containerized

✔ CI/CD operational

✔ Monitoring enabled

✔ Logging centralized

✔ Secrets protected

✔ Kubernetes manifests ready

✔ Rollback documented

✔ Backups configured

✔ Security scans passing

✔ Deployment reproducible

---

END OF DOCUMENT