using Nemeris.Core.Enums;

namespace Nemeris.Api.Contracts;

/// <summary>
/// Admin-only request; not in Nemeris.Shared because the App never updates order
/// status. JsonStringEnumConverter lets clients send "Shipped" rather than 3.
/// </summary>
public sealed class UpdateOrderStatusRequest
{
    public OrderStatus Status { get; set; }
}
