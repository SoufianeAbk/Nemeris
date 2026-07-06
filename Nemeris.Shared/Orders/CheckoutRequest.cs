namespace Nemeris.Shared.Orders;

/// <summary>
/// Turns the server-side cart into an order. Deliberately minimal: prices,
/// stock and address snapshots are all resolved server-side so a client can
/// never tamper with totals.
/// </summary>
public sealed class CheckoutRequest
{
    public Guid ShippingAddressId { get; set; }

    public string? Notes { get; set; }
}
