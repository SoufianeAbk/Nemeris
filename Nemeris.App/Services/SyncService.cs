using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Nemeris.App.Models;
using Nemeris.Shared.Cart;
using Nemeris.Shared.Orders;

namespace Nemeris.App.Services;

/// <summary>
/// Write side of offline-first. Two jobs:
/// 1. Try-now-or-enqueue: actions attempt the Api immediately and fall into the
///    encrypted queue when that fails.
/// 2. Replay: when connectivity returns (ConnectivityChanged) or on demand, the
///    queue drains strictly FIFO. Replays are safe because the wire contract is
///    idempotent (UpsertCartItemRequest carries absolute quantities).
/// </summary>
public class SyncService : IDisposable
{
    public const string ActionUpsertCartItem = "UpsertCartItem";
    public const string ActionCheckout = "Checkout";

    private const int MaxAttempts = 5;

    private readonly HttpClient _http;
    private readonly AuthService _auth;
    private readonly LocalDatabaseService _localDb;
    private readonly SemaphoreSlim _syncLock = new(1, 1);

    /// <summary>Raised whenever the queue length changes; the UI shows a "waiting to sync" badge.</summary>
    public event EventHandler<int>? PendingCountChanged;

    public SyncService(HttpClient http, AuthService auth, LocalDatabaseService localDb)
    {
        _http = http;
        _auth = auth;
        _localDb = localDb;
        Connectivity.Current.ConnectivityChanged += OnConnectivityChanged;
    }

    public Task<int> GetPendingCountAsync() => _localDb.GetPendingActionCountAsync();

    /// <summary>Returns true when sent to the server now, false when queued for later.</summary>
    public async Task<bool> UpsertCartItemAsync(UpsertCartItemRequest request, CancellationToken ct = default)
    {
        if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
        {
            try
            {
                if (await SendUpsertCartItemAsync(request, ct))
                {
                    return true;
                }
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                // Connectivity said "internet" but the call still failed — queue it.
            }
        }

        await EnqueueAsync(ActionUpsertCartItem, request);
        return false;
    }

    /// <summary>Orders queue offline too; the server re-validates stock and prices at replay time.</summary>
    public async Task<bool> CheckoutAsync(CheckoutRequest request, CancellationToken ct = default)
    {
        if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
        {
            try
            {
                if (await SendCheckoutAsync(request, ct))
                {
                    return true;
                }
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
            }
        }

        await EnqueueAsync(ActionCheckout, request);
        return false;
    }

    /// <summary>
    /// Drains the queue FIFO. Stops (leaving the rest queued) when the connection
    /// drops again or auth is unavailable; drops actions the server permanently
    /// rejects (after MaxAttempts) so one poison action can't wedge the queue forever.
    /// </summary>
    public async Task<int> SyncPendingAsync(CancellationToken ct = default)
    {
        // A sync is already running — don't double-replay.
        if (!await _syncLock.WaitAsync(0, ct))
        {
            return 0;
        }

        try
        {
            var synced = 0;

            foreach (var action in await _localDb.GetPendingActionsAsync())
            {
                bool handled;
                try
                {
                    handled = await ReplayAsync(action, ct);
                }
                catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
                {
                    break; // offline again; try later, queue stays intact
                }
                catch (NotAuthenticatedException)
                {
                    break; // user must log in first; keep everything queued
                }

                if (handled)
                {
                    await _localDb.DeleteActionAsync(action.Id);
                    synced++;
                }
                else
                {
                    action.Attempts++;
                    if (action.Attempts >= MaxAttempts)
                    {
                        await _localDb.DeleteActionAsync(action.Id); // poison — give up
                    }
                    else
                    {
                        await _localDb.UpdateActionAsync(action);
                    }

                    break; // preserve ordering: don't run later actions past a failed one
                }
            }

            await RaisePendingCountAsync();
            return synced;
        }
        finally
        {
            _syncLock.Release();
        }
    }

    private async Task<bool> ReplayAsync(PendingAction action, CancellationToken ct)
    {
        switch (action.Type)
        {
            case ActionUpsertCartItem:
                var cartRequest = JsonSerializer.Deserialize<UpsertCartItemRequest>(action.PayloadJson, ApiConfig.JsonOptions);
                return cartRequest is null || await SendUpsertCartItemAsync(cartRequest, ct);

            case ActionCheckout:
                var checkoutRequest = JsonSerializer.Deserialize<CheckoutRequest>(action.PayloadJson, ApiConfig.JsonOptions);
                return checkoutRequest is null || await SendCheckoutAsync(checkoutRequest, ct);

            default:
                return true; // unknown legacy action — drop it
        }
    }

    private async Task<bool> SendUpsertCartItemAsync(UpsertCartItemRequest request, CancellationToken ct)
    {
        var response = await SendAuthorizedAsync(HttpMethod.Put, "api/cart/items", request, ct);

        // 404 = product no longer exists; retrying can never succeed, count it handled.
        return response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound;
    }

    private async Task<bool> SendCheckoutAsync(CheckoutRequest request, CancellationToken ct)
    {
        var response = await SendAuthorizedAsync(HttpMethod.Post, "api/orders", request, ct);

        // 400 = business rejection (empty cart, no stock) — a retry won't change it.
        return response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.BadRequest;
    }

    private async Task<HttpResponseMessage> SendAuthorizedAsync(HttpMethod method, string url, object payload, CancellationToken ct)
    {
        var token = await _auth.GetValidAccessTokenAsync(ct)
            ?? throw new NotAuthenticatedException();

        using var request = new HttpRequestMessage(method, url)
        {
            Content = JsonContent.Create(payload, payload.GetType(), options: ApiConfig.JsonOptions),
        };
        request.Headers.Authorization = new("Bearer", token);

        return await _http.SendAsync(request, ct);
    }

    private async Task EnqueueAsync(string type, object payload)
    {
        await _localDb.EnqueueActionAsync(new PendingAction
        {
            Type = type,
            PayloadJson = JsonSerializer.Serialize(payload, payload.GetType(), ApiConfig.JsonOptions),
            CreatedAtUtc = DateTime.UtcNow,
        });
        await RaisePendingCountAsync();
    }

    private async void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
    {
        if (e.NetworkAccess != NetworkAccess.Internet)
        {
            return;
        }

        try
        {
            await SyncPendingAsync();
        }
        catch
        {
            // Background trigger — never let it crash the app; the next trigger retries.
        }
    }

    private async Task RaisePendingCountAsync() =>
        PendingCountChanged?.Invoke(this, await _localDb.GetPendingActionCountAsync());

    public void Dispose()
    {
        Connectivity.Current.ConnectivityChanged -= OnConnectivityChanged;
        _syncLock.Dispose();
    }

    private sealed class NotAuthenticatedException : Exception;
}
