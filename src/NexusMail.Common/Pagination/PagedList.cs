namespace NexusMail.Common.Pagination;

/// <summary>
/// A paged list result — wraps items with total count and pagination metadata.
/// </summary>
public sealed class PagedList<T>
{
    public IReadOnlyList<T> Items { get; }
    public int TotalCount { get; }
    public int Page { get; }
    public int PageSize { get; }

    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;

    public PagedList(IReadOnlyList<T> items, int totalCount, int page, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        Page = page;
        PageSize = pageSize;
    }

    public static PagedList<T> Empty(int page = 1, int pageSize = PaginationParams.DefaultPageSize)
        => new(Array.Empty<T>(), 0, page, pageSize);

    public PagedList<TOut> Map<TOut>(Func<T, TOut> mapper)
        => new(Items.Select(mapper).ToList().AsReadOnly(), TotalCount, Page, PageSize);
}
