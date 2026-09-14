using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NexusMail.Application.Abstractions.AI;
using NexusMail.Application.Abstractions.Copilot;
using NexusMail.Application.Abstractions.Search;
using NexusMail.Application.Features.Copilot;
using NexusMail.Application.Features.Email.Abstractions;
using NexusMail.Domain.Email.Entities;
using NexusMail.Shared.Common;
using Xunit;

namespace NexusMail.Application.Tests.Copilot;

public class CopilotOrchestratorTests
{
    private readonly Mock<ISearchEngine> _mockSearchEngine;
    private readonly Mock<IEmbeddingService> _mockEmbeddingService;
    private readonly Mock<ICopilotLanguageService> _mockLanguageService;
    private readonly Mock<IEmailRepository> _mockEmailRepository;
    private readonly CopilotOrchestrator _orchestrator;

    public CopilotOrchestratorTests()
    {
        _mockSearchEngine = new Mock<ISearchEngine>();
        _mockEmbeddingService = new Mock<IEmbeddingService>();
        _mockLanguageService = new Mock<ICopilotLanguageService>();
        _mockEmailRepository = new Mock<IEmailRepository>();

        _orchestrator = new CopilotOrchestrator(
            _mockSearchEngine.Object,
            _mockEmbeddingService.Object,
            _mockLanguageService.Object,
            _mockEmailRepository.Object);
    }

    [Fact]
    public async Task TestA_Grounded_SearchHitsExist_And_LLMReturnsGrounded_ShouldReturnEvidence()
    {
        // Arrange
        var query = new CopilotQuery { WorkspaceId = Guid.NewGuid(), QueryText = "Hello" };
        var emailId = Guid.NewGuid();
        
        _mockEmbeddingService
            .Setup(s => s.GenerateEmbeddingAsync(query.QueryText, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new float[] { 1f, 0f }));

        _mockSearchEngine
            .Setup(s => s.SearchAsync(It.IsAny<SearchQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SearchResult 
            { 
                Hits = new List<SearchHit> { new SearchHit { EmailId = emailId, Score = 0.9f, Subject = "Subject", Sender = "Sender", ReceivedAt = DateTimeOffset.UtcNow } }
            });

        // Use reflection to create Email for testing since constructor is private
        var email = (Email)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(Email));
        typeof(Email).GetProperty("Id").SetValue(email, emailId);
        typeof(Email).GetProperty("Subject").SetValue(email, "Test Subject");
        typeof(Email).GetProperty("Content").SetValue(email, "Test Content");

        _mockEmailRepository
            .Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), query.WorkspaceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Email> { email });

        _mockLanguageService
            .Setup(s => s.SynthesizeAnswerAsync(query.QueryText, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LLMSynthesisResult { Answer = "Yes", State = GroundingState.Grounded });

        // Act
        var response = await _orchestrator.AskAsync(query);

        // Assert
        response.State.Should().Be(GroundingState.Grounded);
        response.Answer.Should().Be("Yes");
        response.Evidence.Should().HaveCount(1);
        response.Evidence.First().EmailId.Should().Be(emailId);
        response.Evidence.First().Subject.Should().Be("Test Subject");
    }

    [Fact]
    public async Task TestB_Insufficient_SearchHitsEmpty_ShouldReturnNoEvidence()
    {
        // Arrange
        var query = new CopilotQuery { WorkspaceId = Guid.NewGuid(), QueryText = "Hello" };
        
        _mockEmbeddingService
            .Setup(s => s.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new float[] { 1f, 0f }));

        _mockSearchEngine
            .Setup(s => s.SearchAsync(It.IsAny<SearchQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SearchResult { Hits = new List<SearchHit>() });

        _mockLanguageService
            .Setup(s => s.SynthesizeAnswerAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LLMSynthesisResult { Answer = "I don't know", State = GroundingState.InsufficientEvidence });

        // Act
        var response = await _orchestrator.AskAsync(query);

        // Assert
        response.State.Should().Be(GroundingState.InsufficientEvidence);
        response.Evidence.Should().BeEmpty();
    }

    [Fact]
    public async Task TestC_Workspace_WorkspaceIsPropagatedToSearchEngine()
    {
        // Arrange
        var workspaceId = Guid.NewGuid();
        var query = new CopilotQuery { WorkspaceId = workspaceId, QueryText = "Hello" };
        
        _mockEmbeddingService
            .Setup(s => s.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new float[] { 1f }));

        _mockSearchEngine
            .Setup(s => s.SearchAsync(It.IsAny<SearchQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SearchResult { Hits = new List<SearchHit>() });

        _mockLanguageService
            .Setup(s => s.SynthesizeAnswerAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LLMSynthesisResult { Answer = "No context", State = GroundingState.InsufficientEvidence });

        // Act
        await _orchestrator.AskAsync(query);

        // Assert
        _mockSearchEngine.Verify(s => s.SearchAsync(It.Is<SearchQuery>(q => q.WorkspaceId == workspaceId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TestD_Provenance_LLMCannotFabricateEvidence()
    {
        // Arrange
        var query = new CopilotQuery { WorkspaceId = Guid.NewGuid(), QueryText = "Hello" };
        var realEmailId = Guid.NewGuid();
        
        _mockEmbeddingService
            .Setup(s => s.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new float[] { 1f }));

        _mockSearchEngine
            .Setup(s => s.SearchAsync(It.IsAny<SearchQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SearchResult 
            { 
                Hits = new List<SearchHit> { new SearchHit { EmailId = realEmailId, Subject = "Real Evidence", Sender = "Sender", ReceivedAt = DateTimeOffset.UtcNow } }
            });

        var email = (Email)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(Email));
        typeof(Email).GetProperty("Id").SetValue(email, realEmailId);
        typeof(Email).GetProperty("Subject").SetValue(email, "Real Evidence");

        _mockEmailRepository
            .Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Email> { email });

        _mockLanguageService
            .Setup(s => s.SynthesizeAnswerAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LLMSynthesisResult 
            { 
                // LLM attempts to claim citation of some fake ID in the text, but the state is Grounded
                Answer = "Based on fake ID " + Guid.NewGuid(), 
                State = GroundingState.Grounded 
            });

        // Act
        var response = await _orchestrator.AskAsync(query);

        // Assert
        // Application controls provenance, so it strictly binds evidence to what ISearchEngine returned.
        response.State.Should().Be(GroundingState.Grounded);
        response.Evidence.Should().HaveCount(1);
        response.Evidence.Single().EmailId.Should().Be(realEmailId);
        response.Evidence.Single().Subject.Should().Be("Real Evidence");
    }

    [Fact]
    public async Task TestE_LLMFailure_ThrowsException_And_NotTranslatedToInsufficientEvidence()
    {
        // Arrange
        var query = new CopilotQuery { WorkspaceId = Guid.NewGuid(), QueryText = "Hello" };
        
        _mockEmbeddingService
            .Setup(s => s.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new float[] { 1f }));

        _mockSearchEngine
            .Setup(s => s.SearchAsync(It.IsAny<SearchQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SearchResult { Hits = new List<SearchHit>() });

        _mockLanguageService
            .Setup(s => s.SynthesizeAnswerAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("API Key missing"));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _orchestrator.AskAsync(query));
        ex.Message.Should().Be("API Key missing");
    }
}
