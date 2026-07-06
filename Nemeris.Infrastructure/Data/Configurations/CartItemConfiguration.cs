using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nemeris.Core.Entities;
using Nemeris.Infrastructure.Identity;

namespace Nemeris.Infrastructure.Data.Configurations;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        // One live cart line per (user, product); UpsertItemAsync relies on this.
        builder.HasIndex(c => new { c.UserId, c.ProductId })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Product)
            .WithMany()
            .HasForeignKey(c => c.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
