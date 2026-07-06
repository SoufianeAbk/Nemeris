using System.Text.Json;

namespace Nemeris.App;

/// <summary>
/// Central API endpoint + serializer settings. Ports come from
/// Nemeris.Api/Properties/launchSettings.json — adjust when they change.
/// </summary>
public static class ApiConfig
{
    /// <summary>Android emulators reach the host machine via 10.0.2.2, everything else via localhost.</summary>
    public static string BaseUrl { get; } =
        DeviceInfo.Platform == DevicePlatform.Android
            ? "https://10.0.2.2:7052"
            : "https://localhost:7052";

    /// <summary>Matches ASP.NET Core's wire format (camelCase, case-insensitive read).</summary>
    public static JsonSerializerOptions JsonOptions { get; } = new(JsonSerializerDefaults.Web);
}
