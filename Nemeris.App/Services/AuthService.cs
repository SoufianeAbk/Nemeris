using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using Nemeris.Shared.Auth;

namespace Nemeris.App.Services;

/// <summary>
/// Owns the JWT lifecycle on the device: tokens live in SecureStorage (platform
/// keystore), and GetValidAccessTokenAsync transparently refreshes near expiry so
/// callers never touch refresh logic.
/// </summary>
public class AuthService(HttpClient http)
{
    private const string AccessTokenKey = "auth_access_token";
    private const string RefreshTokenKey = "auth_refresh_token";
    private const string ExpiryKey = "auth_access_expiry";

    public async Task<bool> LoginAsync(string email, string password, CancellationToken ct = default)
    {
        try
        {
            var response = await http.PostAsJsonAsync(
                "api/auth/login",
                new LoginRequest { Email = email, Password = password },
                ApiConfig.JsonOptions, ct);

            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var auth = await response.Content.ReadFromJsonAsync<AuthResponse>(ApiConfig.JsonOptions, ct);
            if (auth is null)
            {
                return false;
            }

            await StoreTokensAsync(auth);
            return true;
        }
        catch (HttpRequestException)
        {
            return false; // offline — logging in requires the server
        }
    }

    public async Task<bool> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        try
        {
            var response = await http.PostAsJsonAsync("api/auth/register", request, ApiConfig.JsonOptions, ct);
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var auth = await response.Content.ReadFromJsonAsync<AuthResponse>(ApiConfig.JsonOptions, ct);
            if (auth is null)
            {
                return false;
            }

            await StoreTokensAsync(auth);
            return true;
        }
        catch (HttpRequestException)
        {
            return false;
        }
    }

    /// <summary>
    /// The access token if still valid, otherwise the result of a refresh.
    /// Null means "not authenticated right now" (never logged in, session revoked,
    /// or offline with an expired access token).
    /// </summary>
    public async Task<string?> GetValidAccessTokenAsync(CancellationToken ct = default)
    {
        var accessToken = await SecureStorage.Default.GetAsync(AccessTokenKey);
        var expiryText = await SecureStorage.Default.GetAsync(ExpiryKey);

        if (accessToken is not null
            && DateTime.TryParse(expiryText, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var expiry)
            && expiry > DateTime.UtcNow.AddMinutes(1)) // refresh proactively, before it actually lapses
        {
            return accessToken;
        }

        return await RefreshAsync(ct);
    }

    private async Task<string?> RefreshAsync(CancellationToken ct)
    {
        var refreshToken = await SecureStorage.Default.GetAsync(RefreshTokenKey);
        if (refreshToken is null)
        {
            return null;
        }

        try
        {
            var response = await http.PostAsJsonAsync(
                "api/auth/refresh",
                new RefreshTokenRequest { RefreshToken = refreshToken },
                ApiConfig.JsonOptions, ct);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                // Server rejected the session (revoked/expired/reuse-detected): local
                // tokens are dead weight, clear them so the app shows "signed out".
                ClearTokens();
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var auth = await response.Content.ReadFromJsonAsync<AuthResponse>(ApiConfig.JsonOptions, ct);
            if (auth is null)
            {
                return null;
            }

            await StoreTokensAsync(auth);
            return auth.AccessToken;
        }
        catch (HttpRequestException)
        {
            return null; // offline — keep the refresh token for when we're back
        }
    }

    public async Task LogoutAsync(CancellationToken ct = default)
    {
        var refreshToken = await SecureStorage.Default.GetAsync(RefreshTokenKey);
        var accessToken = await SecureStorage.Default.GetAsync(AccessTokenKey);

        // Best-effort server-side revocation; local sign-out succeeds regardless.
        if (refreshToken is not null && accessToken is not null)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/revoke")
                {
                    Content = JsonContent.Create(
                        new RefreshTokenRequest { RefreshToken = refreshToken }, options: ApiConfig.JsonOptions),
                };
                request.Headers.Authorization = new("Bearer", accessToken);
                await http.SendAsync(request, ct);
            }
            catch (HttpRequestException)
            {
                // Offline logout: the refresh token still dies server-side at its expiry.
            }
        }

        ClearTokens();
    }

    public async Task<bool> IsLoggedInAsync() =>
        await SecureStorage.Default.GetAsync(RefreshTokenKey) is not null;

    private static async Task StoreTokensAsync(AuthResponse auth)
    {
        await SecureStorage.Default.SetAsync(AccessTokenKey, auth.AccessToken);
        await SecureStorage.Default.SetAsync(RefreshTokenKey, auth.RefreshToken);
        await SecureStorage.Default.SetAsync(ExpiryKey, auth.AccessTokenExpiresAtUtc.ToString("O"));
    }

    private static void ClearTokens()
    {
        SecureStorage.Default.Remove(AccessTokenKey);
        SecureStorage.Default.Remove(RefreshTokenKey);
        SecureStorage.Default.Remove(ExpiryKey);
    }
}
