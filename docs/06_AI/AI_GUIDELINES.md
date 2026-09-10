# AI_GUIDELINES.md

> NexusMail AI - AI Engineering Guidelines

Version: 1.0.0
Status: Production
Priority: High

---

## 1. Core Principles
- AI never modifies user data without explicit permission.
- Always use the AI Gateway; never call LLMs directly from the backend.
- Fallback mechanisms must be implemented for all AI features.

## 2. OpenAI Compatibility
- All AI integrations must implement the IAIProvider interface.
- Must support multiple providers (OpenAI, Azure OpenAI, Ollama, Anthropic).

## 3. Prompt Engineering
- Store prompts in version control (not in code strings).
- Use clear, deterministic instructions.
- Provide few-shot examples for classification tasks.

## 4. Performance & Scalability
- Offload heavy AI tasks to Background Queues (RabbitMQ).
- Implement caching for repeated AI requests (Redis).
- AI Summary must return within 5 seconds.
