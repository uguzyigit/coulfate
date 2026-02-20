using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Widgets.GoogleAnalytics4.Models;

/// <summary>
/// Configuration model for GA4 plugin
/// </summary>
public record ConfigurationModel : BaseNopModel
{
    public int ActiveStoreScopeConfiguration { get; set; }

    [NopResourceDisplayName("Plugins.Widgets.GoogleAnalytics4.MeasurementId")]
    public string MeasurementId { get; set; }
    public bool MeasurementId_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Widgets.GoogleAnalytics4.ApiSecret")]
    public string ApiSecret { get; set; }
    public bool ApiSecret_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Widgets.GoogleAnalytics4.Enabled")]
    public bool Enabled { get; set; }
    public bool Enabled_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Widgets.GoogleAnalytics4.EnableEcommerce")]
    public bool EnableEcommerce { get; set; }
    public bool EnableEcommerce_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Widgets.GoogleAnalytics4.IncludeTax")]
    public bool IncludeTax { get; set; }
    public bool IncludeTax_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Widgets.GoogleAnalytics4.IncludeCustomerId")]
    public bool IncludeCustomerId { get; set; }
    public bool IncludeCustomerId_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Widgets.GoogleAnalytics4.EnableDebugMode")]
    public bool EnableDebugMode { get; set; }
    public bool EnableDebugMode_OverrideForStore { get; set; }

    // Event tracking options
    [NopResourceDisplayName("Plugins.Widgets.GoogleAnalytics4.TrackViewItemList")]
    public bool TrackViewItemList { get; set; }
    public bool TrackViewItemList_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Widgets.GoogleAnalytics4.TrackViewItem")]
    public bool TrackViewItem { get; set; }
    public bool TrackViewItem_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Widgets.GoogleAnalytics4.TrackAddToCart")]
    public bool TrackAddToCart { get; set; }
    public bool TrackAddToCart_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Widgets.GoogleAnalytics4.TrackRemoveFromCart")]
    public bool TrackRemoveFromCart { get; set; }
    public bool TrackRemoveFromCart_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Widgets.GoogleAnalytics4.TrackViewCart")]
    public bool TrackViewCart { get; set; }
    public bool TrackViewCart_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Widgets.GoogleAnalytics4.TrackBeginCheckout")]
    public bool TrackBeginCheckout { get; set; }
    public bool TrackBeginCheckout_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Widgets.GoogleAnalytics4.TrackAddShippingInfo")]
    public bool TrackAddShippingInfo { get; set; }
    public bool TrackAddShippingInfo_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Widgets.GoogleAnalytics4.TrackAddPaymentInfo")]
    public bool TrackAddPaymentInfo { get; set; }
    public bool TrackAddPaymentInfo_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Widgets.GoogleAnalytics4.TrackPurchase")]
    public bool TrackPurchase { get; set; }
    public bool TrackPurchase_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Widgets.GoogleAnalytics4.TrackRefund")]
    public bool TrackRefund { get; set; }
    public bool TrackRefund_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Widgets.GoogleAnalytics4.TrackSearch")]
    public bool TrackSearch { get; set; }
    public bool TrackSearch_OverrideForStore { get; set; }
}
