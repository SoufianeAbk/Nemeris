using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nemeris.Core.Dtos;
using Nemeris.Core.Interfaces;
using Nemeris.Infrastructure.Data;
using Nemeris.Shared.Catalog;
using Nemeris.Shared.Common;

namespace Nemeris.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController(
    IProductService productService,
    IMapper mapper,
    IValidator<ProductCreateDto> createValidator,
    IValidator<ProductUpdateDto> updateValidator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetProducts([FromQuery] ProductFilterDto filter, CancellationToken ct)
    {
        // Hidden products are an admin-only view, whatever the query string claims.
        if (filter.IncludeInactive && !User.IsInRole(DbSeeder.AdminRole))
        {
            filter.IncludeInactive = false;
        }

        var result = await productService.GetPagedAsync(filter, ct);
        return Ok(mapper.Map<PagedResult<ProductDto>>(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductDto>> GetProduct(Guid id, CancellationToken ct)
    {
        var product = await productService.GetByIdAsync(id, ct);
        return product is null ? NotFound() : Ok(mapper.Map<ProductDto>(product));
    }

    [HttpGet("slug/{slug}")]
    public async Task<ActionResult<ProductDto>> GetProductBySlug(string slug, CancellationToken ct)
    {
        var product = await productService.GetBySlugAsync(slug, ct);
        return product is null ? NotFound() : Ok(mapper.Map<ProductDto>(product));
    }

    [Authorize(Roles = DbSeeder.AdminRole)]
    [HttpPost]
    public async Task<ActionResult<ProductDto>> CreateProduct(ProductCreateDto dto, CancellationToken ct)
    {
        var validation = await createValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));
        }

        var product = await productService.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, mapper.Map<ProductDto>(product));
    }

    [Authorize(Roles = DbSeeder.AdminRole)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProductDto>> UpdateProduct(Guid id, ProductUpdateDto dto, CancellationToken ct)
    {
        var validation = await updateValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));
        }

        var product = await productService.UpdateAsync(id, dto, ct);
        return product is null ? NotFound() : Ok(mapper.Map<ProductDto>(product));
    }

    [Authorize(Roles = DbSeeder.AdminRole)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteProduct(Guid id, CancellationToken ct) =>
        await productService.DeleteAsync(id, ct) ? NoContent() : NotFound();
}
