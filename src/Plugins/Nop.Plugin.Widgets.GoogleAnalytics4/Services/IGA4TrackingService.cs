using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;

namespace Nop.Plugin.Widgets.GoogleAnalytics4.Services;

/// <summary>
/// GA4 tracking service interface
/// </summary>
public interface IGA4TrackingService
{
    /// <summary>
    /// Gets the GA4 base script
    /// </summary>
    Task<string> GetGA4ScriptAsync();

    /// <summary>
    /// Gets product item data for GA4 event
    /// </summary>
    Task<GA4Item> GetProductItemAsync(Product product, decimal? price = null, int quantity = 1, int? index = null);

    /// <summary>
    /// Gets product item data for GA4 event from order item
    /// </summary>
    Task<GA4Item> GetOrderItemAsync(OrderItem orderItem);

    /// <summary>
    /// Sends purchase event via Measurement Protocol (server-side)
    /// </summary>
    Task SendPurchaseEventAsync(Order order);

    /// <summary>
    /// Sends refund event via Measurement Protocol (server-side)
    /// </summary>
    Task SendRefundEventAsync(Order order);

    /// <summary>
    /// Parses client ID from _ga cookie value
    /// </summary>
    string ParseClientId(string gaCookieValue);

    /// <summary>
    /// Parses session ID from _ga_XXXXX cookie value
    /// </summary>
    string ParseSessionId(string gaSessionCookieValue);

    /// <summary>
    /// Saves GA cookies to order generic attributes for server-side tracking
    /// </summary>
    Task SaveGACookiesToOrderAsync(Order order);
}

/// <summary>
/// GA4 Item model for events
/// </summary>
public class GA4Item
{
    public string ItemId { get; set; }
    public string ItemName { get; set; }
    public string ItemBrand { get; set; }
    public string ItemCategory { get; set; }
    public string ItemCategory2 { get; set; }
    public string ItemCategory3 { get; set; }
    public string ItemVariant { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public int? Index { get; set; }
    public string ItemListId { get; set; }
    public string ItemListName { get; set; }
}
