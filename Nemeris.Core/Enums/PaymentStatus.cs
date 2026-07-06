namespace Nemeris.Core.Enums;

/// <summary>Payment lifecycle, tracked independently of fulfilment (OrderStatus).</summary>
public enum PaymentStatus
{
    Pending = 0,
    Paid = 1,
    Failed = 2,
    Refunded = 3
}
