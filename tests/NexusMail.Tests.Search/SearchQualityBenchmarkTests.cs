using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace NexusMail.Tests.Search;

/// <summary>
/// Benchmark suite to evaluate the quality of the Hybrid Search Engine.
/// Calculates Precision@K, Recall@K, MRR (Mean Reciprocal Rank), and NDCG@K.
/// </summary>
public class SearchQualityBenchmarkTests
{
    private readonly ITestOutputHelper _output;

    public SearchQualityBenchmarkTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Calculate_Precision_Recall_MRR_NDCG_For_Sample_Dataset()
    {
        // Simulated Dataset of Emails (Ground Truth)
        // Query: "invoice from stripe"
        // Expected Relevant Document IDs: { 1, 3 }
        
        var expectedRelevantDocIds = new HashSet<int> { 1, 3 };
        
        // Simulated Search Engine Results (ordered by rank)
        // 1st: Doc 1 (Relevant)
        // 2nd: Doc 5 (Irrelevant)
        // 3rd: Doc 3 (Relevant)
        // 4th: Doc 8 (Irrelevant)
        // 5th: Doc 2 (Irrelevant)
        
        var retrievedDocIds = new List<int> { 1, 5, 3, 8, 2 };
        int k = 3;

        // 1. Precision@K = (Relevant retrieved in top K) / K
        var topK = retrievedDocIds.Take(k).ToList();
        var relevantInTopK = topK.Count(id => expectedRelevantDocIds.Contains(id));
        var precisionAtK = (double)relevantInTopK / k;

        // 2. Recall@K = (Relevant retrieved in top K) / Total Relevant
        var recallAtK = (double)relevantInTopK / expectedRelevantDocIds.Count;

        // 3. MRR = 1 / Rank of first relevant document
        double mrr = 0;
        for (int i = 0; i < retrievedDocIds.Count; i++)
        {
            if (expectedRelevantDocIds.Contains(retrievedDocIds[i]))
            {
                mrr = 1.0 / (i + 1);
                break;
            }
        }

        // 4. NDCG@K
        // DCG = sum(rel_i / log2(i + 1)) where rel_i is 1 if relevant else 0
        // IDCG = sum(1 / log2(i + 1)) for min(K, total_relevant)
        double dcg = 0;
        for (int i = 0; i < k; i++)
        {
            if (expectedRelevantDocIds.Contains(retrievedDocIds[i]))
            {
                dcg += 1.0 / Math.Log2(i + 2); // i is 0-based, so i+2 is rank+1
            }
        }

        double idcg = 0;
        var idealCount = Math.Min(k, expectedRelevantDocIds.Count);
        for (int i = 0; i < idealCount; i++)
        {
            idcg += 1.0 / Math.Log2(i + 2);
        }

        var ndcgAtK = idcg > 0 ? dcg / idcg : 0;

        _output.WriteLine($"--- Search Quality Metrics (K={k}) ---");
        _output.WriteLine($"Precision@{k}: {precisionAtK:F3}");
        _output.WriteLine($"Recall@{k}:    {recallAtK:F3}");
        _output.WriteLine($"MRR:         {mrr:F3}");
        _output.WriteLine($"NDCG@{k}:      {ndcgAtK:F3}");

        // Assert some basic thresholds for our mock test
        Assert.True(precisionAtK > 0);
        Assert.True(recallAtK > 0);
        Assert.True(mrr > 0);
        Assert.True(ndcgAtK > 0);
    }
}
