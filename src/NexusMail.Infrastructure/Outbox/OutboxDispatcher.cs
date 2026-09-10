using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NexusMail.Application.Abstractions.Outbox;
using NexusMail.Infrastructure.Persistence;
using NexusMail.Shared.Domain;

namespace NexusMail.Infrastructure.Outbox;

public sealed class OutboxDispatcher : IOutboxDispatcher
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IPublisher _publisher;
    private readonly IOutboxSerializer _serializer;
    private readonly ILogger<OutboxDispatcher> _logger;
    private readonly OutboxOptions _options;

    public OutboxDispatcher(
        ApplicationDbContext dbContext,
        IPublisher publisher,
        IOutboxSerializer serializer,
        ILogger<OutboxDispatcher> logger,
        Microsoft.Extensions.Options.IOptions<OutboxOptions> options)
    {
        _dbContext = dbContext;
        _publisher = publisher;
        _serializer = serializer;
        _logger = logger;
        _options = options.Value;
    }

    public async Task DispatchUnprocessedMessagesAsync(CancellationToken cancellationToken = default)
    {
        var lockId = Guid.NewGuid().ToString("N");
        var lockedUntil = DateTimeOffset.UtcNow.Add(_options.LeaseTimeout);
        var correlationId = Guid.NewGuid().ToString("N");

        using var scope = _logger.BeginScope("{CorrelationId}", correlationId);

        // 1. Claim messages (skip dead lettered)
        var claimCount = await _dbContext.Database.ExecuteSqlRawAsync(
            @"UPDATE ""OutboxMessages""
              SET ""LockId"" = {0}, ""LockedUntilUtc"" = {1}
              WHERE ""Id"" IN (
                  SELECT ""Id"" FROM ""OutboxMessages""
                  WHERE ""ProcessedOnUtc"" IS NULL
                    AND ""DeadLetteredAt"" IS NULL
                    AND (""LockedUntilUtc"" IS NULL OR ""LockedUntilUtc"" < {2})
                  ORDER BY ""OccurredOnUtc""
                  LIMIT {3}
                  FOR UPDATE SKIP LOCKED
              )", lockId, lockedUntil, DateTimeOffset.UtcNow, _options.BatchSize);

        if (claimCount == 0)
        {
            return;
        }

        _logger.LogInformation("Claimed {Count} outbox messages for processing.", claimCount);

        var messages = await _dbContext.Set<OutboxMessage>()
            .Where(m => m.LockId == lockId)
            .ToListAsync(cancellationToken);

        // 2. Process messages
        foreach (var message in messages)
        {
            try
            {
                var domainEvent = _serializer.Deserialize(message.Type, message.Content);
                if (domainEvent == null)
                {
                    _logger.LogWarning("Could not deserialize message {MessageId} of type {Type}", message.Id, message.Type);
                    HandleFailure(message, FailureReason.JsonSerialization, "Deserialization failed");
                    continue;
                }

                // Inject correlation ID if supported by event
                await _publisher.Publish(domainEvent, cancellationToken);
                
                message.ProcessedOnUtc = DateTimeOffset.UtcNow;
                message.Error = null;
                message.FailureReason = null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process message {MessageId}", message.Id);
                HandleFailure(message, FailureReason.HandlerException, ex.Message);
            }
            finally
            {
                // Release lock
                message.LockId = null;
                message.LockedUntilUtc = null;
            }
        }

        // 3. Complete
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private void HandleFailure(OutboxMessage message, FailureReason reason, string error)
    {
        message.Error = error;
        message.FailureReason = reason;
        message.Attempts++;

        if (message.Attempts >= _options.MaxAttempts)
        {
            message.DeadLetteredAt = DateTimeOffset.UtcNow;
            _logger.LogWarning("Message {MessageId} reached max attempts and is now dead-lettered.", message.Id);
        }
    }
}
