namespace Nemeris.Shared.Orders;

/// <summary>One order line — name and price are the checkout-time snapshots, not live product data.</summary>
public sealed class OrderItemDto
{
    public Guid ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public decimal LineTotal { get; set; }
}
