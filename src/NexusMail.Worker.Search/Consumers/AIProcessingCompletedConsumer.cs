using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NexusMail.Contracts.AI;
using NexusMail.Contracts.Search;
using NexusMail.Domain.Search.Entities;
using NexusMail.Infrastructure.Persistence;
using NexusMail.Infrastructure.Search.Persistence;
using Pgvector;

namespace NexusMail.Worker.Search.Consumers;

public class AIProcessingCompletedConsumer : IConsumer<AIProcessingCompletedMessage>
{
    private readonly ApplicationDbContext _appDb;
    private readonly SearchDbContext _searchDb;
    private readonly ILogger<AIProcessingCompletedConsumer> _logger;

    public AIProcessingCompletedConsumer(
        ApplicationDbContext appDb,
        SearchDbContext searchDb,
        ILogger<AIProcessingCompletedConsumer> logger)
    {
        _appDb = appDb;
        _searchDb = searchDb;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AIProcessingCompletedMessage> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Processing search index for EmailId: {EmailId}", msg.EmailId);

        // 1. Fetch Email to get Subject and Body for FTS
        var email = await _appDb.Emails
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == msg.EmailId, context.CancellationToken);

        if (email == null)
        {
            _logger.LogWarning("Email {EmailId} not found. Cannot index.", msg.EmailId);
            return;
        }

        // 2. Fetch or create SearchIndex
        var searchIndex = await _searchDb.EmailSearchIndices
            .FirstOrDefaultAsync(e => e.Id == msg.EmailId, context.CancellationToken);

        if (searchIndex == null)
        {
            searchIndex = EmailSearchIndex.Create(
                msg.EmailId,
                msg.WorkspaceId,
                msg.PriorityResult?.Succeeded == true ? msg.PriorityResult.Value.GetValueOrDefault() : 0,
                email.ReceivedAt);
            
            _searchDb.EmailSearchIndices.Add(searchIndex);
        }
        else
        {
            // Update priority and received at just in case
        }

        // 3. Update Vector
        if (msg.EmbeddingResult?.Succeeded == true && msg.EmbeddingResult.Value != null && msg.EmbeddingResult.Value.Length > 0)
        {
            searchIndex.UpdateEmbedding(
                new Vector(msg.EmbeddingResult.Value),
                "text-embedding-3-small", 
                "v1", 
                msg.EmbeddingResult.Value.Length);
        }

        // 4. Update FTS
        var tagsStr = msg.ClassificationResult?.Succeeded == true && msg.ClassificationResult.Value?.Tags != null 
            ? string.Join(" ", msg.ClassificationResult.Value.Tags) 
            : "";
        var summary = msg.SummaryResult?.Succeeded == true ? msg.SummaryResult.Value : "";
        var searchableText = $"{email.Subject} {email.Content} {summary} {tagsStr}";
        searchIndex.UpdateFullTextSearch(searchableText);

        try
        {
            await _searchDb.SaveChangesAsync(context.CancellationToken);
            _logger.LogInformation("Successfully indexed EmailId: {EmailId}", msg.EmailId);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogWarning(ex, "Concurrency issue when indexing EmailId: {EmailId}. A race condition occurred, retrying...", msg.EmailId);
            throw; // MassTransit will retry the message, at which point FirstOrDefaultAsync will find it.
        }

        // Publish SearchIndexedEvent
        await context.Publish(new SearchIndexedEvent
        {
            EmailId = msg.EmailId,
            WorkspaceId = msg.WorkspaceId,
            IndexedAt = DateTimeOffset.UtcNow
        });
    }
}
