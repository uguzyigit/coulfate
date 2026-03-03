namespace Nop.Plugin.Marketplace.VendorExtensions.Models;

public class VendorProductDashboardModel
{
    public int TotalProducts { get; set; }
    public int ApprovedProducts { get; set; }
    public int PendingApproval { get; set; }
    public int OutOfStockProducts { get; set; }
    public int ActiveProducts { get; set; }
}
