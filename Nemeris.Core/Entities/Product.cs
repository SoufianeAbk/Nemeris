namespace Nemeris.Core.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    /// <summary>URL-friendly unique identifier used in storefront routes.</summary>
    public string Slug { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Sku { get; set; } = string.Empty;

    public decimal Price { get; set; }

    /// <summary>Original price shown struck-through when the product is on sale.</summary>
    public decimal? CompareAtPrice { get; set; }

    public int StockQuantity { get; set; }

    public string? ImageUrl { get; set; }

    /// <summary>Inactive products are hidden from the storefront but kept for order history.</summary>
    public bool IsActive { get; set; } = true;

    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
