using System.ComponentModel;
using System.Globalization;
using System.Resources;

namespace Nemeris.App.Localization;

/// <summary>
/// In-app localization singleton. XAML binds through the string indexer
/// (Text="{Binding Loc[SomeKey]}"), and SetCulture raises a blanket PropertyChanged
/// so every bound label re-reads its resource instantly — live language switching
/// without restarting the app.
///
/// The culture comes from Preferences (default "en"), NOT from the device locale:
/// the requirement is an explicit in-app language choice that survives restarts
/// and ignores whatever the OS is set to.
/// </summary>
public sealed class LocalizationResourceManager : INotifyPropertyChanged
{
    private const string CulturePreferenceKey = "app_culture";

    public static LocalizationResourceManager Instance { get; } = new();

    private readonly ResourceManager _resources = new(
        "Nemeris.App.Localization.AppResources",
        typeof(LocalizationResourceManager).Assembly);

    private CultureInfo _culture;

    public event PropertyChangedEventHandler? PropertyChanged;

    private LocalizationResourceManager()
    {
        _culture = new CultureInfo(Preferences.Default.Get(CulturePreferenceKey, "en"));
        ApplyThreadCulture(_culture);
    }

    /// <summary>Unknown keys render as the key itself — visible in the UI instead of crashing it.</summary>
    public string this[string key] => _resources.GetString(key, _culture) ?? key;

    public CultureInfo Culture => _culture;

    public void SetCulture(string cultureCode)
    {
        if (_culture.Name == cultureCode)
        {
            return;
        }

        _culture = new CultureInfo(cultureCode);
        Preferences.Default.Set(CulturePreferenceKey, cultureCode);
        ApplyThreadCulture(_culture);

        // null property name = "everything changed": all indexer bindings refresh.
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
    }

    /// <summary>Keeps number/date formatting (e.g. price StringFormat) in step with the chosen language.</summary>
    private static void ApplyThreadCulture(CultureInfo culture)
    {
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
    }
}
