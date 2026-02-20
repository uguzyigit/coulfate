using Nop.Core.Configuration;

namespace Nop.Plugin.Integration.TrendyolMarketplace;

/// <summary>
/// Represents global plugin settings
/// </summary>
public class TrendyolSettings : ISettings
{
    /// <summary>
    /// Gets or sets a value indicating whether the plugin is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the Trendyol API base URL
    /// </summary>
    public string ApiBaseUrl { get; set; } = "https://apigw.trendyol.com/integration";

    /// <summary>
    /// Gets or sets the default product sync interval in minutes
    /// </summary>
    public int DefaultProductSyncIntervalMinutes { get; set; } = 60;

    /// <summary>
    /// Gets or sets the default stock/price sync interval in minutes
    /// </summary>
    public int DefaultStockPriceSyncIntervalMinutes { get; set; } = 15;

    /// <summary>
    /// Gets or sets the category/brand sync interval in hours
    /// </summary>
    public int CategoryBrandSyncIntervalHours { get; set; } = 24;

    /// <summary>
    /// Gets or sets the API request delay in milliseconds (rate limiting)
    /// </summary>
    public int ApiRequestDelayMs { get; set; } = 1000;

    /// <summary>
    /// Gets or sets the maximum retry count for failed API requests
    /// </summary>
    public int MaxRetryCount { get; set; } = 3;

    /// <summary>
    /// Gets or sets the page size for Trendyol API requests
    /// </summary>
    public int ApiPageSize { get; set; } = 50;

    /// <summary>
    /// Gets or sets a value indicating whether to auto-create manufacturers for unmapped brands
    /// </summary>
    public bool AutoCreateManufacturers { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to download product images
    /// </summary>
    public bool DownloadProductImages { get; set; } = true;

    /// <summary>
    /// Gets or sets the default store ID for imported products (0 = all stores)
    /// </summary>
    public int DefaultStoreId { get; set; } = 0;

    /// <summary>
    /// Gets or sets a value indicating whether imported products should be published immediately
    /// </summary>
    public bool PublishImportedProducts { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to skip products without mapped categories
    /// </summary>
    public bool SkipUnmappedCategories { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to import Trendyol product attributes as specification attributes
    /// </summary>
    public bool ImportSpecificationAttributes { get; set; } = true;

    /// <summary>
    /// Gets or sets the encryption key for storing API secrets (auto-generated on install)
    /// </summary>
    public string EncryptionKey { get; set; }
}
