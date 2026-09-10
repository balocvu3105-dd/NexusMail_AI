using System;
using System.Collections.Generic;
using FluentAssertions;
using NexusMail.Shared.Domain;
using Xunit;

namespace NexusMail.Shared.Tests.Domain;

public class ValueObjectTests
{
    private class EmailAddress : ValueObject
    {
        public string Value { get; }

        public EmailAddress(string value)
        {
            Value = value;
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }
    }

    [Fact]
    public void Equals_ShouldReturnTrue_WhenPropertiesMatch()
    {
        // Arrange
        var email1 = new EmailAddress("a@b.com");
        var email2 = new EmailAddress("a@b.com");

        // Act & Assert
        (email1 == email2).Should().BeTrue();
        email1.Equals(email2).Should().BeTrue();
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenPropertiesDiffer()
    {
        // Arrange
        var email1 = new EmailAddress("a@b.com");
        var email2 = new EmailAddress("x@y.com");

        // Act & Assert
        (email1 == email2).Should().BeFalse();
        (email1 != email2).Should().BeTrue();
        email1.Equals(email2).Should().BeFalse();
    }
}
