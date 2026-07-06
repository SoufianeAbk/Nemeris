namespace Nemeris.Core.Entities;

/// <summary>A saved address in the user's address book (orders snapshot it, never reference it).</summary>
public class Address : BaseEntity
{
    public Guid UserId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Street { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string PostalCode { get; set; } = string.Empty;

    public string Country { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public bool IsDefault { get; set; }
}
