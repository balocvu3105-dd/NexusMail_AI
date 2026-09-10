namespace NexusMail.Contracts.Automation;

/// <summary>
/// Message published when automation rules should be evaluated for a received email.
/// Consumed by: NexusMail.Worker.Automation
/// NOTE: Automation is independent from Email domain — it only receives this contract.
/// </summary>
public sealed record EvaluateRulesMessage
{
    public Guid EmailId { get; init; }
    public Guid WorkspaceId { get; init; }
    public Guid EmailAccountId { get; init; }
    public string Subject { get; init; } = string.Empty;
    public string Sender { get; init; } = string.Empty;
    public string SenderDomain { get; init; } = string.Empty;
    public string Preview { get; init; } = string.Empty;
    public IReadOnlyList<string> Labels { get; init; } = [];
    public bool HasAttachments { get; init; }
    public DateTimeOffset ReceivedAt { get; init; }
    public string CorrelationId { get; init; } = string.Empty;
    public Dictionary<string, object> AIMetadata { get; init; } = new();
}
