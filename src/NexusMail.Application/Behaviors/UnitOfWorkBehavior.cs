using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using NexusMail.Application.Abstractions.Messaging;
using NexusMail.Application.Abstractions.Persistence;
using NexusMail.Shared.Common;

namespace NexusMail.Application.Behaviors;

/// <summary>
/// Pipeline behavior chịu trách nhiệm duy nhất là commit transaction sau khi Handler hoàn tất.
/// Chỉ apply cho Commands (ICommand&lt;TResponse&gt;) — không apply cho Queries.
///
/// Chiến lược commit:
/// - Nếu TResponse implement IResult: chỉ commit khi IsSuccess == true.
/// - Nếu TResponse không phải IResult (ví dụ Guid, string, int): luôn commit (giả định thành công).
/// - Nếu Handler throw exception: không commit (exception propagates tự nhiên).
/// </summary>
public sealed class UnitOfWorkBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand<TResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UnitOfWorkBehavior<TRequest, TResponse>> _logger;

    public UnitOfWorkBehavior(IUnitOfWork unitOfWork, ILogger<UnitOfWorkBehavior<TRequest, TResponse>> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var response = await next();

        var shouldCommit = response is IOperationResult result
            ? result.IsSuccess
            : true; // Non-Result responses: assume success

        if (shouldCommit)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogDebug("UnitOfWork committed for {RequestName}", typeof(TRequest).Name);
        }
        else
        {
            _logger.LogDebug("UnitOfWork skipped (Result.IsFailure) for {RequestName}", typeof(TRequest).Name);
        }

        return response;
    }
}
