using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Nemeris.Infrastructure;
using Nemeris.Infrastructure.Data;
using Nemeris.Web;

var builder = WebApplication.CreateBuilder(args);

// Same composition root as the Api: DbContext, Identity core, services, validators.
builder.Services.AddInfrastructure(builder.Configuration);

// Cookie-based Identity auth — the Web counterpart of the Api's JWT bearer.
// AddIdentityCore (in Infrastructure) registers no scheme, so this is where Web picks cookies.
builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddIdentityCookies();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
    options.SlidingExpiration = true;
});

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    // DataAnnotations ErrorMessage values are resource keys resolved against SharedResources.
    .AddDataAnnotationsLocalization(options =>
        options.DataAnnotationLocalizerProvider = (_, factory) => factory.Create(typeof(SharedResources)));

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    string[] supportedCultures = ["en", "nl", "fr"];
    options.SetDefaultCulture("en");
    options.AddSupportedCultures(supportedCultures);
    options.AddSupportedUICultures(supportedCultures);

    // The storefront remembers the visitor's language in a cookie
    // (set by Home/SetLanguage), not in headers or query strings.
    options.RequestCultureProviders = [new CookieRequestCultureProvider()];
});

var app = builder.Build();

await DbSeeder.SeedAsync(app.Services);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRequestLocalization();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
