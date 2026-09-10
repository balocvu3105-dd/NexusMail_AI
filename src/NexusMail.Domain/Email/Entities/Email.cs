using System;
using NexusMail.Shared.Domain;
using NexusMail.Domain.Email.Events;

namespace NexusMail.Domain.Email.Entities;

public class Email : AggregateRoot
{
    public Guid AccountId { get; private set; }
    public string MessageId { get; private set; } = string.Empty;
    public string Sender { get; private set; } = string.Empty;
    public string Subject { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public string LifecycleState { get; private set; } = "Received";
    public DateTimeOffset ReceivedAt { get; private set; }

    private Email() { }

    /// <param name="workspaceId">
    /// Required for domain event propagation. Not persisted on the entity —
    /// derivable via AccountId → EmailAccount.WorkspaceId join.
    /// </param>
    public Email(Guid accountId, Guid workspaceId, string messageId, string sender, string subject, string content, DateTimeOffset receivedAt)
    {
        Id = Guid.NewGuid();
        AccountId = accountId;
        MessageId = messageId;
        Sender = sender;
        Subject = subject;
        Content = content;
        ReceivedAt = receivedAt.ToUniversalTime();

        AddDomainEvent(new EmailReceived
        {
            EmailId = Id,
            AccountId = AccountId,
            WorkspaceId = workspaceId,
            Sender = Sender,
            Subject = Subject,
            Body = Content,
            ReceivedAt = ReceivedAt
        });
    }
}

