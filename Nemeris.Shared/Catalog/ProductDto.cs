namespace Nemeris.Shared.Catalog;

public sealed class ProductDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Sku { get; set; } = string.Empty;

    public decimal Price { get; set; }

    /// <summary>Struck-through original price when on sale; null otherwise.</summary>
    public decimal? CompareAtPrice { get; set; }

    public int StockQuantity { get; set; }

    public string? ImageUrl { get; set; }

    public Guid CategoryId { get; set; }

    /// <summary>Denormalized so list screens never need a second category lookup.</summary>
    public string CategoryName { get; set; } = string.Empty;
}
