using MediatR;

namespace NexusMail.Application.Abstractions.Messaging;

/// <summary>
/// Marker interface cho Commands: requests có side effects (write operations).
/// Được nhận diện bởi UnitOfWorkBehavior để quản lý transaction boundary.
/// </summary>
public interface ICommand<out TResponse> : IRequest<TResponse>
{
}

/// <summary>
/// Marker interface cho void Commands (không trả về giá trị).
/// </summary>
public interface ICommand : IRequest
{
}
