using System.Security.Claims;

namespace Nemeris.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// The authenticated user's Guid id. Only call under [Authorize] — a missing or
    /// malformed claim is a programming error, not a user error, hence the throw.
    /// </summary>
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("Missing user id claim.");
        return Guid.Parse(value);
    }
}
