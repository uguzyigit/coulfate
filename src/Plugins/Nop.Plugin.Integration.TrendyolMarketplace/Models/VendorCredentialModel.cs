using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace Nop.Plugin.Integration.TrendyolMarketplace.Models;

/// <summary>
/// Represents the vendor credential model
/// </summary>
public record VendorCredentialModel : BaseNopEntityModel
{
    public int VendorId { get; set; }

    public string VendorName { get; set; }

    [NopResourceDisplayName("Plugins.Integration.TrendyolMarketplace.Vendor.Fields.TrendyolSupplierId")]
    [Required]
    public long TrendyolSupplierId { get; set; }

    [NopResourceDisplayName("Plugins.Integration.TrendyolMarketplace.Vendor.Fields.ApiKey")]
    [Required]
    public string ApiKey { get; set; }

    [NopResourceDisplayName("Plugins.Integration.TrendyolMarketplace.Vendor.Fields.ApiSecret")]
    [DataType(DataType.Password)]
    public string ApiSecret { get; set; }

    [NopResourceDisplayName("Plugins.Integration.TrendyolMarketplace.Vendor.Fields.IntegrationReferenceCode")]
    public string IntegrationReferenceCode { get; set; }

    [NopResourceDisplayName("Plugins.Integration.TrendyolMarketplace.Vendor.Fields.IsActive")]
    public bool IsActive { get; set; }

    [NopResourceDisplayName("Plugins.Integration.TrendyolMarketplace.Vendor.Fields.AutoSyncEnabled")]
    public bool AutoSyncEnabled { get; set; }

    [NopResourceDisplayName("Plugins.Integration.TrendyolMarketplace.Vendor.Fields.SyncIntervalMinutes")]
    public int SyncIntervalMinutes { get; set; }

    public DateTime? LastSyncOnUtc { get; set; }

    public string LastSyncOnDisplay { get; set; }

    public bool HasCredentials { get; set; }
}

/// <summary>
/// Represents vendor credential list model for admin
/// </summary>
public record VendorCredentialListModel : BasePagedListModel<VendorCredentialModel>
{
}

/// <summary>
/// Represents vendor credential search model
/// </summary>
public record VendorCredentialSearchModel : BaseSearchModel
{
    public bool? IsActive { get; set; }
}
