using System;

namespace NexusMail.Application.Features.Automation.DTOs;

public record ExecutionAnalyticsDto(
    Guid Id,
    Guid RuleId,
    string RuleName,
    Guid EmailId,
    string Status,
    DateTimeOffset CreatedAt
);
