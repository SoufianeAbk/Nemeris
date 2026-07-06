namespace Nemeris.Core.Entities;

public class Review : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public Guid UserId { get; set; }

    /// <summary>1–5, validated by ReviewCreateDtoValidator and a check constraint.</summary>
    public int Rating { get; set; }

    public string? Title { get; set; }

    public string? Body { get; set; }

    /// <summary>Reviews are moderated: only approved ones appear on the storefront.</summary>
    public bool IsApproved { get; set; }
}
