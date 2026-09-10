using MediatR;

namespace NexusMail.Application.Abstractions.Messaging;

/// <summary>
/// Marker interface cho Queries: requests không có side effects (read-only operations).
/// UnitOfWorkBehavior KHÔNG apply cho Queries.
/// </summary>
public interface IQuery<out TResponse> : IRequest<TResponse>
{
}
