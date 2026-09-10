using System;

namespace NexusMail.Application.Features.Automation.DTOs;

public record AutomationOverviewDto(
    int TotalRules,
    int ActiveRules,
    int TotalExecutions,
    int SucceededExecutions,
    int FailedExecutions,
    int UnknownExecutions,
    int PendingExecutions,
    double SuccessRate
);
