using Nemeris.Core.Dtos;
using Nemeris.Core.Entities;

namespace Nemeris.Core.Interfaces;

public interface ICartService
{
    /// <summary>Cart lines including their Product, for price/summary display.</summary>
    Task<IReadOnlyList<CartItem>> GetItemsAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Adds the product or overwrites the quantity; quantity 0 removes the line. Returns null when the product is unknown or inactive.</summary>
    Task<CartItem?> UpsertItemAsync(Guid userId, CartItemUpsertDto dto, CancellationToken ct = default);

    Task<bool> RemoveItemAsync(Guid userId, Guid productId, CancellationToken ct = default);

    Task ClearAsync(Guid userId, CancellationToken ct = default);
}
