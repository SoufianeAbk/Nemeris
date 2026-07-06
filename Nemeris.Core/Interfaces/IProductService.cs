using Nemeris.Core.Common;
using Nemeris.Core.Dtos;
using Nemeris.Core.Entities;

namespace Nemeris.Core.Interfaces;

public interface IProductService
{
    Task<PagedList<Product>> GetPagedAsync(ProductFilterDto filter, CancellationToken ct = default);

    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<Product?> GetBySlugAsync(string slug, CancellationToken ct = default);

    Task<Product> CreateAsync(ProductCreateDto dto, CancellationToken ct = default);

    /// <summary>Returns null when the product does not exist.</summary>
    Task<Product?> UpdateAsync(Guid id, ProductUpdateDto dto, CancellationToken ct = default);

    /// <summary>Soft delete; returns false when the product does not exist.</summary>
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
