using Nop.Core.Configuration;

namespace Nop.Plugin.Marketplace.Commission;

public class CommissionSettings : ISettings
{
    /// <summary>
    /// Default commission rate (%)
    /// </summary>
    public decimal DefaultCommissionRate { get; set; } = 15.0m;

    /// <summary>
    /// Fixed fee per order (TL)
    /// </summary>
    public decimal FixedFeePerOrder { get; set; } = 5.0m;

    /// <summary>
    /// Marketplace fee per order (TL) - Alternative name
    /// </summary>
    public decimal MarketplaceFeePerOrder
    {
        get => FixedFeePerOrder;
        set => FixedFeePerOrder = value;
    }

    /// <summary>
    /// Tax withholding rate (%)
    /// </summary>
    public decimal TaxWithholdingRate { get; set; } = 1.0m;

    /// <summary>
    /// Auto process commission on payment
    /// </summary>
    public bool AutoProcessOnPayment { get; set; } = true;

    /// <summary>
    /// Calculate commission on discounted price
    /// </summary>
    public bool CalculateOnDiscountedPrice { get; set; } = true;

    /// <summary>
    /// Include shipping cost in commission calculation
    /// </summary>
    public bool IncludeShippingInCommission { get; set; } = false;
}