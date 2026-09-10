using System;

namespace NexusMail.Application.Features.Email.Queries.GetInbox;

public record InboxMessageDto(
    Guid Id,
    string MessageId,
    string Sender,
    string Subject,
    string Content,
    DateTimeOffset ReceivedAt,
    string Summary,
    int PriorityScore,
    string Priority,
    string Category
);
