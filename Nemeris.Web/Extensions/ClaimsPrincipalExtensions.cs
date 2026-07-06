using System.Security.Claims;

namespace Nemeris.Web.Extensions;

public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// The signed-in user's Guid id from the Identity cookie principal.
    /// Only call under [Authorize].
    /// </summary>
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("Missing user id claim.");
        return Guid.Parse(value);
    }
}
