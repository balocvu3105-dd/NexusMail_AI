using System;

namespace NexusMail.Application.Abstractions.Time;

public interface IClock
{
    DateTime UtcNow { get; }
}
