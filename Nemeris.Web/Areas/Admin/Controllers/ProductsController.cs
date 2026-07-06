using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nemeris.Core.Dtos;
using Nemeris.Core.Interfaces;
using Nemeris.Infrastructure.Data;

namespace Nemeris.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = DbSeeder.AdminRole)]
public class ProductsController(
    IProductService productService,
    ICategoryService categoryService,
    IValidator<ProductCreateDto> createValidator,
    IValidator<ProductUpdateDto> updateValidator) : Controller
{
    public async Task<IActionResult> Index(int page = 1, string? search = null, CancellationToken ct = default)
    {
        var filter = new ProductFilterDto
        {
            Page = page,
            PageSize = 20,
            Search = search,
            IncludeInactive = true, // admins manage the full catalog, hidden items included
        };

        ViewBag.Search = search;
        return View(await productService.GetPagedAsync(filter, ct));
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        ViewBag.Categories = await categoryService.GetAllAsync(ct);
        return View(new ProductCreateDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductCreateDto dto, CancellationToken ct)
    {
        // Same FluentValidation rules the Api uses — one source of truth in Core.
        var validation = await createValidator.ValidateAsync(dto, ct);
        foreach (var error in validation.Errors)
        {
            ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Categories = await categoryService.GetAllAsync(ct);
            return View(dto);
        }

        await productService.CreateAsync(dto, ct);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
    {
        var product = await productService.GetByIdAsync(id, ct);
        if (product is null)
        {
            return NotFound();
        }

        ViewBag.Categories = await categoryService.GetAllAsync(ct);
        ViewBag.ProductId = id;

        return View(new ProductUpdateDto
        {
            Name = product.Name,
            Slug = product.Slug,
            Description = product.Description,
            Sku = product.Sku,
            Price = product.Price,
            CompareAtPrice = product.CompareAtPrice,
            StockQuantity = product.StockQuantity,
            ImageUrl = product.ImageUrl,
            IsActive = product.IsActive,
            CategoryId = product.CategoryId,
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, ProductUpdateDto dto, CancellationToken ct)
    {
        var validation = await updateValidator.ValidateAsync(dto, ct);
        foreach (var error in validation.Errors)
        {
            ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Categories = await categoryService.GetAllAsync(ct);
            ViewBag.ProductId = id;
            return View(dto);
        }

        var updated = await productService.UpdateAsync(id, dto, ct);
        return updated is null ? NotFound() : RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await productService.DeleteAsync(id, ct);
        return RedirectToAction(nameof(Index));
    }
}
