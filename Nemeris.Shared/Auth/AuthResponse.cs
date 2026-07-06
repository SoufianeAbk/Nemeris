namespace Nemeris.Shared.Auth;

/// <summary>Returned by login, register and refresh — always the full pair plus enough profile to render a header.</summary>
public sealed class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>UTC expiry of the access token, so clients can refresh proactively instead of parsing the JWT.</summary>
    public DateTime AccessTokenExpiresAtUtc { get; set; }

    public string RefreshToken { get; set; } = string.Empty;

    public Guid UserId { get; set; }

    public string Email { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public List<string> Roles { get; set; } = [];
}
