using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Integration.TrendyolMarketplace.Models;

/// <summary>
/// Represents the plugin configuration model
/// </summary>
public record ConfigurationModel : BaseNopModel
{
    [NopResourceDisplayName("Plugins.Integration.TrendyolMarketplace.Fields.Enabled")]
    public bool Enabled { get; set; }

    [NopResourceDisplayName("Plugins.Integration.TrendyolMarketplace.Fields.ApiBaseUrl")]
    public string ApiBaseUrl { get; set; }

    [NopResourceDisplayName("Plugins.Integration.TrendyolMarketplace.Fields.DefaultProductSyncIntervalMinutes")]
    public int DefaultProductSyncIntervalMinutes { get; set; }

    [NopResourceDisplayName("Plugins.Integration.TrendyolMarketplace.Fields.DefaultStockPriceSyncIntervalMinutes")]
    public int DefaultStockPriceSyncIntervalMinutes { get; set; }

    [NopResourceDisplayName("Plugins.Integration.TrendyolMarketplace.Fields.CategoryBrandSyncIntervalHours")]
    public int CategoryBrandSyncIntervalHours { get; set; }

    [NopResourceDisplayName("Plugins.Integration.TrendyolMarketplace.Fields.ApiRequestDelayMs")]
    public int ApiRequestDelayMs { get; set; }

    [NopResourceDisplayName("Plugins.Integration.TrendyolMarketplace.Fields.MaxRetryCount")]
    public int MaxRetryCount { get; set; }

    [NopResourceDisplayName("Plugins.Integration.TrendyolMarketplace.Fields.ApiPageSize")]
    public int ApiPageSize { get; set; }

    [NopResourceDisplayName("Plugins.Integration.TrendyolMarketplace.Fields.AutoCreateManufacturers")]
    public bool AutoCreateManufacturers { get; set; }

    [NopResourceDisplayName("Plugins.Integration.TrendyolMarketplace.Fields.DownloadProductImages")]
    public bool DownloadProductImages { get; set; }

    [NopResourceDisplayName("Plugins.Integration.TrendyolMarketplace.Fields.PublishImportedProducts")]
    public bool PublishImportedProducts { get; set; }

    [NopResourceDisplayName("Plugins.Integration.TrendyolMarketplace.Fields.SkipUnmappedCategories")]
    public bool SkipUnmappedCategories { get; set; }

    [NopResourceDisplayName("Plugins.Integration.TrendyolMarketplace.Fields.ImportSpecificationAttributes")]
    public bool ImportSpecificationAttributes { get; set; }
}
