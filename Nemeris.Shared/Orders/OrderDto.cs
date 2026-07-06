namespace Nemeris.Shared.Orders;

public sealed class OrderDto
{
    public Guid Id { get; set; }

    public string OrderNumber { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Status names as strings ("Pending", "Shipped", …) rather than enum ints:
    /// Shared has no reference to Core's enums, and strings keep old app versions
    /// readable when new statuses are introduced server-side.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    public string PaymentStatus { get; set; } = string.Empty;

    public decimal SubTotal { get; set; }

    public decimal ShippingCost { get; set; }

    public decimal Total { get; set; }

    public string? Notes { get; set; }

    public string ShippingName { get; set; } = string.Empty;
    public string ShippingStreet { get; set; } = string.Empty;
    public string ShippingCity { get; set; } = string.Empty;
    public string ShippingPostalCode { get; set; } = string.Empty;
    public string ShippingCountry { get; set; } = string.Empty;

    public List<OrderItemDto> Items { get; set; } = [];
}
