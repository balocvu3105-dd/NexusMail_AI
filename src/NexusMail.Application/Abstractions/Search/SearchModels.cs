namespace NexusMail.Application.Abstractions.Search;

/// <summary>
/// Provider-agnostic DTO for indexing an email into the search engine.
/// Populated from Contracts.IndexEmailMessage — no Application or Domain references needed.
/// </summary>
public sealed record IndexableEmail
{
    public required Guid EmailId { get; init; }
    public required Guid WorkspaceId { get; init; }
    public required Guid EmailAccountId { get; init; }
    public required string MessageId { get; init; }
    public required string Subject { get; init; }
    public string Body { get; init; } = string.Empty;
    public required string Sender { get; init; }
    public string SenderName { get; init; } = string.Empty;
    public DateTimeOffset ReceivedAt { get; init; }
    public IReadOnlyList<string> Labels { get; init; } = [];
    public bool HasAttachments { get; init; }

    /// <summary>
    /// Vector embedding — populated by AI Worker BEFORE indexing.
    /// Enables semantic (similarity) search.
    /// Null until AI Worker processes the email.
    /// </summary>
    public float[]? EmbeddingVector { get; init; }
}

/// <summary>Search query with text, filters, and pagination.</summary>
public sealed record SearchQuery
{
    public required Guid WorkspaceId { get; init; }
    public required string QueryText { get; init; }
    public SearchMode Mode { get; init; } = SearchMode.Hybrid;
    public DateTimeOffset? From { get; init; }
    public DateTimeOffset? To { get; init; }
    public string? Sender { get; init; }
    public IReadOnlyList<string> Labels { get; init; } = [];
    public bool? HasAttachments { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    
    /// <summary>
    /// Embedding vector of the query text, used for semantic search.
    /// Should be populated before passing to the search engine.
    /// </summary>
    public float[]? Embedding { get; init; }
}

public enum SearchMode
{
    FullText = 0,   // Traditional keyword search (Postgres FTS)
    Semantic = 1,   // Vector similarity search (Qdrant)
    Hybrid = 2      // Full-text + Semantic, re-ranked
}

/// <summary>Paginated search result with relevance scores.</summary>
public sealed record SearchResult
{
    public IReadOnlyList<SearchHit> Hits { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public string SearchMode { get; init; } = string.Empty;
    public double SearchDurationMs { get; init; }
}

/// <summary>A single search result item.</summary>
public sealed record SearchHit
{
    public required Guid EmailId { get; init; }
    public required string Subject { get; init; }
    public required string Sender { get; init; }
    public required DateTimeOffset ReceivedAt { get; init; }
    public string? Snippet { get; init; }
    public float Score { get; init; }
}
