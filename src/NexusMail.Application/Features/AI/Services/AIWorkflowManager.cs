using System;
using System.Threading;
using System.Threading.Tasks;
using NexusMail.Application.Abstractions.AI;
using NexusMail.Application.Abstractions.Persistence;
using NexusMail.Application.Features.Email.Abstractions;
using NexusMail.Domain.AI.Entities;
using NexusMail.Domain.AI.Enums;
using NexusMail.Shared.Domain;

namespace NexusMail.Application.Features.AI.Services;

public interface IAIWorkflowManager
{
    Task<Result<Guid>> ProcessEmailAIAsync(Guid emailId, Guid workspaceId, CancellationToken cancellationToken);
    Task<Result<Guid>> GenerateEmbeddingAsync(Guid emailId, Guid workspaceId, CancellationToken cancellationToken);
}

public sealed class AIWorkflowManager : IAIWorkflowManager
{
    private readonly IEmailRepository _emailRepository;
    private readonly IAIProcessingService _aiProcessingService;
    private readonly IEmbeddingService _embeddingService;
    private readonly IUnitOfWork _unitOfWork;

    public AIWorkflowManager(
        IEmailRepository emailRepository,
        IAIProcessingService aiProcessingService,
        IEmbeddingService embeddingService,
        IUnitOfWork unitOfWork)
    {
        _emailRepository = emailRepository;
        _aiProcessingService = aiProcessingService;
        _embeddingService = embeddingService;
        _unitOfWork = unitOfWork;
    }

    private async Task<AIAnalysis> InitializeAnalysisAsync(NexusMail.Domain.Email.Entities.Email email, AIAnalysis? analysis, CancellationToken cancellationToken)
    {
        if (analysis == null)
        {
            analysis = AIAnalysis.Create(email.Id);
            await _emailRepository.AddAnalysisAsync(analysis, cancellationToken);
            try
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex) when (ex.GetType().Name == "DbUpdateException")
            {
                throw new AIProviderTransientException("Concurrent initialization detected. Retrying to load existing AIAnalysis.");
            }
        }
        return analysis;
    }

    public async Task<Result<Guid>> ProcessEmailAIAsync(Guid emailId, Guid workspaceId, CancellationToken cancellationToken)
    {
        var result = await _emailRepository.GetEmailWithAnalysisAsync(emailId, workspaceId, cancellationToken);
        if (result == null)
            return Result.Failure<Guid>(new Error("Email.NotFound", "Email not found."));

        var email = result.Value.Email;
        var analysis = await InitializeAnalysisAsync(email, result.Value.Analysis, cancellationToken);

        Guid attemptId = Guid.NewGuid();
        if (!analysis.StartProcessing(attemptId))
        {
            if (analysis.ProcessingState == AIProcessingStatus.Processing)
            {
                throw new AIProviderTransientException("Email is currently being processed by another attempt. Retrying...");
            }
            return Result.Success(analysis.Id); // Already succeeded
        }

        _emailRepository.UpdateAnalysis(analysis);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            var aiResult = await _aiProcessingService.ProcessEmailAsync(email.Subject, email.Content, email.Sender, cancellationToken);
            if (aiResult.IsSuccess)
            {
                var val = aiResult.Value;
                analysis.CompleteProcessing(
                    attemptId, 
                    val.Summary, 
                    val.PriorityScore, 
                    val.PriorityReason, 
                    val.Category, 
                    val.Confidence, 
                    new System.Collections.Generic.List<string>(val.Tags), 
                    val.NeedsAttention);
            }
            else
            {
                analysis.FailProcessing(attemptId, aiResult.Error.Description);
            }

            _emailRepository.UpdateAnalysis(analysis);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success(analysis.Id);
        }
        catch (AIProviderTransientException)
        {
            analysis.ResetProcessingToPending(attemptId);
            _emailRepository.UpdateAnalysis(analysis);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            throw;
        }
    }

    public async Task<Result<Guid>> GenerateEmbeddingAsync(Guid emailId, Guid workspaceId, CancellationToken cancellationToken)
    {
        var result = await _emailRepository.GetEmailWithAnalysisAsync(emailId, workspaceId, cancellationToken);
        if (result == null)
            return Result.Failure<Guid>(new Error("Email.NotFound", "Email not found."));

        var email = result.Value.Email;
        var analysis = await InitializeAnalysisAsync(email, result.Value.Analysis, cancellationToken);

        Guid attemptId = Guid.NewGuid();
        if (!analysis.StartEmbedding(attemptId))
            return Result.Success(analysis.Id);

        _emailRepository.UpdateAnalysis(analysis);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            var embeddingContent = $"Subject: {email.Subject}\n\n{email.Content}";
            var embeddingResult = await _embeddingService.GenerateEmbeddingAsync(embeddingContent, cancellationToken);
            if (embeddingResult.IsSuccess)
                analysis.CompleteEmbedding(attemptId, embeddingResult.Value);
            else
                analysis.FailEmbedding(attemptId, embeddingResult.Error.Description);

            _emailRepository.UpdateAnalysis(analysis);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success(analysis.Id);
        }
        catch (AIProviderTransientException)
        {
            analysis.ResetEmbeddingToPending(attemptId);
            _emailRepository.UpdateAnalysis(analysis);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            throw;
        }
    }
}
