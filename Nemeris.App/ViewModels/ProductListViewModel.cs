using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nemeris.App.Localization;
using Nemeris.App.Services;
using Nemeris.Shared.Cart;
using Nemeris.Shared.Catalog;

namespace Nemeris.App.ViewModels;

/// <summary>
/// Demonstrates the offline-first pattern end to end: reads fall back to the
/// encrypted cache (IsFromCache → banner), writes fall into the sync queue
/// (PendingSyncCount → badge), and the queue drains when connectivity returns.
/// </summary>
public partial class ProductListViewModel : ObservableObject
{
    private readonly ProductApiService _products;
    private readonly SyncService _sync;

    /// <summary>Bound in XAML as {Binding Loc[Key]}; SetCulture refreshes every label live.</summary>
    public LocalizationResourceManager Loc => LocalizationResourceManager.Instance;

    public ObservableCollection<ProductDto> Products { get; } = [];

    private bool _isRefreshing;
    public bool IsRefreshing
    {
        get => _isRefreshing;
        set => SetProperty(ref _isRefreshing, value);
    }

    private bool _isFromCache;
    public bool IsFromCache
    {
        get => _isFromCache;
        set => SetProperty(ref _isFromCache, value);
    }

    private string? _searchText;
    public string? SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    private int _pendingSyncCount;
    public int PendingSyncCount
    {
        get => _pendingSyncCount;
        set
        {
            if (SetProperty(ref _pendingSyncCount, value))
            {
                OnPropertyChanged(nameof(HasPendingSync));
            }
        }
    }

    public bool HasPendingSync => PendingSyncCount > 0;

    public ProductListViewModel(ProductApiService products, SyncService sync)
    {
        _products = products;
        _sync = sync;

        _sync.PendingCountChanged += (_, count) =>
            MainThread.BeginInvokeOnMainThread(() => PendingSyncCount = count);
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        try
        {
            var result = await _products.GetProductsAsync(page: 1, SearchText);

            IsFromCache = result.FromCache;

            Products.Clear();
            foreach (var product in result.Items)
            {
                Products.Add(product);
            }

            PendingSyncCount = await _sync.GetPendingCountAsync();
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private Task SearchAsync() => LoadAsync();

    [RelayCommand]
    private async Task AddToCartAsync(ProductDto product)
    {
        // Absolute quantity 1 keeps the action idempotent for queue replays.
        var sentNow = await _sync.UpsertCartItemAsync(
            new UpsertCartItemRequest { ProductId = product.Id, Quantity = 1 });

        await Toast.Make(Loc[sentNow ? "CartAdded" : "CartQueued"]).Show();
    }

    [RelayCommand]
    private async Task SyncNowAsync()
    {
        var synced = await _sync.SyncPendingAsync();
        if (synced > 0)
        {
            await LoadAsync();
        }
    }

    [RelayCommand]
    private void SetLanguage(string cultureCode) => Loc.SetCulture(cultureCode);
}
