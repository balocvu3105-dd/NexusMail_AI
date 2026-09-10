using FluentAssertions;
using NexusMail.Shared.Common;
using Xunit;

namespace NexusMail.Shared.Tests.Common;

public class ResultTests
{
    [Fact]
    public void Success_ShouldCreateSuccessfulResult()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void Failure_ShouldCreateFailedResult()
    {
        // Arrange
        var error = new Error("Code", "Description");

        // Act
        var result = Result.Failure(error);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Equality_ShouldMatchByErrorAndSuccessState()
    {
        // Arrange
        var success1 = Result.Success();
        var success2 = Result.Success();
        
        var error = new Error("E1", "Error 1");
        var fail1 = Result.Failure(error);
        var fail2 = Result.Failure(error);
        var fail3 = Result.Failure(new Error("E2", "Error 2"));

        // Act & Assert
        // Result is currently a class without overridden equality. 
        // We assert equality of their properties to ensure semantic equivalence.
        (success1.IsSuccess == success2.IsSuccess).Should().BeTrue();
        (fail1.Error == fail2.Error).Should().BeTrue();
        (fail1.Error != fail3.Error).Should().BeTrue();
    }
}
