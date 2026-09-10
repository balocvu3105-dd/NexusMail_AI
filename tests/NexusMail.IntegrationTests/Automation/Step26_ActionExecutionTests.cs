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
using MassTransit;
using NexusMail.Application.Abstractions.Email;
using Moq;

namespace NexusMail.IntegrationTests.Automation
{
    public class Step26_ActionExecutionTests : IClassFixture<CustomWebApplicationFactory<NexusMail.API.ApiMarker>>
    {
        private readonly CustomWebApplicationFactory<NexusMail.API.ApiMarker> _factory;

        public Step26_ActionExecutionTests(CustomWebApplicationFactory<NexusMail.API.ApiMarker> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task ActionState_ShouldTransitionToExecutingAndSuccess()
        {
            // Golden path test
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            var executionId = Guid.NewGuid();
            var actionKey = "0";
            
            dbContext.AutomationActionExecutions.Add(AutomationActionExecution.Create(executionId, actionKey, "ForwardEmail"));
            await dbContext.SaveChangesAsync();

            var mockEmailProvider = new Mock<IEmailProvider>();
            mockEmailProvider
                .Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), default))
                .Returns(Task.CompletedTask);

            var executorFactory = new NexusMail.Automation.Actions.ActionExecutorFactory(
                new[] { new NexusMail.Automation.Actions.ForwardActionExecutor(mockEmailProvider.Object) });
            
            var logger = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<NexusMail.Worker.Automation.Consumers.ActionExecutionConsumer>>();
            var consumer = new NexusMail.Worker.Automation.Consumers.ActionExecutionConsumer(executorFactory, logger, dbContext);

            var actionEvent = new ActionExecutionEvent
            {
                ExecutionId = executionId,
                RuleId = Guid.NewGuid(),
                EmailId = Guid.NewGuid(),
                WorkspaceId = Guid.NewGuid(),
                ActionsJson = @"[{""Type"":""ForwardEmail"",""Parameters"":{""to"":""test@test.com""}}]",
                EvaluateContext = new EvaluateRulesMessage()
            };

            var mockContext = new Mock<ConsumeContext<ActionExecutionEvent>>();
            mockContext.Setup(c => c.Message).Returns(actionEvent);
            mockContext.Setup(c => c.CancellationToken).Returns(System.Threading.CancellationToken.None);

            await consumer.Consume(mockContext.Object);

            var actionExecution = await dbContext.AutomationActionExecutions.FirstAsync(x => x.ExecutionId == executionId && x.ActionKey == actionKey);
            actionExecution.Status.Should().Be("Success");
            
