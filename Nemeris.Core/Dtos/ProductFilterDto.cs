namespace Nemeris.Core.Dtos;

/// <summary>Catalog query parameters: search, category, price range, sorting and paging.</summary>
public class ProductFilterDto
{
    public string? Search { get; set; }

    public Guid? CategoryId { get; set; }

    public decimal? MinPrice { get; set; }

    public decimal? MaxPrice { get; set; }

    /// <summary>"name" | "price" | "price_desc" | "newest" — anything else falls back to name.</summary>
    public string? SortBy { get; set; }

    /// <summary>Admin-only: include products hidden from the storefront.</summary>
    public bool IncludeInactive { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}
