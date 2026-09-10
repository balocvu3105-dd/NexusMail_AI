namespace NexusMail.Application.Abstractions.Search;

/// <summary>
/// Executes search queries against the search engine.
///
/// Switching between Postgres FTS → Qdrant → Elasticsearch → OpenSearch
/// requires only a new ISearchEngine implementation — Domain and Application unchanged.
/// </summary>
public interface ISearchEngine
{
    /// <summary>Full-text + semantic hybrid search.</summary>
    Task<SearchResult> SearchAsync(SearchQuery query, CancellationToken cancellationToken = default);

    /// <summary>Find emails semantically similar to a given email.</summary>
    Task<SearchResult> FindSimilarAsync(Guid emailId, Guid workspaceId, int limit = 5, CancellationToken cancellationToken = default);
}
