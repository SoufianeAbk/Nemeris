using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Nemeris.Core.Common;
using Nemeris.Core.Dtos;
using Nemeris.Core.Entities;
using Nemeris.Core.Interfaces;
using Nemeris.Infrastructure.Data;

namespace Nemeris.Infrastructure.Services;

public class ProductService(NemerisDbContext db, IMapper mapper) : IProductService
{
    public async Task<PagedList<Product>> GetPagedAsync(ProductFilterDto filter, CancellationToken ct = default)
    {
        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);

        var query = db.Products.AsNoTracking().Include(p => p.Category).AsQueryable();

        if (!filter.IncludeInactive)
        {
            query = query.Where(p => p.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim();
            query = query.Where(p => p.Name.Contains(term) || p.Sku.Contains(term));
        }

        if (filter.CategoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == filter.CategoryId.Value);
        }

        if (filter.MinPrice.HasValue)
        {
            query = query.Where(p => p.Price >= filter.MinPrice.Value);
        }

        if (filter.MaxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= filter.MaxPrice.Value);
        }

        query = filter.SortBy switch
        {
            "price" => query.OrderBy(p => p.Price).ThenBy(p => p.Name),
            "price_desc" => query.OrderByDescending(p => p.Price).ThenBy(p => p.Name),
            "newest" => query.OrderByDescending(p => p.CreatedAt),
            _ => query.OrderBy(p => p.Name),
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        return new PagedList<Product>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
        };
    }

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Products.AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<Product?> GetBySlugAsync(string slug, CancellationToken ct = default) =>
        db.Products.AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Slug == slug && p.IsActive, ct);

    public async Task<Product> CreateAsync(ProductCreateDto dto, CancellationToken ct = default)
    {
        var categoryExists = await db.Categories.AnyAsync(c => c.Id == dto.CategoryId, ct);
        if (!categoryExists)
        {
            throw new InvalidOperationException("Error.CategoryNotFound");
        }

        var product = mapper.Map<Product>(dto);
        db.Products.Add(product);
        await db.SaveChangesAsync(ct);
        return product;
    }

    public async Task<Product?> UpdateAsync(Guid id, ProductUpdateDto dto, CancellationToken ct = default)
    {
        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (product is null)
        {
            return null;
        }

        if (product.CategoryId != dto.CategoryId
            && !await db.Categories.AnyAsync(c => c.Id == dto.CategoryId, ct))
        {
            throw new InvalidOperationException("Error.CategoryNotFound");
        }

        mapper.Map(dto, product);
        await db.SaveChangesAsync(ct);
        return product;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (product is null)
        {
            return false;
        }

        // Remove() is converted to a soft delete by the DbContext.
        db.Products.Remove(product);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
