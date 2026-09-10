using System;
using System.Runtime.CompilerServices;

namespace NexusMail.Shared.Domain;

public static class Guard
{
    public static class Against
    {
        public static void Null<T>(T? value, [CallerArgumentExpression("value")] string? parameterName = null)
        {
            if (value is null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        public static void Empty(string? value, [CallerArgumentExpression("value")] string? parameterName = null)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Value cannot be null or empty.", parameterName);
            }
        }

        public static void OutOfRange(int value, int min, int max, [CallerArgumentExpression("value")] string? parameterName = null)
        {
            if (value < min || value > max)
            {
                throw new ArgumentOutOfRangeException(parameterName, $"Value must be between {min} and {max}.");
            }
        }
    }
}
