using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexusMail.Domain.Automation.Entities;
using NexusMail.Domain.Automation.Enums;
using NexusMail.Infrastructure.Persistence;
using NexusMail.IntegrationTests.Infrastructure;
using Xunit;
using FluentAssertions;
using NexusMail.Application.Features.Automation.DTOs;
using NexusMail.Common.Pagination;
using System.Text.Json;

namespace NexusMail.IntegrationTests.Automation.Analytics;

[Collection("Integration")]
public class AutomationAnalyticsTests : IClassFixture<CustomWebApplicationFactory<NexusMail.API.ApiMarker>>, IAsyncLifetime
{
    private readonly CustomWebApplicationFactory<NexusMail.API.ApiMarker> _factory;
    private ApplicationDbContext _dbContext = null!;
    
    public AutomationAnalyticsTests(CustomWebApplicationFactory<NexusMail.API.ApiMarker> factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        var scope = _factory.Services.CreateScope();
        _dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
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

    // 1-3 Overview
    [Fact]
    public async Task GetOverview_ShouldRespectWorkspace()
    {
        var workspaceA = Guid.NewGuid();
        var workspaceB = Guid.NewGuid();

        // Seed data for Workspace A
        var ruleA = AutomationRule.Create(workspaceA, "Rule A", TriggerType.EmailReceived);
        _dbContext.AutomationRules.Add(ruleA);
        _dbContext.AutomationExecutions.Add(AutomationExecution.Create(ruleA.Id, Guid.NewGuid(), "Succeeded"));

        // Seed data for Workspace B
        var ruleB = AutomationRule.Create(workspaceB, "Rule B", TriggerType.EmailReceived);
        _dbContext.AutomationRules.Add(ruleB);
        _dbContext.AutomationExecutions.Add(AutomationExecution.Create(ruleB.Id, Guid.NewGuid(), "Failed"));
        _dbContext.AutomationExecutions.Add(AutomationExecution.Create(ruleB.Id, Guid.NewGuid(), "Unknown"));

        await _dbContext.SaveChangesAsync();

        var client = CreateAuthenticatedClient(workspaceB);
        var response = await client.GetFromJsonAsync<AutomationOverviewDto>("/api/v1/automation/analytics/overview");

        Assert.NotNull(response);
        Assert.Equal(1, response.TotalRules);
        Assert.Equal(2, response.TotalExecutions);
        Assert.Equal(0, response.SucceededExecutions); // None in Workspace B
        Assert.Equal(1, response.FailedExecutions);
        Assert.Equal(1, response.UnknownExecutions);
    }

    [Fact]
    public async Task GetOverview_ShouldAggregateExecutionCounts()
    {
        var workspaceId = Guid.NewGuid();
        var rule = AutomationRule.Create(workspaceId, "Rule", TriggerType.EmailReceived);
        _dbContext.AutomationRules.Add(rule);

        _dbContext.AutomationExecutions.Add(AutomationExecution.Create(rule.Id, Guid.NewGuid(), "Succeeded"));
        _dbContext.AutomationExecutions.Add(AutomationExecution.Create(rule.Id, Guid.NewGuid(), "Succeeded"));
        _dbContext.AutomationExecutions.Add(AutomationExecution.Create(rule.Id, Guid.NewGuid(), "Failed"));
        _dbContext.AutomationExecutions.Add(AutomationExecution.Create(rule.Id, Guid.NewGuid(), "Unknown"));
        _dbContext.AutomationExecutions.Add(AutomationExecution.Create(rule.Id, Guid.NewGuid(), "Pending"));
        _dbContext.AutomationExecutions.Add(AutomationExecution.Create(rule.Id, Guid.NewGuid(), "Executing"));

        await _dbContext.SaveChangesAsync();

        var client = CreateAuthenticatedClient(workspaceId);
        var response = await client.GetFromJsonAsync<AutomationOverviewDto>("/api/v1/automation/analytics/overview");

        Assert.NotNull(response);
        Assert.Equal(6, response.TotalExecutions);
        Assert.Equal(2, response.SucceededExecutions);
        Assert.Equal(1, response.FailedExecutions);
        Assert.Equal(1, response.UnknownExecutions);
        Assert.Equal(2, response.PendingExecutions); // Pending + Executing
    }

    [Fact]
    public async Task GetOverview_ShouldCalculateSuccessRate()
    {
        var workspaceId = Guid.NewGuid();
        var rule = AutomationRule.Create(workspaceId, "Rule", TriggerType.EmailReceived);
        _dbContext.AutomationRules.Add(rule);

        // 3 Succeeded, 1 Failed, 1 Unknown, 5 Pending
        for (int i = 0; i < 3; i++) _dbContext.AutomationExecutions.Add(AutomationExecution.Create(rule.Id, Guid.NewGuid(), "Succeeded"));
        _dbContext.AutomationExecutions.Add(AutomationExecution.Create(rule.Id, Guid.NewGuid(), "Failed"));
        _dbContext.AutomationExecutions.Add(AutomationExecution.Create(rule.Id, Guid.NewGuid(), "Unknown"));
        for (int i = 0; i < 5; i++) _dbContext.AutomationExecutions.Add(AutomationExecution.Create(rule.Id, Guid.NewGuid(), "Pending"));

        await _dbContext.SaveChangesAsync();

        var client = CreateAuthenticatedClient(workspaceId);
        var response = await client.GetFromJsonAsync<AutomationOverviewDto>("/api/v1/automation/analytics/overview");

        Assert.NotNull(response);
        // Success Rate = 3 / (3 + 1 + 1) = 3 / 5 = 0.6
        Assert.Equal(0.6, response.SuccessRate);
    }

    // 4-6 Rule Analytics
    [Fact]
    public async Task RuleAnalytics_ShouldReturnOnlyCurrentWorkspace()
    {
        var workspaceA = Guid.NewGuid();
        var workspaceB = Guid.NewGuid();

        var ruleA = AutomationRule.Create(workspaceA, "Rule A", TriggerType.EmailReceived);
        _dbContext.AutomationRules.Add(ruleA);
        await _dbContext.SaveChangesAsync();

        var clientB = CreateAuthenticatedClient(workspaceB);
        var response = await clientB.GetAsync($"/api/v1/automation/rules/{ruleA.Id}/analytics");

        Assert.False(response.IsSuccessStatusCode); // MediatR failure results in 400 or 404
    }

    [Fact]
    public async Task RuleAnalytics_ShouldAggregateCorrectly()
    {
        var workspaceId = Guid.NewGuid();
        var rule = AutomationRule.Create(workspaceId, "Rule", TriggerType.EmailReceived);
        _dbContext.AutomationRules.Add(rule);

        _dbContext.AutomationExecutions.Add(AutomationExecution.Create(rule.Id, Guid.NewGuid(), "Succeeded"));
        _dbContext.AutomationExecutions.Add(AutomationExecution.Create(rule.Id, Guid.NewGuid(), "Failed"));
        await _dbContext.SaveChangesAsync();

        var client = CreateAuthenticatedClient(workspaceId);
        var response = await client.GetFromJsonAsync<RuleAnalyticsDto>($"/api/v1/automation/rules/{rule.Id}/analytics");

        Assert.NotNull(response);
        Assert.Equal(2, response.TotalExecutions);
        Assert.Equal(1, response.SucceededExecutions);
        Assert.Equal(1, response.FailedExecutions);
        Assert.Equal(0.5, response.SuccessRate);
    }

    [Fact]
    public async Task RuleAnalytics_ShouldReturnZeroForNoExecutions()
    {
        var workspaceId = Guid.NewGuid();
        var rule = AutomationRule.Create(workspaceId, "Rule", TriggerType.EmailReceived);
        _dbContext.AutomationRules.Add(rule);
        await _dbContext.SaveChangesAsync();

        var client = CreateAuthenticatedClient(workspaceId);
        var response = await client.GetFromJsonAsync<RuleAnalyticsDto>($"/api/v1/automation/rules/{rule.Id}/analytics");

        Assert.NotNull(response);
        Assert.Equal(0, response.TotalExecutions);
        Assert.Equal(0, response.SuccessRate);
    }

    // 7-9 Execution Analytics
    [Fact]
    public async Task ExecutionAnalytics_ShouldFilterByDateRange()
    {
        var workspaceId = Guid.NewGuid();
        var rule = AutomationRule.Create(workspaceId, "Rule", TriggerType.EmailReceived);
        _dbContext.AutomationRules.Add(rule);
        
        var exec1 = AutomationExecution.Create(rule.Id, Guid.NewGuid(), "Succeeded");
        var exec2 = AutomationExecution.Create(rule.Id, Guid.NewGuid(), "Failed");

        // Use reflection to manipulate CreatedAt since it's set in Create() and EF InMemory doesn't support raw SQL
        var prop = typeof(AutomationExecution).GetProperty("CreatedAt");
        prop?.SetValue(exec1, DateTimeOffset.Parse("2020-01-01T00:00:00Z"));

        _dbContext.AutomationExecutions.AddRange(exec1, exec2);
        await _dbContext.SaveChangesAsync();

        var client = CreateAuthenticatedClient(workspaceId);
        
        // Filter out exec1
        var response = await client.GetFromJsonAsync<PagedList<ExecutionAnalyticsDto>>("/api/v1/automation/analytics/executions?from=2021-01-01T00:00:00Z");
        
        Assert.NotNull(response);
        Assert.Equal(1, response.TotalCount);
        Assert.Equal(exec2.Id, response.Items[0].Id);
    }

    [Fact]
    public async Task ExecutionAnalytics_ShouldFilterByRule()
    {
        var workspaceId = Guid.NewGuid();
        var rule1 = AutomationRule.Create(workspaceId, "Rule 1", TriggerType.EmailReceived);
        var rule2 = AutomationRule.Create(workspaceId, "Rule 2", TriggerType.EmailReceived);
        _dbContext.AutomationRules.AddRange(rule1, rule2);

        _dbContext.AutomationExecutions.Add(AutomationExecution.Create(rule1.Id, Guid.NewGuid(), "Succeeded"));
        _dbContext.AutomationExecutions.Add(AutomationExecution.Create(rule2.Id, Guid.NewGuid(), "Failed"));
        await _dbContext.SaveChangesAsync();

        var client = CreateAuthenticatedClient(workspaceId);
        
        var response = await client.GetFromJsonAsync<PagedList<ExecutionAnalyticsDto>>($"/api/v1/automation/analytics/executions?ruleId={rule1.Id}");
        
        Assert.NotNull(response);
        Assert.Equal(1, response.TotalCount);
        Assert.Equal(rule1.Id, response.Items[0].RuleId);
    }

    [Fact]
    public async Task ExecutionAnalytics_ShouldAggregateStatuses()
    {
        var workspaceId = Guid.NewGuid();
        var rule = AutomationRule.Create(workspaceId, "Rule", TriggerType.EmailReceived);
        _dbContext.AutomationRules.Add(rule);

        _dbContext.AutomationExecutions.Add(AutomationExecution.Create(rule.Id, Guid.NewGuid(), "Succeeded"));
        _dbContext.AutomationExecutions.Add(AutomationExecution.Create(rule.Id, Guid.NewGuid(), "Failed"));
        await _dbContext.SaveChangesAsync();

        var client = CreateAuthenticatedClient(workspaceId);
        
        var response = await client.GetFromJsonAsync<PagedList<ExecutionAnalyticsDto>>("/api/v1/automation/analytics/executions?status=Failed");
        
        Assert.NotNull(response);
        Assert.Equal(1, response.TotalCount);
        Assert.Equal("Failed", response.Items[0].Status);
    }

    // 10-12 Safety
    [Fact]
    public async Task UnknownAnalytics_ShouldExposeUnknownExecutions()
    {
        var workspaceId = Guid.NewGuid();
        var rule = AutomationRule.Create(workspaceId, "Rule", TriggerType.EmailReceived);
        _dbContext.AutomationRules.Add(rule);

        _dbContext.AutomationExecutions.Add(AutomationExecution.Create(rule.Id, Guid.NewGuid(), "Unknown"));
        await _dbContext.SaveChangesAsync();

        var client = CreateAuthenticatedClient(workspaceId);
        
        var response = await client.GetFromJsonAsync<PagedList<ExecutionAnalyticsDto>>("/api/v1/automation/analytics/executions?status=Unknown");
        
        Assert.NotNull(response);
        Assert.Equal(1, response.TotalCount);
        Assert.Equal("Unknown", response.Items[0].Status);
    }

    [Fact]
    public async Task UnknownAnalytics_ShouldNotMutateExecutionState()
    {
        var workspaceId = Guid.NewGuid();
        var rule = AutomationRule.Create(workspaceId, "Rule", TriggerType.EmailReceived);
        _dbContext.AutomationRules.Add(rule);

        var execution = AutomationExecution.Create(rule.Id, Guid.NewGuid(), "Unknown");
        _dbContext.AutomationExecutions.Add(execution);
        await _dbContext.SaveChangesAsync();

        var client = CreateAuthenticatedClient(workspaceId);
        var response = await client.GetAsync("/api/v1/automation/analytics/overview");
        response.EnsureSuccessStatusCode();

        await _dbContext.Entry(execution).ReloadAsync();
        Assert.Equal("Unknown", execution.Status); // Assert state is unmodified
    }

    [Fact]
    public async Task AnalyticsQuery_ShouldNotTriggerExecution()
    {
        var workspaceId = Guid.NewGuid();
        var rule = AutomationRule.Create(workspaceId, "Rule", TriggerType.EmailReceived);
        _dbContext.AutomationRules.Add(rule);
        
        var execution = AutomationExecution.Create(rule.Id, Guid.NewGuid(), "Unknown");
        _dbContext.AutomationExecutions.Add(execution);
        await _dbContext.SaveChangesAsync();

        var client = CreateAuthenticatedClient(workspaceId);
        
        // Query everything
        await client.GetAsync("/api/v1/automation/analytics/overview");
        await client.GetAsync($"/api/v1/automation/rules/{rule.Id}/analytics");
        await client.GetAsync("/api/v1/automation/analytics/executions");

        // Reload execution count from DB
        await _dbContext.Entry(rule).ReloadAsync();
        Assert.Equal(0, rule.ExecutionCount); // Not incremented, meaning engine was not invoked
    }
}
