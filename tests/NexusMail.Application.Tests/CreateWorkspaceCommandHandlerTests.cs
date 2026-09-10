using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NexusMail.Application.Abstractions.Authentication;
using NexusMail.Application.Abstractions.Persistence;
using NexusMail.Application.Features.Workspace.Commands.CreateWorkspace;
using NexusMail.Domain.Workspace.Entities;
using NexusMail.Domain.Workspace.Repositories;
using Xunit;

namespace NexusMail.Application.Tests;

public class CreateWorkspaceCommandHandlerTests
{
    private readonly Mock<IWorkspaceRepository> _workspaceRepositoryMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly CreateWorkspaceCommandHandler _handler;

    public CreateWorkspaceCommandHandlerTests()
    {
        _workspaceRepositoryMock = new Mock<IWorkspaceRepository>();
        _currentUserMock = new Mock<ICurrentUser>();

        _handler = new CreateWorkspaceCommandHandler(
            _workspaceRepositoryMock.Object,
            _currentUserMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccessAndSaveToDatabase()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var command = new CreateWorkspaceCommand("Test Workspace", "Pro");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be("Test Workspace");
        result.Value.Plan.Should().Be("Pro");
        result.Value.OwnerId.Should().Be(userId);

        _workspaceRepositoryMock.Verify(
            x => x.AddAsync(It.Is<Workspace>(w => w.Name == "Test Workspace"), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
