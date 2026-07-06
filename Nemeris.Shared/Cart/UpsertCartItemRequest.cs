namespace Nemeris.Shared.Cart;

/// <summary>
/// Write shape for the cart: idempotent add-or-set-quantity, where 0 removes the
/// line. Idempotency matters for the App's offline queue — replaying a synced
/// action twice must not double the quantity.
/// </summary>
public sealed class UpsertCartItemRequest
{
    public Guid ProductId { get; set; }

    public int Quantity { get; set; }
}
