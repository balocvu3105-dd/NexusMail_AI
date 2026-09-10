using System;

namespace NexusMail.Domain.Email.Exceptions;

public class OAuthExchangeException : Exception
{
    public OAuthExchangeException(string provider, string reason)
        : base($"Failed to exchange OAuth code with {provider}. Reason: {reason}")
    {
    }
}
