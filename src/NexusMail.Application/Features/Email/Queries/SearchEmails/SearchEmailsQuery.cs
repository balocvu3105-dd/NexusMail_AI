using System;
using System.Collections.Generic;
using MediatR;
using NexusMail.Application.Abstractions.Search;
using NexusMail.Shared.Domain;

namespace NexusMail.Application.Features.Email.Queries.SearchEmails;

public record SearchEmailsQuery(
    string QueryText,
    SearchMode Mode = SearchMode.Hybrid,
    int Page = 1,
    int PageSize = 20,
    DateTimeOffset? From = null,
    DateTimeOffset? To = null,
    string? Sender = null,
    List<string>? Labels = null,
    bool? HasAttachments = null
) : IRequest<Result<SearchResult>>;
