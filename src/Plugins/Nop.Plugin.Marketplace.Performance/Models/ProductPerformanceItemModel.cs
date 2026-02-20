namespace Nop.Plugin.Marketplace.Performance.Models;

public class ProductPerformanceItemModel
{
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public string Sku { get; set; }
    public string ThumbnailUrl { get; set; }
    public int Views { get; set; }
    public int Wishlist { get; set; }
    public int AddToCart { get; set; }
    public int GrossOrders { get; set; }
    public string ConversionRate { get; set; }
    public int GrossQty { get; set; }
    public decimal GrossRevenue { get; set; }
    public decimal FinalScore { get; set; }
}
