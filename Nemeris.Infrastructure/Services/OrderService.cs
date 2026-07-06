using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Nemeris.Core.Common;
using Nemeris.Core.Dtos;
using Nemeris.Core.Entities;
using Nemeris.Core.Enums;
using Nemeris.Core.Interfaces;
using Nemeris.Infrastructure.Data;

namespace Nemeris.Infrastructure.Services;

public class OrderService(NemerisDbContext db) : IOrderService
{
    private const decimal FreeShippingThreshold = 50m;
    private const decimal StandardShippingCost = 4.95m;

    public async Task<Order> CheckoutAsync(Guid userId, CheckoutDto dto, CancellationToken ct = default)
    {
        // EnableRetryOnFailure requires user transactions to go through the execution strategy.
        var strategy = db.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await db.Database.BeginTransactionAsync(ct);

            var cartItems = await db.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .ToListAsync(ct);

            if (cartItems.Count == 0)
            {
                throw new InvalidOperationException("Error.CartEmpty");
            }

            var address = await db.Addresses
                .FirstOrDefaultAsync(a => a.Id == dto.ShippingAddressId && a.UserId == userId, ct)
                ?? throw new InvalidOperationException("Error.AddressNotFound");

            var order = new Order
            {
                OrderNumber = GenerateOrderNumber(),
                UserId = userId,
                Notes = dto.Notes,
                ShippingName = address.FullName,
                ShippingStreet = address.Street,
                ShippingCity = address.City,
                ShippingPostalCode = address.PostalCode,
                ShippingCountry = address.Country,
            };

            foreach (var cartItem in cartItems)
            {
                var product = cartItem.Product;

                if (!product.IsActive)
                {
                    throw new InvalidOperationException("Error.ProductUnavailable");
                }

                if (product.StockQuantity < cartItem.Quantity)
                {
                    throw new InvalidOperationException("Error.InsufficientStock");
                }

                product.StockQuantity -= cartItem.Quantity;

                order.Items.Add(new OrderItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    UnitPrice = product.Price,
                    Quantity = cartItem.Quantity,
                    LineTotal = product.Price * cartItem.Quantity,
                });
            }

            order.SubTotal = order.Items.Sum(i => i.LineTotal);
            order.ShippingCost = order.SubTotal >= FreeShippingThreshold ? 0m : StandardShippingCost;
            order.Total = order.SubTotal + order.ShippingCost;

            db.Orders.Add(order);
            db.CartItems.RemoveRange(cartItems);

            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return order;
        });
    }

    public async Task<PagedList<Order>> GetForUserAsync(Guid userId, int page, int pageSize, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = db.Orders.AsNoTracking()
            .Include(o => o.Items)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt);

        return new PagedList<Order>
        {
            Items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct),
            Page = page,
            PageSize = pageSize,
            TotalCount = await query.CountAsync(ct),
        };
    }

    public Task<Order?> GetByIdAsync(Guid orderId, Guid? userId = null, CancellationToken ct = default)
    {
        var query = db.Orders.AsNoTracking().Include(o => o.Items).AsQueryable();

        if (userId.HasValue)
        {
            query = query.Where(o => o.UserId == userId.Value);
        }

        return query.FirstOrDefaultAsync(o => o.Id == orderId, ct);
    }

    public async Task<PagedList<Order>> GetPagedAsync(int page, int pageSize, OrderStatus? status = null, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = db.Orders.AsNoTracking().Include(o => o.Items).AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(o => o.Status == status.Value);
        }

        query = query.OrderByDescending(o => o.CreatedAt);

        return new PagedList<Order>
        {
            Items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct),
            Page = page,
            PageSize = pageSize,
            TotalCount = await query.CountAsync(ct),
        };
    }

    public async Task<Order?> UpdateStatusAsync(Guid orderId, OrderStatus status, CancellationToken ct = default)
    {
        var order = await db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId, ct);
        if (order is null)
        {
            return null;
        }

        order.Status = status;
        await db.SaveChangesAsync(ct);
        return order;
    }

    /// <summary>e.g. NEM-20260706-4F2A9C — date for humans, random suffix for uniqueness.</summary>
    private static string GenerateOrderNumber() =>
        $"NEM-{DateTime.UtcNow:yyyyMMdd}-{Convert.ToHexString(RandomNumberGenerator.GetBytes(3))}";
}
