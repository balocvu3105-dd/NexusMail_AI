using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using MediatR;
using NexusMail.Application.Abstractions.Authentication;
using NexusMail.Application.Features.Automation.Commands.CreateAutomationRule;
using NexusMail.Application.Features.Automation.Commands.UpdateAutomationRule;
using NexusMail.Application.Features.Automation.Commands.EnableAutomation;
using NexusMail.Application.Features.Automation.Commands.DisableAutomation;
using NexusMail.Application.Features.Automation.Queries.GetAutomationRules;
using NexusMail.Application.Features.Automation.Queries.GetAutomationRuleById;
using NexusMail.Application.Features.Automation.Queries.GetAutomationExecutions;
using NexusMail.Application.Features.Automation.Queries.GetAutomationExecutionById;
using NexusMail.Application.Features.Automation.Queries.GetOverview;
using NexusMail.Application.Features.Automation.Queries.GetExecutionAnalytics;
using NexusMail.Application.Features.Automation.Queries.GetRuleAnalytics;
using NexusMail.Domain.Automation.Enums;
using NexusMail.API.Extensions;

namespace NexusMail.API.Endpoints.Automation;

public static class AutomationEndpoints
{
    public static void MapAutomationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v{version:apiVersion}/automation")
            .WithTags("Automation")
            .HasApiVersion(1.0)
            .AddEndpointFilter<NexusMail.API.Middleware.WorkspaceContextEndpointFilter>();

        group.MapGet("rules", async (IMediator mediator, IWorkspaceContext context) =>
        {
            var query = new GetAutomationRulesQuery(context.WorkspaceId!.Value);
            var result = await mediator.Send(query);
            return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblemDetails();
        });

        group.MapGet("rules/{id:guid}", async (Guid id, IMediator mediator, IWorkspaceContext context) =>
        {
            var query = new GetAutomationRuleByIdQuery(id, context.WorkspaceId!.Value);
            var result = await mediator.Send(query);
            return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblemDetails();
        });

        group.MapPost("rules", async ([FromBody] CreateRuleRequest request, IMediator mediator, IWorkspaceContext context) =>
        {
            var command = new CreateAutomationRuleCommand(
                context.WorkspaceId!.Value,
                request.Name,
                request.TriggerType,
                request.ConditionsJson,
                request.ActionsJson,
                request.Description,
                request.DryRun
            );
            var result = await mediator.Send(command);
            return result.IsSuccess ? Results.Created($"/api/v1/automation/rules/{result.Value}", result.Value) : result.ToProblemDetails();
        });

        group.MapPut("rules/{id:guid}", async (Guid id, [FromBody] UpdateRuleRequest request, IMediator mediator, IWorkspaceContext context) =>
        {
            var command = new UpdateAutomationRuleCommand(
                id,
                context.WorkspaceId!.Value,
                request.Name,
                request.ConditionsJson,
                request.ActionsJson,
                request.ExpectedRuleVersion,
                request.Description
            );
            var result = await mediator.Send(command);
            return result.IsSuccess ? Results.NoContent() : result.ToProblemDetails();
        });

        group.MapPost("rules/{id:guid}/enable", async (Guid id, [FromBody] ChangeStateRequest request, IMediator mediator, IWorkspaceContext context) =>
        {
            var command = new EnableAutomationCommand(id, context.WorkspaceId!.Value, request.ExpectedRuleVersion);
            var result = await mediator.Send(command);
            return result.IsSuccess ? Results.NoContent() : result.ToProblemDetails();
        });

        group.MapPost("rules/{id:guid}/disable", async (Guid id, [FromBody] ChangeStateRequest request, IMediator mediator, IWorkspaceContext context) =>
        {
            var command = new DisableAutomationCommand(id, context.WorkspaceId!.Value, request.ExpectedRuleVersion);
            var result = await mediator.Send(command);
            return result.IsSuccess ? Results.NoContent() : result.ToProblemDetails();
        });

        group.MapGet("executions", async (IMediator mediator, IWorkspaceContext context) =>
        {
            var query = new GetAutomationExecutionsQuery(context.WorkspaceId!.Value);
            var result = await mediator.Send(query);
            return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblemDetails();
        });

        group.MapGet("executions/{id:guid}", async (Guid id, IMediator mediator, IWorkspaceContext context) =>
        {
            var query = new GetAutomationExecutionByIdQuery(id, context.WorkspaceId!.Value);
            var result = await mediator.Send(query);
            return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblemDetails();
        });

        group.MapGet("analytics/overview", async (IMediator mediator, IWorkspaceContext context) =>
        {
            var query = new GetOverviewQuery(context.WorkspaceId!.Value);
            var result = await mediator.Send(query);
            return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblemDetails();
        });

        group.MapGet("analytics/executions", async (DateTimeOffset? from, DateTimeOffset? to, Guid? ruleId, string? status, int? page, int? pageSize, IMediator mediator, IWorkspaceContext context) =>
        {
            var query = new GetExecutionAnalyticsQuery(
                context.WorkspaceId!.Value,
                from,
                to,
                ruleId,
                status,
                page ?? 1,
                pageSize ?? 20
            );
            var result = await mediator.Send(query);
            return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblemDetails();
        });

        group.MapGet("rules/{id:guid}/analytics", async (Guid id, IMediator mediator, IWorkspaceContext context) =>
        {
            var query = new GetRuleAnalyticsQuery(context.WorkspaceId!.Value, id);
            var result = await mediator.Send(query);
            return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblemDetails();
        });
    }

    public record CreateRuleRequest(
        string Name,
        TriggerType TriggerType,
        string ConditionsJson,
        string ActionsJson,
        string? Description,
        bool DryRun
    );

    public record UpdateRuleRequest(
        string Name,
        string ConditionsJson,
        string ActionsJson,
        int ExpectedRuleVersion,
        string? Description
    );

    public record ChangeStateRequest(int ExpectedRuleVersion);
}
