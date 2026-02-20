using Nop.Core.Configuration;

namespace Nop.Plugin.Widgets.GoogleAnalytics4.Domain;

/// <summary>
/// Google Analytics 4 settings
/// </summary>
public class GA4Settings : ISettings
{
    /// <summary>
    /// Gets or sets the Google Analytics Measurement ID (G-XXXXXXXXXX)
    /// </summary>
    public string MeasurementId { get; set; }

    /// <summary>
    /// Gets or sets the API Secret for Measurement Protocol
    /// </summary>
    public string ApiSecret { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the plugin is enabled
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to track e-commerce events
    /// </summary>
    public bool EnableEcommerce { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to use prices including tax
    /// </summary>
    public bool IncludeTax { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to include customer ID in tracking
    /// </summary>
    public bool IncludeCustomerId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to enable debug mode (console logging)
    /// </summary>
    public bool EnableDebugMode { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to track view_item_list events
    /// </summary>
    public bool TrackViewItemList { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to track view_item events
    /// </summary>
    public bool TrackViewItem { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to track add_to_cart events
    /// </summary>
    public bool TrackAddToCart { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to track remove_from_cart events
    /// </summary>
    public bool TrackRemoveFromCart { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to track view_cart events
    /// </summary>
    public bool TrackViewCart { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to track begin_checkout events
    /// </summary>
    public bool TrackBeginCheckout { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to track add_shipping_info events
    /// </summary>
    public bool TrackAddShippingInfo { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to track add_payment_info events
    /// </summary>
    public bool TrackAddPaymentInfo { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to track purchase events (server-side)
    /// </summary>
    public bool TrackPurchase { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to track refund events (server-side)
    /// </summary>
    public bool TrackRefund { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to track search events
    /// </summary>
    public bool TrackSearch { get; set; }
}
