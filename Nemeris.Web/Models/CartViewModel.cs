using Nemeris.Core.Entities;

namespace Nemeris.Web.Models;

public class CartViewModel
{
    public IReadOnlyList<CartItem> Items { get; set; } = [];

    /// <summary>Live prices — carts always show the current product price.</summary>
    public decimal SubTotal => Items.Sum(i => i.Product.Price * i.Quantity);
}
