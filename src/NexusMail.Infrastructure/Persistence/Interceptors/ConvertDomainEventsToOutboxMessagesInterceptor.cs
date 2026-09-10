using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NexusMail.Application.Abstractions.Outbox;
using NexusMail.Shared.Domain;

namespace NexusMail.Infrastructure.Persistence.Interceptors;

public sealed class ConvertDomainEventsToOutboxMessagesInterceptor : SaveChangesInterceptor
{
    private readonly IOutboxSerializer _serializer;
    private readonly NexusMail.Application.Abstractions.Context.IExecutionContextAccessor _executionContextAccessor;

    public ConvertDomainEventsToOutboxMessagesInterceptor(
        IOutboxSerializer serializer,
        NexusMail.Application.Abstractions.Context.IExecutionContextAccessor executionContextAccessor)
    {
        _serializer = serializer;
        _executionContextAccessor = executionContextAccessor;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        DbContext? dbContext = eventData.Context;

        if (dbContext is null)
        {
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        var outboxMessages = dbContext.ChangeTracker
            .Entries()
            .Select(x => x.Entity as AggregateRoot)
            .Where(x => x != null)
            .Cast<AggregateRoot>()
            .SelectMany(aggregateRoot =>
            {
                var domainEvents = aggregateRoot.DomainEvents.ToList();

                aggregateRoot.ClearDomainEvents();
                return domainEvents;
            })
            .Select(domainEvent => new OutboxMessage
            {
                Id = Guid.NewGuid(),
                OccurredOnUtc = domainEvent.OccurredOnUtc,
                CorrelationId = _executionContextAccessor.Context.CorrelationId,
                Type = domainEvent.GetType().AssemblyQualifiedName ?? domainEvent.GetType().Name,
                Content = _serializer.Serialize(domainEvent),
                Attempts = 0
            })
            .ToList();

        if (outboxMessages.Count > 0)
        {
            dbContext.Set<OutboxMessage>().AddRange(outboxMessages);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
