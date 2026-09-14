using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using NexusMail.Contracts.Automation;
using NexusMail.Domain.Automation.Entities;
using NexusMail.Infrastructure.Persistence;
using MassTransit;
using NexusMail.Application.Abstractions.Email;
using Moq;
using NexusMail.Worker.Automation.Consumers;

namespace NexusMail.IntegrationTests.Automation
{
    public class Step28_AnalyticsAndObservabilityTests : IClassFixture<CustomWebApplicationFactory<NexusMail.API.ApiMarker>>
    {
        private readonly CustomWebApplicationFactory<NexusMail.API.ApiMarker> _factory;

        public Step28_AnalyticsAndObservabilityTests(CustomWebApplicationFactory<NexusMail.API.ApiMarker> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task TestA_Success_AllActionsSuccess_ShouldUpdateRootToSucceeded()
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            var executionId = Guid.NewGuid();
            var root = AutomationExecution.Create(Guid.NewGuid(), Guid.NewGuid(), "Matched");
            
            typeof(AutomationExecution).GetProperty("Id").SetValue(root, executionId);
            
            dbContext.AutomationExecutions.Add(root);

            dbContext.AutomationActionExecutions.Add(AutomationActionExecution.Create(executionId, "0", "ForwardEmail"));
            dbContext.AutomationActionExecutions.Add(AutomationActionExecution.Create(executionId, "1", "ApplyLabel"));
            await dbContext.SaveChangesAsync();

            var mockEmailProvider = new Mock<IEmailProvider>();
            mockEmailProvider.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), default)).Returns(Task.CompletedTask);
            var mockExecutor = new Mock<NexusMail.Automation.Actions.IActionExecutor>();
            mockExecutor.SetupGet(x => x.ActionType).Returns(NexusMail.Domain.Automation.Enums.ActionType.ApplyLabel);
            mockExecutor.Setup(x => x.ExecuteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<EvaluateRulesMessage>(), It.IsAny<System.Threading.CancellationToken>())).ReturnsAsync(NexusMail.Domain.Automation.Enums.ActionResult.Success);

            var executorFactory = new NexusMail.Automation.Actions.ActionExecutorFactory(
                new NexusMail.Automation.Actions.IActionExecutor[] { 
                    new NexusMail.Automation.Actions.ForwardActionExecutor(mockEmailProvider.Object),
                    mockExecutor.Object 
                });
            
            var logger = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<ActionExecutionConsumer>>();
            var consumer = new ActionExecutionConsumer(executorFactory, logger, dbContext);

            var actionEvent = new ActionExecutionEvent
            {
                ExecutionId = executionId,
                RuleId = root.RuleId,
                EmailId = root.EmailId,
                ActionsJson = @"[{""Type"":""ForwardEmail"",""Parameters"":{""to"":""test@test.com""}}, {""Type"":""ApplyLabel"",""Parameters"":{""label"":""test""}}]",
                EvaluateContext = new EvaluateRulesMessage()
            };

            var mockContext = new Mock<ConsumeContext<ActionExecutionEvent>>();
            mockContext.Setup(c => c.Message).Returns(actionEvent);
            mockContext.Setup(c => c.CancellationToken).Returns(System.Threading.CancellationToken.None);

            await consumer.Consume(mockContext.Object);

            var updatedRoot = await dbContext.AutomationExecutions.FirstAsync(x => x.Id == executionId);
            updatedRoot.Status.Should().Be("Succeeded");
            updatedRoot.CompletedAt.Should().NotBeNull();
            updatedRoot.ErrorMessage.Should().BeNull();
        }

        [Fact]
        public async Task TestB_PermanentFailure_OneActionFailed_ShouldUpdateRootToFailed()
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            var executionId = Guid.NewGuid();
            var root = AutomationExecution.Create(Guid.NewGuid(), Guid.NewGuid(), "Matched");
            typeof(AutomationExecution).GetProperty("Id").SetValue(root, executionId);
            dbContext.AutomationExecutions.Add(root);

            dbContext.AutomationActionExecutions.Add(AutomationActionExecution.Create(executionId, "0", "ForwardEmail"));
            await dbContext.SaveChangesAsync();

            var executorFactory = new NexusMail.Automation.Actions.ActionExecutorFactory(
                new[] { new NexusMail.Automation.Actions.ForwardActionExecutor(new Mock<IEmailProvider>().Object) });
            
            var logger = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<ActionExecutionConsumer>>();
            var consumer = new ActionExecutionConsumer(executorFactory, logger, dbContext);

            var actionEvent = new ActionExecutionEvent
            {
                ExecutionId = executionId,
                ActionsJson = @"[{""Type"":""ForwardEmail"",""Parameters"":{}}]" // Missing 'to', causes PermanentFailure
            };

            var mockContext = new Mock<ConsumeContext<ActionExecutionEvent>>();
            mockContext.Setup(c => c.Message).Returns(actionEvent);
            mockContext.Setup(c => c.CancellationToken).Returns(System.Threading.CancellationToken.None);

            await consumer.Consume(mockContext.Object);

            var updatedRoot = await dbContext.AutomationExecutions.FirstAsync(x => x.Id == executionId);
            updatedRoot.Status.Should().Be("Failed");
            updatedRoot.CompletedAt.Should().NotBeNull();
            updatedRoot.ErrorMessage.Should().Contain("failed permanently");
        }

        [Fact]
        public async Task TestC_Unknown_ActionUnknown_ShouldUpdateRootToUnknown()
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            var executionId = Guid.NewGuid();
            var root = AutomationExecution.Create(Guid.NewGuid(), Guid.NewGuid(), "Matched");
            typeof(AutomationExecution).GetProperty("Id").SetValue(root, executionId);
            dbContext.AutomationExecutions.Add(root);

            dbContext.AutomationActionExecutions.Add(AutomationActionExecution.Create(executionId, "0", "ForwardEmail"));
            await dbContext.SaveChangesAsync();

            var mockEmailProvider = new Mock<IEmailProvider>();
            mockEmailProvider.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), default))
                .ThrowsAsync(new Exception("Network timeout"));

            var executorFactory = new NexusMail.Automation.Actions.ActionExecutorFactory(
                new[] { new NexusMail.Automation.Actions.ForwardActionExecutor(mockEmailProvider.Object) });
            
            var logger = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<ActionExecutionConsumer>>();
            var consumer = new ActionExecutionConsumer(executorFactory, logger, dbContext);

            var actionEvent = new ActionExecutionEvent
            {
                ExecutionId = executionId,
                RuleId = root.RuleId,
                EmailId = root.EmailId,
                ActionsJson = @"[{""Type"":""ForwardEmail"",""Parameters"":{""to"":""test@test.com""}}]"
            };

            var mockContext = new Mock<ConsumeContext<ActionExecutionEvent>>();
            mockContext.Setup(c => c.Message).Returns(actionEvent);
            mockContext.Setup(c => c.CancellationToken).Returns(System.Threading.CancellationToken.None);

            await consumer.Consume(mockContext.Object);

            var updatedRoot = await dbContext.AutomationExecutions.FirstAsync(x => x.Id == executionId);
            updatedRoot.Status.Should().Be("Unknown");
            updatedRoot.CompletedAt.Should().NotBeNull();
            updatedRoot.ErrorMessage.Should().Contain("unknown state");
        }

        [Fact]
        public async Task TestD_Retry_TransientFailure_ShouldNotTerminalizeRoot()
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            var executionId = Guid.NewGuid();
            var root = AutomationExecution.Create(Guid.NewGuid(), Guid.NewGuid(), "Matched");
            typeof(AutomationExecution).GetProperty("Id").SetValue(root, executionId);
            dbContext.AutomationExecutions.Add(root);

            dbContext.AutomationActionExecutions.Add(AutomationActionExecution.Create(executionId, "0", "ForwardEmail"));
            await dbContext.SaveChangesAsync();

            var mockExecutor = new Mock<NexusMail.Automation.Actions.IActionExecutor>();
            mockExecutor.SetupGet(x => x.ActionType).Returns(NexusMail.Domain.Automation.Enums.ActionType.ForwardEmail);
            mockExecutor.Setup(x => x.ExecuteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<EvaluateRulesMessage>(), It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(NexusMail.Domain.Automation.Enums.ActionResult.TransientFailure);

            var mockFactory = new NexusMail.Automation.Actions.ActionExecutorFactory(new[] { mockExecutor.Object });
            
            var logger = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<ActionExecutionConsumer>>();
            var consumer = new ActionExecutionConsumer(mockFactory, logger, dbContext);

            var actionEvent = new ActionExecutionEvent
            {
                ExecutionId = executionId,
                ActionsJson = @"[{""Type"":""ForwardEmail"",""Parameters"":{}}]"
            };

            var mockContext = new Mock<ConsumeContext<ActionExecutionEvent>>();
            mockContext.Setup(c => c.Message).Returns(actionEvent);
            mockContext.Setup(c => c.CancellationToken).Returns(System.Threading.CancellationToken.None);

            var act = () => consumer.Consume(mockContext.Object);
            await act.Should().ThrowAsync<Exception>().WithMessage("*transient failure*");

            // Verify state
            var updatedRoot = await dbContext.AutomationExecutions.FirstAsync(x => x.Id == executionId);
            updatedRoot.Status.Should().Be("Matched"); // Should NOT change
            updatedRoot.CompletedAt.Should().BeNull();
        }

        [Fact]
        public async Task TestE_IdempotentFinalization_ShouldNotBreakCompletedAt()
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            var executionId = Guid.NewGuid();
            var root = AutomationExecution.Create(Guid.NewGuid(), Guid.NewGuid(), "Matched");
            typeof(AutomationExecution).GetProperty("Id").SetValue(root, executionId);
            root.Complete("Succeeded"); // Terminalize it
            var completedAt = root.CompletedAt;
            dbContext.AutomationExecutions.Add(root);

            var actionExecution = AutomationActionExecution.Create(executionId, "0", "ForwardEmail");
            actionExecution.MarkSuccess();
            dbContext.AutomationActionExecutions.Add(actionExecution);
            await dbContext.SaveChangesAsync();

            var executorFactory = new NexusMail.Automation.Actions.ActionExecutorFactory(new NexusMail.Automation.Actions.IActionExecutor[0]);
            var logger = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<ActionExecutionConsumer>>();
            var consumer = new ActionExecutionConsumer(executorFactory, logger, dbContext);

            var actionEvent = new ActionExecutionEvent
            {
                ExecutionId = executionId,
                ActionsJson = @"[{""Type"":""ForwardEmail"",""Parameters"":{}}]"
            };

            var mockContext = new Mock<ConsumeContext<ActionExecutionEvent>>();
            mockContext.Setup(c => c.Message).Returns(actionEvent);
            mockContext.Setup(c => c.CancellationToken).Returns(System.Threading.CancellationToken.None);

            // Emulate redelivery
            await consumer.Consume(mockContext.Object);

            var updatedRoot = await dbContext.AutomationExecutions.FirstAsync(x => x.Id == executionId);
            updatedRoot.Status.Should().Be("Succeeded");
            updatedRoot.CompletedAt.Should().Be(completedAt); // Shouldn't have changed
        }

        [Fact]
        public async Task TestF_ConcurrentFinalization_ShouldNotThrowOrLoseState()
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext1 = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            using var scope2 = _factory.Services.CreateScope();
            var dbContext2 = scope2.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            var executionId = Guid.NewGuid();
            var root = AutomationExecution.Create(Guid.NewGuid(), Guid.NewGuid(), "Matched");
            typeof(AutomationExecution).GetProperty("Id").SetValue(root, executionId);
            dbContext1.AutomationExecutions.Add(root);
            await dbContext1.SaveChangesAsync();

            // Simulate two threads loading the root at the same time
            var root1 = await dbContext1.AutomationExecutions.FirstAsync(x => x.Id == executionId);
            var root2 = await dbContext2.AutomationExecutions.FirstAsync(x => x.Id == executionId);

            // Thread 1 finalizes
            root1.Complete("Succeeded");
            await dbContext1.SaveChangesAsync();

            // Thread 2 attempts to finalize
            root2.Complete("Failed", "Some error");
            var act = () => dbContext2.SaveChangesAsync();
            
            // Should throw DbUpdateConcurrencyException because ExecutionVersion is an IsConcurrencyToken
            await act.Should().ThrowAsync<DbUpdateConcurrencyException>();
            
            // After catching in ActionExecutionConsumer, the final state is Succeeded
            using var scope3 = _factory.Services.CreateScope();
            var dbContext3 = scope3.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var finalRoot = await dbContext3.AutomationExecutions.FirstAsync(x => x.Id == executionId);
            finalRoot.Status.Should().Be("Succeeded");
            finalRoot.ExecutionVersion.Should().Be(2); // Initial was 1, Thread 1 incremented to 2
        }
    }
}
