using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Nemeris.Core.Dtos;
using Nemeris.Core.Entities;
using Nemeris.Core.Interfaces;
using Nemeris.Infrastructure.Data;

namespace Nemeris.Infrastructure.Services;

public class AddressService(NemerisDbContext db, IMapper mapper) : IAddressService
{
    public async Task<IReadOnlyList<Address>> GetForUserAsync(Guid userId, CancellationToken ct = default) =>
        await db.Addresses.AsNoTracking()
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.IsDefault)
            .ThenBy(a => a.CreatedAt)
            .ToListAsync(ct);

    public async Task<Address> CreateAsync(Guid userId, AddressUpsertDto dto, CancellationToken ct = default)
    {
        var address = mapper.Map<Address>(dto);
        address.UserId = userId;

        var hasExisting = await db.Addresses.AnyAsync(a => a.UserId == userId, ct);
        if (!hasExisting)
        {
            // The first address is always the default, whatever the form said.
            address.IsDefault = true;
        }
        else if (address.IsDefault)
        {
            var currentDefaults = await db.Addresses
                .Where(a => a.UserId == userId && a.IsDefault)
                .ToListAsync(ct);
            foreach (var other in currentDefaults)
            {
                other.IsDefault = false;
            }
        }

        db.Addresses.Add(address);
        await db.SaveChangesAsync(ct);
        return address;
    }

    public async Task<bool> DeleteAsync(Guid userId, Guid addressId, CancellationToken ct = default)
    {
        var address = await db.Addresses
            .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId, ct);
        if (address is null)
        {
            return false;
        }

        db.Addresses.Remove(address);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
