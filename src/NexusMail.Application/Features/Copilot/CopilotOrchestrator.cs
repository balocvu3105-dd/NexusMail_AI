using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NexusMail.Application.Abstractions.AI;
using NexusMail.Application.Abstractions.Copilot;
using NexusMail.Application.Abstractions.Search;
using NexusMail.Application.Features.Email.Abstractions;
using NexusMail.Shared.Domain;

namespace NexusMail.Application.Features.Copilot;

public sealed class CopilotOrchestrator : ICopilotOrchestrator
{
    private readonly ISearchEngine _searchEngine;
    private readonly IEmbeddingService _embeddingService;
    private readonly ICopilotLanguageService _languageService;
    private readonly IEmailRepository _emailRepository;

    public CopilotOrchestrator(
        ISearchEngine searchEngine,
        IEmbeddingService embeddingService,
        ICopilotLanguageService languageService,
        IEmailRepository emailRepository)
    {
        _searchEngine = searchEngine;
        _embeddingService = embeddingService;
        _languageService = languageService;
        _emailRepository = emailRepository;
    }

    public async Task<CopilotResponse> AskAsync(CopilotQuery query, CancellationToken cancellationToken = default)
    {
        var (contextDocument, evidenceItems) = await BuildCopilotContextAsync(query, cancellationToken);

        // 4. Synthesize Answer
        var synthesisResult = await _languageService.SynthesizeAnswerAsync(query.QueryText, contextDocument, cancellationToken);

        // 5. Return Copilot Response
        // Note: We use the Application's evidence list, NOT anything returned by the LLM.
        // We also only include evidence if the state is Grounded.
        return new CopilotResponse
        {
            Answer = synthesisResult.Answer,
            State = synthesisResult.State,
            Evidence = synthesisResult.State == GroundingState.Grounded ? evidenceItems : new List<EvidenceItem>(),
            SuggestedActions = synthesisResult.State == GroundingState.Grounded ? synthesisResult.SuggestedActions : new List<ActionProposal>()
        };
    }

    public async IAsyncEnumerable<CopilotStreamChunk> AskStreamAsync(CopilotQuery query, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var (contextDocument, evidenceItems) = await BuildCopilotContextAsync(query, cancellationToken);

        var stream = _languageService.SynthesizeStreamAsync(query.QueryText, contextDocument, cancellationToken);

        await foreach (var chunk in stream)
        {
            if (chunk.Text != null)
            {
                yield return new CopilotStreamChunk
                {
                    ChunkText = chunk.Text,
                    IsDone = false
                };
            }
            else if (chunk.FinalState.HasValue)
            {
                var isGrounded = chunk.FinalState.Value == GroundingState.Grounded;
                yield return new CopilotStreamChunk
                {
                    State = chunk.FinalState.Value,
                    Evidence = isGrounded ? evidenceItems : new List<EvidenceItem>(),
                    SuggestedActions = isGrounded ? chunk.SuggestedActions : new List<ActionProposal>(),
                    IsDone = true
                };
            }
        }
    }

    private async Task<(string ContextDocument, List<EvidenceItem> EvidenceItems)> BuildCopilotContextAsync(CopilotQuery query, CancellationToken cancellationToken)
    {
        // 1. Generate embedding for query
        var embeddingResult = await _embeddingService.GenerateEmbeddingAsync(query.QueryText, cancellationToken);
        if (embeddingResult.IsFailure)
        {
            throw new InvalidOperationException($"Failed to generate embedding for query: {embeddingResult.Error.Description}");
        }

        // 2. Perform Hybrid Search
        var searchQuery = new SearchQuery
        {
            WorkspaceId = query.WorkspaceId,
            QueryText = query.QueryText,
            Embedding = embeddingResult.Value,
            Mode = SearchMode.Hybrid,
            Page = 1,
            PageSize = 10 // Deterministic limit: Top 10
        };

        var searchResult = await _searchEngine.SearchAsync(searchQuery, cancellationToken);
        var searchHits = searchResult.Hits ?? new List<SearchHit>();

        // 3. Assemble Context & Evidence Items
        var emailIds = searchHits.Select(h => h.EmailId).Distinct().ToList();
        var emails = new List<NexusMail.Domain.Email.Entities.Email>();
        if (emailIds.Any())
        {
            emails = await _emailRepository.GetByIdsAsync(emailIds, query.WorkspaceId, cancellationToken);
        }

        var contextBuilder = new StringBuilder();
        var evidenceItems = new List<EvidenceItem>();

        foreach (var hit in searchHits)
        {
            var email = emails.FirstOrDefault(e => e.Id == hit.EmailId);
            if (email == null) continue;

            evidenceItems.Add(new EvidenceItem
            {
                EmailId = hit.EmailId,
                Subject = email.Subject,
                RelevanceScore = hit.Score
            });

            // Format context entry
            contextBuilder.AppendLine($"[EmailId: {email.Id}]");
            contextBuilder.AppendLine($"Subject: {email.Subject}");
            contextBuilder.AppendLine($"Date: {email.ReceivedAt:O}");
            
            // Limit snippet to max 500 chars per email
            string snippet = email.Content ?? string.Empty;
            if (snippet.Length > 500)
            {
                snippet = snippet.Substring(0, 500) + "...";
            }
            contextBuilder.AppendLine($"Content: {snippet}");
            contextBuilder.AppendLine();
        }

        return (contextBuilder.ToString(), evidenceItems);
    }
}
