# NexusMail AI
# AI Pipeline Architecture Document

Version:
1.0.0

Status:
Approved

Document Type:
Artificial Intelligence Processing Architecture

Technology:
Python AI Service
.NET Worker
OpenAI Compatible API
Ollama
Vector Database
RabbitMQ

---

# 1. AI Architecture Overview

NexusMail AI separates:
Business Layer
and
Intelligence Layer

Business Layer:
.NET Backend

Responsible for:
- Users
- Permission
- Workspace
- Email ownership
- Billing

Intelligence Layer:
AI Service

Responsible for:
- Understanding
- Classification
- Prediction
- Generation
- Recommendation

---

# 2. High Level Architecture

Email Sync Engine
    |
    |
    v
EmailReceived Event
    |
    |
    v
RabbitMQ
    |
    |
    v
AI Worker
    |
    |
    v
AI Pipeline
    |
    +----------------+
    |                |
    v                v
AI Database Vector Storage

---

# 3. AI Components

AI Platform
├── AI Gateway
├── Processing Worker
├── Prompt Engine
├── Classification Engine
├── Embedding Engine
├── Summary Engine
├── Recommendation Engine
└── Model Manager

---

# 4. AI Service Boundary

AI Service does NOT own:
- User authentication
- Permission
- Billing
- Email ownership

AI Service receives:
EmailId
WorkspaceId
Email Content

Returns:
AIResult

---

# 5. AI Processing Lifecycle

EmailReceived
    |
    v
Normalize
    |
    v
Language Detection
    |
    v
Content Extraction
    |
    v
Embedding Generation
    |
    v
Classification
    |
    v
Priority Score
    |
    v
Spam Detection
    |
    v
Phishing Detection
    |
    v
Summary Generation
    |
    v
Recommendation
    |
    v
Store Result

---

# 6. AI Worker Architecture

Project:
NexusMail.Worker.AI

Responsibilities:
- Consume AI queue
- Execute pipeline
- Save results
- Publish completion event

---

# 7. AI Queue Design

RabbitMQ queues:
ai.email.process
ai.summary.generate
ai.embedding.create
ai.completed
ai.failed

---

# 8. AI Processing Message

Example:
```json
{
 "EmailId": "uuid",
 "WorkspaceId": "uuid",
 "Operation": "FullAnalysis"
}
```

---

# 9. AI Provider Abstraction

Backend interface:
```csharp
IAIService
```

Example:
```csharp
Task<AIResult> AnalyzeEmailAsync(Email email, CancellationToken token);
```

---

# 10. Supported AI Providers

Phase 1:
OpenAI Compatible API

Future:
Azure OpenAI
Ollama
Local LLM
Custom Model

---

# 11. Model Abstraction

Never hardcode model.

Configuration:
AIOptions
Provider
ModelName
Temperature
MaxTokens

---

# 12. Prompt Management

Prompts are externalized.

Location:
/prompts

Structure:
prompts/
email/
classification.txt
summary.txt
phishing.txt

---

# 13. Prompt Versioning

Every prompt has:
Name
Version
CreatedAt
Active

Example:
email-summary-v2

---

# 14. Email Classification

AI detects:

Examples:
Finance
Work
Personal
Promotion
Newsletter
Important
Spam

Output:
Category
Confidence

---

# 15. Priority Scoring

AI calculates:
PriorityScore 0-100

Factors:
- Sender
- Keywords
- Urgency
- User behavior
- Context

---

# 16. Spam Detection

Output:
IsSpam
Confidence

---

# 17. Phishing Detection

Analyze:
- Suspicious links
- Sender mismatch
- Social engineering
- Attachment risk

Output:
RiskLevel
Confidence
Reason

---

# 18. Email Summarization

Output:
ShortSummary
KeyPoints
ActionItems

Example:
Input: Long email
Output: 3 bullet summary

---

# 19. Embedding Pipeline

Purpose:
Semantic Search

Flow:
Email Content
    |
    v
Embedding Model
    |
    v
Vector Database

---

# 20. Vector Storage

Technology options:

Phase 1:
PostgreSQL + pgvector

Future:
OpenSearch Vector

---

# 21. AI Result Entity

Stored:
AIResult
Id
EmailId
Category
Summary
PriorityScore
RiskScore
EmbeddingId
CreatedAt

---

# 22. AI Confidence Handling

Every AI result requires:
ConfidenceScore

Range:
0-1

---

# 23. Human Feedback Loop

Future:

User correction:

Example:
AI: "Promotion"
User: "Work"

Store feedback:
AITrainingFeedback

---

# 24. Personal AI Adaptation

Future:

AI learns:
- User preferences
- Important senders
- Working patterns
- Response habits

---

# 25. AI Security

Required:
- No secret in prompt
- Data isolation by workspace
- Sensitive content protection
- Provider access control

---

# 26. Data Privacy

Before sending external AI:

Rules:
- Remove unnecessary metadata
- Respect tenant isolation
- Support local AI mode

---

# 27. Local AI Support

Future architecture:
Backend
|
AI Gateway
|
Ollama
|
Local Model

---

# 28. AI Performance Target

Goals:
Classification: < 2 seconds
Summary: < 5 seconds
Embedding: < 1 second

---

# 29. Scaling Strategy

Stage 1: Single AI Worker
Stage 2: Multiple AI Workers
Stage 3: GPU AI Cluster

---

# 30. Monitoring Metrics

Track:
ai_requests_total
ai_latency
token_usage
model_error
confidence_average

---

# 31. AI Testing

Required:
Prompt Test
Model Output Test
Regression Test
Accuracy Evaluation

---

# 32. AI Versioning

Version:
Pipeline Version
Model Version
Prompt Version

Every AIResult stores versions.

---

# 33. Failure Handling

Failures:
- Provider unavailable
- Timeout
- Invalid response
- Token limit

Actions:
Retry
Fallback Model
Mark Failed

---

# 34. Future AI Agents

Architecture supports:
- Email Agent
- Research Agent
- Scheduling Agent
- Personal Assistant Agent

---

# 35. Final AI Rules

Rule 1: AI never owns business data.
Rule 2: AI processing is asynchronous.
Rule 3: Every AI output has confidence.
Rule 4: Every model is replaceable.
Rule 5: Every prompt is versioned.
Rule 6: User data isolation is mandatory.

---

# End Of Document
