using Microsoft.AspNetCore.Identity;

namespace Nemeris.Infrastructure.Identity;

/// <summary>
/// Identity user with a Guid primary key so it lines up with the Guid UserId
/// columns on Order, CartItem, Address, Review and RefreshToken.
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public string FullName => $"{FirstName} {LastName}".Trim();
}
