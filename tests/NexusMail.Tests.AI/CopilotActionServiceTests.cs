using System;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using NexusMail.Application.Abstractions.Copilot;
using NexusMail.Application.Features.Copilot;
using NexusMail.Application.Features.Copilot.Actions;
using NexusMail.Automation.Actions;
using NexusMail.Contracts.Automation;
using NexusMail.Domain.Automation.Enums;
using NexusMail.Infrastructure.AI.Services;
using NexusMail.Shared.Common;
using Xunit;

namespace NexusMail.Tests.AI;

public class CopilotActionServiceTests
{
    private readonly IMemoryCache _cache;
    private readonly StubActionExecutor _createTaskExecutor;
    private readonly StubActionExecutor _applyLabelExecutor;
    private readonly StubEmailRepository _emailRepository;
    private readonly ActionExecutorFactory _factory;
    private readonly CopilotActionService _service;

    public CopilotActionServiceTests()
    {
        _cache = new MemoryCache(new MemoryCacheOptions());
        _createTaskExecutor = new StubActionExecutor(ActionType.CreateTask);
        _applyLabelExecutor = new StubActionExecutor(ActionType.ApplyLabel);
        _emailRepository = new StubEmailRepository();
        
        _factory = new ActionExecutorFactory(new IActionExecutor[] 
        {
            _createTaskExecutor,
            _applyLabelExecutor
        });

        _service = new CopilotActionService(_factory, _cache, _emailRepository, NullLogger<CopilotActionService>.Instance);
    }

    [Fact]
    public async Task ExecuteActionAsync_CreateTask_ValidProposal_ShouldExecute()
    {
        // Arrange
        var request = new ActionExecutionRequest
        {
            WorkspaceId = Guid.NewGuid(),
            IdempotencyKey = Guid.NewGuid().ToString(),
            Proposal = new ActionProposal
            {
                ActionType = "CreateTask",
                Parameters = JsonSerializer.SerializeToElement(new { title = "Valid Task" })
            }
        };

        // Act
        var result = await _service.ExecuteActionAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(1, _createTaskExecutor.ExecutionCount);
    }

    [Fact]
    public async Task ExecuteActionAsync_ApplyLabel_ValidProposal_ShouldExecute()
    {
        // Arrange
        var workspaceId = Guid.NewGuid();
        var emailId = Guid.NewGuid();
        _emailRepository.AddEmail(emailId, workspaceId);

        var request = new ActionExecutionRequest
        {
            WorkspaceId = workspaceId,
            IdempotencyKey = Guid.NewGuid().ToString(),
            Proposal = new ActionProposal
            {
                ActionType = "ApplyLabel",
                Parameters = JsonSerializer.SerializeToElement(new { label = "Important", emailId = emailId.ToString() })
            }
        };

        // Act
        var result = await _service.ExecuteActionAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(1, _applyLabelExecutor.ExecutionCount);
    }

    [Fact]
    public async Task CrossWorkspace_Action_ShouldReject()
    {
        // Arrange
        var workspaceA = Guid.NewGuid();
        var workspaceB = Guid.NewGuid();
        var emailIdInB = Guid.NewGuid();
        _emailRepository.AddEmail(emailIdInB, workspaceB);

        var request = new ActionExecutionRequest
        {
            WorkspaceId = workspaceA, // User is in Workspace A
            IdempotencyKey = Guid.NewGuid().ToString(),
            Proposal = new ActionProposal
            {
                ActionType = "ApplyLabel",
                Parameters = JsonSerializer.SerializeToElement(new { label = "Important", emailId = emailIdInB.ToString() })
            }
        };

        // Act
        var result = await _service.ExecuteActionAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Email not found or does not belong to the requested workspace", result.Error.Description);
        Assert.Equal(0, _applyLabelExecutor.ExecutionCount);
    }

