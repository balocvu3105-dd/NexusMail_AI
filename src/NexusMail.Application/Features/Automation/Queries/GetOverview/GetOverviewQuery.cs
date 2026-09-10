using System;
using MediatR;
using NexusMail.Application.Features.Automation.DTOs;
using NexusMail.Shared.Domain;

namespace NexusMail.Application.Features.Automation.Queries.GetOverview;

public record GetOverviewQuery(Guid WorkspaceId) : IRequest<Result<AutomationOverviewDto>>;
