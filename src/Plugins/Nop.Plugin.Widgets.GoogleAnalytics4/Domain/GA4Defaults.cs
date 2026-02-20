namespace Nop.Plugin.Widgets.GoogleAnalytics4.Domain;

/// <summary>
/// Google Analytics 4 plugin defaults
/// </summary>
public static class GA4Defaults
{
    /// <summary>
    /// Gets the plugin system name
    /// </summary>
    public static string SystemName => "Widgets.GoogleAnalytics4";

    /// <summary>
    /// Gets the configuration route name
    /// </summary>
    public static string ConfigurationRouteName => "Plugin.Widgets.GoogleAnalytics4.Configure";

    /// <summary>
    /// Gets the Measurement Protocol API endpoint
    /// </summary>
    public static string MeasurementProtocolUrl => "https://www.google-analytics.com/mp/collect";

    /// <summary>
    /// Gets the debug endpoint for Measurement Protocol
    /// </summary>
    public static string MeasurementProtocolDebugUrl => "https://www.google-analytics.com/debug/mp/collect";

    /// <summary>
    /// Cookie name for GA client ID
    /// </summary>
    public static string ClientIdCookieName => "_ga";

    /// <summary>
    /// Cookie name prefix for GA session ID
    /// </summary>
    public static string SessionIdCookiePrefix => "_ga_";

    /// <summary>
    /// Generic attribute key for storing client ID on order
    /// </summary>
    public static string ClientIdAttributeKey => "GA4.ClientId";

    /// <summary>
    /// Generic attribute key for storing session ID on order
    /// </summary>
    public static string SessionIdAttributeKey => "GA4.SessionId";

    #region Event Names

    public static string EventViewItemList => "view_item_list";
    public static string EventSelectItem => "select_item";
    public static string EventViewItem => "view_item";
    public static string EventAddToCart => "add_to_cart";
    public static string EventRemoveFromCart => "remove_from_cart";
    public static string EventViewCart => "view_cart";
    public static string EventBeginCheckout => "begin_checkout";
    public static string EventAddShippingInfo => "add_shipping_info";
    public static string EventAddPaymentInfo => "add_payment_info";
    public static string EventPurchase => "purchase";
    public static string EventRefund => "refund";
    public static string EventSearch => "search";

    #endregion
}
