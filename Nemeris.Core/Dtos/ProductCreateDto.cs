namespace Nemeris.Core.Dtos;

/// <summary>Write-side input for creating a product (admin surfaces in Api and Web).</summary>
public class ProductCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Sku { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public int StockQuantity { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid CategoryId { get; set; }
}
