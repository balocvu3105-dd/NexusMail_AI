using MassTransit;
using NexusMail.Application.Abstractions.Persistence;

namespace NexusMail.Application.Features.Search.Services;

public interface ISearchReindexerService
{
    Task ReindexEmailAsync(Guid emailId, CancellationToken cancellationToken = default);
    Task ReindexWorkspaceAsync(Guid workspaceId, CancellationToken cancellationToken = default);
    Task ReindexAllAsync(CancellationToken cancellationToken = default);
    Task ReindexModelVersionAsync(string oldModelVersion, CancellationToken cancellationToken = default);
}

public sealed class SearchReindexerService : ISearchReindexerService
{
    private readonly IUnitOfWork _uow;
    private readonly IBus _bus; // Or IPublishEndpoint

    public SearchReindexerService(IUnitOfWork uow, IBus bus)
    {
        _uow = uow;
        _bus = bus;
    }

    public async Task ReindexEmailAsync(Guid emailId, CancellationToken cancellationToken = default)
    {
        // Re-publish a command/event to trigger the AI pipeline
        await _bus.Publish(new NexusMail.Contracts.Email.EmailReceivedMessage
        {
            EmailId = emailId,
            // other properties omitted for brevity, ideally we'd load them from a repository.
        }, cancellationToken);
    }

    public async Task ReindexWorkspaceAsync(Guid workspaceId, CancellationToken cancellationToken = default)
    {
        // Fetch all email IDs for workspace, batch publish
    }

    public async Task ReindexAllAsync(CancellationToken cancellationToken = default)
    {
        // Fetch all email IDs, batch publish
    }

    public async Task ReindexModelVersionAsync(string oldModelVersion, CancellationToken cancellationToken = default)
    {
        // Fetch all email IDs where model version matches, batch publish
    }
}
