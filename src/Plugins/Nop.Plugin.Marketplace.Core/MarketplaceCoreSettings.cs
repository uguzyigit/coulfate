using Nop.Core.Configuration;

namespace Nop.Plugin.Marketplace.Core;

/// <summary>
/// Represents settings for Marketplace.Core plugin
/// </summary>
public class MarketplaceCoreSettings : ISettings
{
    /// <summary>
    /// Gets or sets a value indicating whether the marketplace is enabled
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the default vendor approval requirement
    /// </summary>
    public bool RequireVendorApproval { get; set; } = true;

    /// <summary>
    /// Gets or sets minimum payout amount
    /// </summary>
    public decimal MinimumPayoutAmount { get; set; } = 100m;

    /// <summary>
    /// Gets or sets maximum pending days before auto-cancel
    /// </summary>
    public int MaxPendingDays { get; set; } = 30;
}
