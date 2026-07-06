using System.Security.Cryptography;
using Nemeris.App.Models;
using Nemeris.Shared.Catalog;
using SQLite;

namespace Nemeris.App.Services;

/// <summary>
/// Encrypted local store (SQLCipher). Holds the product cache and the offline
/// action queue. The encryption key is generated once and kept in the platform
/// keystore via SecureStorage — it never ships with the app or leaves the device.
/// </summary>
public class LocalDatabaseService
{
    private const string DbKeyStorageKey = "local_db_key";
    private const string DbFileName = "nemeris.db3";

    private readonly SemaphoreSlim _initLock = new(1, 1);
    private SQLiteAsyncConnection? _connection;

    private async Task<SQLiteAsyncConnection> GetConnectionAsync()
    {
        if (_connection is not null)
        {
            return _connection;
        }

        await _initLock.WaitAsync();
        try
        {
            if (_connection is not null)
            {
                return _connection;
            }

            var key = await SecureStorage.Default.GetAsync(DbKeyStorageKey);
            if (key is null)
            {
                key = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
                await SecureStorage.Default.SetAsync(DbKeyStorageKey, key);
            }

            var options = new SQLiteConnectionString(
                Path.Combine(FileSystem.AppDataDirectory, DbFileName),
                storeDateTimeAsTicks: true,
                key: key);

            var connection = new SQLiteAsyncConnection(options);
            await connection.CreateTableAsync<LocalProduct>();
            await connection.CreateTableAsync<PendingAction>();

            _connection = connection;
            return connection;
        }
        finally
        {
            _initLock.Release();
        }
    }

    // ---- Product cache ----

    /// <summary>Replaces the cache with the latest server page — the cache mirrors the server, it doesn't accumulate.</summary>
    public async Task CacheProductsAsync(IEnumerable<ProductDto> products)
    {
        var db = await GetConnectionAsync();
        var rows = products.Select(LocalProduct.FromDto).ToList();

        await db.RunInTransactionAsync(sync =>
        {
            sync.DeleteAll<LocalProduct>();
            sync.InsertAll(rows);
        });
    }

    public async Task<List<ProductDto>> GetCachedProductsAsync(string? search = null)
    {
        var db = await GetConnectionAsync();

        var query = db.Table<LocalProduct>();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(p => p.Name.Contains(term));
        }

        var rows = await query.OrderBy(p => p.Name).ToListAsync();
        return rows.Select(r => r.ToDto()).ToList();
    }

    // ---- Offline action queue ----

    public async Task EnqueueActionAsync(PendingAction action)
    {
        var db = await GetConnectionAsync();
        await db.InsertAsync(action);
    }

    /// <summary>Oldest first — actions must replay in the order the user performed them.</summary>
    public async Task<List<PendingAction>> GetPendingActionsAsync()
    {
        var db = await GetConnectionAsync();
        return await db.Table<PendingAction>().OrderBy(a => a.Id).ToListAsync();
    }

    public async Task<int> GetPendingActionCountAsync()
    {
        var db = await GetConnectionAsync();
        return await db.Table<PendingAction>().CountAsync();
    }

    public async Task UpdateActionAsync(PendingAction action)
    {
        var db = await GetConnectionAsync();
        await db.UpdateAsync(action);
    }

    public async Task DeleteActionAsync(int id)
    {
        var db = await GetConnectionAsync();
        await db.DeleteAsync<PendingAction>(id);
    }
}
