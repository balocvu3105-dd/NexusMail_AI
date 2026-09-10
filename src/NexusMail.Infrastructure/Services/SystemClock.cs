using System;
using NexusMail.Application.Abstractions.Time;

namespace NexusMail.Infrastructure.Services;

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
