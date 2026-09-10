using System;

namespace NexusMail.Application.Abstractions.AI;

/// <summary>
/// Thrown when an AI provider encounters a transient failure (e.g., 429 Rate Limit, 503 Unavailable, Timeout).
/// This exception signals to the message bus that the operation should be retried.
/// </summary>
public class AIProviderTransientException : Exception
{
    public AIProviderTransientException(string message) : base(message) { }
    
    public AIProviderTransientException(string message, Exception innerException) : base(message, innerException) { }
}
