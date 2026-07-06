using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nemeris.Core.Enums;
using Nemeris.Core.Interfaces;
using Nemeris.Infrastructure.Data;

namespace Nemeris.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = DbSeeder.AdminRole)]
public class OrdersController(IOrderService orderService) : Controller
{
    public async Task<IActionResult> Index(int page = 1, OrderStatus? status = null, CancellationToken ct = default)
    {
        ViewBag.Status = status;
        return View(await orderService.GetPagedAsync(page, 20, status, ct));
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id, CancellationToken ct)
    {
        // userId: null → unscoped admin access.
        var order = await orderService.GetByIdAsync(id, userId: null, ct);
        return order is null ? NotFound() : View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(Guid id, OrderStatus status, CancellationToken ct)
    {
        var order = await orderService.UpdateStatusAsync(id, status, ct);
        return order is null ? NotFound() : RedirectToAction(nameof(Details), new { id });
    }
}
