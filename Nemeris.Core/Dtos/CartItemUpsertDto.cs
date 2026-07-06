namespace Nemeris.Core.Dtos;

/// <summary>Add-or-update a cart line; a quantity of 0 removes the line.</summary>
public class CartItemUpsertDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}
