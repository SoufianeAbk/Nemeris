using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Nemeris.Core.Common;
using Nemeris.Core.Dtos;
using Nemeris.Core.Entities;
using Nemeris.Core.Interfaces;
using Nemeris.Infrastructure.Data;

namespace Nemeris.Infrastructure.Services;

public class ReviewService(NemerisDbContext db, IMapper mapper) : IReviewService
{
    public async Task<PagedList<Review>> GetForProductAsync(Guid productId, int page, int pageSize, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = db.Reviews.AsNoTracking()
            .Where(r => r.ProductId == productId && r.IsApproved)
            .OrderByDescending(r => r.CreatedAt);

        return new PagedList<Review>
        {
            Items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct),
            Page = page,
            PageSize = pageSize,
            TotalCount = await query.CountAsync(ct),
        };
    }

    public async Task<Review?> AddAsync(Guid userId, ReviewCreateDto dto, CancellationToken ct = default)
    {
        var productExists = await db.Products.AnyAsync(p => p.Id == dto.ProductId && p.IsActive, ct);
        if (!productExists)
        {
            return null;
        }

        var alreadyReviewed = await db.Reviews
            .AnyAsync(r => r.ProductId == dto.ProductId && r.UserId == userId, ct);
        if (alreadyReviewed)
        {
            return null;
        }

        var review = mapper.Map<Review>(dto);
        review.UserId = userId;
        review.IsApproved = false; // moderation queue

        db.Reviews.Add(review);
        await db.SaveChangesAsync(ct);
        return review;
    }

    public async Task<bool> ApproveAsync(Guid reviewId, CancellationToken ct = default)
    {
        var review = await db.Reviews.FirstOrDefaultAsync(r => r.Id == reviewId, ct);
        if (review is null)
        {
            return false;
        }

        review.IsApproved = true;
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid reviewId, CancellationToken ct = default)
    {
        var review = await db.Reviews.FirstOrDefaultAsync(r => r.Id == reviewId, ct);
        if (review is null)
        {
            return false;
        }

        db.Reviews.Remove(review);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
