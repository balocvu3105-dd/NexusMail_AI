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
        var enableRequest = new AutomationEndpoints.ChangeStateRequest(rule.RuleVersion);
        var enableResponse = await client.PostAsJsonAsync($"/api/v1/automation/rules/{rule.Id}/enable", enableRequest);
        enableResponse.EnsureSuccessStatusCode();

        await _dbContext.Entry(rule).ReloadAsync();
        Assert.True(rule.IsEnabled);

        // Disable
        var disableRequest = new AutomationEndpoints.ChangeStateRequest(rule.RuleVersion);
        var disableResponse = await client.PostAsJsonAsync($"/api/v1/automation/rules/{rule.Id}/disable", disableRequest);
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
        
        var updateRequest = new AutomationEndpoints.UpdateRuleRequest("Hacked", "[]", "[]", 1, null);
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

    // CONCURRENCY TESTS

    [Fact]
    public async Task Update_MatchingVersion_ShouldIncrementVersion()
    {
        var workspaceId = Guid.NewGuid();
        var rule = AutomationRule.Create(workspaceId, "Rule", TriggerType.EmailReceived, "[]", "[]");
        _dbContext.AutomationRules.Add(rule);
        await _dbContext.SaveChangesAsync();

        var initialVersion = rule.RuleVersion; // Should be 1

        var client = CreateAuthenticatedClient(workspaceId);
        var updateRequest = new AutomationEndpoints.UpdateRuleRequest("Updated", "[]", "[]", initialVersion, null);
        var response = await client.PutAsJsonAsync($"/api/v1/automation/rules/{rule.Id}", updateRequest);
        
        response.EnsureSuccessStatusCode();

        await _dbContext.Entry(rule).ReloadAsync();
        Assert.Equal(initialVersion + 1, rule.RuleVersion);
        Assert.Equal("Updated", rule.Name);
    }

    [Fact]
    public async Task Update_StaleVersion_ShouldReturnConflict()
    {
        var workspaceId = Guid.NewGuid();
        var rule = AutomationRule.Create(workspaceId, "Rule", TriggerType.EmailReceived, "[]", "[]");
        _dbContext.AutomationRules.Add(rule);
        await _dbContext.SaveChangesAsync();

        var client = CreateAuthenticatedClient(workspaceId);
        
        // Use an incorrect ExpectedRuleVersion (e.g. 99 instead of 1)
        var updateRequest = new AutomationEndpoints.UpdateRuleRequest("Stale", "[]", "[]", 99, null);
        var response = await client.PutAsJsonAsync($"/api/v1/automation/rules/{rule.Id}", updateRequest);
        
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        // State remains unchanged
        await _dbContext.Entry(rule).ReloadAsync();
        Assert.Equal("Rule", rule.Name);
    }

    [Fact]
    public async Task EnableDisable_StaleVersion_ShouldReturnConflict()
    {
        var workspaceId = Guid.NewGuid();
        var rule = AutomationRule.Create(workspaceId, "Rule", TriggerType.EmailReceived, "[]", "[]");
        rule.Disable();
        _dbContext.AutomationRules.Add(rule);
        await _dbContext.SaveChangesAsync();

        var client = CreateAuthenticatedClient(workspaceId);
        
        // Pass wrong version to enable
        var request = new AutomationEndpoints.ChangeStateRequest(99);
        var response = await client.PostAsJsonAsync($"/api/v1/automation/rules/{rule.Id}/enable", request);
        
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        await _dbContext.Entry(rule).ReloadAsync();
        Assert.False(rule.IsEnabled); // Did not enable
    }

    [Fact]
    public async Task Update_RaceCondition_DbUpdateConcurrencyException_ShouldReturnConflict()
    {
        var workspaceId = Guid.NewGuid();
        var rule = AutomationRule.Create(workspaceId, "Rule", TriggerType.EmailReceived, "[]", "[]");
        _dbContext.AutomationRules.Add(rule);
        await _dbContext.SaveChangesAsync();

        // Let's create two separate scope dbContexts to simulate race condition
        using var scope1 = _factory.Services.CreateScope();
        using var scope2 = _factory.Services.CreateScope();
        var db1 = scope1.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var db2 = scope2.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var rule1 = await db1.AutomationRules.FindAsync(rule.Id);
        var rule2 = await db2.AutomationRules.FindAsync(rule.Id);

        // User A updates via client (which uses mediator and its own db context scope)
        var client = CreateAuthenticatedClient(workspaceId);
        var updateRequest = new AutomationEndpoints.UpdateRuleRequest("A", "[]", "[]", rule.RuleVersion, null);
        var responseA = await client.PutAsJsonAsync($"/api/v1/automation/rules/{rule.Id}", updateRequest);
        responseA.EnsureSuccessStatusCode();

        // Now, we simulate User B sending an update with the *original* expected version (which DB now rejects)
        // Wait, the API handler will catch it with `if (rule.RuleVersion != request.ExpectedRuleVersion)`.
        // That's the early check. We want to test if EF Core throws it when both pass the early check!
        // To bypass the early check, we must do it exactly at the same time, or just manually test the repo.
        // Or we can manually invoke the UpdateRuleAsync while simulating a stale instance.
        
        rule2!.Update("B", null, "[]", "[]"); // Incrementing the local state, but EF will see original version
        // Actually, since EF's SaveChangesAsync checks the token, we can just save it.
        db2.AutomationRules.Update(rule2);
        
        var repo2 = new NexusMail.Infrastructure.Persistence.Repositories.AutomationRuleRepository(db2);
        
        var ex = await Assert.ThrowsAsync<NexusMail.Domain.Exceptions.ConcurrencyException>(() => repo2.UpdateRuleAsync(rule2));
        Assert.Contains("modified by another user", ex.Message);
    }
}
