namespace Nemeris.Core.Entities;

/// <summary>
/// Server-side persistent cart line. One row per (user, product) pair,
/// enforced by a unique index in the entity configuration.
/// </summary>
public class CartItem : BaseEntity
{
    public Guid UserId { get; set; }

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int Quantity { get; set; }
}
