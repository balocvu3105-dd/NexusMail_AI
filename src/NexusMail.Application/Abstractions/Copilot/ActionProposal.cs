using System.Text.Json;

namespace NexusMail.Application.Abstractions.Copilot;

public sealed record ActionProposal
{
    public string ActionType { get; init; } = string.Empty;
    public JsonElement Parameters { get; init; }
}
