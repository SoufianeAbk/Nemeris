using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nemeris.Core.Dtos;
using Nemeris.Core.Interfaces;
using Nemeris.Infrastructure.Data;
using Nemeris.Shared.Catalog;

namespace Nemeris.Api.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController(ICategoryService categoryService, IMapper mapper) : ControllerBase
{
    /// <summary>Flat list; clients rebuild the tree from ParentCategoryId.</summary>
    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>>> GetCategories(CancellationToken ct)
    {
        var categories = await categoryService.GetAllAsync(ct);
        return Ok(mapper.Map<List<CategoryDto>>(categories));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CategoryDto>> GetCategory(Guid id, CancellationToken ct)
    {
        var category = await categoryService.GetByIdAsync(id, ct);
        return category is null ? NotFound() : Ok(mapper.Map<CategoryDto>(category));
    }

    [Authorize(Roles = DbSeeder.AdminRole)]
    [HttpPost]
    public async Task<ActionResult<CategoryDto>> CreateCategory(CategoryUpsertDto dto, CancellationToken ct)
    {
        var category = await categoryService.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, mapper.Map<CategoryDto>(category));
    }

    [Authorize(Roles = DbSeeder.AdminRole)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CategoryDto>> UpdateCategory(Guid id, CategoryUpsertDto dto, CancellationToken ct)
    {
        var category = await categoryService.UpdateAsync(id, dto, ct);
        return category is null ? NotFound() : Ok(mapper.Map<CategoryDto>(category));
    }

    /// <summary>Fails with a localized 400 (Error.CategoryInUse) while products or children remain.</summary>
    [Authorize(Roles = DbSeeder.AdminRole)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCategory(Guid id, CancellationToken ct) =>
        await categoryService.DeleteAsync(id, ct) ? NoContent() : NotFound();
}
