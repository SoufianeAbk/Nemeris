using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Nemeris.Core.Dtos;
using Nemeris.Core.Interfaces;
using Nemeris.Infrastructure.Data;

namespace Nemeris.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = DbSeeder.AdminRole)]
public class CategoriesController(
    ICategoryService categoryService,
    IValidator<CategoryUpsertDto> validator,
    IStringLocalizer<SharedResources> localizer) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct) =>
        View(await categoryService.GetAllAsync(ct));

    /// <summary>Handles the inline create form on the Index view.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryUpsertDto dto, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
        {
            TempData["Error"] = string.Join(" ", validation.Errors.Select(e => e.ErrorMessage));
            return RedirectToAction(nameof(Index));
        }

        await categoryService.CreateAsync(dto, ct);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
    {
        var category = await categoryService.GetByIdAsync(id, ct);
        if (category is null)
        {
            return NotFound();
        }

        ViewBag.CategoryId = id;
        ViewBag.Categories = await categoryService.GetAllAsync(ct);
        return View(new CategoryUpsertDto
        {
            Name = category.Name,
            Slug = category.Slug,
            Description = category.Description,
            ParentCategoryId = category.ParentCategoryId,
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, CategoryUpsertDto dto, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(dto, ct);
        foreach (var error in validation.Errors)
        {
            ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
        }

        if (!ModelState.IsValid)
        {
            ViewBag.CategoryId = id;
            ViewBag.Categories = await categoryService.GetAllAsync(ct);
            return View(dto);
        }

        try
        {
            var updated = await categoryService.UpdateAsync(id, dto, ct);
            return updated is null ? NotFound() : RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, localizer[ex.Message]);
            ViewBag.CategoryId = id;
            ViewBag.Categories = await categoryService.GetAllAsync(ct);
            return View(dto);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        try
        {
            await categoryService.DeleteAsync(id, ct);
        }
        catch (InvalidOperationException ex)
        {
            // Error.CategoryInUse — still has products or children.
            TempData["Error"] = localizer[ex.Message].Value;
        }

        return RedirectToAction(nameof(Index));
    }
}
