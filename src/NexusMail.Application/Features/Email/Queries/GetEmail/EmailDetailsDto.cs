using System;
using System.Collections.Generic;

namespace NexusMail.Application.Features.Email.Queries.GetEmail;

public record EmailDetailsDto(
    Guid Id,
    string MessageId,
    string Sender,
    string Subject,
    string Content,
    DateTimeOffset ReceivedAt,
    string? Category,
    string? Language,
    double? Confidence,
    string? Summary,
    int? PriorityScore,
    string? Priority,
    List<string>? Tags
);
