using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Nemeris.Core.Dtos;
using Nemeris.Core.Entities;
using Nemeris.Core.Interfaces;
using Nemeris.Infrastructure.Data;

namespace Nemeris.Infrastructure.Services;

public class CategoryService(NemerisDbContext db, IMapper mapper) : ICategoryService
{
    public async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken ct = default) =>
        await db.Categories.AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync(ct);

    public Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Categories.AsNoTracking()
            .Include(c => c.Children)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<Category> CreateAsync(CategoryUpsertDto dto, CancellationToken ct = default)
    {
        var category = mapper.Map<Category>(dto);
        db.Categories.Add(category);
        await db.SaveChangesAsync(ct);
        return category;
    }

    public async Task<Category?> UpdateAsync(Guid id, CategoryUpsertDto dto, CancellationToken ct = default)
    {
        var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (category is null)
        {
            return null;
        }

        if (dto.ParentCategoryId == id)
        {
            throw new InvalidOperationException("Error.CategoryOwnParent");
        }

        mapper.Map(dto, category);
        await db.SaveChangesAsync(ct);
        return category;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (category is null)
        {
            return false;
        }

        var inUse = await db.Products.AnyAsync(p => p.CategoryId == id, ct)
            || await db.Categories.AnyAsync(c => c.ParentCategoryId == id, ct);
        if (inUse)
        {
            throw new InvalidOperationException("Error.CategoryInUse");
        }

        db.Categories.Remove(category);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
