using Nop.Core.Configuration;

namespace Nop.Plugin.Marketplace.ShippingManager;

/// <summary>
/// Shipping Manager plugin settings
/// Note: Free shipping settings are handled by NopCommerce's built-in ShippingSettings
/// </summary>
public class ShippingManagerSettings : ISettings
{
    /// <summary>
    /// Default shipping rate when vendor hasn't configured their own rate
    /// </summary>
    public decimal DefaultShippingRate { get; set; } = 0;

    /// <summary>
    /// Shipping method name shown to customers at checkout
    /// </summary>
    public string ShippingMethodName { get; set; } = "Standart Kargo";

    /// <summary>
    /// Shipping method description shown to customers at checkout
    /// </summary>
    public string ShippingMethodDescription { get; set; } = "Satıcının tercih ettiği kargo ile gönderilir";
}
