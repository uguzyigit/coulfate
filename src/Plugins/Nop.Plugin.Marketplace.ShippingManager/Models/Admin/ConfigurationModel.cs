using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Marketplace.ShippingManager.Models.Admin;

/// <summary>
/// Admin configuration model
/// Note: Free shipping is configured in NopCommerce's built-in shipping settings
/// </summary>
public record ConfigurationModel : BaseNopModel
{
    [NopResourceDisplayName("Plugins.Shipping.Marketplace.DefaultShippingRate")]
    public decimal DefaultShippingRate { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.Marketplace.ShippingMethodName")]
    public string ShippingMethodName { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.Marketplace.ShippingMethodDescription")]
    public string ShippingMethodDescription { get; set; }
}
