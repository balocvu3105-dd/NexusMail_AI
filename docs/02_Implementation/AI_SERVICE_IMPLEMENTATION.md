# NexusMail AI
# AI Service Implementation Document

Version:
1.0.0

Status:
Implementation Ready

Document Type:
Artificial Intelligence Service Implementation

Technology:
.NET 8
Python AI Service
OpenAI Compatible API
Ollama
PostgreSQL pgvector
RabbitMQ

---

# 1. AI Implementation Goal

This document defines:
- AI service architecture
- Model abstraction
- Prompt management
- Classification
- Summarization
- Embedding
- AI result storage

---

# 2. AI Service Location

Backend:
backend/src/NexusMail.AI

Worker:
backend/src/NexusMail.Worker.AI

Future:
ai/
Python AI Engine

---

# 3. AI Architecture

EmailReceived Event
    |
    v
AI Worker
    |
    v
AI Pipeline
    |
+--------------------+
|                    |
v                    v
LLM Service    Embedding Service
    |
    v
AI Result Storage

---

# 4. AI Service Boundary

AI Service receives:
```json
{
"EmailId": "",
"WorkspaceId": "",
"Content": "",
"Metadata": ""
}
```

Returns:
```json
{
"Category": "",
"Summary": "",
"Priority": 0,
"Confidence": 0
}
```

---

# 5. AI Provider Abstraction

Interface:
```csharp
public interface IAIProvider
{
    Task<string> CompleteAsync(string prompt, CancellationToken token);
}
```

---

# 6. Supported Providers

Phase 1:
OpenAI Compatible API

Future:
Azure OpenAI
Ollama
Local LLM
Custom Model

---

# 7. AI Provider Implementation

Structure:
AI/
├── Providers/
│   ├── OpenAI/
│   ├── Ollama/
│   └── Azure/

---

# 8. OpenAI Provider

Class:
OpenAIProvider

Responsibilities:
Send prompt
Handle response
Track usage
Handle errors

---

# 9. Model Configuration

Configuration:
```json
{
  "AI": {
    "Provider": "OpenAI",
    "Model": "gpt-5-mini",
    "Temperature": 0.2
  }
}
```

---

# 10. Prompt Engine

Location:
Prompts/

Purpose:
Externalize AI instructions.

---

# 11. Prompt Template

Example:
email/classification/v1.txt

Variables:
{{subject}}
{{sender}}
{{body}}

---

# 12. Prompt Versioning

Every prompt stores:
Name
Version
CreatedAt
Active

Example:
email-summary-v1
email-summary-v2

---

# 13. AI Pipeline

Flow:
Email
 |
Normalize
 |
Extract Content
 |
Classify
 |
Calculate Priority
 |
Generate Summary
 |
Create Embedding
 |
Save Result

---

# 14. AI Pipeline Interface

```csharp
public interface IEmailAIProcessor
{
    Task<AIResult> ProcessAsync(Email email, CancellationToken token);
}
```

---

# 15. Classification Service

Purpose:
Determine category.

Example:
Input: Invoice from AWS
Output:
```json
{
"category": "Finance",
"confidence": 0.95
}
```

---

# 16. Classification Entity

Stored:
EmailCategory
ConfidenceScore

---

# 17. Priority Service

Interface:
IPriorityAnalyzer

Output:
0 - 100

Factors:
Sender importance
Keywords
Previous behavior
Urgency

---

# 18. Summary Service

Interface:
ISummaryGenerator

Output:
Short Summary
Key Points
Action Items

---

# 19. Phishing Detection

Service:
IThreatAnalyzer

Analyze:
Links
Sender
Language
Attachment

Output:
```json
{
"Risk": "High",
"Confidence": 0.92
}
```

---

# 20. Embedding Service

Purpose:
Semantic Search.

Interface:
IEmbeddingService

Output:
float[] vector

---

# 21. Vector Storage

Database:
PostgreSQL

Extension:
pgvector

Table:
email_embeddings

---

# 22. AI Result Entity

Table:
ai_results

Fields:
Id
EmailId
Category
Summary
PriorityScore
RiskScore
Confidence
ModelVersion
PromptVersion
CreatedAt

---

# 23. AI Worker

Project:
NexusMail.Worker.AI

Consumes:
ai.email.process

---

# 24. AI Processing Message

Example:
```json
{
"EmailId": "uuid",
"Operation": "FullAnalysis"
}
```

---

# 25. Processing Flow

Receive Message
        |
Load Email
        |
Execute AI Pipeline
        |
Store AIResult
        |
Publish AICompleted Event

---

# 26. AI Completed Event

Event:
EmailAIProcessed

Payload:
```json
{
"EmailId": "",
"AIResultId": ""
}
```

---

# 27. Error Handling

Errors:
Model Timeout
Invalid Response
Token Limit
Provider Down

Strategy:
Retry
Fallback Model
Mark Failed

---

# 28. JSON Response Validation

AI output must pass:
Schema validation.

Example:
```json
{
"summary": "string",
"confidence": "number"
}
```

---

# 29. AI Security

Required:
Remove unnecessary metadata
Encrypt sensitive data
Tenant isolation
Provider access control

---

# 30. Privacy Mode

Future:
Support: External AI Mode, Local AI Mode

---

# 31. AI Cost Control

Track:
Token Usage
Request Count
Model Cost

---

# 32. AI Metrics

Monitor:
ai_request_total
ai_latency
token_usage
failure_rate
average_confidence

---

# 33. Testing Strategy

Unit:
Prompt builder
Parser
Confidence calculation

Integration:
AI Provider
Vector Database
Worker

Evaluation:
Accuracy dataset
Regression testing

---

# 34. AI Version Tracking

Every result stores:
Model Version
Prompt Version
Pipeline Version

---

# 35. Completion Checklist

[x] AI abstraction
[x] Provider system
[x] Prompt engine
[x] Classification
[x] Summary
[x] Embedding
[x] AI worker
[x] Storage
[x] Monitoring

---

# 36. Final AI Rules

Rule 1: AI provider is replaceable.
Rule 2: AI output requires confidence.
Rule 3: Prompts are version controlled.
Rule 4: AI processing is asynchronous.
Rule 5: AI never bypasses authorization.
Rule 6: Every AI result is traceable.

---

# End Of Document
