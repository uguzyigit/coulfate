using System.ComponentModel.DataAnnotations;
using Marketplace.Abstractions.Domain;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Marketplace.Core.Models;

/// <summary>
/// Represents a vendor extension model
/// Used for marketplace-specific vendor settings
/// </summary>
public record VendorExtensionModel : BaseNopEntityModel
{
    /// <summary>
    /// Gets or sets the NopCommerce vendor ID
    /// </summary>
    public int VendorId { get; set; }

    /// <summary>
    /// Gets or sets the vendor name (from NopCommerce Vendor)
    /// </summary>
    [NopResourceDisplayName("Plugins.Marketplace.Core.Fields.VendorName")]
    public string VendorName { get; set; }

    /// <summary>
    /// Gets or sets the vendor code
    /// </summary>
    [NopResourceDisplayName("Plugins.Marketplace.Core.Fields.Code")]
    [Required]
    public string Code { get; set; }

    /// <summary>
    /// Gets or sets the marketplace status
    /// </summary>
    [NopResourceDisplayName("Plugins.Marketplace.Core.Fields.MarketplaceStatus")]
    public MarketplaceVendorStatus MarketplaceStatus { get; set; }

    /// <summary>
    /// Gets or sets the phone number
    /// </summary>
    [NopResourceDisplayName("Plugins.Marketplace.Core.Fields.Phone")]
    public string Phone { get; set; }

    /// <summary>
    /// Gets or sets the bank account holder name
    /// </summary>
    [NopResourceDisplayName("Plugins.Marketplace.Core.Fields.BankAccountName")]
    public string BankAccountName { get; set; }

    /// <summary>
    /// Gets or sets the IBAN
    /// </summary>
    [NopResourceDisplayName("Plugins.Marketplace.Core.Fields.BankIban")]
    public string BankIban { get; set; }

    /// <summary>
    /// Gets or sets the tax number
    /// </summary>
    [NopResourceDisplayName("Plugins.Marketplace.Core.Fields.TaxNumber")]
    public string TaxNumber { get; set; }

    /// <summary>
    /// Gets or sets the trade registry number
    /// </summary>
    [NopResourceDisplayName("Plugins.Marketplace.Core.Fields.TradeRegistryNumber")]
    public string TradeRegistryNumber { get; set; }

    /// <summary>
    /// Gets or sets whether product approval is required
    /// </summary>
    [NopResourceDisplayName("Plugins.Marketplace.Core.Fields.RequiresProductApproval")]
    public bool RequiresProductApproval { get; set; }

    /// <summary>
    /// Gets or sets the minimum payout amount
    /// </summary>
    [NopResourceDisplayName("Plugins.Marketplace.Core.Fields.MinimumPayoutAmount")]
    public decimal MinimumPayoutAmount { get; set; }

    public DateTime CreatedOnUtc { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }
}
