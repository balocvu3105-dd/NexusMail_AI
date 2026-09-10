# USER_PERSONAS.md

> NexusMail AI User Personas

Version: 1.0.0

Status: Product Definition

Priority: HIGH

Document Type: User Research

Applies To

- Product Team
- UX/UI Designers
- Engineering
- AI Team
- QA
- AI Coding Agents

---

# Purpose

Tài liệu này mô tả các nhóm người dùng mục tiêu của NexusMail AI.

Mỗi Persona đại diện cho một nhóm khách hàng có nhu cầu, hành vi và mục tiêu khác nhau.

Mọi tính năng mới đều phải trả lời được câu hỏi:

> Persona nào sẽ sử dụng?

> Họ nhận được giá trị gì?

---

# Persona Overview

| ID | Persona | Priority |
|----|----------|----------|
| P-001 | Freelancer | High |
| P-002 | Software Developer | High |
| P-003 | Small Business Owner | High |
| P-004 | Project Manager | High |
| P-005 | Customer Support Agent | Medium |
| P-006 | Sales Representative | Medium |
| P-007 | HR Recruiter | Medium |
| P-008 | Executive / CEO | Medium |
| P-009 | Student | Low |
| P-010 | Enterprise Administrator | Future |

---

# Persona P-001

## Freelancer

### Profile

Làm việc độc lập.

Có nhiều khách hàng.

Có nhiều email.

---

### Goals

- Không bỏ sót email khách hàng
- Phản hồi nhanh
- Quản lý nhiều tài khoản Gmail
- Theo dõi hợp đồng
- Theo dõi hóa đơn

---

### Pain Points

- Quá nhiều email
- Khó tìm hợp đồng cũ
- Spam nhiều
- Quên trả lời khách

---

### AI Benefits

AI Summary

AI Reply

Invoice Tag

Contract Detection

Priority Prediction

Reminder

---

### Typical Workflow

```
Open NexusMail

↓

Read AI Summary

↓

Reply

↓

Archive

↓

Done
```

---

# Persona P-002

## Software Developer

### Profile

Làm việc với

GitHub

GitLab

Azure DevOps

Jira

CI/CD

Cloud

---

### Email Types

Pull Request

CI Failure

Issue

Security Alert

Cloud Billing

Monitoring

Support

---

### Goals

Không bỏ sót

Production Error

Security Alert

Deployment Failure

---

### AI Benefits

Bug Detection

Priority Tag

Security Classification

Incident Summary

CI Notification

---

### Typical Workflow

```
Receive Email

↓

AI detects Production Incident

↓

Desktop Notification

↓

Open Incident

↓

Resolve

```

---

# Persona P-003

## Small Business Owner

### Profile

Quản lý

Nhân viên

Khách hàng

Đối tác

Đơn hàng

Thanh toán

---

### Goals

Theo dõi

Invoice

Payment

Orders

Customer Requests

---

### Pain Points

Khó kiểm soát

Nhiều email

Nhiều nhân viên

Nhiều mailbox

---

### AI Benefits

Invoice Detection

Payment Reminder

Customer Priority

Daily Summary

Dashboard

---

# Persona P-004

## Project Manager

### Profile

Quản lý

Team

Sprint

Meeting

Timeline

Stakeholders

---

### Goals

Không bỏ sót

Meeting

Deadline

Customer Feedback

Risk

---

### AI Benefits

Meeting Detection

Task Extraction

Deadline Detection

Project Summary

Reminder

---

# Persona P-005

## Customer Support Agent

### Profile

Tiếp nhận

Ticket

Complaint

Support

Bug Report

Refund

---

### Goals

Trả lời nhanh

Phân loại Ticket

Ưu tiên khách hàng VIP

---

### AI Benefits

Automatic Classification

Suggested Reply

Urgency Detection

Customer Sentiment

Ticket Summary

---

# Persona P-006

## Sales Representative

### Profile

Theo dõi

Lead

Deal

Quotation

Invoice

Contract

---

### Goals

Không bỏ lỡ khách hàng tiềm năng

