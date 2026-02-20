using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Shipping.PttKargo.Models;

/// <summary>
/// PTT Kargo Configuration Model
/// </summary>
public record ConfigurationModel : BaseNopModel
{
    [NopResourceDisplayName("Plugins.Shipping.PttKargo.MarketplaceMusteriId")]
    public string MarketplaceMusteriId { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.PttKargo.MarketplaceSifre")]
    public string MarketplaceSifre { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.PttKargo.UseSandbox")]
    public bool UseSandbox { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.PttKargo.MarketplaceContractEnabled")]
    public bool MarketplaceContractEnabled { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.PttKargo.AllowVendorContracts")]
    public bool AllowVendorContracts { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.PttKargo.DefaultSenderName")]
    public string DefaultSenderName { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.PttKargo.DefaultSenderAddress")]
    public string DefaultSenderAddress { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.PttKargo.DefaultSenderCityCode")]
    public int DefaultSenderCityCode { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.PttKargo.DefaultSenderDistrictCode")]
    public int DefaultSenderDistrictCode { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.PttKargo.DefaultSenderPhone")]
    public string DefaultSenderPhone { get; set; }
}
