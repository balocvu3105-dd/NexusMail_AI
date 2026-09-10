using System;
using System.Threading;
using System.Threading.Tasks;
using NexusMail.Application.Abstractions.AI;
using NexusMail.Application.Abstractions.Persistence;
using NexusMail.Application.Features.Email.Abstractions;
using NexusMail.Domain.AI.Entities;
using NexusMail.Shared.Domain;

namespace NexusMail.Application.Features.AI.Services;

public interface IAIWorkflowManager
{
    Task<Result<Guid>> GenerateSummaryAsync(Guid emailId, Guid workspaceId, CancellationToken cancellationToken);
    Task<Result<Guid>> ScorePriorityAsync(Guid emailId, Guid workspaceId, CancellationToken cancellationToken);
    Task<Result<Guid>> CategorizeEmailAsync(Guid emailId, Guid workspaceId, CancellationToken cancellationToken);
    Task<Result<Guid>> GenerateEmbeddingAsync(Guid emailId, Guid workspaceId, CancellationToken cancellationToken);
}

public sealed class AIWorkflowManager : IAIWorkflowManager
{
    private readonly IEmailRepository _emailRepository;
    private readonly ISummaryService _summaryService;
    private readonly IPriorityService _priorityService;
    private readonly IClassificationService _classificationService;
    private readonly IEmbeddingService _embeddingService;
    private readonly IUnitOfWork _unitOfWork;

    public AIWorkflowManager(
        IEmailRepository emailRepository,
        ISummaryService summaryService,
        IPriorityService priorityService,
        IClassificationService classificationService,
        IEmbeddingService embeddingService,
        IUnitOfWork unitOfWork)
    {
        _emailRepository = emailRepository;
        _summaryService = summaryService;
        _priorityService = priorityService;
        _classificationService = classificationService;
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

    public async Task<Result<Guid>> GenerateSummaryAsync(Guid emailId, Guid workspaceId, CancellationToken cancellationToken)
    {
        var result = await _emailRepository.GetEmailWithAnalysisAsync(emailId, workspaceId, cancellationToken);
        if (result == null)
            return Result.Failure<Guid>(new Error("Email.NotFound", "Email not found."));

        var email = result.Value.Email;
        var analysis = await InitializeAnalysisAsync(email, result.Value.Analysis, cancellationToken);

        Guid attemptId = Guid.NewGuid();
        if (!analysis.StartSummary(attemptId))
            return Result.Success(analysis.Id);

        _emailRepository.UpdateAnalysis(analysis);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            var summaryResult = await _summaryService.GenerateSummaryAsync(email.Subject, email.Content, cancellationToken);
            if (summaryResult.IsSuccess)
                analysis.CompleteSummary(attemptId, summaryResult.Value);
            else
                analysis.FailSummary(attemptId, summaryResult.Error.Description);

            _emailRepository.UpdateAnalysis(analysis);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success(analysis.Id);
        }
        catch (AIProviderTransientException)
        {
            analysis.ResetSummaryToPending(attemptId);
            _emailRepository.UpdateAnalysis(analysis);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            throw;
        }
    }

    public async Task<Result<Guid>> ScorePriorityAsync(Guid emailId, Guid workspaceId, CancellationToken cancellationToken)
    {
        var result = await _emailRepository.GetEmailWithAnalysisAsync(emailId, workspaceId, cancellationToken);
        if (result == null)
            return Result.Failure<Guid>(new Error("Email.NotFound", "Email not found."));

        var email = result.Value.Email;
        var analysis = await InitializeAnalysisAsync(email, result.Value.Analysis, cancellationToken);

        Guid attemptId = Guid.NewGuid();
        if (!analysis.StartPriority(attemptId))
            return Result.Success(analysis.Id);

        _emailRepository.UpdateAnalysis(analysis);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            var priorityResult = await _priorityService.GeneratePriorityAsync(email.Subject, email.Content, email.Sender, cancellationToken);
            if (priorityResult.IsSuccess)
                analysis.CompletePriority(attemptId, priorityResult.Value.Score, priorityResult.Value.Reason);
            else
                analysis.FailPriority(attemptId, priorityResult.Error.Description);

            _emailRepository.UpdateAnalysis(analysis);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success(analysis.Id);
        }
        catch (AIProviderTransientException)
        {
            analysis.ResetPriorityToPending(attemptId);
            _emailRepository.UpdateAnalysis(analysis);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            throw;
        }
    }

    public async Task<Result<Guid>> CategorizeEmailAsync(Guid emailId, Guid workspaceId, CancellationToken cancellationToken)
    {
        var result = await _emailRepository.GetEmailWithAnalysisAsync(emailId, workspaceId, cancellationToken);
        if (result == null)
            return Result.Failure<Guid>(new Error("Email.NotFound", "Email not found."));

        var email = result.Value.Email;
        var analysis = await InitializeAnalysisAsync(email, result.Value.Analysis, cancellationToken);

        Guid attemptId = Guid.NewGuid();
        if (!analysis.StartClassification(attemptId))
            return Result.Success(analysis.Id);

        _emailRepository.UpdateAnalysis(analysis);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            var classificationResult = await _classificationService.GenerateClassificationAsync(email.Subject, email.Content, email.Sender, cancellationToken);
            if (classificationResult.IsSuccess)
            {
                var val = classificationResult.Value;
                analysis.CompleteClassification(attemptId, val.Category, val.Confidence, new System.Collections.Generic.List<string>(val.Tags));
            }
            else
            {
                analysis.FailClassification(attemptId, classificationResult.Error.Description);
            }

            _emailRepository.UpdateAnalysis(analysis);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success(analysis.Id);
        }
        catch (AIProviderTransientException)
        {
            analysis.ResetClassificationToPending(attemptId);
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
            var embeddingResult = await _embeddingService.GenerateEmbeddingAsync(email.Content, cancellationToken);
            if (embeddingResult.IsSuccess)
                analysis.CompleteEmbedding(attemptId);
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
