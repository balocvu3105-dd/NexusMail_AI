namespace NexusMail.Application.Abstractions.Search;

/// <summary>
/// Indexes emails into the search engine.
/// Consumed by the Search Indexer Worker which listens to IndexEmailMessage contracts.
///
/// IMPORTANT: Search does NOT reference Application directly.
/// Flow: EmailReceived → Outbox → IndexEmailMessage (Contract) → SearchIndexerWorker → ISearchIndexer
///
/// Implementations: PostgresFtsSearchProvider (Phase 2 MVP), QdrantSearchProvider (Phase 2+)
/// </summary>
public interface ISearchIndexer
{
    /// <summary>Index or update a single email.</summary>
    Task IndexAsync(IndexableEmail email, CancellationToken cancellationToken = default);

    /// <summary>Batch index multiple emails (more efficient for bulk sync).</summary>
    Task IndexBatchAsync(IReadOnlyList<IndexableEmail> emails, CancellationToken cancellationToken = default);

    /// <summary>Remove an email from the index (e.g., deleted or moved to trash).</summary>
    Task RemoveAsync(Guid emailId, Guid workspaceId, CancellationToken cancellationToken = default);
}
