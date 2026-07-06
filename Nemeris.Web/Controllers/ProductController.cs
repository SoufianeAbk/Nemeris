using Microsoft.AspNetCore.Mvc;
using Nemeris.Core.Dtos;
using Nemeris.Core.Interfaces;
using Nemeris.Web.Models;

namespace Nemeris.Web.Controllers;

public class ProductController(IProductService productService, ICategoryService categoryService) : Controller
{
    public async Task<IActionResult> Index([FromQuery] ProductFilterDto filter, CancellationToken ct)
    {
        // The storefront never shows hidden products; that's the admin area's job.
        filter.IncludeInactive = false;
        if (filter.PageSize is < 1 or > 48)
        {
            filter.PageSize = 12;
        }

        var model = new ProductListViewModel
        {
            Products = await productService.GetPagedAsync(filter, ct),
            Categories = await categoryService.GetAllAsync(ct),
            Filter = filter,
        };

        return View(model);
    }

    /// <summary>Slug-based route (/products/wireless-mouse) — SEO-friendly storefront URLs.</summary>
    [HttpGet("/products/{slug}")]
    public async Task<IActionResult> Details(string slug, CancellationToken ct)
    {
        var product = await productService.GetBySlugAsync(slug, ct);
        return product is null ? NotFound() : View(product);
    }
}
