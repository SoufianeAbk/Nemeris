namespace Nemeris.Core.Enums;

/// <summary>
/// Fulfilment lifecycle of an order. Values are explicit because they are
/// persisted as ints and shared over the wire with the MAUI app.
/// </summary>
public enum OrderStatus
{
    Pending = 0,
    Confirmed = 1,
    Processing = 2,
    Shipped = 3,
    Delivered = 4,
    Cancelled = 5,
    Refunded = 6
}
