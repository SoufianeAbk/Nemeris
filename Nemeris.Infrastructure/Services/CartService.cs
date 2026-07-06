using Microsoft.EntityFrameworkCore;
using Nemeris.Core.Dtos;
using Nemeris.Core.Entities;
using Nemeris.Core.Interfaces;
using Nemeris.Infrastructure.Data;

namespace Nemeris.Infrastructure.Services;

public class CartService(NemerisDbContext db) : ICartService
{
    public async Task<IReadOnlyList<CartItem>> GetItemsAsync(Guid userId, CancellationToken ct = default) =>
        await db.CartItems.AsNoTracking()
            .Include(c => c.Product)
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(ct);

    public async Task<CartItem?> UpsertItemAsync(Guid userId, CartItemUpsertDto dto, CancellationToken ct = default)
    {
        var product = await db.Products
            .FirstOrDefaultAsync(p => p.Id == dto.ProductId && p.IsActive, ct);
        if (product is null)
        {
            return null;
        }

        var item = await db.CartItems
            .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == dto.ProductId, ct);

        if (dto.Quantity == 0)
        {
            if (item is not null)
            {
                db.CartItems.Remove(item);
                await db.SaveChangesAsync(ct);
            }

            return null;
        }

        if (item is null)
        {
            item = new CartItem { UserId = userId, ProductId = dto.ProductId, Quantity = dto.Quantity };
            db.CartItems.Add(item);
        }
        else
        {
            item.Quantity = dto.Quantity;
        }

        await db.SaveChangesAsync(ct);
        item.Product = product;
        return item;
    }

    public async Task<bool> RemoveItemAsync(Guid userId, Guid productId, CancellationToken ct = default)
    {
        var item = await db.CartItems
            .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId, ct);
        if (item is null)
        {
            return false;
        }

        db.CartItems.Remove(item);
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task ClearAsync(Guid userId, CancellationToken ct = default)
    {
        var items = await db.CartItems.Where(c => c.UserId == userId).ToListAsync(ct);
        db.CartItems.RemoveRange(items);
        await db.SaveChangesAsync(ct);
    }
}
