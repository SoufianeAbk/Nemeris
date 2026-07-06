using Nemeris.Core.Dtos;
using Nemeris.Core.Entities;

namespace Nemeris.Core.Interfaces;

public interface IAddressService
{
    /// <summary>Default address first, then oldest to newest.</summary>
    Task<IReadOnlyList<Address>> GetForUserAsync(Guid userId, CancellationToken ct = default);

    /// <summary>The first address a user creates automatically becomes the default.</summary>
    Task<Address> CreateAsync(Guid userId, AddressUpsertDto dto, CancellationToken ct = default);

    /// <summary>Scoped to the owner; returns false when the address is not theirs.</summary>
    Task<bool> DeleteAsync(Guid userId, Guid addressId, CancellationToken ct = default);
}