            var audit = await dbContext.AutomationAudits.FirstAsync(x => x.ExecutionId == executionId);
            audit.Result.Should().Be("Success");
        }

        [Fact]
        public async Task SucceededAction_ShouldNeverExecuteAgain()
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            var executionId = Guid.NewGuid();
            var actionKey = "0";
            
            var execution = AutomationActionExecution.Create(executionId, actionKey, "ForwardEmail");
            execution.MarkSuccess();
            dbContext.AutomationActionExecutions.Add(execution);
            await dbContext.SaveChangesAsync();

            var mockEmailProvider = new Mock<IEmailProvider>();
            
            var executorFactory = new NexusMail.Automation.Actions.ActionExecutorFactory(
                new[] { new NexusMail.Automation.Actions.ForwardActionExecutor(mockEmailProvider.Object) });
            
            var logger = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<NexusMail.Worker.Automation.Consumers.ActionExecutionConsumer>>();
            var consumer = new NexusMail.Worker.Automation.Consumers.ActionExecutionConsumer(executorFactory, logger, dbContext);

            var actionEvent = new ActionExecutionEvent
            {
                ExecutionId = executionId,
                ActionsJson = @"[{""Type"":""ForwardEmail"",""Parameters"":{""to"":""test@test.com""}}]"
            };

            var mockContext = new Mock<ConsumeContext<ActionExecutionEvent>>();
            mockContext.Setup(c => c.Message).Returns(actionEvent);
            mockContext.Setup(c => c.CancellationToken).Returns(System.Threading.CancellationToken.None);

            await consumer.Consume(mockContext.Object);

            // Verify exactly ZERO invocations due to Delivery Idempotency
            mockEmailProvider.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), default), Times.Never);
        }

        [Fact]
        public async Task CrashAfterExternalExecution_ShouldNotClaimSafeRetry()
        {
            // If the state is 'Executing', we assume the process crashed and outcome is unknown.
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            var executionId = Guid.NewGuid();
            var actionKey = "0";
            
            var execution = AutomationActionExecution.Create(executionId, actionKey, "ForwardEmail");
            execution.MarkExecuting(); // Simulating a crash while in Executing state
            dbContext.AutomationActionExecutions.Add(execution);
            await dbContext.SaveChangesAsync();

            var mockEmailProvider = new Mock<IEmailProvider>();
            
            var executorFactory = new NexusMail.Automation.Actions.ActionExecutorFactory(
                new[] { new NexusMail.Automation.Actions.ForwardActionExecutor(mockEmailProvider.Object) });
            
            var logger = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<NexusMail.Worker.Automation.Consumers.ActionExecutionConsumer>>();
            var consumer = new NexusMail.Worker.Automation.Consumers.ActionExecutionConsumer(executorFactory, logger, dbContext);

            var actionEvent = new ActionExecutionEvent
            {
                ExecutionId = executionId,
                ActionsJson = @"[{""Type"":""ForwardEmail"",""Parameters"":{""to"":""test@test.com""}}]"
            };

            var mockContext = new Mock<ConsumeContext<ActionExecutionEvent>>();
            mockContext.Setup(c => c.Message).Returns(actionEvent);
            mockContext.Setup(c => c.CancellationToken).Returns(System.Threading.CancellationToken.None);

            // Act - MassTransit redelivers the message
            await consumer.Consume(mockContext.Object);

            // Assert
            var reloadedExecution = await dbContext.AutomationActionExecutions.FirstAsync(x => x.ExecutionId == executionId && x.ActionKey == actionKey);
            
            // It MUST be marked as Unknown, not blindly retried
            reloadedExecution.Status.Should().Be("Unknown");
            
            // Executor should NEVER be called on a redelivery if it's already Executing (without external idempotency)
            mockEmailProvider.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), default), Times.Never);
        }

        [Fact]
        public async Task ProviderException_ShouldResultInUnknownOutcome()
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            var executionId = Guid.NewGuid();
            var actionKey = "0";
            
            dbContext.AutomationActionExecutions.Add(AutomationActionExecution.Create(executionId, actionKey, "ForwardEmail"));
            await dbContext.SaveChangesAsync();

            var mockEmailProvider = new Mock<IEmailProvider>();
            mockEmailProvider
                .Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), default))
                .ThrowsAsync(new Exception("Network timeout or SMTP reset"));

            var executorFactory = new NexusMail.Automation.Actions.ActionExecutorFactory(
                new[] { new NexusMail.Automation.Actions.ForwardActionExecutor(mockEmailProvider.Object) });
            
            var logger = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<NexusMail.Worker.Automation.Consumers.ActionExecutionConsumer>>();
            var consumer = new NexusMail.Worker.Automation.Consumers.ActionExecutionConsumer(executorFactory, logger, dbContext);

            var actionEvent = new ActionExecutionEvent
            {
                ExecutionId = executionId,
                RuleId = Guid.NewGuid(),
                EmailId = Guid.NewGuid(),
                ActionsJson = @"[{""Type"":""ForwardEmail"",""Parameters"":{""to"":""test@test.com""}}]"
            };

            var mockContext = new Mock<ConsumeContext<ActionExecutionEvent>>();
            mockContext.Setup(c => c.Message).Returns(actionEvent);
            mockContext.Setup(c => c.CancellationToken).Returns(System.Threading.CancellationToken.None);

            // Act: MassTransit consumes it
            await consumer.Consume(mockContext.Object);

            // Assert: It should NOT throw (because it's an Unknown outcome and we don't want a blind retry)
            // It should be ACKed and recorded as Unknown.
            var execution = await dbContext.AutomationActionExecutions.FirstAsync(x => x.ExecutionId == executionId && x.ActionKey == actionKey);
            execution.Status.Should().Be("Unknown");
            execution.RetryCount.Should().Be(0);
        }

        [Fact]
        public async Task PermanentFailure_ShouldAckMessage()
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            var executionId = Guid.NewGuid();
            var actionKey = "0";
            
            dbContext.AutomationActionExecutions.Add(AutomationActionExecution.Create(executionId, actionKey, "ForwardEmail"));
            await dbContext.SaveChangesAsync();

            var mockEmailProvider = new Mock<IEmailProvider>();
            
            var executorFactory = new NexusMail.Automation.Actions.ActionExecutorFactory(
                new[] { new NexusMail.Automation.Actions.ForwardActionExecutor(mockEmailProvider.Object) });
            
            var logger = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<NexusMail.Worker.Automation.Consumers.ActionExecutionConsumer>>();
            var consumer = new NexusMail.Worker.Automation.Consumers.ActionExecutionConsumer(executorFactory, logger, dbContext);

            var actionEvent = new ActionExecutionEvent
            {
                ExecutionId = executionId,
                ActionsJson = @"[{""Type"":""ForwardEmail"",""Parameters"":{}}]" // Missing 'to', causes PermanentFailure
            };

            var mockContext = new Mock<ConsumeContext<ActionExecutionEvent>>();
            mockContext.Setup(c => c.Message).Returns(actionEvent);
            mockContext.Setup(c => c.CancellationToken).Returns(System.Threading.CancellationToken.None);

            // Should not throw, which ACKs the message
            await consumer.Consume(mockContext.Object);

            var execution = await dbContext.AutomationActionExecutions.FirstAsync(x => x.ExecutionId == executionId && x.ActionKey == actionKey);
            execution.Status.Should().Be("Failed");
            execution.RetryCount.Should().Be(0); // Did not increment retry for permanent
        }

        [Fact]
        public async Task ExplicitTransientFailure_ShouldThrowForMassTransitRetry()
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            var executionId = Guid.NewGuid();
            var actionKey = "0";
            
            dbContext.AutomationActionExecutions.Add(AutomationActionExecution.Create(executionId, actionKey, "ForwardEmail"));
            await dbContext.SaveChangesAsync();

            var mockExecutor = new Mock<NexusMail.Automation.Actions.IActionExecutor>();
            mockExecutor.SetupGet(x => x.ActionType).Returns(ActionType.ForwardEmail);
            mockExecutor.Setup(x => x.ExecuteAsync(It.IsAny<string>(), It.IsAny<EvaluateRulesMessage>(), It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(ActionResult.TransientFailure);

            var mockFactory = new NexusMail.Automation.Actions.ActionExecutorFactory(new[] { mockExecutor.Object });

            var logger = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<NexusMail.Worker.Automation.Consumers.ActionExecutionConsumer>>();
            var consumer = new NexusMail.Worker.Automation.Consumers.ActionExecutionConsumer(mockFactory, logger, dbContext);

            var actionEvent = new ActionExecutionEvent
            {
                ExecutionId = executionId,
                ActionsJson = @"[{""Type"":""ForwardEmail"",""Parameters"":{}}]"
            };

            var mockContext = new Mock<ConsumeContext<ActionExecutionEvent>>();
            mockContext.Setup(c => c.Message).Returns(actionEvent);
            mockContext.Setup(c => c.CancellationToken).Returns(System.Threading.CancellationToken.None);

            var act = () => consumer.Consume(mockContext.Object);

            // Should throw so MassTransit will retry it
            await act.Should().ThrowAsync<Exception>().WithMessage("*transient failure*");

            // Verify state
            var execution = await dbContext.AutomationActionExecutions.FirstAsync(x => x.ExecutionId == executionId && x.ActionKey == actionKey);
            execution.Status.Should().Be("Failed");
            execution.RetryCount.Should().Be(1);
        }
    }
}
