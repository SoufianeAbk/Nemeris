namespace Nemeris.Core.Interfaces;

/// <summary>An issued access token together with its expiry, so callers never re-parse the JWT.</summary>
public record AccessTokenResult(string Token, DateTime ExpiresAtUtc);

/// <summary>
/// JWT issuing abstraction. Takes primitives (not ApplicationUser) on purpose:
/// Core must not depend on Identity types, which live in Infrastructure.
/// </summary>
public interface ITokenService
{
    AccessTokenResult CreateAccessToken(Guid userId, string email, IEnumerable<string> roles);

    /// <summary>Cryptographically random opaque string; persisted via the RefreshToken entity.</summary>
    string GenerateRefreshToken();
}
