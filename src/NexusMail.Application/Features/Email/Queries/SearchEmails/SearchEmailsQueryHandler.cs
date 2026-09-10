using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NexusMail.Application.Abstractions.Authentication;
using NexusMail.Application.Abstractions.Search;
using NexusMail.Application.Features.Email.Abstractions;
using NexusMail.Shared.Domain;

namespace NexusMail.Application.Features.Email.Queries.SearchEmails;

public sealed class SearchEmailsQueryHandler : IRequestHandler<SearchEmailsQuery, Result<SearchResult>>
{
    private readonly ISearchEngine _searchEngine;
    private readonly IEmailRepository _emailRepository;
    private readonly IWorkspaceContext _workspaceContext;

    public SearchEmailsQueryHandler(ISearchEngine searchEngine, IEmailRepository emailRepository, IWorkspaceContext workspaceContext)
    {
        _searchEngine = searchEngine;
        _emailRepository = emailRepository;
        _workspaceContext = workspaceContext;
    }

    public async Task<Result<SearchResult>> Handle(SearchEmailsQuery request, CancellationToken cancellationToken)
    {
        var workspaceId = _workspaceContext.WorkspaceId;
        if (!workspaceId.HasValue)
        {
            return Result.Failure<SearchResult>(new Error("Workspace.Unauthorized", "No active workspace context."));
        }

        var searchQuery = new SearchQuery
        {
            WorkspaceId = workspaceId.Value,
            QueryText = request.QueryText,
            Mode = request.Mode,
            Page = request.Page,
            PageSize = request.PageSize,
            From = request.From,
            To = request.To,
            Sender = request.Sender,
            Labels = request.Labels ?? [],
            HasAttachments = request.HasAttachments
        };

        var searchResult = await _searchEngine.SearchAsync(searchQuery, cancellationToken);
        if (searchResult.Hits.Count == 0)
        {
            return Result.Success(searchResult);
        }

        // Enrich with real DB data since Search index might only have placeholders for some fields (like Subject)
        var emailIds = searchResult.Hits.Select(h => h.EmailId).Distinct().ToList();
        var dbEmails = await _emailRepository.GetByIdsAsync(emailIds, workspaceId.Value, cancellationToken);
        var emailDict = dbEmails.ToDictionary(e => e.Id);

        var enrichedHits = searchResult.Hits.Select(hit =>
        {
            if (emailDict.TryGetValue(hit.EmailId, out var dbEmail))
            {
                return hit with
                {
                    Subject = dbEmail.Subject,
                    Sender = dbEmail.Sender,
                    ReceivedAt = dbEmail.ReceivedAt
                };
            }
            return hit;
        }).ToList();

        var enrichedResult = searchResult with { Hits = enrichedHits };

        return Result.Success(enrichedResult);
    }
}
