using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexusMail.Contracts.Automation;
using NexusMail.Domain.Automation.Entities;
using NexusMail.Domain.Automation.Enums;
using NexusMail.Infrastructure.Persistence;
using Xunit;

namespace NexusMail.IntegrationTests.Automation;

/// <summary>
/// Step 31 — AI-dependent Automation Rules
///
/// These tests prove the three-way semantic distinction:
///   Match        = system knows the value and it satisfies the condition
///   NoMatch      = system knows the value and it does NOT satisfy the condition
///   NotApplicable = system does not know the value (AI context absent or failed)
///
/// Critical invariant: Failed AI capability ≠ False AI value.
/// "Unknown" must never be silently converted to "False".
/// </summary>
public class Step31_AIDependentRulesTests : IClassFixture<CustomWebApplicationFactory<NexusMail.API.ApiMarker>>
{
    private readonly CustomWebApplicationFactory<NexusMail.API.ApiMarker> _factory;
    private readonly ApplicationDbContext _dbContext;

    public Step31_AIDependentRulesTests(CustomWebApplicationFactory<NexusMail.API.ApiMarker> factory)
    {
        _factory = factory;
        var scope = factory.Services.CreateScope();
        _dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Contract A — NotApplicable on First Pass (no AI context in EvaluateRulesMessage)
    // Proves: AI-dependent rule does not fire prematurely during EmailReceived evaluation
    // ─────────────────────────────────────────────────────────────────────────────
    [Fact]
    public async Task AIDependentRule_WithNoAIContext_ShouldBeNotApplicable_AndNotCreateExecution()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var workspaceId = Guid.NewGuid();
        var emailId = Guid.NewGuid();

        // Rule: category == "Invoice" — requires AI classification context
        var rule = AutomationRule.Create(
            workspaceId,
            "Invoice Rule",
            TriggerType.EmailReceived,
            conditionsJson: """{"type":"comparison","property":"category","operator":"==","value":"Invoice"}""",
            actionsJson: """[{"type":"Label","parameters":{"labelName":"Invoice"}}]"""
        );
        db.AutomationRules.Add(rule);
        await db.SaveChangesAsync();

        // Simulate EmailReceived first-pass: empty AIMetadata (AI has not completed yet)
        var message = new EvaluateRulesMessage
        {
            EmailId = emailId,
            WorkspaceId = workspaceId,
            Subject = "Invoice for March",
            Sender = "billing@acme.com",
            AIMetadata = new Dictionary<string, object>()  // No AI keys — classification not yet run
        };

        // Act
        var service = scope.ServiceProvider.GetRequiredService<NexusMail.Automation.RuleEngine.IRuleEvaluationService>();
        await service.EvaluateAsync(message, default);

        // Assert
        var executions = await db.AutomationExecutions.Where(x => x.EmailId == emailId).ToListAsync();
        executions.Should().BeEmpty("no execution should be created when AI context is missing");

        var metrics = await db.AutomationRuleMetrics.Where(x => x.EmailId == emailId).ToListAsync();
        metrics.Should().ContainSingle();
        metrics[0].Result.Should().Be("NotApplicable",
            "absent AI key must yield NotApplicable, not NoMatch or Match");

        // Also verify RequiresAI derived property
        rule.RequiresAI.Should().BeTrue("category is an AI property");
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Contract B — Match on Second Pass (AI context available and satisfies condition)
    // Proves: AI-dependent rule fires correctly when AIProcessingCompleted context arrives
    // ─────────────────────────────────────────────────────────────────────────────
    [Fact]
    public async Task AIDependentRule_WithMatchingAIContext_ShouldMatch_AndCreateExecution()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var workspaceId = Guid.NewGuid();
        var emailId = Guid.NewGuid();

        var rule = AutomationRule.Create(
            workspaceId,
            "Invoice Rule",
            TriggerType.AIProcessingCompleted,
            conditionsJson: """{"type":"comparison","property":"category","operator":"==","value":"Invoice"}""",
            actionsJson: """[{"type":"Label","parameters":{"labelName":"Invoice"}}]"""
        );
        db.AutomationRules.Add(rule);
        await db.SaveChangesAsync();

        // Simulate AIProcessingCompleted second-pass: category = "Invoice" available
        var message = new EvaluateRulesMessage
        {
            EmailId = emailId,
            WorkspaceId = workspaceId,
            Subject = "Invoice for March",
            Sender = "billing@acme.com",
            AIMetadata = new Dictionary<string, object>
            {
                { "category", "Invoice" }  // Classification Succeeded, key populated
            }
        };

        // Act
        var service = scope.ServiceProvider.GetRequiredService<NexusMail.Automation.RuleEngine.IRuleEvaluationService>();
        await service.EvaluateAsync(message, default);

        // Assert
        var executions = await db.AutomationExecutions.Where(x => x.EmailId == emailId).ToListAsync();
        executions.Should().ContainSingle("exactly one execution should be created when rule matches");
        executions[0].Status.Should().Be("Matched");

        var metrics = await db.AutomationRuleMetrics.Where(x => x.EmailId == emailId).ToListAsync();
        metrics.Should().ContainSingle();
        metrics[0].Result.Should().Be("Matched");
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Contract C — Idempotency on Duplicate Delivery
    // Proves: at-least-once delivery of AIProcessingCompleted is safe
    // ─────────────────────────────────────────────────────────────────────────────
    [Fact]
    public async Task AIDependentRule_WithDuplicateDelivery_ShouldCreateOnlyOneExecution()
    {
        // Arrange
        using var scope1 = _factory.Services.CreateScope();
        using var scope2 = _factory.Services.CreateScope();
        var db1 = scope1.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var db2 = scope2.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var workspaceId = Guid.NewGuid();
        var emailId = Guid.NewGuid();

        var rule = AutomationRule.Create(
            workspaceId,
            "Priority Rule",
            TriggerType.AIProcessingCompleted,
            conditionsJson: """{"type":"comparison","property":"priorityscore","operator":">=","value":8}""",
            actionsJson: """[{"type":"Label","parameters":{"labelName":"Urgent"}}]"""
        );
        db1.AutomationRules.Add(rule);
        await db1.SaveChangesAsync();

        var message = new EvaluateRulesMessage
        {
            EmailId = emailId,
            WorkspaceId = workspaceId,
            Subject = "Critical: Production Down",
            Sender = "ops@company.com",
            AIMetadata = new Dictionary<string, object>
            {
                { "priorityscore", 9 }  // High priority
            }
        };

        // Act — evaluate twice (simulating at-least-once delivery)
        var service1 = scope1.ServiceProvider.GetRequiredService<NexusMail.Automation.RuleEngine.IRuleEvaluationService>();
        var service2 = scope2.ServiceProvider.GetRequiredService<NexusMail.Automation.RuleEngine.IRuleEvaluationService>();

        await service1.EvaluateAsync(message, default);
        await service2.EvaluateAsync(message, default);  // Duplicate delivery

        // Assert
        using var verifyScope = _factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var executions = await verifyDb.AutomationExecutions.Where(x => x.EmailId == emailId).ToListAsync();
        executions.Should().ContainSingle(
            "idempotency: (RuleId, EmailId) unique constraint must prevent duplicate execution");
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Contract D — Partial AI Failure → NotApplicable (Not NoMatch, Not False)
    // The critical "Unknown ≠ False" semantic test
    // ─────────────────────────────────────────────────────────────────────────────
    [Fact]
    public async Task AIDependentRule_WithFailedAICapability_ShouldBeNotApplicable_NotNoMatch()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var workspaceId = Guid.NewGuid();
        var emailId = Guid.NewGuid();

        // Rule: category == "Invoice"
        var rule = AutomationRule.Create(
            workspaceId,
            "Invoice Rule — Partial Failure Test",
            TriggerType.AIProcessingCompleted,
            conditionsJson: """{"type":"comparison","property":"category","operator":"==","value":"Invoice"}""",
            actionsJson: """[{"type":"Label","parameters":{"labelName":"Invoice"}}]"""
        );
        db.AutomationRules.Add(rule);
        await db.SaveChangesAsync();

        // Classification Failed → AIProcessingCompletedConsumer does NOT add "category" key
        // This simulates the correctly-fixed consumer behavior from Step 31
        var message = new EvaluateRulesMessage
        {
            EmailId = emailId,
            WorkspaceId = workspaceId,
            Subject = "Invoice for March",
            Sender = "billing@acme.com",
            AIMetadata = new Dictionary<string, object>()  // No "category" key — classification failed
        };

        // Act
        var service = scope.ServiceProvider.GetRequiredService<NexusMail.Automation.RuleEngine.IRuleEvaluationService>();
        await service.EvaluateAsync(message, default);

        // Assert
        var executions = await db.AutomationExecutions.Where(x => x.EmailId == emailId).ToListAsync();
        executions.Should().BeEmpty("failed classification must not accidentally match or fail the rule");

        var metrics = await db.AutomationRuleMetrics.Where(x => x.EmailId == emailId).ToListAsync();
        metrics.Should().ContainSingle();
        metrics[0].Result.Should().Be("NotApplicable",
            "failed capability → absent key → MissingContextException → NotApplicable, never NoMatch");
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Contract E — Known Non-Matching Value → NoMatch (NOT NotApplicable)
    // Distinguishes "unknown" from "known but wrong"
    // Proves: Failed ≠ False distinction is symmetric — NoMatch when value is present but wrong
    // ─────────────────────────────────────────────────────────────────────────────
    [Fact]
    public async Task AIDependentRule_WithKnownNonMatchingAIValue_ShouldBeNoMatch_NotNotApplicable()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var workspaceId = Guid.NewGuid();
        var emailId = Guid.NewGuid();

        // Rule: category == "Invoice"
        var rule = AutomationRule.Create(
            workspaceId,
            "Invoice Rule — NoMatch Test",
            TriggerType.AIProcessingCompleted,
            conditionsJson: """{"type":"comparison","property":"category","operator":"==","value":"Invoice"}""",
            actionsJson: """[{"type":"Label","parameters":{"labelName":"Invoice"}}]"""
        );
        db.AutomationRules.Add(rule);
        await db.SaveChangesAsync();

        // Classification Succeeded, but category = "Spam" (not "Invoice") → NoMatch
        var message = new EvaluateRulesMessage
        {
            EmailId = emailId,
            WorkspaceId = workspaceId,
            Subject = "You won a prize!",
            Sender = "spam@phishing.com",
            AIMetadata = new Dictionary<string, object>
            {
                { "category", "Spam" }  // Key present, value known, but doesn't match "Invoice"
            }
        };

        // Act
        var service = scope.ServiceProvider.GetRequiredService<NexusMail.Automation.RuleEngine.IRuleEvaluationService>();
        await service.EvaluateAsync(message, default);

        // Assert — must be NoMatch, NOT NotApplicable
        var executions = await db.AutomationExecutions.Where(x => x.EmailId == emailId).ToListAsync();
        executions.Should().BeEmpty();

        var metrics = await db.AutomationRuleMetrics.Where(x => x.EmailId == emailId).ToListAsync();
        metrics.Should().ContainSingle();
        metrics[0].Result.Should().Be("Evaluated",  // RuleEvaluationService maps NoMatch → "Evaluated"
            "known non-matching value must yield NoMatch/Evaluated, not NotApplicable — system knows the value");
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Contract F — tags CONTAINS through full serialization boundary
    // Proves: List<string> tags survive the AIMetadata → Dictionary<string, object> → CONTAINS path
    // ─────────────────────────────────────────────────────────────────────────────
    [Fact]
    public async Task AIDependentRule_TagsContains_ShouldMatchThroughSerializationBoundary()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var workspaceId = Guid.NewGuid();
        var emailId = Guid.NewGuid();

        // Rule: tags CONTAINS "urgent"
        var rule = AutomationRule.Create(
            workspaceId,
            "Urgent Tag Rule",
            TriggerType.AIProcessingCompleted,
            conditionsJson: """{"type":"comparison","property":"tags","operator":"CONTAINS","value":"urgent"}""",
            actionsJson: """[{"type":"Label","parameters":{"labelName":"Urgent"}}]"""
        );
        db.AutomationRules.Add(rule);
        await db.SaveChangesAsync();

        // AIMetadata populated with tags as List<string> (as AIProcessingCompletedConsumer would do)
        // The key question is: does CONTAINS work when tags goes through Dictionary<string,object> boxing?
        var message = new EvaluateRulesMessage
        {
            EmailId = emailId,
            WorkspaceId = workspaceId,
            Subject = "Server outage",
            Sender = "alerts@ops.com",
            AIMetadata = new Dictionary<string, object>
            {
                { "category", "Infrastructure" },
                { "tags", new List<string> { "urgent", "production", "outage" } }  // List<string> boxed as object
            }
        };

        // Act
        var service = scope.ServiceProvider.GetRequiredService<NexusMail.Automation.RuleEngine.IRuleEvaluationService>();
        await service.EvaluateAsync(message, default);

        // Assert — CONTAINS must work with boxed List<string>
        var executions = await db.AutomationExecutions.Where(x => x.EmailId == emailId).ToListAsync();
        executions.Should().ContainSingle(
            "tags CONTAINS 'urgent' should match when the tag is present in the AI-classified tags list");

        var metrics = await db.AutomationRuleMetrics.Where(x => x.EmailId == emailId).ToListAsync();
        metrics.Should().ContainSingle();
        metrics[0].Result.Should().Be("Matched");
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Contract G — Failed ≠ False (Direct Comparison)
    // The MOST important test in Step 31.
    // Runs both scenarios in a single test to prove they produce DIFFERENT results:
    //   category absent  → NotApplicable  ("I don't know the category")
    //   category = "Spam" → NoMatch        ("I know the category, it's not Invoice")
    // If both yielded the same result, the system would be silently converting
    // "unknown" into "false" — a semantic bug that Step 29–31 exists to prevent.
    // ─────────────────────────────────────────────────────────────────────────────
    [Fact]
    public async Task Contract_G_FailedNotEqualFalse_AbsentCategory_Vs_WrongCategory_MustDiffer()
    {
        // ── Scenario 1: category ABSENT (classification failed) ──
        using var scope1 = _factory.Services.CreateScope();
        var db1 = scope1.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var workspaceId = Guid.NewGuid();
        var emailId_absent = Guid.NewGuid();

        var rule1 = AutomationRule.Create(
            workspaceId,
            "Invoice Rule — Absent Category",
            TriggerType.AIProcessingCompleted,
            conditionsJson: """{"type":"comparison","property":"category","operator":"==","value":"Invoice"}""",
            actionsJson: """[{"type":"Label","parameters":{"labelName":"Invoice"}}]"""
        );
        db1.AutomationRules.Add(rule1);
        await db1.SaveChangesAsync();

        var message_absent = new EvaluateRulesMessage
        {
            EmailId = emailId_absent,
            WorkspaceId = workspaceId,
            Subject = "Invoice for March",
            Sender = "billing@acme.com",
            AIMetadata = new Dictionary<string, object>()  // No "category" key — classification failed
        };

        var service1 = scope1.ServiceProvider.GetRequiredService<NexusMail.Automation.RuleEngine.IRuleEvaluationService>();
        await service1.EvaluateAsync(message_absent, default);

        var metrics_absent = await db1.AutomationRuleMetrics
            .Where(x => x.EmailId == emailId_absent)
            .ToListAsync();

        // ── Scenario 2: category = "Spam" (classification succeeded, wrong value) ──
        using var scope2 = _factory.Services.CreateScope();
        var db2 = scope2.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var emailId_wrong = Guid.NewGuid();

        // Reuse the same rule — it's already in the DB
        var message_wrong = new EvaluateRulesMessage
        {
            EmailId = emailId_wrong,
            WorkspaceId = workspaceId,
            Subject = "You won a prize!",
            Sender = "spam@phishing.com",
            AIMetadata = new Dictionary<string, object>
            {
                { "category", "Spam" }  // Key present, value known, doesn't match "Invoice"
            }
        };

        var service2 = scope2.ServiceProvider.GetRequiredService<NexusMail.Automation.RuleEngine.IRuleEvaluationService>();
        await service2.EvaluateAsync(message_wrong, default);

        var metrics_wrong = await db2.AutomationRuleMetrics
            .Where(x => x.EmailId == emailId_wrong)
            .ToListAsync();

        // ── THE CRITICAL ASSERTION: Results must be DIFFERENT ──
        metrics_absent.Should().ContainSingle();
        metrics_wrong.Should().ContainSingle();

        var resultAbsent = metrics_absent[0].Result;
        var resultWrong = metrics_wrong[0].Result;

        resultAbsent.Should().Be("NotApplicable",
            "absent category (classification failed) must yield NotApplicable — 'I don't know'");

        resultWrong.Should().Be("Evaluated",
            "known wrong category (Spam ≠ Invoice) must yield NoMatch/Evaluated — 'I know, it doesn't match'");

        resultAbsent.Should().NotBe(resultWrong,
            "CRITICAL: Failed ≠ False. 'Unknown' and 'known but wrong' MUST produce different semantic results. " +
            "If they are the same, the system is silently converting 'I don't know' into 'I know it's false'.");

        // Neither scenario should create an execution
        var executions_absent = await db1.AutomationExecutions.Where(x => x.EmailId == emailId_absent).ToListAsync();
        var executions_wrong = await db2.AutomationExecutions.Where(x => x.EmailId == emailId_wrong).ToListAsync();
        executions_absent.Should().BeEmpty();
        executions_wrong.Should().BeEmpty();
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Contract H — tags CONTAINS through JsonElement serialization boundary
    // Proves: tags survive serialization/deserialization and CONTAINS still works
    // when the runtime type is JsonElement (not List<string>).
    // This simulates what happens when AIMetadata goes through a message broker
    // (MassTransit serialization) where List<string> becomes JsonElement.
    // ─────────────────────────────────────────────────────────────────────────────
    [Fact]
    public async Task Contract_H_TagsContains_ThroughJsonElementBoundary_ShouldMatch()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var workspaceId = Guid.NewGuid();
        var emailId = Guid.NewGuid();

        var rule = AutomationRule.Create(
            workspaceId,
            "Urgent Tag Rule — JsonElement Path",
            TriggerType.AIProcessingCompleted,
            conditionsJson: """{"type":"comparison","property":"tags","operator":"CONTAINS","value":"urgent"}""",
            actionsJson: """[{"type":"Label","parameters":{"labelName":"Urgent"}}]"""
        );
        db.AutomationRules.Add(rule);
        await db.SaveChangesAsync();

        // Simulate the serialization boundary:
        // When AIMetadata goes through MassTransit JSON serialization/deserialization,
        // List<string> in Dictionary<string, object> becomes System.Text.Json.JsonElement.
        // We replicate this by round-tripping through JSON.
        var originalMetadata = new Dictionary<string, object>
        {
            { "category", "Infrastructure" },
            { "tags", new List<string> { "urgent", "production", "outage" } }
        };

        // Serialize → Deserialize to simulate message broker boundary
        var json = System.Text.Json.JsonSerializer.Serialize(originalMetadata);
        var deserializedMetadata = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(json,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        // Verify the type IS JsonElement after round-trip (proving we're testing the right path)
        deserializedMetadata["tags"].Should().BeOfType<System.Text.Json.JsonElement>(
            "after JSON round-trip, tags should be JsonElement, not List<string>");

        var message = new EvaluateRulesMessage
        {
            EmailId = emailId,
            WorkspaceId = workspaceId,
            Subject = "Server outage",
            Sender = "alerts@ops.com",
            AIMetadata = deserializedMetadata  // JsonElement, NOT List<string>
        };

        // Act
        var service = scope.ServiceProvider.GetRequiredService<NexusMail.Automation.RuleEngine.IRuleEvaluationService>();
        await service.EvaluateAsync(message, default);

        // Assert — CONTAINS must work with JsonElement array
        var executions = await db.AutomationExecutions.Where(x => x.EmailId == emailId).ToListAsync();
        executions.Should().ContainSingle(
            "tags CONTAINS 'urgent' must match even when tags is a JsonElement (post-serialization), " +
            "not just when it's a List<string> (in-process)");

        var metrics = await db.AutomationRuleMetrics.Where(x => x.EmailId == emailId).ToListAsync();
        metrics.Should().ContainSingle();
        metrics[0].Result.Should().Be("Matched");
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Contract I — RequiresAI is purely informational, does NOT change evaluation
    // Proves: RequiresAI == true does not create a parallel evaluation mechanism.
    // A rule with RequiresAI == true must still go through the normal path:
    //   GetAIProperty → MissingContextException → NotApplicable
    // RequiresAI must never be checked by RuleEvaluator or RuleEvaluationService.
    // ─────────────────────────────────────────────────────────────────────────────
    [Fact]
    public async Task Contract_I_RequiresAI_IsPurelyInformational_DoesNotAffectEvaluation()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var workspaceId = Guid.NewGuid();
        var emailId = Guid.NewGuid();

        // Rule with AI property (category) — RequiresAI should be true
        var aiRule = AutomationRule.Create(
            workspaceId,
            "AI Rule — RequiresAI Test",
            TriggerType.AIProcessingCompleted,
            conditionsJson: """{"type":"comparison","property":"category","operator":"==","value":"Invoice"}""",
            actionsJson: """[{"type":"Label","parameters":{"labelName":"Invoice"}}]"""
        );

        // Rule WITHOUT AI property — RequiresAI should be false
        var nonAiRule = AutomationRule.Create(
            workspaceId,
            "Non-AI Rule — RequiresAI Test",
            TriggerType.AIProcessingCompleted,
            conditionsJson: """{"type":"comparison","property":"subject","operator":"==","value":"Test Subject"}""",
            actionsJson: """[{"type":"Label","parameters":{"labelName":"Basic"}}]"""
        );

        db.AutomationRules.AddRange(aiRule, nonAiRule);
        await db.SaveChangesAsync();

        // Verify RequiresAI derived property
        aiRule.RequiresAI.Should().BeTrue("rule references 'category', an AI property");
        nonAiRule.RequiresAI.Should().BeFalse("rule only references 'subject', not an AI property");

        // Now evaluate both rules with matching context — RequiresAI should not influence the result
        var message = new EvaluateRulesMessage
        {
            EmailId = emailId,
            WorkspaceId = workspaceId,
            Subject = "Test Subject",
            Sender = "test@test.com",
            AIMetadata = new Dictionary<string, object>
            {
                { "category", "Invoice" }
            }
        };

        var service = scope.ServiceProvider.GetRequiredService<NexusMail.Automation.RuleEngine.IRuleEvaluationService>();
        await service.EvaluateAsync(message, default);

        // Both rules should match — RequiresAI == true does NOT prevent execution
        var executions = await db.AutomationExecutions.Where(x => x.EmailId == emailId).ToListAsync();
        executions.Should().HaveCount(2,
            "RequiresAI is informational only — both rules should evaluate and match normally");

        var metrics = await db.AutomationRuleMetrics.Where(x => x.EmailId == emailId).ToListAsync();
        metrics.Should().HaveCount(2);
        metrics.Should().AllSatisfy(m => m.Result.Should().Be("Matched",
            "RequiresAI must not create a parallel evaluation path or prevent matching"));

        // Now test WITHOUT AI context — both rules should still evaluate through normal mechanism
        var emailId2 = Guid.NewGuid();
        var message2 = new EvaluateRulesMessage
        {
            EmailId = emailId2,
            WorkspaceId = workspaceId,
            Subject = "Test Subject",
            Sender = "test@test.com",
            AIMetadata = new Dictionary<string, object>()  // No AI context
        };

        await service.EvaluateAsync(message2, default);

        var metrics2 = await db.AutomationRuleMetrics.Where(x => x.EmailId == emailId2).ToListAsync();
        metrics2.Should().HaveCount(2);

        // AI rule → NotApplicable (via MissingContextException, NOT via RequiresAI check)
        var aiMetric = metrics2.First(m => m.RuleId == aiRule.Id);
        aiMetric.Result.Should().Be("NotApplicable",
            "AI-dependent rule without context → NotApplicable via MissingContextException, not RequiresAI");

        // Non-AI rule → Match (subject matches)
        var nonAiMetric = metrics2.First(m => m.RuleId == nonAiRule.Id);
        nonAiMetric.Result.Should().Be("Matched",
            "Non-AI rule should match normally — RequiresAI == false has no bearing on evaluation");

        // Only the non-AI rule should have an execution for emailId2
        var executions2 = await db.AutomationExecutions.Where(x => x.EmailId == emailId2).ToListAsync();
        executions2.Should().ContainSingle();
        executions2[0].RuleId.Should().Be(nonAiRule.Id);
    }
}
