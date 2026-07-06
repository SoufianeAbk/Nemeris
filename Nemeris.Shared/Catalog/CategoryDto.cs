namespace Nemeris.Shared.Catalog;

public sealed class CategoryDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string? Description { get; set; }

    /// <summary>Null for root categories; clients rebuild the tree from this.</summary>
    public Guid? ParentCategoryId { get; set; }
}
