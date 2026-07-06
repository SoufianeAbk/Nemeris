namespace Nemeris.Core.Dtos;

/// <summary>Create/update payload for categories — the shape is identical for both operations.</summary>
public class CategoryUpsertDto
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentCategoryId { get; set; }
}
