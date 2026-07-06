using AutoMapper;
using Nemeris.Core.Dtos;
using Nemeris.Core.Entities;

namespace Nemeris.Core.Mapping;

/// <summary>
/// Maps write-side DTOs onto entities. Read-side mappings to the wire DTOs in
/// Nemeris.Shared live in the Api project (Core cannot reference Shared).
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<ProductCreateDto, Product>();

        // Never let a mapper overwrite identity/audit columns on update.
        CreateMap<ProductUpdateDto, Product>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.UpdatedAt, o => o.Ignore())
            .ForMember(d => d.IsDeleted, o => o.Ignore());

        CreateMap<CategoryUpsertDto, Category>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.UpdatedAt, o => o.Ignore())
            .ForMember(d => d.IsDeleted, o => o.Ignore());

        CreateMap<AddressUpsertDto, Address>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.UserId, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.UpdatedAt, o => o.Ignore())
            .ForMember(d => d.IsDeleted, o => o.Ignore());

        CreateMap<ReviewCreateDto, Review>();
    }
}
