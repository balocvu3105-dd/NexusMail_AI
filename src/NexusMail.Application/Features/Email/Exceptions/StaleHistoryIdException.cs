using System;

namespace NexusMail.Application.Features.Email.Exceptions;

public class StaleHistoryIdException : Exception
{
    public StaleHistoryIdException(string message) : base(message)
    {
    }
}
