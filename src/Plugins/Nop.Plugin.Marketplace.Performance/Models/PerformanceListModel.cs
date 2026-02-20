using Microsoft.AspNetCore.Mvc.Rendering;

namespace Nop.Plugin.Marketplace.Performance.Models;

public class PerformanceListModel
{
    public int VendorId { get; set; }
    public IList<SelectListItem> AvailableVendors { get; set; } = new List<SelectListItem>();
    public bool IsAdmin { get; set; }
}
