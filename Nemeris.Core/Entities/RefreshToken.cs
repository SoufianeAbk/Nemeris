namespace Nemeris.Core.Entities;

/// <summary>
/// One row per issued refresh token. Rotation: when a token is used it is revoked
/// and ReplacedByToken points at its successor, so reuse of an old token is detectable.
/// </summary>
public class RefreshToken : BaseEntity
{
    public Guid UserId { get; set; }

    public string Token { get; set; } = string.Empty;

    public DateTime ExpiresAtUtc { get; set; }

    public DateTime? RevokedAtUtc { get; set; }

    public string? ReplacedByToken { get; set; }

    public bool IsActive => RevokedAtUtc is null && DateTime.UtcNow < ExpiresAtUtc;
}
