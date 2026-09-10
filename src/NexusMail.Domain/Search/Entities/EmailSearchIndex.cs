using Pgvector;
using NexusMail.Shared.Domain;

namespace NexusMail.Domain.Search.Entities;

public sealed class EmailSearchIndex : EntityBase
{
    // PK is the same as EmailId for 1:1 mapping
    
    public Guid WorkspaceId { get; private set; }
    
    public Vector? Embedding { get; private set; }
    public string? EmbeddingModel { get; private set; }
    public string? EmbeddingVersion { get; private set; }
    public int? EmbeddingDimension { get; private set; }
    
    public string? SearchableText { get; private set; }
    
    public int PriorityScore { get; private set; }
    public DateTimeOffset ReceivedAt { get; private set; }
    public DateTimeOffset IndexedAt { get; private set; }
    
    private EmailSearchIndex() { } // EF Core

    public static EmailSearchIndex Create(
        Guid emailId,
        Guid workspaceId,
        int priorityScore,
        DateTimeOffset receivedAt)
    {
        return new EmailSearchIndex
        {
            Id = emailId,
            WorkspaceId = workspaceId,
            PriorityScore = priorityScore,
            ReceivedAt = receivedAt.ToUniversalTime(),
            IndexedAt = DateTimeOffset.UtcNow
        };
    }

    public void UpdateEmbedding(Vector embedding, string model, string version, int dimension)
    {
        Embedding = embedding;
        EmbeddingModel = model;
        EmbeddingVersion = version;
        EmbeddingDimension = dimension;
        IndexedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateFullTextSearch(string searchableContent)
    {
        SearchableText = searchableContent;
        IndexedAt = DateTimeOffset.UtcNow;
    }
}
