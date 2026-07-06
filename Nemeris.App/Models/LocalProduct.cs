using Nemeris.Shared.Catalog;
using SQLite;

namespace Nemeris.App.Models;

/// <summary>
/// Encrypted-SQLite cache row. Mirrors ProductDto (the app caches the wire
/// contract, not a domain model) plus a CachedAtUtc stamp for staleness display.
/// </summary>
[Table("products")]
public class LocalProduct
{
    [PrimaryKey]
    public Guid Id { get; set; }

    [Indexed]
    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Sku { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public decimal? CompareAtPrice { get; set; }

    public int StockQuantity { get; set; }

    public string? ImageUrl { get; set; }

    public Guid CategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public DateTime CachedAtUtc { get; set; }

    public static LocalProduct FromDto(ProductDto dto) => new()
    {
        Id = dto.Id,
        Name = dto.Name,
        Slug = dto.Slug,
        Description = dto.Description,
        Sku = dto.Sku,
        Price = dto.Price,
        CompareAtPrice = dto.CompareAtPrice,
        StockQuantity = dto.StockQuantity,
        ImageUrl = dto.ImageUrl,
        CategoryId = dto.CategoryId,
        CategoryName = dto.CategoryName,
        CachedAtUtc = DateTime.UtcNow,
    };

    public ProductDto ToDto() => new()
    {
        Id = Id,
        Name = Name,
        Slug = Slug,
        Description = Description,
        Sku = Sku,
        Price = Price,
        CompareAtPrice = CompareAtPrice,
        StockQuantity = StockQuantity,
        ImageUrl = ImageUrl,
        CategoryId = CategoryId,
        CategoryName = CategoryName,
    };
}
