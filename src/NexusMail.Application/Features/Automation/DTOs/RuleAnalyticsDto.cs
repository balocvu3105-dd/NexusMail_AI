using System;

namespace NexusMail.Application.Features.Automation.DTOs;

public record RuleAnalyticsDto(
    Guid RuleId,
    string RuleName,
    int TotalExecutions,
    int SucceededExecutions,
    int FailedExecutions,
    int UnknownExecutions,
    double SuccessRate,
    DateTimeOffset? LastExecutedAt
);
