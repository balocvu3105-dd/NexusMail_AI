using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using NexusMail.Contracts.Automation;
using NexusMail.Domain.Automation.Entities;
using NexusMail.Domain.Automation.Enums;
using NexusMail.Infrastructure.Persistence;
using MassTransit.Testing;
using MassTransit;

namespace NexusMail.IntegrationTests.Automation
{
    public class AutomationVerificationTests : IClassFixture<CustomWebApplicationFactory<NexusMail.API.ApiMarker>>
    {
        private readonly CustomWebApplicationFactory<NexusMail.API.ApiMarker> _factory;

        public AutomationVerificationTests(CustomWebApplicationFactory<NexusMail.API.ApiMarker> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task ImmediateEvaluationOfAiDependentRule_ShouldNotExecuteAction()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var testHarness = scope.ServiceProvider.GetRequiredService<ITestHarness>();

            var workspaceId = Guid.NewGuid();
            var emailId = Guid.NewGuid();

            var rule = AutomationRule.Create(
                workspaceId, 
                "Require AI", 
                TriggerType.EmailReceived, 
                @"{""type"":""comparison"",""Property"":""Category"",""Operator"":""=="",""Value"":""Invoice""}",
                @"[{""Type"":""Label"",""Parameters"":{""LabelName"":""Processing""}}]"
            );
            
            dbContext.AutomationRules.Add(rule);
            await dbContext.SaveChangesAsync();

            var evaluateMessage = new EvaluateRulesMessage
            {
                EmailId = emailId,
                WorkspaceId = workspaceId,
                Subject = "Hello",
                Sender = "test@test.com"
            };

            // Act
            var service = scope.ServiceProvider.GetRequiredService<NexusMail.Automation.RuleEngine.IRuleEvaluationService>();
            await service.EvaluateAsync(evaluateMessage, default);

            // Assert
            var executions = await dbContext.AutomationExecutions.Where(x => x.EmailId == emailId).ToListAsync();
            executions.Should().BeEmpty();

            var metrics = await dbContext.AutomationRuleMetrics.Where(x => x.EmailId == emailId).ToListAsync();
            metrics.Should().ContainSingle();
            metrics[0].Result.Should().Be("NotApplicable");
        }

        [Fact]
        public async Task ConcurrentSameRuleSameEmail_ShouldCreateSingleExecution()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            var workspaceId = Guid.NewGuid();
            var emailId = Guid.NewGuid();
            var ruleId = Guid.NewGuid();

            var evaluateMessage = new EvaluateRulesMessage
            {
                EmailId = emailId,
                WorkspaceId = workspaceId,
                Subject = "Invoice",
                Sender = "test@test.com"
            };

            var rule = AutomationRule.Create(
                workspaceId, 
                "Concurrent Test", 
                TriggerType.EmailReceived, 
                @"{""type"":""comparison"",""Property"":""Subject"",""Operator"":""=="",""Value"":""Invoice""}",
                @"[{""Type"":""Label"",""Parameters"":{""LabelName"":""Processing""}}]"
            );
            
            // Bypass the Create method's new Guid generation by reflection or just letting it generate one
            dbContext.AutomationRules.Add(rule);
            await dbContext.SaveChangesAsync();

            // Run concurrently using separate scopes to avoid DbContext threading issues
            var t1 = Task.Run(async () => {
                using var scope1 = _factory.Services.CreateScope();
                var s1 = scope1.ServiceProvider.GetRequiredService<NexusMail.Automation.RuleEngine.IRuleEvaluationService>();
                await s1.EvaluateAsync(evaluateMessage, default);
            });
            var t2 = Task.Run(async () => {
                using var scope2 = _factory.Services.CreateScope();
                var s2 = scope2.ServiceProvider.GetRequiredService<NexusMail.Automation.RuleEngine.IRuleEvaluationService>();
                await s2.EvaluateAsync(evaluateMessage, default);
            });
            var t3 = Task.Run(async () => {
                using var scope3 = _factory.Services.CreateScope();
                var s3 = scope3.ServiceProvider.GetRequiredService<NexusMail.Automation.RuleEngine.IRuleEvaluationService>();
                await s3.EvaluateAsync(evaluateMessage, default);
            });

            await Task.WhenAll(t1, t2, t3);

            // Assert
            var executions = await dbContext.AutomationExecutions
                .Where(x => x.EmailId == emailId && x.RuleId == rule.Id)
                .ToListAsync();

            executions.Should().ContainSingle("Concurrent evaluations should only result in one AutomationExecution record due to unique constraint handling.");

            // Ghost Event Protection Check
            var outboxMessages = await dbContext.OutboxMessages
                .Where(x => x.Type.Contains("RuleMatchedDomainEvent") && x.Content.Contains(emailId.ToString()))
                .ToListAsync();
            
            outboxMessages.Should().ContainSingle("Only one OutboxMessage should be created because the failed transactions rolled back their Outbox insertions");

        }

