using Nemeris.Core.Enums;

namespace Nemeris.Core.Entities;

public class Order : BaseEntity
{
    /// <summary>Human-readable reference (e.g. NEM-20260706-A1B2C3) shown to customers and support.</summary>
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary>FK to ApplicationUser (Identity lives in Infrastructure, so only the Guid is referenced here).</summary>
    public Guid UserId { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

    public decimal SubTotal { get; set; }

    public decimal ShippingCost { get; set; }

    public decimal Total { get; set; }

    public string? Notes { get; set; }

    // Shipping address is snapshotted onto the order at checkout so that
    // editing/deleting an address later never rewrites order history.
    public string ShippingName { get; set; } = string.Empty;
    public string ShippingStreet { get; set; } = string.Empty;
    public string ShippingCity { get; set; } = string.Empty;
    public string ShippingPostalCode { get; set; } = string.Empty;
    public string ShippingCountry { get; set; } = string.Empty;

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
