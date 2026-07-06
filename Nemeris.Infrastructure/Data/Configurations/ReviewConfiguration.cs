using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nemeris.Core.Entities;
using Nemeris.Infrastructure.Identity;

namespace Nemeris.Infrastructure.Data.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.Property(r => r.Title).HasMaxLength(150);
        builder.Property(r => r.Body).HasMaxLength(4000);

        // The validator enforces 1–5 at the boundary; this enforces it in the database.
        builder.ToTable(t => t.HasCheckConstraint("CK_Reviews_Rating", "[Rating] BETWEEN 1 AND 5"));

        // One live review per user per product.
        builder.HasIndex(r => new { r.ProductId, r.UserId })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasOne(r => r.Product)
            .WithMany(p => p.Reviews)
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
