using System;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NexusMail.Contracts.AI;
using NexusMail.Contracts.Automation;
using NexusMail.Domain.Email.Events;
using NexusMail.EventHandlers.Email;
using Xunit;

namespace NexusMail.Application.Tests.EventHandlers;

public class EmailReceivedDomainEventHandlerTests
{
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<ILogger<EmailReceivedDomainEventHandler>> _loggerMock;
    private readonly EmailReceivedDomainEventHandler _handler;

    public EmailReceivedDomainEventHandlerTests()
    {
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _configurationMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<EmailReceivedDomainEventHandler>>();

        _handler = new EmailReceivedDomainEventHandler(
            _publishEndpointMock.Object,
            _configurationMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_BeforeCutoff_ShouldNotPublishAIProcessingRequestedMessage()
    {
        // Arrange
        var cutoff = new DateTimeOffset(2026, 9, 10, 0, 0, 0, TimeSpan.Zero);
        _configurationMock.Setup(c => c["AI:ProcessingGateCutoffDate"]).Returns(cutoff.ToString("O"));

        var emailReceived = new EmailReceived
        {
            EmailId = Guid.NewGuid(),
            WorkspaceId = Guid.NewGuid(),
            ReceivedAt = cutoff.AddSeconds(-1) // Before cutoff
        };

        // Act
        await _handler.Handle(emailReceived, CancellationToken.None);

        // Assert
        _publishEndpointMock.Verify(
            x => x.Publish(It.IsAny<AIProcessingRequestedMessage>(), It.IsAny<CancellationToken>()),
            Times.Never);
            
        _publishEndpointMock.Verify(
            x => x.Publish(It.IsAny<EvaluateRulesMessage>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ExactlyCutoff_ShouldPublishAIProcessingRequestedMessage()
    {
        // Arrange
        var cutoff = new DateTimeOffset(2026, 9, 10, 0, 0, 0, TimeSpan.Zero);
        _configurationMock.Setup(c => c["AI:ProcessingGateCutoffDate"]).Returns(cutoff.ToString("O"));

        var emailReceived = new EmailReceived
        {
            EmailId = Guid.NewGuid(),
            WorkspaceId = Guid.NewGuid(),
            ReceivedAt = cutoff // Exactly cutoff
        };

        // Act
        await _handler.Handle(emailReceived, CancellationToken.None);

        // Assert
        _publishEndpointMock.Verify(
            x => x.Publish(It.IsAny<AIProcessingRequestedMessage>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_AfterCutoff_ShouldPublishAIProcessingRequestedMessage()
    {
        // Arrange
        var cutoff = new DateTimeOffset(2026, 9, 10, 0, 0, 0, TimeSpan.Zero);
        _configurationMock.Setup(c => c["AI:ProcessingGateCutoffDate"]).Returns(cutoff.ToString("O"));

        var emailReceived = new EmailReceived
        {
            EmailId = Guid.NewGuid(),
            WorkspaceId = Guid.NewGuid(),
            ReceivedAt = cutoff.AddSeconds(1) // After cutoff
        };

        // Act
        await _handler.Handle(emailReceived, CancellationToken.None);

        // Assert
        _publishEndpointMock.Verify(
            x => x.Publish(It.IsAny<AIProcessingRequestedMessage>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
