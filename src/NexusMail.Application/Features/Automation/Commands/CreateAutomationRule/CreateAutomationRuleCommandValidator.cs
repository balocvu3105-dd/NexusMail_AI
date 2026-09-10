using System.Text.Json;
using System.Text.Json.Nodes;
using FluentValidation;
using NexusMail.Domain.Automation.Enums;
using System.Collections.Generic;

namespace NexusMail.Application.Features.Automation.Commands.CreateAutomationRule;

public class CreateAutomationRuleCommandValidator : AbstractValidator<CreateAutomationRuleCommand>
{
    public CreateAutomationRuleCommandValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Name is required.");

        RuleFor(v => v.ConditionsJson)
            .Must(BeValidJson).WithMessage("ConditionsJson must be valid JSON.");

        RuleFor(v => v.ActionsJson)
            .Must(BeValidJson).WithMessage("ActionsJson must be valid JSON.");

        RuleFor(x => x)
            .Must(HaveValidDependencies).WithMessage("Rule conditions require context that is not available for the selected TriggerType.");
    }

    private bool BeValidJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return true;
        try
        {
            JsonDocument.Parse(json);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private bool HaveValidDependencies(CreateAutomationRuleCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.ConditionsJson) || command.ConditionsJson == "[]" || command.ConditionsJson == "{}")
            return true;

        try
        {
            var dependencies = ExtractDependencies(command.ConditionsJson);
            
            // EmailReceived provides both Email and AI context (eventually)
            if (command.TriggerType == TriggerType.EmailReceived) return true;

            // Scheduled provides NO email context and NO AI context
            if (command.TriggerType == TriggerType.Scheduled)
            {
                if (dependencies.Contains("Email") || dependencies.Contains("AI")) return false;
            }

            // EmailSent provides Email context but NOT AI context
            if (command.TriggerType == TriggerType.EmailSent)
            {
                if (dependencies.Contains("AI")) return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    private HashSet<string> ExtractDependencies(string conditionsJson)
    {
        var deps = new HashSet<string>();
        var root = JsonNode.Parse(conditionsJson);
        ExtractFromNode(root, deps);
        return deps;
    }

    private void ExtractFromNode(JsonNode? node, HashSet<string> deps)
    {
        if (node is JsonObject obj)
        {
            if (obj.TryGetPropertyValue("Property", out var propNode))
            {
                var propName = propNode?.ToString().ToLowerInvariant();
                if (propName == "priorityscore" || propName == "category" || propName == "sentiment")
                {
                    deps.Add("AI");
                }
                else if (propName == "subject" || propName == "sender" || propName == "senderdomain" || propName == "hasattachments")
                {
                    deps.Add("Email");
                }
            }
            
            if (obj.TryGetPropertyValue("Operands", out var operandsNode) && operandsNode is JsonArray arr)
            {
                foreach (var item in arr)
                {
                    ExtractFromNode(item, deps);
                }
            }

            if (obj.TryGetPropertyValue("Operand", out var singleOperandNode))
            {
                ExtractFromNode(singleOperandNode, deps);
            }
        }
    }
}
