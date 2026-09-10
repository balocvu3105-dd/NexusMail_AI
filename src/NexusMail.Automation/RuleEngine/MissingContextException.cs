using System;

namespace NexusMail.Automation.RuleEngine;

public class MissingContextException : Exception
{
    public string PropertyName { get; }

    public MissingContextException(string propertyName) 
        : base($"Missing required context for property: {propertyName}")
    {
        PropertyName = propertyName;
    }
}
