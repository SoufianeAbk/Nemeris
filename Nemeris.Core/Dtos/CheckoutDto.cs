namespace Nemeris.Core.Dtos;

/// <summary>
/// Input for turning the user's cart into an order. The service loads the
/// address, snapshots it onto the order and clears the cart atomically.
/// </summary>
public class CheckoutDto
{
    public Guid ShippingAddressId { get; set; }
    public string? Notes { get; set; }
}
