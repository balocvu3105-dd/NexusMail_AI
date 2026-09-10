using Microsoft.Extensions.Configuration;

namespace NexusMail.Search.Services;

public interface ISearchRankingService
{
    float CalculateHybridScore(float semanticScore, float ftsScore, int priorityScore, DateTimeOffset receivedAt);
}

public sealed class SearchRankingService : ISearchRankingService
{
    private readonly float _semanticWeight;
    private readonly float _ftsWeight;
    private readonly float _priorityWeight;
    private readonly float _recencyWeight;

    public SearchRankingService(IConfiguration configuration)
    {
        _semanticWeight = configuration.GetValue<float>("SearchRanking:SemanticWeight", 0.45f);
        _ftsWeight = configuration.GetValue<float>("SearchRanking:FtsWeight", 0.30f);
        _priorityWeight = configuration.GetValue<float>("SearchRanking:PriorityWeight", 0.15f);
        _recencyWeight = configuration.GetValue<float>("SearchRanking:RecencyWeight", 0.10f);
    }

    public float CalculateHybridScore(float semanticScore, float ftsScore, int priorityScore, DateTimeOffset receivedAt)
    {
        // Semantic score is usually cosine similarity (0 to 1) or inner product. We assume 0 to 1.
        // FTS score is usually a float that varies. We might need to normalize it, but let's assume it's normalized for now.
        // Priority score is 1-5, so we normalize to 0.2-1.0
        float normalizedPriority = priorityScore / 5.0f;
        
        // Recency score (e.g. exponential decay over 30 days)
        var daysOld = (DateTimeOffset.UtcNow - receivedAt).TotalDays;
        float normalizedRecency = daysOld <= 0 ? 1.0f : (float)Math.Exp(-daysOld / 30.0);

        return (semanticScore * _semanticWeight) + 
               (ftsScore * _ftsWeight) + 
               (normalizedPriority * _priorityWeight) + 
               (normalizedRecency * _recencyWeight);
    }
}