Theo dõi báo giá

Theo dõi hợp đồng

---

### AI Benefits

Lead Detection

Quotation Summary

Customer Priority

Follow-up Reminder

Sales Dashboard

---

# Persona P-007

## HR Recruiter

### Profile

Quản lý

CV

Interview

Offer Letter

Onboarding

---

### Goals

Không bỏ sót ứng viên

Theo dõi lịch phỏng vấn

Đánh giá CV

---

### AI Benefits

Resume Classification

Interview Reminder

Candidate Summary

Recruitment Dashboard

---

# Persona P-008

## Executive / CEO

### Profile

Lãnh đạo doanh nghiệp.

Nhận hàng trăm email mỗi ngày.

---

### Goals

Đọc ít nhất

Nhưng hiểu nhiều nhất.

---

### AI Benefits

Executive Summary

Priority Inbox

Decision Support

Daily Digest

Important Contacts

---

# Persona P-009

## Student

### Profile

Sử dụng email để

Học tập

Nộp bài

Thông báo trường

Học bổng

Việc làm

---

### Goals

Không bỏ lỡ

Deadline

Scholarship

Interview

Exam

---

### AI Benefits

Deadline Reminder

Scholarship Detection

Assignment Summary

Calendar Suggestion

---

# Persona P-010

## Enterprise Administrator

### Profile

Quản trị

Workspace

Users

Permissions

Security

Audit

---

### Goals

Kiểm soát toàn hệ thống.

Đảm bảo bảo mật.

Theo dõi hoạt động.

---

### AI Benefits

Security Alerts

Audit Insights

Usage Analytics

Workspace Health

Compliance Reports

---

# Persona Needs Matrix

| Feature | Freelancer | Developer | Business | PM | Support | CEO |
|----------|------------|-----------|-----------|----|----------|-----|
| AI Summary | ✔ | ✔ | ✔ | ✔ | ✔ | ✔ |
| AI Reply | ✔ | ✔ | ✔ | ✔ | ✔ | ✔ |
| Smart Search | ✔ | ✔ | ✔ | ✔ | ✔ | ✔ |
| Automation | ✔ | ✔ | ✔ | ✔ | ✔ | △ |
| Dashboard | △ | ✔ | ✔ | ✔ | ✔ | ✔ |
| Notifications | ✔ | ✔ | ✔ | ✔ | ✔ | ✔ |
| Multi Inbox | ✔ | ✔ | ✔ | ✔ | △ | ✔ |

Legend

✔ = Required

△ = Useful

---

# User Expectations

Người dùng mong muốn

- Đồng bộ nhanh
- Không mất email
- AI chính xác
- Giao diện đơn giản
- Tìm kiếm tức thì
- Không phải cấu hình phức tạp
- Dữ liệu an toàn

---

# Accessibility Requirements

Ứng dụng cần hỗ trợ

- Keyboard Navigation
- Screen Reader
- High Contrast Mode
- Dark Mode
- Responsive Layout
- Large Font Option

---

# Common User Journey

```
Register

↓

Login

↓

Connect Gmail

↓

Sync Email

↓

AI Processing

↓

Read Summary

↓

Search

↓

Reply

↓

Archive

↓

Logout
```

---

# Persona Prioritization

Version 1

- Freelancer
- Software Developer
- Small Business Owner
- Project Manager

Version 2

- Sales
- HR
- Customer Support

Version 3

- Enterprise
- Government
- Education

---

# Design Principles Derived from Personas

Every feature should

- Reduce cognitive load
- Save time
- Minimize clicks
- Surface important information first
- Allow manual override
- Be understandable without training

---

# AI Coding Agent Instructions

When implementing a feature

Identify Persona

↓

Identify Goal

↓

Identify Pain Point

↓

Implement Solution

↓

Validate User Value

Never implement features without mapping them to at least one Persona.

---

# Definition of Done

User Personas are complete when

✔ Target users identified

✔ Goals documented

✔ Pain points documented

✔ AI value explained

✔ Feature mapping completed

✔ Product priorities established

---

END OF DOCUMENT