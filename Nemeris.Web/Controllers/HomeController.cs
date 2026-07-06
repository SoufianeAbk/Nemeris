using System.Diagnostics;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Nemeris.Core.Dtos;
using Nemeris.Core.Interfaces;
using Nemeris.Web.Models;

namespace Nemeris.Web.Controllers;

public class HomeController(IProductService productService) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var featured = await productService.GetPagedAsync(
            new ProductFilterDto { SortBy = "newest", PageSize = 8 }, ct);
        return View(featured.Items);
    }

    /// <summary>
    /// Cookie-based culture switch: stores the choice in ASP.NET Core's culture
    /// cookie for a year, then returns to the page the visitor was on.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SetLanguage(string culture, string? returnUrl = null)
    {
        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true });

        return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl! : "/");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
