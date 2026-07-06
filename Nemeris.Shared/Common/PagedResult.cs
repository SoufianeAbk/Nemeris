namespace Nemeris.Shared.Common;

/// <summary>
/// Wire shape for every paged list the Api returns. TotalPages/HasNextPage are
/// derived on read so the JSON stays small and can never contradict itself.
/// </summary>
public sealed class PagedResult<T>
{
    public List<T> Items { get; set; } = [];

    public int Page { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasNextPage => Page < TotalPages;

    public bool HasPreviousPage => Page > 1;
}
