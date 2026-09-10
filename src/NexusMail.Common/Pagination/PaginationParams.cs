namespace NexusMail.Common.Pagination;

/// <summary>
/// Pagination parameters for list queries.
/// Zero external dependencies.
/// </summary>
public sealed record PaginationParams
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    public int Page { get; init; } = DefaultPage;
    public int PageSize { get; init; } = DefaultPageSize;

    public int Skip => (Page - 1) * PageSize;
    public int Take => PageSize;

    public static PaginationParams Default => new();

    public static PaginationParams Of(int page, int pageSize)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);
        return new PaginationParams { Page = page, PageSize = pageSize };
    }
}
