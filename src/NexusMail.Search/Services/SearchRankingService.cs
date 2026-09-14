using Microsoft.Extensions.Configuration;

namespace NexusMail.Search.Services;

public interface ISearchRankingService
{
    float CalculateHybridScore(float semanticScore, float ftsScore, int priorityScore, DateTimeOffset receivedAt);
}

public sealed class SearchRankingService : ISearchRankingService
{
    private readonly float _alpha;

    public SearchRankingService(IConfiguration configuration)
    {
        // Default alpha = 0.5 (equal weight for semantic and FTS)
        _alpha = configuration.GetValue<float>("SearchRanking:Alpha", 0.5f);
    }

    public float CalculateHybridScore(float semanticScore, float ftsScore, int priorityScore, DateTimeOffset receivedAt)
    {
        // Ensure inputs are bounded [0, 1]
        float vectorScore = Math.Clamp(semanticScore, 0f, 1f);
        float textScore = Math.Clamp(ftsScore, 0f, 1f);

        // HybridScore = α * VectorScore + (1 - α) * TextScore
        float hybridScore = (_alpha * vectorScore) + ((1.0f - _alpha) * textScore);

        // Deterministic tie-breaking: add tiny fractional value based on recency to break exact ties,
        // without affecting the primary ranking order significantly.
        var daysOld = (DateTimeOffset.UtcNow - receivedAt).TotalDays;
        float tieBreaker = daysOld <= 0 ? 0.0001f : (float)Math.Exp(-daysOld / 30.0) * 0.0001f;

        return hybridScore + tieBreaker;
    }
}
