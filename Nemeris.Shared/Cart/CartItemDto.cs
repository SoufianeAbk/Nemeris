namespace Nemeris.Shared.Cart;

/// <summary>
/// Read shape of one cart line. Prices come from the live product at read time
/// (carts, unlike orders, always reflect current prices).
/// </summary>
public sealed class CartItemDto
{
    public Guid ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string? ProductImageUrl { get; set; }

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public decimal LineTotal { get; set; }
}
