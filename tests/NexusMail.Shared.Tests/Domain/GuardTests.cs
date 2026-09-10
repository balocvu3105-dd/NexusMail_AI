using System;
using FluentAssertions;
using NexusMail.Shared.Domain;
using Xunit;

namespace NexusMail.Shared.Tests.Domain;

public class GuardTests
{
    [Fact]
    public void AgainstNull_ShouldThrow_WhenValueIsNull()
    {
        // Arrange
        object? nullObj = null;

        // Act
        Action act = () => Guard.Against.Null(nullObj, "myParam");

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("myParam");
    }

    [Fact]
    public void AgainstNull_ShouldNotThrow_WhenValueIsNotNull()
    {
        // Arrange
        object notNullObj = new object();

        // Act
        Action act = () => Guard.Against.Null(notNullObj);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void AgainstEmpty_ShouldThrow_WhenStringIsEmptyOrWhitespace()
    {
        // Arrange
        string emptyStr = "";
        string whitespaceStr = "   ";

        // Act & Assert
        Action actEmpty = () => Guard.Against.Empty(emptyStr, "myString");
        actEmpty.Should().Throw<ArgumentException>().WithParameterName("myString");

        Action actWhitespace = () => Guard.Against.Empty(whitespaceStr, "myString");
        actWhitespace.Should().Throw<ArgumentException>().WithParameterName("myString");
    }

    [Fact]
    public void AgainstEmpty_ShouldNotThrow_WhenStringIsNotEmpty()
    {
        // Arrange
        string validStr = "hello";

        // Act
        Action act = () => Guard.Against.Empty(validStr);

        // Assert
        act.Should().NotThrow();
    }
}
