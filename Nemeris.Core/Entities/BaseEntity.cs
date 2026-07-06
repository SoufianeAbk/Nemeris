namespace Nemeris.Core.Entities;

/// <summary>
/// Common base for every persisted domain entity. Timestamps are set centrally
/// by the DbContext on save; IsDeleted drives the global soft-delete query filter.
/// </summary>
public abstract class BaseEntity
{
    // Version-7 GUIDs are time-ordered, which keeps SQL Server clustered
    // indexes from fragmenting the way random GUIDs do.
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
}
