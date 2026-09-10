using System;
using System.Linq;
using FluentAssertions;
using NexusMail.Domain.Workspace.Entities;
using NexusMail.Domain.Workspace.Events;
using Xunit;

using WorkspaceEntity = NexusMail.Domain.Workspace.Entities.Workspace;

namespace NexusMail.Domain.Tests;

public class WorkspaceTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateWorkspaceAndRaiseEvent()
    {
        // Arrange
        var name = "Engineering";
        var ownerId = Guid.NewGuid();
        var plan = "Pro";

        // Act
        var workspace = WorkspaceEntity.Create(name, ownerId, plan);

        // Assert
        workspace.Should().NotBeNull();
        workspace.Id.Should().NotBeEmpty();
        workspace.Name.Should().Be(name);
        workspace.OwnerId.Should().Be(ownerId);
        workspace.Plan.Should().Be(plan);

        var domainEvent = workspace.DomainEvents.FirstOrDefault() as WorkspaceCreated;
        domainEvent.Should().NotBeNull();
        domainEvent!.WorkspaceId.Should().Be(workspace.Id);
        domainEvent.OwnerId.Should().Be(ownerId);
        domainEvent.Plan.Should().Be(plan);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyName_ShouldThrowArgumentException(string? invalidName)
    {
        // Arrange
        var ownerId = Guid.NewGuid();

        // Act
        Action act = () => WorkspaceEntity.Create(invalidName!, ownerId);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*Workspace name cannot be empty.*");
    }

    [Fact]
    public void Create_WithEmptyOwnerId_ShouldThrowArgumentException()
    {
        // Arrange
        var name = "Engineering";
        var emptyOwnerId = Guid.Empty;

        // Act
        Action act = () => WorkspaceEntity.Create(name, emptyOwnerId);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*OwnerId cannot be empty.*");
    }
}
