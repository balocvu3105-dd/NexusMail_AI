using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using NexusMail.Application.Abstractions.Copilot;
using NexusMail.Application.Features.Copilot;
using NexusMail.Application.Features.Copilot.Actions;
using NexusMail.Application.Features.Email.Abstractions;
using NexusMail.Automation.Actions;
using NexusMail.Domain.Automation.Enums;
using NexusMail.Shared.Common;

namespace NexusMail.Infrastructure.AI.Services;

public sealed class CopilotActionService : ICopilotActionService
{
    private readonly ActionExecutorFactory _executorFactory;
    private readonly IMemoryCache _idempotencyCache;
    private readonly IEmailRepository _emailRepository;
    private readonly ILogger<CopilotActionService> _logger;

    public CopilotActionService(
        ActionExecutorFactory executorFactory,
        IMemoryCache idempotencyCache,
        IEmailRepository emailRepository,
        ILogger<CopilotActionService> logger)
    {
        _executorFactory = executorFactory;
        _idempotencyCache = idempotencyCache;
        _emailRepository = emailRepository;
        _logger = logger;
    }

    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, byte> _activeKeys = new();

    public async Task<Result> ExecuteActionAsync(ActionExecutionRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.IdempotencyKey))
        {
            return Result.Failure(new Error("CopilotAction.MissingIdempotencyKey", "Missing IdempotencyKey."));
        }

        var cacheKey = $"CopilotAction_{request.WorkspaceId}_{request.IdempotencyKey}";
        
        // Prevent concurrent double-submit race conditions
        if (!_activeKeys.TryAdd(cacheKey, 1))
        {
            _logger.LogWarning("Concurrent action execution prevented for IdempotencyKey: {IdempotencyKey}", request.IdempotencyKey);
            return Result.Failure(new Error("CopilotAction.Duplicate", "Action is currently executing."));
        }

        try
        {
            if (_idempotencyCache.TryGetValue(cacheKey, out _))
            {
                _logger.LogWarning("Duplicate action execution prevented for IdempotencyKey: {IdempotencyKey}", request.IdempotencyKey);
                return Result.Failure(new Error("CopilotAction.Duplicate", "Action has already been executed."));
            }

        var proposal = request.Proposal;
        if (proposal == null || string.IsNullOrWhiteSpace(proposal.ActionType))
        {
            return Result.Failure(new Error("CopilotAction.InvalidProposal", "Invalid ActionProposal."));
        }

        if (!Enum.TryParse<ActionType>(proposal.ActionType, out var actionType))
        {
            return Result.Failure(new Error("CopilotAction.UnsupportedType", $"Unsupported ActionType: {proposal.ActionType}"));
        }

        // 1. Tier 1 Restrictions
        if (actionType != ActionType.CreateTask && actionType != ActionType.ApplyLabel)
        {
            return Result.Failure(new Error("CopilotAction.NotAllowed", $"ActionType {actionType} is not allowed via Copilot."));
        }

        // 2. Validate Parameters Schema
        object? typedParameters = null;
        try
        {
            if (actionType == ActionType.CreateTask)
            {
                var p = proposal.Parameters.Deserialize<CreateTaskParameters>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (p == null || string.IsNullOrWhiteSpace(p.Title))
                    return Result.Failure(new Error("CopilotAction.InvalidParameters", "CreateTask requires a valid Title."));
                typedParameters = p;
            }
            else if (actionType == ActionType.ApplyLabel)
            {
                var p = proposal.Parameters.Deserialize<ApplyLabelParameters>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (p == null || string.IsNullOrWhiteSpace(p.Label) || string.IsNullOrWhiteSpace(p.EmailId))
                    return Result.Failure(new Error("CopilotAction.InvalidParameters", "ApplyLabel requires a valid Label and EmailId."));
                
                if (Guid.TryParse(p.EmailId, out var emailIdGuid))
                {
                    var email = await _emailRepository.GetByIdAsync(emailIdGuid, request.WorkspaceId, cancellationToken);
                    if (email == null)
                    {
                        return Result.Failure(new Error("CopilotAction.Unauthorized", "Email not found or does not belong to the requested workspace."));
                    }
                }
                else
                {
                    return Result.Failure(new Error("CopilotAction.InvalidEmailId", "Invalid EmailId format."));
                }
                
                typedParameters = p;
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse parameters for ActionType {ActionType}", actionType);
            return Result.Failure(new Error("CopilotAction.InvalidSchema", "Invalid parameters schema."));
        }

        // 3. Mark Idempotency Key (Set just before execution)
        _idempotencyCache.Set(cacheKey, true, TimeSpan.FromHours(24));

        // 4. Execution via Automation Engine
        var executor = _executorFactory.GetExecutor(actionType);
        if (executor == null)
        {
            return Result.Failure(new Error("CopilotAction.NoExecutor", $"No executor found for ActionType {actionType}"));
        }

        string parametersJson = typedParameters != null ? JsonSerializer.Serialize(typedParameters) : "{}";
        
        // EvaluateRulesMessage is null for Copilot triggered actions. ActionExecutor must handle it.
        var actionResult = await executor.ExecuteAsync(request.IdempotencyKey, parametersJson, null!, cancellationToken);

        if (actionResult == ActionResult.Success)
        {
            return Result.Success();
        }

        // If execution fails, we might want to clear the idempotency cache so they can retry, but usually we don't.
        return Result.Failure(new Error("CopilotAction.ExecutionFailed", $"Action execution failed with result: {actionResult}"));
        }
        finally
        {
            _activeKeys.TryRemove(cacheKey, out _);
        }
    }
}
