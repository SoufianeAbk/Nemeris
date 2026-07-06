using Nemeris.Core.Common;
using Nemeris.Core.Dtos;
using Nemeris.Core.Entities;

namespace Nemeris.Core.Interfaces;

public interface IReviewService
{
    /// <summary>Approved reviews only — the storefront listing.</summary>
    Task<PagedList<Review>> GetForProductAsync(Guid productId, int page, int pageSize, CancellationToken ct = default);

    /// <summary>Returns null when the product is unknown or the user already reviewed it.</summary>
    Task<Review?> AddAsync(Guid userId, ReviewCreateDto dto, CancellationToken ct = default);

    Task<bool> ApproveAsync(Guid reviewId, CancellationToken ct = default);

    Task<bool> DeleteAsync(Guid reviewId, CancellationToken ct = default);
}
