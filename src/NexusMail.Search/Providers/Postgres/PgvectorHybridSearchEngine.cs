using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using NexusMail.Application.Abstractions.Search;
using NexusMail.Infrastructure.Search.Persistence;
using NexusMail.Search.Services;
using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace NexusMail.Search.Providers.Postgres;

public sealed class PgvectorHybridSearchEngine : ISearchEngine
{
    private readonly SearchDbContext _searchDb;
    private readonly ISearchRankingService _rankingService;

    public PgvectorHybridSearchEngine(SearchDbContext searchDb, ISearchRankingService rankingService)
    {
        _searchDb = searchDb;
        _rankingService = rankingService;
    }

    public Task<SearchResult> FindSimilarAsync(Guid emailId, Guid workspaceId, int limit = 5, CancellationToken cancellationToken = default)
    {
        // Out of scope for this immediate sprint step, but good to have signature
        throw new NotImplementedException();
    }

    public async Task<SearchResult> SearchAsync(SearchQuery query, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        var baseQuery = _searchDb.EmailSearchIndices
            .Where(x => x.WorkspaceId == query.WorkspaceId);
            
        List<SearchHit> hits = new();

        if (query.Mode == SearchMode.Semantic || query.Mode == SearchMode.Hybrid)
        {
            if (query.Embedding != null && query.Embedding.Length > 0)
            {
                var vector = new Vector(query.Embedding);
                
                var items = await baseQuery
                    .Select(x => new 
                    {
                        x.Id,
                        x.PriorityScore,
                        x.ReceivedAt,
                        // Pgvector cosine distance function (<=>)
                        Distance = x.Embedding!.CosineDistance(vector),
                        FtsMatch = query.Mode == SearchMode.Hybrid && !string.IsNullOrWhiteSpace(query.QueryText)
                                   ? EF.Functions.ToTsVector("english", x.SearchableText!).Matches(query.QueryText)
                                   : false
                    })
                    .OrderBy(x => x.Distance)
                    .Take(query.PageSize * 2)
                    .ToListAsync(cancellationToken);

                hits = items.Select(x => new SearchHit
                {
                    EmailId = x.Id,
                    Subject = "Loaded elsewhere", // From application Db
                    Sender = "Loaded elsewhere",
                    ReceivedAt = x.ReceivedAt,
                    // Convert distance to similarity score (1 - distance)
                    Score = _rankingService.CalculateHybridScore(
                        semanticScore: 1f - (float)x.Distance, 
                        ftsScore: x.FtsMatch ? 1.0f : 0.0f, 
                        priorityScore: x.PriorityScore, 
                        receivedAt: x.ReceivedAt)
                }).ToList();
            }
        }
        else if (query.Mode == SearchMode.FullText)
        {
            var items = await baseQuery
                .Where(x => EF.Functions.ToTsVector("english", x.SearchableText!).Matches(query.QueryText))
                .Take(query.PageSize * 2)
                .Select(x => new 
                {
                    x.Id,
                    x.PriorityScore,
                    x.ReceivedAt
                })
                .ToListAsync(cancellationToken);

            hits = items.Select(x => new SearchHit
            {
                EmailId = x.Id,
                Subject = "Loaded elsewhere",
                Sender = "Loaded elsewhere",
                ReceivedAt = x.ReceivedAt,
                Score = _rankingService.CalculateHybridScore(0f, 1.0f, x.PriorityScore, x.ReceivedAt)
            }).ToList();
        }

        var finalHits = hits
            .OrderByDescending(x => x.Score)
            .Take(query.PageSize)
            .ToList();

        stopwatch.Stop();

        return new SearchResult
        {
            Hits = finalHits,
            TotalCount = finalHits.Count, // For true count we'd need a separate query
            Page = query.Page,
            PageSize = query.PageSize,
            SearchMode = query.Mode.ToString(),
            SearchDurationMs = stopwatch.ElapsedMilliseconds
        };
    }
}
