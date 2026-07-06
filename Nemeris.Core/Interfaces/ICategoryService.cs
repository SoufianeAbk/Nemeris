using Nemeris.Core.Dtos;
using Nemeris.Core.Entities;

namespace Nemeris.Core.Interfaces;

public interface ICategoryService
{
    Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken ct = default);

    Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<Category> CreateAsync(CategoryUpsertDto dto, CancellationToken ct = default);

    Task<Category?> UpdateAsync(Guid id, CategoryUpsertDto dto, CancellationToken ct = default);

    /// <summary>Fails (returns false) when the category still has products or children.</summary>
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
