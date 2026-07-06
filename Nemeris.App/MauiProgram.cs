using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Nemeris.App.Services;
using Nemeris.App.ViewModels;
using Nemeris.App.Views;

namespace Nemeris.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // One shared HttpClient for all Api traffic. Auth headers are attached
        // per request (SyncService/AuthService), never as defaults.
        builder.Services.AddSingleton(_ =>
        {
#if DEBUG
            // Local dev only: accept the ASP.NET Core dev certificate, which
            // emulators/simulators don't trust. Compiled out of release builds.
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true,
            };
            return new HttpClient(handler) { BaseAddress = new Uri(ApiConfig.BaseUrl) };
#else
            return new HttpClient { BaseAddress = new Uri(ApiConfig.BaseUrl) };
#endif
        });

        // Singletons: one encrypted DB connection, one token holder, one sync
        // queue (it owns the ConnectivityChanged subscription) per app lifetime.
        builder.Services.AddSingleton<LocalDatabaseService>();
        builder.Services.AddSingleton<AuthService>();
        builder.Services.AddSingleton<ProductApiService>();
        builder.Services.AddSingleton<SyncService>();

        // Singleton VM keeps list state alive across navigation.
        builder.Services.AddSingleton<ProductListViewModel>();
        builder.Services.AddSingleton<ProductListPage>();

        return builder.Build();
    }
}
