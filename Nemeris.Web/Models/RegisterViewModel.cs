using System.ComponentModel.DataAnnotations;

namespace Nemeris.Web.Models;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Validation.Required")]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Validation.Required")]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Validation.Required")]
    [EmailAddress(ErrorMessage = "Validation.Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Validation.Required")]
    [MinLength(8, ErrorMessage = "Validation.PasswordLength")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Validation.Required")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Validation.PasswordMismatch")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
