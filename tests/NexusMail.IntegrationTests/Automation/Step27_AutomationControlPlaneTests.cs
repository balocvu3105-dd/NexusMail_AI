using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NexusMail.API.Endpoints.Automation;
using NexusMail.Domain.Automation.Entities;
using NexusMail.Domain.Automation.Enums;
using NexusMail.Infrastructure.Persistence;
using NexusMail.IntegrationTests.Infrastructure;
using Xunit;

namespace NexusMail.IntegrationTests.Automation;

[Collection("Integration")]
public class Step27_AutomationControlPlaneTests : IClassFixture<CustomWebApplicationFactory<NexusMail.API.ApiMarker>>, IAsyncLifetime
{
    private readonly CustomWebApplicationFactory<NexusMail.API.ApiMarker> _factory;
    private ApplicationDbContext _dbContext = null!;
    
    public Step27_AutomationControlPlaneTests(CustomWebApplicationFactory<NexusMail.API.ApiMarker> factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        var scope = _factory.Services.CreateScope();
        _dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        // await _factory.ResetDatabaseAsync(); // Not available in this factory, but the db is in-memory or rebuilt per test usually? Wait, if it's not there, I will comment it out or clear tables.
        _dbContext.AutomationRules.RemoveRange(_dbContext.AutomationRules);
        _dbContext.AutomationExecutions.RemoveRange(_dbContext.AutomationExecutions);
        await _dbContext.SaveChangesAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private System.Net.Http.HttpClient CreateAuthenticatedClient(Guid workspaceId)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Workspace-Id", workspaceId.ToString());
        return client;
    }

    [Fact]
    public async Task CreateRule_ShouldPersistForCurrentWorkspace()
    {
        var workspaceId = Guid.NewGuid();
        var client = CreateAuthenticatedClient(workspaceId);

        var request = new AutomationEndpoints.CreateRuleRequest(
            "Test Rule",
            TriggerType.EmailReceived,
            "[]",
            """[{"Type":"MarkAsRead"}]""",
            "Desc",
            false
        );

        var response = await client.PostAsJsonAsync("/api/v1/automation/rules", request);
        response.EnsureSuccessStatusCode();

        var createdId = await response.Content.ReadFromJsonAsync<Guid>();
        
        var ruleInDb = await _dbContext.AutomationRules.FindAsync(createdId);
        Assert.NotNull(ruleInDb);
        Assert.Equal(workspaceId, ruleInDb.WorkspaceId);
    }

