using AutoMapper;
using Nemeris.Core.Common;
using Nemeris.Core.Entities;
using Nemeris.Shared.Cart;
using Nemeris.Shared.Catalog;
using Nemeris.Shared.Common;
using Nemeris.Shared.Orders;

namespace Nemeris.Api.Mapping;

/// <summary>
/// Read-side mappings: entity → wire DTO. This profile lives in the Api (not Core)
/// because it is the only project that references both Core and Shared.
/// </summary>
public class ApiMappingProfile : Profile
{
    public ApiMappingProfile()
    {
        // CategoryName is filled by flattening convention (Category.Name).
        CreateMap<Product, ProductDto>();

        CreateMap<Category, CategoryDto>();

        // ProductName/ProductImageUrl flatten from Product; money fields are explicit
        // because cart lines price from the live product.
        CreateMap<CartItem, CartItemDto>()
            .ForMember(d => d.UnitPrice, o => o.MapFrom(s => s.Product.Price))
            .ForMember(d => d.LineTotal, o => o.MapFrom(s => s.Product.Price * s.Quantity));

        CreateMap<OrderItem, OrderItemDto>();

        CreateMap<Order, OrderDto>()
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.PaymentStatus, o => o.MapFrom(s => s.PaymentStatus.ToString()));

        // Open generic: any PagedList<TEntity> → PagedResult<TDto> for which an
        // element mapping exists.
        CreateMap(typeof(PagedList<>), typeof(PagedResult<>));
    }
}
