using Nemeris.Core.Common;
using Nemeris.Core.Dtos;
using Nemeris.Core.Entities;

namespace Nemeris.Web.Models;

public class ProductListViewModel
{
    public PagedList<Product> Products { get; set; } = new();

    public IReadOnlyList<Category> Categories { get; set; } = [];

    /// <summary>Echoed back so the filter form and pager links keep their state.</summary>
    public ProductFilterDto Filter { get; set; } = new();
}
