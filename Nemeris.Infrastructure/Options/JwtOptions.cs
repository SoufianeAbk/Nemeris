namespace Nemeris.Infrastructure.Options;

/// <summary>Bound from the "Jwt" section of configuration by DependencyInjection.</summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    /// <summary>Symmetric signing key; at least 32 bytes. Keep it in user-secrets / environment, never in appsettings.json.</summary>
    public string Key { get; set; } = string.Empty;

    public int AccessTokenMinutes { get; set; } = 15;

    public int RefreshTokenDays { get; set; } = 30;
}
