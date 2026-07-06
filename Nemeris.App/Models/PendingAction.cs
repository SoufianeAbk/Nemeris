using SQLite;

namespace Nemeris.App.Models;

/// <summary>
/// One queued offline action. The payload is the serialized wire-contract request
/// (UpsertCartItemRequest, CheckoutRequest), so replaying is just "send this JSON".
/// AutoIncrement id doubles as strict FIFO ordering during sync.
/// </summary>
[Table("pending_actions")]
public class PendingAction
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    /// <summary>SyncService.ActionUpsertCartItem or SyncService.ActionCheckout.</summary>
    public string Type { get; set; } = string.Empty;

    public string PayloadJson { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    /// <summary>Failed replays increment this; poison actions are dropped after a cap.</summary>
    public int Attempts { get; set; }

    public string? LastError { get; set; }
}
