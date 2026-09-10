using System;
using System.Text.Json;
using System.Collections.Generic;
using NexusMail.Domain.Automation.Enums;
using NexusMail.Shared.Common;

namespace NexusMail.Application.Features.Automation.Commands;

public static class AutomationConfigurationValidator
{
    public static Error? ValidateConditions(string conditionsJson)
    {
        try
        {
            using var document = JsonDocument.Parse(conditionsJson);
            if (document.RootElement.ValueKind != JsonValueKind.Array)
            {
                return new Error("Automation.InvalidConditions", "Conditions must be a JSON array.");
            }

            foreach (var element in document.RootElement.EnumerateArray())
            {
                if (element.ValueKind != JsonValueKind.Object)
                {
                    return new Error("Automation.InvalidConditions", "Each condition must be a JSON object.");
                }

                if (!element.TryGetProperty("Type", out var typeProp) || typeProp.ValueKind != JsonValueKind.String)
                {
                    return new Error("Automation.InvalidConditions", "Condition is missing a valid 'Type'.");
                }
                
                // Add operator and value checks if necessary, but keep it as configuration validation
                if (!element.TryGetProperty("Operator", out var operatorProp) || operatorProp.ValueKind != JsonValueKind.String)
                {
                    return new Error("Automation.InvalidConditions", "Condition is missing a valid 'Operator'.");
                }
            }

            return null; // Valid
        }
        catch (JsonException)
        {
            return new Error("Automation.InvalidConditions", "Conditions JSON is malformed.");
        }
    }

    public static Error? ValidateActions(string actionsJson)
    {
        try
        {
            using var document = JsonDocument.Parse(actionsJson);
            if (document.RootElement.ValueKind != JsonValueKind.Array)
            {
                return new Error("Automation.InvalidActions", "Actions must be a JSON array.");
            }

            foreach (var element in document.RootElement.EnumerateArray())
            {
                if (element.ValueKind != JsonValueKind.Object)
                {
                    return new Error("Automation.InvalidActions", "Each action must be a JSON object.");
                }

                if (!element.TryGetProperty("Type", out var typeProp) || typeProp.ValueKind != JsonValueKind.String)
                {
                    return new Error("Automation.InvalidActions", "Action is missing a valid 'Type'.");
                }

                var typeStr = typeProp.GetString();
                if (!Enum.TryParse<ActionType>(typeStr, out _))
                {
                    return new Error("Automation.InvalidActions", $"ActionType '{typeStr}' is not supported.");
                }
            }

            return null; // Valid
        }
        catch (JsonException)
        {
            return new Error("Automation.InvalidActions", "Actions JSON is malformed.");
        }
    }
}
