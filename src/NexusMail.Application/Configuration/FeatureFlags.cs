namespace NexusMail.Application.Configuration;

public class FeatureFlags
{
    public const string SectionName = "FeatureFlags";

    public bool AiEnabled { get; set; } = true;
    public bool SearchEnabled { get; set; } = true;
    public bool AutomationEnabled { get; set; } = true;
    public bool AutoReplyEnabled { get; set; } = true;
}
