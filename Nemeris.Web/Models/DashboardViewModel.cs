using Nemeris.Core.Entities;

namespace Nemeris.Web.Models;

public class DashboardViewModel
{
    public int TotalProducts { get; set; }
    public int TotalCategories { get; set; }
    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public int TotalCustomers { get; set; }

    public IReadOnlyList<Order> RecentOrders { get; set; } = [];
}
