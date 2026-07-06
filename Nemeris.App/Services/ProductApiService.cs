using System.Net.Http.Json;
using Nemeris.Shared.Catalog;
using Nemeris.Shared.Common;

namespace Nemeris.App.Services;

/// <summary>What the caller gets either way; FromCache drives the offline banner in the UI.</summary>
public record ProductListResult(List<ProductDto> Items, bool HasNextPage, bool FromCache);

/// <summary>
/// Read side of offline-first: network first, encrypted cache as fallback.
/// A successful unfiltered first page refreshes the cache; any failure
/// (no connectivity, timeout, server down) degrades to cached data.
/// </summary>
public class ProductApiService(HttpClient http, LocalDatabaseService localDb)
{
    public async Task<ProductListResult> GetProductsAsync(int page = 1, string? search = null, CancellationToken ct = default)
    {
        if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
        {
            try
            {
                var url = $"api/products?Page={page}&PageSize=20";
                if (!string.IsNullOrWhiteSpace(search))
                {
                    url += $"&Search={Uri.EscapeDataString(search.Trim())}";
                }

                var result = await http.GetFromJsonAsync<PagedResult<ProductDto>>(url, ApiConfig.JsonOptions, ct);
                if (result is not null)
                {
                    // Only the canonical view (page 1, no filter) refreshes the cache;
                    // caching filtered results would leave a misleading partial mirror.
                    if (page == 1 && string.IsNullOrWhiteSpace(search))
                    {
                        await localDb.CacheProductsAsync(result.Items);
                    }

                    return new ProductListResult(result.Items, result.HasNextPage, FromCache: false);
                }
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                // Fall through to the cache.
            }
        }

        var cached = await localDb.GetCachedProductsAsync(search);
        return new ProductListResult(cached, HasNextPage: false, FromCache: true);
    }
}
