using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Marketplace.Commission.Models;

public record ConfigurationModel : BaseNopModel
{
    [NopResourceDisplayName("Plugins.Marketplace.Commission.Fields.DefaultCommissionRate")]
    public decimal DefaultCommissionRate { get; set; }

    [NopResourceDisplayName("Plugins.Marketplace.Commission.Fields.MarketplaceFeePerOrder")]
    public decimal MarketplaceFeePerOrder { get; set; }

    [NopResourceDisplayName("Plugins.Marketplace.Commission.Fields.TaxWithholdingRate")]
    public decimal TaxWithholdingRate { get; set; }

    [NopResourceDisplayName("Plugins.Marketplace.Commission.Fields.AutoProcessOnPayment")]
    public bool AutoProcessOnPayment { get; set; }

    [NopResourceDisplayName("Plugins.Marketplace.Commission.Fields.CalculateOnDiscountedPrice")]
    public bool CalculateOnDiscountedPrice { get; set; }

    [NopResourceDisplayName("Plugins.Marketplace.Commission.Fields.IncludeShippingInCommission")]
    public bool IncludeShippingInCommission { get; set; }
}
