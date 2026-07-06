using Nemeris.Core.Common;
using Nemeris.Core.Dtos;
using Nemeris.Core.Entities;
using Nemeris.Core.Enums;

namespace Nemeris.Core.Interfaces;

public interface IOrderService
{
    /// <summary>
    /// Converts the user's cart into an order inside a transaction: validates stock,
    /// snapshots prices and the shipping address, decrements stock, clears the cart.
    /// Throws InvalidOperationException with a translatable message key on business-rule violations.
    /// </summary>
    Task<Order> CheckoutAsync(Guid userId, CheckoutDto dto, CancellationToken ct = default);

    Task<PagedList<Order>> GetForUserAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);

    /// <summary>Pass userId to scope to the owner (customer endpoints); null for admin access.</summary>
    Task<Order?> GetByIdAsync(Guid orderId, Guid? userId = null, CancellationToken ct = default);

    Task<PagedList<Order>> GetPagedAsync(int page, int pageSize, OrderStatus? status = null, CancellationToken ct = default);

    Task<Order?> UpdateStatusAsync(Guid orderId, OrderStatus status, CancellationToken ct = default);
}
