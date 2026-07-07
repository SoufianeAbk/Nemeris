using System.ComponentModel.DataAnnotations;

namespace Nemeris.Web.Models;

public class ContactViewModel
{
    [Required(ErrorMessage = "Validation.Required")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Validation.Required")]
    [EmailAddress(ErrorMessage = "Validation.Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Validation.Required")]
    [StringLength(150)]
    public string Subject { get; set; } = string.Empty;

    [Required(ErrorMessage = "Validation.Required")]
    [StringLength(4000)]
    public string Message { get; set; } = string.Empty;
}