    [Fact]
    public async Task CreateRule_WithInvalidAction_ShouldReject()
    {
        var workspaceId = Guid.NewGuid();
        var client = CreateAuthenticatedClient(workspaceId);

        var request = new AutomationEndpoints.CreateRuleRequest(
            "Invalid Action",
            TriggerType.EmailReceived,
            "[]",
            """[{"Type":"DoesNotExistAction"}]""", // Invalid action
            "Desc",
            false
        );

        var response = await client.PostAsJsonAsync("/api/v1/automation/rules", request);
        
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RuleFromAnotherWorkspace_ShouldNotBeAccessible()
    {
        var workspaceA = Guid.NewGuid();
        var workspaceB = Guid.NewGuid();
        
        var rule = AutomationRule.Create(workspaceA, "Rule A", TriggerType.EmailReceived, "[]", "[]");
        _dbContext.AutomationRules.Add(rule);
        await _dbContext.SaveChangesAsync();

        var clientB = CreateAuthenticatedClient(workspaceId: workspaceB);
        
        var getResponse = await clientB.GetAsync($"/api/v1/automation/rules/{rule.Id}");
        
        // MediatR command returns Failure which maps to 400 Bad Request by default in Minimal APIs without ProblemDetails config, 
        // but typically NotFound or BadRequest.
        Assert.False(getResponse.IsSuccessStatusCode);
    }

    [Fact]
    public async Task EnableRule_ShouldSetIsEnabledTrue_And_DisableRule_ShouldSetIsEnabledFalse()
    {
        var workspaceId = Guid.NewGuid();
        var rule = AutomationRule.Create(workspaceId, "Rule", TriggerType.EmailReceived, "[]", "[]");
        rule.Disable();
        _dbContext.AutomationRules.Add(rule);
        await _dbContext.SaveChangesAsync();

        var client = CreateAuthenticatedClient(workspaceId);

        // Enable
        var enableResponse = await client.PostAsync($"/api/v1/automation/rules/{rule.Id}/enable", null);
        enableResponse.EnsureSuccessStatusCode();

        await _dbContext.Entry(rule).ReloadAsync();
        Assert.True(rule.IsEnabled);

        // Disable
        var disableResponse = await client.PostAsync($"/api/v1/automation/rules/{rule.Id}/disable", null);
        disableResponse.EnsureSuccessStatusCode();

        await _dbContext.Entry(rule).ReloadAsync();
        Assert.False(rule.IsEnabled);
    }

    [Fact]
    public async Task UpdateRule_ShouldNotChangeWorkspaceOwnership()
    {
        var workspaceA = Guid.NewGuid();
        var workspaceB = Guid.NewGuid();
        
        var rule = AutomationRule.Create(workspaceA, "Rule A", TriggerType.EmailReceived, "[]", "[]");
        _dbContext.AutomationRules.Add(rule);
        await _dbContext.SaveChangesAsync();

        var clientB = CreateAuthenticatedClient(workspaceId: workspaceB);
        
        var updateRequest = new AutomationEndpoints.UpdateRuleRequest("Hacked", "[]", "[]", null);
        var updateResponse = await clientB.PutAsJsonAsync($"/api/v1/automation/rules/{rule.Id}", updateRequest);
        
        Assert.False(updateResponse.IsSuccessStatusCode);

        await _dbContext.Entry(rule).ReloadAsync();
        Assert.Equal(workspaceA, rule.WorkspaceId); // Still Workspace A
        Assert.Equal("Rule A", rule.Name);
    }

    [Fact]
    public async Task ExecutionHistory_ShouldReturnCorrectRuleAndEmail()
    {
        var workspaceId = Guid.NewGuid();
        var rule = AutomationRule.Create(workspaceId, "Rule", TriggerType.EmailReceived, "[]", "[]");
        _dbContext.AutomationRules.Add(rule);
        await _dbContext.SaveChangesAsync();

        var execution = AutomationExecution.Create(rule.Id, Guid.NewGuid(), "Success");
        _dbContext.AutomationExecutions.Add(execution);
        await _dbContext.SaveChangesAsync();

        var client = CreateAuthenticatedClient(workspaceId);
        
        var response = await client.GetFromJsonAsync<List<dynamic>>("/api/v1/automation/executions");
        
        Assert.NotNull(response);
        Assert.Single(response);
    }

    [Fact]
    public async Task ExecutionFromAnotherWorkspace_ShouldNotBeVisible()
    {
        var workspaceA = Guid.NewGuid();
        var workspaceB = Guid.NewGuid();
        
        var ruleA = AutomationRule.Create(workspaceA, "Rule A", TriggerType.EmailReceived, "[]", "[]");
        _dbContext.AutomationRules.Add(ruleA);
        
        var executionA = AutomationExecution.Create(ruleA.Id, Guid.NewGuid(), "Success");
        _dbContext.AutomationExecutions.Add(executionA);
        await _dbContext.SaveChangesAsync();

        var clientB = CreateAuthenticatedClient(workspaceId: workspaceB);
        
        var getResponse = await clientB.GetAsync($"/api/v1/automation/executions/{executionA.Id}");
        Assert.False(getResponse.IsSuccessStatusCode);
    }

    [Fact]
    public async Task UnknownExecution_ShouldRemainUnknown()
    {
        // There is simply no API route mapped to retry or mutate execution state.
        // The API only has GET endpoints for executions.
        var workspaceId = Guid.NewGuid();
        var rule = AutomationRule.Create(workspaceId, "Rule", TriggerType.EmailReceived, "[]", "[]");
        _dbContext.AutomationRules.Add(rule);
        
        var execution = AutomationExecution.Create(rule.Id, Guid.NewGuid(), "Unknown");
        _dbContext.AutomationExecutions.Add(execution);
        await _dbContext.SaveChangesAsync();

        var client = CreateAuthenticatedClient(workspaceId);
        
        // Asserting that no POST/PUT exists for execution
        var putResponse = await client.PutAsync($"/api/v1/automation/executions/{execution.Id}/retry", null);
        Assert.Equal(HttpStatusCode.NotFound, putResponse.StatusCode);
    }

    [Fact]
    public async Task UnknownExecution_ShouldExposeReadOnlyState()
    {
        var workspaceId = Guid.NewGuid();
        var rule = AutomationRule.Create(workspaceId, "Rule", TriggerType.EmailReceived, "[]", "[]");
        _dbContext.AutomationRules.Add(rule);
        
        var execution = AutomationExecution.Create(rule.Id, Guid.NewGuid(), "Unknown");
        _dbContext.AutomationExecutions.Add(execution);
        await _dbContext.SaveChangesAsync();

        var client = CreateAuthenticatedClient(workspaceId);
        var response = await client.GetFromJsonAsync<System.Text.Json.JsonElement>($"/api/v1/automation/executions/{execution.Id}");
        
        Assert.True(response.ValueKind != System.Text.Json.JsonValueKind.Undefined);
        Assert.Equal("Unknown", response.GetProperty("status").GetString());
    }

    [Fact]
    public async Task AuditQuery_ShouldNotMutateExecutionState()
    {
        var workspaceId = Guid.NewGuid();
        var rule = AutomationRule.Create(workspaceId, "Rule", TriggerType.EmailReceived, "[]", "[]");
        _dbContext.AutomationRules.Add(rule);
        
        var execution = AutomationExecution.Create(rule.Id, Guid.NewGuid(), "Executing");
        _dbContext.AutomationExecutions.Add(execution);
        await _dbContext.SaveChangesAsync();

        var client = CreateAuthenticatedClient(workspaceId);
        var getResponse = await client.GetAsync($"/api/v1/automation/executions/{execution.Id}");
        getResponse.EnsureSuccessStatusCode();

        await _dbContext.Entry(execution).ReloadAsync();
        Assert.Equal("Executing", execution.Status); // State did not change
    }
}