    [Fact]
    public async Task ForeignEmail_Action_ShouldReject()
    {
        // Arrange
        var workspaceId = Guid.NewGuid();
        var foreignEmailId = Guid.NewGuid(); // Email does not exist in any workspace, or belongs to another

        var request = new ActionExecutionRequest
        {
            WorkspaceId = workspaceId,
            IdempotencyKey = Guid.NewGuid().ToString(),
            Proposal = new ActionProposal
            {
                ActionType = "ApplyLabel",
                Parameters = JsonSerializer.SerializeToElement(new { label = "Important", emailId = foreignEmailId.ToString() })
            }
        };

        // Act
        var result = await _service.ExecuteActionAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Email not found or does not belong to the requested workspace", result.Error.Description);
        Assert.Equal(0, _applyLabelExecutor.ExecutionCount);
    }

    [Fact]
    public async Task ExecuteActionAsync_UnknownActionType_ShouldReject()
    {
        // Arrange
        var request = new ActionExecutionRequest
        {
            WorkspaceId = Guid.NewGuid(),
            IdempotencyKey = Guid.NewGuid().ToString(),
            Proposal = new ActionProposal
            {
                ActionType = "DeleteDatabase", // Unknown
                Parameters = JsonSerializer.SerializeToElement(new { })
            }
        };

        // Act
        var result = await _service.ExecuteActionAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Unsupported ActionType", result.Error.Description);
        Assert.Equal(0, _createTaskExecutor.ExecutionCount);
    }

    [Fact]
    public async Task ExecuteActionAsync_InvalidTypedParameters_ShouldReject()
    {
        // Arrange
        var request = new ActionExecutionRequest
        {
            WorkspaceId = Guid.NewGuid(),
            IdempotencyKey = Guid.NewGuid().ToString(),
            Proposal = new ActionProposal
            {
                ActionType = "CreateTask",
                Parameters = JsonSerializer.SerializeToElement(new { title = "" }) // Missing required title
            }
        };

        // Act
        var result = await _service.ExecuteActionAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("CreateTask requires a valid Title", result.Error.Description);
        Assert.Equal(0, _createTaskExecutor.ExecutionCount);
    }

    [Fact]
    public async Task ExecuteActionAsync_SameIdempotencyKeyTwice_ShouldProduceOneSideEffect()
    {
        // Arrange
        var key = Guid.NewGuid().ToString();
        var request = new ActionExecutionRequest
        {
            WorkspaceId = Guid.NewGuid(),
            IdempotencyKey = key,
            Proposal = new ActionProposal
            {
                ActionType = "CreateTask",
                Parameters = JsonSerializer.SerializeToElement(new { title = "Valid Task" })
            }
        };

        // Act
        var result1 = await _service.ExecuteActionAsync(request);
        var result2 = await _service.ExecuteActionAsync(request); // Second click

        // Assert
        Assert.True(result1.IsSuccess);
        Assert.False(result2.IsSuccess);
        Assert.Contains("Action has already been executed", result2.Error.Description);
        
        // Exactly one execution reached the executor
        Assert.Equal(1, _createTaskExecutor.ExecutionCount);
    }

    [Fact]
    public async Task ExecuteActionAsync_ConcurrentIdempotency_ShouldProduceOneSideEffect()
    {
        // Arrange
        var key = Guid.NewGuid().ToString();
        var request = new ActionExecutionRequest
        {
            WorkspaceId = Guid.NewGuid(),
            IdempotencyKey = key,
            Proposal = new ActionProposal
            {
                ActionType = "CreateTask",
                Parameters = JsonSerializer.SerializeToElement(new { title = "Concurrent Task" })
            }
        };

        // Act
        int successCount = 0;
        int failureCount = 0;

        var tasks = new Task[50];
        for (int i = 0; i < 50; i++)
        {
            tasks[i] = Task.Run(async () =>
            {
                var result = await _service.ExecuteActionAsync(request);
                if (result.IsSuccess) Interlocked.Increment(ref successCount);
                else Interlocked.Increment(ref failureCount);
            });
        }

        await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(1, successCount); // Only one should succeed
        Assert.Equal(49, failureCount); // 49 should be rejected
        Assert.Equal(1, _createTaskExecutor.ExecutionCount); // Executor called exactly once
    }

