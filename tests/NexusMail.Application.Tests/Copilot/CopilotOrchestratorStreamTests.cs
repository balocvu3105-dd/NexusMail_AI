using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NexusMail.Application.Abstractions.AI;
using NexusMail.Application.Abstractions.Copilot;
using NexusMail.Application.Abstractions.Search;
using NexusMail.Application.Features.Copilot;
using NexusMail.Application.Features.Email.Abstractions;
using NexusMail.Domain.Email.Entities;
using Xunit;

namespace NexusMail.Application.Tests.Copilot;

public class CopilotOrchestratorStreamTests
{
    [Fact]
    public async Task AskStreamAsync_StateAfterAnswerTest_ShouldStreamTextBeforeState()
    {
        // 3. State-after-answer test
        var searchEngineMock = new Mock<ISearchEngine>();
        var embeddingServiceMock = new Mock<IEmbeddingService>();
        var languageServiceMock = new Mock<ICopilotLanguageService>();
        var emailRepoMock = new Mock<IEmailRepository>();

        var query = new CopilotQuery { WorkspaceId = Guid.NewGuid(), QueryText = "Test" };
        var emailId = Guid.NewGuid();

        embeddingServiceMock.Setup(s => s.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(NexusMail.Shared.Common.Result<float[]>.Success(new float[1536]));

        searchEngineMock.Setup(s => s.SearchAsync(It.IsAny<SearchQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SearchResult 
            { 
                Hits = new List<SearchHit> { new SearchHit { EmailId = emailId, Score = 0.9f, Subject = "Subject", Sender = "Sender", ReceivedAt = DateTimeOffset.UtcNow } }
            });

        emailRepoMock.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Email>());

        // Mock language service to yield text chunks, then a final state
        async IAsyncEnumerable<LanguageStreamChunk> MockStream()
        {
            yield return new LanguageStreamChunk("Hello", null);
            yield return new LanguageStreamChunk(" World", null);
            yield return new LanguageStreamChunk(null, GroundingState.Grounded);
            await Task.CompletedTask;
        }

        languageServiceMock.Setup(l => l.SynthesizeStreamAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(MockStream());

        var orchestrator = new CopilotOrchestrator(searchEngineMock.Object, embeddingServiceMock.Object, languageServiceMock.Object, emailRepoMock.Object);

        var chunks = new List<CopilotStreamChunk>();
        await foreach (var chunk in orchestrator.AskStreamAsync(query))
        {
            chunks.Add(chunk);
        }

        Assert.Equal(3, chunks.Count);
        
        // Chunk 1 & 2: Text, IsDone = false, State = null
        Assert.Equal("Hello", chunks[0].ChunkText);
        Assert.False(chunks[0].IsDone);
        Assert.Null(chunks[0].State);

        Assert.Equal(" World", chunks[1].ChunkText);
        Assert.False(chunks[1].IsDone);
        Assert.Null(chunks[1].State);

        // Chunk 3: IsDone = true, State = Grounded
        Assert.Null(chunks[2].ChunkText);
        Assert.True(chunks[2].IsDone);
        Assert.Equal(GroundingState.Grounded, chunks[2].State);
        Assert.NotNull(chunks[2].Evidence);
    }

    [Fact]
    public async Task AskStreamAsync_CancellationTest_ShouldPropagateCancellation()
    {
        // 4. Cancellation test
        var searchEngineMock = new Mock<ISearchEngine>();
        var embeddingServiceMock = new Mock<IEmbeddingService>();
        var languageServiceMock = new Mock<ICopilotLanguageService>();
        var emailRepoMock = new Mock<IEmailRepository>();

        embeddingServiceMock.Setup(s => s.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(NexusMail.Shared.Common.Result<float[]>.Success(new float[1536]));

        searchEngineMock.Setup(s => s.SearchAsync(It.IsAny<SearchQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SearchResult { Hits = new List<SearchHit>() });

        async IAsyncEnumerable<LanguageStreamChunk> MockStream([System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
        {
            yield return new LanguageStreamChunk("Start", null);
            await Task.Delay(1000, ct); // Should throw TaskCanceledException
            yield return new LanguageStreamChunk("Never reached", null);
        }

        languageServiceMock.Setup(l => l.SynthesizeStreamAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns((string q, string ctx, CancellationToken ct) => MockStream(ct));

        var orchestrator = new CopilotOrchestrator(searchEngineMock.Object, embeddingServiceMock.Object, languageServiceMock.Object, emailRepoMock.Object);

        using var cts = new CancellationTokenSource();
        var stream = orchestrator.AskStreamAsync(new CopilotQuery { WorkspaceId = Guid.NewGuid(), QueryText = "Test" }, cts.Token);

        var enumerator = stream.GetAsyncEnumerator(cts.Token);
        
        Assert.True(await enumerator.MoveNextAsync());
        Assert.Equal("Start", enumerator.Current.ChunkText);

        cts.Cancel(); // Cancel mid-stream

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await enumerator.MoveNextAsync());
    }

    [Fact]
    public async Task AskStreamAsync_LLMFailureTest_ShouldThrowAndNotEmitInsufficientEvidence()
    {
        // 5. LLM failure test: Failure != InsufficientEvidence
        var searchEngineMock = new Mock<ISearchEngine>();
        var embeddingServiceMock = new Mock<IEmbeddingService>();
        var languageServiceMock = new Mock<ICopilotLanguageService>();
        var emailRepoMock = new Mock<IEmailRepository>();

        embeddingServiceMock.Setup(s => s.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(NexusMail.Shared.Common.Result<float[]>.Success(new float[1536]));

        searchEngineMock.Setup(s => s.SearchAsync(It.IsAny<SearchQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SearchResult { Hits = new List<SearchHit>() });

        async IAsyncEnumerable<LanguageStreamChunk> MockStream()
        {
            yield return new LanguageStreamChunk("Start", null);
            throw new InvalidOperationException("Provider failure");
#pragma warning disable CS0162
            yield break;
#pragma warning restore CS0162
        }

        languageServiceMock.Setup(l => l.SynthesizeStreamAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(MockStream());

        var orchestrator = new CopilotOrchestrator(searchEngineMock.Object, embeddingServiceMock.Object, languageServiceMock.Object, emailRepoMock.Object);

        var stream = orchestrator.AskStreamAsync(new CopilotQuery { WorkspaceId = Guid.NewGuid(), QueryText = "Test" });

        var enumerator = stream.GetAsyncEnumerator();
        
        Assert.True(await enumerator.MoveNextAsync());
        Assert.Equal("Start", enumerator.Current.ChunkText);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () => await enumerator.MoveNextAsync());
        Assert.Equal("Provider failure", ex.Message);
        
        // Ensure no IsDone=true / State=InsufficientEvidence chunk was yielded
    }

    [Fact]
    public async Task AskStreamAsync_WorkspaceIsolationTest_ShouldOnlyFetchEmailsForTargetWorkspace()
    {
        // 6. Workspace isolation test
        var searchEngineMock = new Mock<ISearchEngine>();
        var embeddingServiceMock = new Mock<IEmbeddingService>();
        var languageServiceMock = new Mock<ICopilotLanguageService>();
        var emailRepoMock = new Mock<IEmailRepository>();

        var workspaceA = Guid.NewGuid();
        var workspaceB = Guid.NewGuid(); // Another workspace

        embeddingServiceMock.Setup(s => s.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(NexusMail.Shared.Common.Result<float[]>.Success(new float[1536]));

        searchEngineMock.Setup(s => s.SearchAsync(It.IsAny<SearchQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SearchResult 
            { 
                Hits = new List<SearchHit> { new SearchHit { EmailId = Guid.NewGuid(), Score = 0.9f, Subject = "S", Sender = "S", ReceivedAt = DateTimeOffset.UtcNow } }
            });

        // The key assertion: The repository MUST be called with Workspace A, not B
        emailRepoMock.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), workspaceA, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Email>()).Verifiable();

        languageServiceMock.Setup(l => l.SynthesizeStreamAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(EmptyStream());

        var orchestrator = new CopilotOrchestrator(searchEngineMock.Object, embeddingServiceMock.Object, languageServiceMock.Object, emailRepoMock.Object);

        var stream = orchestrator.AskStreamAsync(new CopilotQuery { WorkspaceId = workspaceA, QueryText = "Test" });
        await foreach(var _ in stream) { }

        emailRepoMock.Verify(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), workspaceA, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    private async IAsyncEnumerable<LanguageStreamChunk> EmptyStream()
    {
        yield break;
    }
}
