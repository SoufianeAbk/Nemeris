using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nemeris.Core.Enums;
using Nemeris.Infrastructure.Data;
using Nemeris.Infrastructure.Identity;
using Nemeris.Web.Models;

namespace Nemeris.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = DbSeeder.AdminRole)]
public class DashboardController(NemerisDbContext db, UserManager<ApplicationUser> userManager) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        // Aggregates only — fine to query the DbContext directly rather than
        // widening every service interface with dashboard-specific counts.
        var model = new DashboardViewModel
        {
            TotalProducts = await db.Products.CountAsync(ct),
            TotalCategories = await db.Categories.CountAsync(ct),
            TotalOrders = await db.Orders.CountAsync(ct),
            PendingOrders = await db.Orders.CountAsync(o => o.Status == OrderStatus.Pending, ct),
            TotalCustomers = await userManager.Users.CountAsync(ct),
            RecentOrders = await db.Orders.AsNoTracking()
                .OrderByDescending(o => o.CreatedAt)
                .Take(5)
                .ToListAsync(ct),
        };

        return View(model);
    }
}
