namespace Nop.Plugin.Marketplace.Performance.Models;

public class SalesPerformanceItemModel
{
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public string Sku { get; set; }
    public string ThumbnailUrl { get; set; }
    public int GrossOrders { get; set; }
    public int GrossQty { get; set; }
    public int CancelQty { get; set; }
    public string CancelRate { get; set; }
    public int ReturnQty { get; set; }
    public string ReturnRate { get; set; }
    public int NetQty { get; set; }
    public decimal GrossRevenue { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal NetRevenue { get; set; }
    public decimal AvgSalePrice { get; set; }
}
