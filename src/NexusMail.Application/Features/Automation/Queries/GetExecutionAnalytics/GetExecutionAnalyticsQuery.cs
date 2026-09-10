using System;
using MediatR;
using NexusMail.Application.Features.Automation.DTOs;
using NexusMail.Common.Pagination;
using NexusMail.Shared.Domain;

namespace NexusMail.Application.Features.Automation.Queries.GetExecutionAnalytics;

public record GetExecutionAnalyticsQuery(
    Guid WorkspaceId,
    DateTimeOffset? FromDate,
    DateTimeOffset? ToDate,
    Guid? RuleId,
    string? Status,
    int Page = 1,
    int PageSize = 20
) : IRequest<Result<PagedList<ExecutionAnalyticsDto>>>;
