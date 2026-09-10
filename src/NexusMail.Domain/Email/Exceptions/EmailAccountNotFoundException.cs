using System;

namespace NexusMail.Domain.Email.Exceptions;

public class EmailAccountNotFoundException : Exception
{
    public EmailAccountNotFoundException(Guid emailAccountId)
        : base($"Email account with ID {emailAccountId} was not found.")
    {
    }
}
