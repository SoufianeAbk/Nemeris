using System.ComponentModel.DataAnnotations;
using Nemeris.Core.Entities;

namespace Nemeris.Web.Models;

public class CheckoutViewModel
{
    public IReadOnlyList<Address> Addresses { get; set; } = [];

    [Required(ErrorMessage = "Validation.Required")]
    public Guid SelectedAddressId { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }
}
