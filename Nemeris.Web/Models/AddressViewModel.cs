using System.ComponentModel.DataAnnotations;

namespace Nemeris.Web.Models;

/// <summary>
/// MVC form model for the address book; mirrors Core's AddressUpsertDto but with
/// DataAnnotations so client-side validation and localization work out of the box.
/// </summary>
public class AddressViewModel
{
    [Required(ErrorMessage = "Validation.Required")]
    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Validation.Required")]
    [MaxLength(200)]
    public string Street { get; set; } = string.Empty;

    [Required(ErrorMessage = "Validation.Required")]
    [MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "Validation.Required")]
    [MaxLength(20)]
    public string PostalCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Validation.Required")]
    [RegularExpression("^[A-Z]{2}$", ErrorMessage = "Validation.CountryCode")]
    public string Country { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? PhoneNumber { get; set; }

    public bool IsDefault { get; set; }

    public string? ReturnUrl { get; set; }
}
