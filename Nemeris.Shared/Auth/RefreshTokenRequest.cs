namespace Nemeris.Shared.Auth;

public sealed class RefreshTokenRequest
{
    /// <summary>The opaque refresh token; the server looks it up, rotates it and returns a fresh pair.</summary>
    public string RefreshToken { get; set; } = string.Empty;
}
