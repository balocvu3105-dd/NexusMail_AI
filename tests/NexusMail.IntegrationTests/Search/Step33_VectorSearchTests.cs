using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexusMail.Application.Abstractions.Search;
using NexusMail.Domain.Search.Entities;
using NexusMail.Infrastructure.Search.Persistence;
using Pgvector;
using Xunit;
using Xunit.Abstractions;

namespace NexusMail.IntegrationTests.Search;

[Collection("IntegrationTest")]
public class Step33_VectorSearchTests : IClassFixture<CustomWebApplicationFactory<NexusMail.API.ApiMarker>>, IAsyncLifetime
{
    private readonly CustomWebApplicationFactory<NexusMail.API.ApiMarker> _factory;
    private readonly ITestOutputHelper _output;

    public Step33_VectorSearchTests(CustomWebApplicationFactory<NexusMail.API.ApiMarker> factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => Task.CompletedTask;

    private Vector CreateVector(float v1, float v2, float v3)
    {
        var array = new float[1536];
        array[0] = v1;
        array[1] = v2;
        array[2] = v3;
        return new Vector(array);
    }

    [Fact]
    public async Task Search_ShouldReturnResults_OrderedByCosineSimilarity_WhenModeIsSemantic()
    {
        using var scope = _factory.Services.CreateScope();
        var searchDb = scope.ServiceProvider.GetRequiredService<SearchDbContext>();
        var searchEngine = scope.ServiceProvider.GetRequiredService<ISearchEngine>();
        var workspaceId = Guid.NewGuid();

        // 1. Arrange: Fake embeddings with known cosine similarity to [1, 0, 0]
        var email1 = EmailSearchIndex.Create(Guid.NewGuid(), workspaceId, 1, DateTimeOffset.UtcNow);
        email1.UpdateEmbedding(CreateVector(1f, 0f, 0f), "test", "v1", 1536); // Cosine Dist = 0 -> Score = 1.0

        var email2 = EmailSearchIndex.Create(Guid.NewGuid(), workspaceId, 1, DateTimeOffset.UtcNow);
        email2.UpdateEmbedding(CreateVector(0f, 1f, 0f), "test", "v1", 1536); // Cosine Dist = 1 -> Score = 0.0

        var email3 = EmailSearchIndex.Create(Guid.NewGuid(), workspaceId, 1, DateTimeOffset.UtcNow);
        email3.UpdateEmbedding(CreateVector(0.9f, 0.1f, 0f), "test", "v1", 1536); // Intermediate score

        searchDb.EmailSearchIndices.AddRange(email1, email2, email3);
        await searchDb.SaveChangesAsync();

        // 2. Act
        var query = new SearchQuery
        {
            WorkspaceId = workspaceId,
            QueryText = "",
            Mode = SearchMode.Semantic,
            Embedding = CreateVector(1f, 0f, 0f).ToArray(), // Search for exactly email1
            PageSize = 10
        };

        var result = await searchEngine.SearchAsync(query);

        // 3. Assert
        result.Hits.Should().HaveCount(3);
        
        // Ranking should be: email1 > email3 > email2 based on vector similarity
        result.Hits[0].EmailId.Should().Be(email1.Id);
        result.Hits[1].EmailId.Should().Be(email3.Id);
        result.Hits[2].EmailId.Should().Be(email2.Id);
    }

    [Fact]
    public async Task Search_ShouldCombineTextAndVectorScores_WhenModeIsHybrid()
    {
        using var scope = _factory.Services.CreateScope();
        var searchDb = scope.ServiceProvider.GetRequiredService<SearchDbContext>();
        var searchEngine = scope.ServiceProvider.GetRequiredService<ISearchEngine>();
        var workspaceId = Guid.NewGuid();

        // emailA: Perfect vector match, no text match
        var emailA = EmailSearchIndex.Create(Guid.NewGuid(), workspaceId, 1, DateTimeOffset.UtcNow);
        emailA.UpdateEmbedding(CreateVector(1f, 0f, 0f), "test", "v1", 1536);
        emailA.UpdateFullTextSearch("apple banana");

        // emailB: Poor vector match, perfect text match
        var emailB = EmailSearchIndex.Create(Guid.NewGuid(), workspaceId, 1, DateTimeOffset.UtcNow);
        emailB.UpdateEmbedding(CreateVector(0f, 1f, 0f), "test", "v1", 1536);
        emailB.UpdateFullTextSearch("targetkeyword");

        searchDb.EmailSearchIndices.AddRange(emailA, emailB);
        await searchDb.SaveChangesAsync();

        var query = new SearchQuery
        {
            WorkspaceId = workspaceId,
            QueryText = "targetkeyword",
            Mode = SearchMode.Hybrid,
            Embedding = CreateVector(0f, 1f, 0f).ToArray(), // perfect vector for emailB, so emailB has BOTH FTS and Vector match
            PageSize = 10
        };

        var result = await searchEngine.SearchAsync(query);

        result.Hits.Should().HaveCount(2);
        
        // EmailB has perfect FTS and perfect Vector, so it must win
        result.Hits[0].EmailId.Should().Be(emailB.Id);
        result.Hits[1].EmailId.Should().Be(emailA.Id);
    }
}
