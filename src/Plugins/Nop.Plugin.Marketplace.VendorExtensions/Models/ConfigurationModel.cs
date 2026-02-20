using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Marketplace.VendorExtensions.Models;

/// <summary>
/// Represents a configuration model for Vendor Extensions plugin
/// </summary>
public record ConfigurationModel : BaseNopModel
{
    /// <summary>
    /// Gets or sets a value indicating whether the plugin is enabled
    /// </summary>
    [NopResourceDisplayName("Plugins.Marketplace.VendorExtensions.Fields.Enabled")]
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the minimum payout amount
    /// </summary>
    [NopResourceDisplayName("Plugins.Marketplace.VendorExtensions.Fields.MinimumPayoutAmount")]
    public decimal MinimumPayoutAmount { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether vendors can request payout
    /// </summary>
    [NopResourceDisplayName("Plugins.Marketplace.VendorExtensions.Fields.AllowVendorPayoutRequest")]
    public bool AllowVendorPayoutRequest { get; set; }
}
