using Microsoft.Extensions.Logging;
using NexusMail.Application.Abstractions.Search;

namespace NexusMail.Search.Providers.Postgres;

/// <summary>
/// Phase 2 MVP search provider using PostgreSQL Full-Text Search.
/// Uses tsvector / tsquery for keyword matching.
///
/// Advantages: No additional infrastructure (reuses existing DB).
/// Limitations: No semantic search, limited ranking.
///
/// TODO Sprint 6: Implement with EF Core raw SQL or Npgsql.
/// Migration: Add tsvector columns + GIN indexes to Email table.
/// </summary>
public sealed class PostgresFullTextSearchProvider : ISearchIndexer, ISearchEngine
{
    private readonly ILogger<PostgresFullTextSearchProvider> _logger;

    public PostgresFullTextSearchProvider(ILogger<PostgresFullTextSearchProvider> logger)
    {
        _logger = logger;
    }

    // ─── ISearchIndexer ────────────────────────────────────────────────────

    public Task IndexAsync(IndexableEmail email, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[PostgresFTS] Indexing email {EmailId} for workspace {WorkspaceId} (stub — Sprint 6).",
            email.EmailId, email.WorkspaceId);

        // TODO Sprint 6: UPDATE emails SET search_vector = to_tsvector('english', subject || ' ' || body)
        return Task.CompletedTask;
    }

    public Task IndexBatchAsync(IReadOnlyList<IndexableEmail> emails, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[PostgresFTS] Batch indexing {Count} emails (stub — Sprint 6).", emails.Count);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(Guid emailId, Guid workspaceId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[PostgresFTS] Removing email {EmailId} from index (stub — Sprint 6).", emailId);
        return Task.CompletedTask;
    }

    // ─── ISearchEngine ─────────────────────────────────────────────────────

    public Task<SearchResult> SearchAsync(SearchQuery query, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[PostgresFTS] Search query: '{Query}' for workspace {WorkspaceId} (stub — Sprint 6).",
            query.QueryText, query.WorkspaceId);

        // TODO Sprint 6: SELECT * FROM emails WHERE search_vector @@ plainto_tsquery('english', @query)
        return Task.FromResult(new SearchResult
        {
            Hits = [],
            TotalCount = 0,
            Page = query.Page,
            PageSize = query.PageSize,
            SearchMode = "postgres-fts",
            SearchDurationMs = 0
        });
    }

    public Task<SearchResult> FindSimilarAsync(Guid emailId, Guid workspaceId, int limit = 5, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[PostgresFTS] FindSimilar for {EmailId} (stub — Sprint 6).", emailId);
        return Task.FromResult(new SearchResult { Hits = [], TotalCount = 0, Page = 1, PageSize = limit, SearchMode = "postgres-fts" });
    }
}
