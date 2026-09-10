namespace NexusMail.Common.Constants;

/// <summary>
/// Global constants shared across all NexusMail modules.
/// No external dependencies.
/// </summary>
public static class NexusConstants
{
    public static class Email
    {
        public const int MaxSubjectLength = 998;
        public const int MaxBodyPreviewLength = 500;
        public const int MaxAttachmentSizeMb = 25;
        public const int SyncBatchSize = 100;
        public const int MaxSyncRetries = 3;
        public const int TokenExpiryBufferSeconds = 300; // Refresh 5 min before expiry
    }

    public static class Search
    {
        public const int MaxQueryLength = 500;
        public const int DefaultResultSize = 20;
        public const int MaxResultSize = 100;
        public const float DefaultSemanticThreshold = 0.75f;
    }

    public static class AI
    {
        public const int MaxSummaryTokens = 512;
        public const int MaxPromptTokens = 4096;
        public const int EmbeddingDimension = 1536; // OpenAI text-embedding-ada-002
        public const double DefaultTemperature = 0.3;
    }

    public static class Automation
    {
        public const int MaxRulesPerWorkspace = 100;
        public const int MaxActionsPerRule = 10;
        public const int MaxConditionsPerRule = 20;
    }

    public static class Identity
    {
        public const int MaxWorkspacesPerUser = 10;
        public const int SessionExpiryDays = 30;
        public const int AccessTokenExpiryMinutes = 60;
        public const int MinPasswordLength = 8;
    }

    public static class Headers
    {
        public const string CorrelationId = "X-Correlation-Id";
        public const string WorkspaceId = "X-Workspace-Id";
        public const string ApiVersion = "X-Api-Version";
    }

    public static class CacheKeys
    {
        public static string EmailAccount(Guid id) => $"email:account:{id}";
        public static string UserWorkspaces(Guid userId) => $"user:{userId}:workspaces";
        public static string AutomationRules(Guid workspaceId) => $"automation:rules:{workspaceId}";
    }
}
