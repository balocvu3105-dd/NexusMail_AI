using System;
using MediatR;
using NexusMail.Application.Features.Automation.DTOs;
using NexusMail.Shared.Domain;

namespace NexusMail.Application.Features.Automation.Queries.GetRuleAnalytics;

public record GetRuleAnalyticsQuery(Guid WorkspaceId, Guid RuleId) : IRequest<Result<RuleAnalyticsDto>>;
