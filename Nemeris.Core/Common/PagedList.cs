namespace Nemeris.Core.Common;

/// <summary>
/// Internal paged result used by services. The Api maps this onto the
/// wire-contract PagedResult&lt;T&gt; in Nemeris.Shared; keeping a separate type
/// here means Core never needs a reference to Shared.
/// </summary>
public class PagedList<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];

    public int Page { get; init; }

    public int PageSize { get; init; }

    public int TotalCount { get; init; }

    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasNextPage => Page < TotalPages;

    public bool HasPreviousPage => Page > 1;
}