    [Fact]
    public async Task ExecuteActionAsync_EditMutatedParameters_RevalidatesServerSide()
    {
        // Simulate frontend maliciously passing a CreateTask but omitting required fields.
        // Server MUST re-validate the final payload, not trust the initial LLM proposal.
        var request = new ActionExecutionRequest
        {
            WorkspaceId = Guid.NewGuid(),
            IdempotencyKey = Guid.NewGuid().ToString(),
            Proposal = new ActionProposal
            {
                ActionType = "CreateTask",
                Parameters = JsonSerializer.SerializeToElement(new { title = "   " }) // Edited to whitespace
            }
        };

        var result = await _service.ExecuteActionAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Contains("CreateTask requires a valid Title", result.Error.Description);
    }

    [Fact]
    public async Task LLM_Cannot_Directly_Trigger_Executor()
    {
        // This is a test asserting our design boundary: IActionExecutor doesn't receive ActionProposal.
        // It only receives typed parameters Json after CopilotActionService has authenticated and validated it.
        
        // The ExecuteAsync interface explicitly forces idempotencyKey & context, meaning the system
        // must intermediate the call.
        var type = typeof(IActionExecutor);
        var method = type.GetMethod("ExecuteAsync");
        
        Assert.NotNull(method);
        
        var parameters = method.GetParameters();
        Assert.DoesNotContain(parameters, p => p.ParameterType == typeof(ActionProposal));
    }
}

public class StubActionExecutor : IActionExecutor
{
    public ActionType ActionType { get; }
    private int _executionCount;

    public int ExecutionCount => _executionCount;

    public StubActionExecutor(ActionType actionType)
    {
        ActionType = actionType;
    }

    public Task<ActionResult> ExecuteAsync(string idempotencyKey, string parametersJson, EvaluateRulesMessage context, CancellationToken cancellationToken = default)
    {
        Interlocked.Increment(ref _executionCount);
        return Task.FromResult(ActionResult.Success);
    }
}

public class StubEmailRepository : NexusMail.Application.Features.Email.Abstractions.IEmailRepository
{
    private readonly System.Collections.Generic.Dictionary<Guid, Guid> _emailWorkspaces = new();

    public void AddEmail(Guid emailId, Guid workspaceId)
    {
        _emailWorkspaces[emailId] = workspaceId;
    }

    public Task<NexusMail.Domain.Email.Entities.Email?> GetByIdAsync(Guid id, Guid workspaceId, CancellationToken cancellationToken = default)
    {
        if (_emailWorkspaces.TryGetValue(id, out var ownerWorkspaceId) && ownerWorkspaceId == workspaceId)
        {
            var email = new NexusMail.Domain.Email.Entities.Email(
                Guid.NewGuid(), workspaceId, "msgId", "sender", "subject", "content", DateTimeOffset.UtcNow);
            return Task.FromResult<NexusMail.Domain.Email.Entities.Email?>(email);
        }
        return Task.FromResult<NexusMail.Domain.Email.Entities.Email?>(null);
    }

    public Task<(NexusMail.Domain.Email.Entities.Email Email, NexusMail.Domain.AI.Entities.AIAnalysis? Analysis)?> GetEmailWithAnalysisAsync(Guid id, Guid workspaceId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<System.Collections.Generic.List<NexusMail.Domain.Email.Entities.Email>> GetByIdsAsync(System.Collections.Generic.IEnumerable<Guid> ids, Guid workspaceId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task AddAnalysisAsync(NexusMail.Domain.AI.Entities.AIAnalysis analysis, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public void UpdateAnalysis(NexusMail.Domain.AI.Entities.AIAnalysis analysis) => throw new NotImplementedException();
}