        [Fact]
        public async Task SameRuleWithTwoForwardActions_ShouldExecuteBothIndependently()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var testHarness = scope.ServiceProvider.GetRequiredService<ITestHarness>();
            
            var workspaceId = Guid.NewGuid();
            var emailId = Guid.NewGuid();

            var rule = AutomationRule.Create(
                workspaceId, 
                "Two Forwards", 
                TriggerType.EmailReceived, 
                @"{""type"":""comparison"",""Property"":""Subject"",""Operator"":""=="",""Value"":""Invoice""}",
                @"[{""Type"":""ForwardEmail"",""Parameters"":{""to"":""a@test.com""}},{""Type"":""ForwardEmail"",""Parameters"":{""to"":""b@test.com""}}]"
            );
            
            dbContext.AutomationRules.Add(rule);
            await dbContext.SaveChangesAsync();

            var executionId = Guid.NewGuid();
            
            var actionEvent = new ActionExecutionEvent
            {
                ExecutionId = executionId,
                RuleId = rule.Id,
                EmailId = emailId,
                WorkspaceId = workspaceId,
                ActionsJson = rule.ActionsJson,
                EvaluateContext = new EvaluateRulesMessage { EmailId = emailId }
            };

            // Initialize pending action executions
            dbContext.AutomationActionExecutions.Add(AutomationActionExecution.Create(executionId, "0", "ForwardEmail"));
            dbContext.AutomationActionExecutions.Add(AutomationActionExecution.Create(executionId, "1", "ForwardEmail"));
            await dbContext.SaveChangesAsync();

            // Act
            var executorFactory = scope.ServiceProvider.GetRequiredService<NexusMail.Automation.Actions.ActionExecutorFactory>();
            var logger = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<NexusMail.Worker.Automation.Consumers.ActionExecutionConsumer>>();
            var consumer = new NexusMail.Worker.Automation.Consumers.ActionExecutionConsumer(executorFactory, logger, dbContext);

            var mockContext = new Moq.Mock<MassTransit.ConsumeContext<ActionExecutionEvent>>();
            mockContext.Setup(c => c.Message).Returns(actionEvent);
            mockContext.Setup(c => c.CancellationToken).Returns(System.Threading.CancellationToken.None);

            await consumer.Consume(mockContext.Object);

            // Assert

            var actionExecutions = await dbContext.AutomationActionExecutions
                .Where(x => x.ExecutionId == executionId)
                .ToListAsync();

            actionExecutions.Should().HaveCount(2);
            actionExecutions.Should().AllSatisfy(a => a.Status.Should().Be("Success"));
            
            var audits = await dbContext.AutomationAudits
                .Where(x => x.RuleId == rule.Id)
                .ToListAsync();

            audits.Should().HaveCount(2);
        }

        [Fact]
        public async Task RuleEvaluation_ShouldRouteDomainEventThroughOutboxInterceptor()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            var workspaceId = Guid.NewGuid();
            var emailId = Guid.NewGuid();

            var evaluateMessage = new EvaluateRulesMessage
            {
                EmailId = emailId,
                WorkspaceId = workspaceId,
                Subject = "Architecture Validation",
                Sender = "test@test.com"
            };

            var rule = AutomationRule.Create(
                workspaceId, 
                "Outbox Interceptor Test", 
                TriggerType.EmailReceived, 
                @"{""type"":""comparison"",""Property"":""Subject"",""Operator"":""=="",""Value"":""Architecture Validation""}",
                @"[{""Type"":""Label"",""Parameters"":{""LabelName"":""Processing""}}]"
            );
            
            dbContext.AutomationRules.Add(rule);
            await dbContext.SaveChangesAsync();

            // Act: Evaluate the rule directly via the service
            var service = scope.ServiceProvider.GetRequiredService<NexusMail.Automation.RuleEngine.IRuleEvaluationService>();
            await service.EvaluateAsync(evaluateMessage, default);

            // Assert: Verify that the DB now contains the OutboxMessage. 
            // Since RuleEvaluationService ONLY uses execution.RaiseRuleMatchedEvent(), 
            // the presence of this OutboxMessage is hard proof that the Step 24 
            // ConvertDomainEventsToOutboxMessagesInterceptor correctly intercepted and persisted it.
            var outboxMessages = await dbContext.OutboxMessages
                .Where(x => x.Type.Contains("RuleMatchedDomainEvent") && x.Content.Contains(emailId.ToString()))
                .ToListAsync();

            outboxMessages.Should().ContainSingle("The RuleMatchedDomainEvent should have been intercepted by the Step 24 EF Core Interceptor and converted to an OutboxMessage.");
            var outboxMsg = outboxMessages.First();
            outboxMsg.ProcessedOnUtc.Should().BeNull("The background dispatcher has not processed it yet in this test context.");
        }
    }
}
