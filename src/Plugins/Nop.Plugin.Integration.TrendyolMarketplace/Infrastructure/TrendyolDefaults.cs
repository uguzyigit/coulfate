using Nop.Core.Caching;

namespace Nop.Plugin.Integration.TrendyolMarketplace.Infrastructure;

/// <summary>
/// Represents plugin constants and cache keys
/// </summary>
public static class TrendyolDefaults
{
    /// <summary>
    /// Plugin system name
    /// </summary>
    public const string SystemName = "Integration.TrendyolMarketplace";

    /// <summary>
    /// Plugin friendly name
    /// </summary>
    public const string FriendlyName = "Trendyol Marketplace Integration";

    /// <summary>
    /// Trendyol API base URL (new gateway - migrated from api.trendyol.com/sapigw in May 2025)
    /// </summary>
    public const string DefaultApiBaseUrl = "https://apigw.trendyol.com/integration";

    /// <summary>
    /// User agent format for Trendyol API
    /// </summary>
    public const string UserAgentFormat = "{0} - SelfIntegration";

    /// <summary>
    /// Localization resource prefix
    /// </summary>
    public const string LocalizationPrefix = "Plugins.Integration.TrendyolMarketplace";

    #region Cache Keys

    /// <summary>
    /// Cache key prefix for vendor credentials
    /// </summary>
    public static string VendorCredentialPrefix => "Nop.trendyol.vendorcredential.";

    /// <summary>
    /// Cache key for vendor credential by vendor ID
    /// </summary>
    public static CacheKey VendorCredentialByVendorIdCacheKey => new("Nop.trendyol.vendorcredential.byvendorid-{0}");

    /// <summary>
    /// Cache key prefix for categories
    /// </summary>
    public static string CategoryPrefix => "Nop.trendyol.category.";

    /// <summary>
    /// Cache key for category by Trendyol category ID
    /// </summary>
    public static CacheKey CategoryByTrendyolIdCacheKey => new("Nop.trendyol.category.bytrendyolid-{0}");

    /// <summary>
    /// Cache key for all categories
    /// </summary>
    public static CacheKey AllCategoriesCacheKey => new("Nop.trendyol.category.all");

    /// <summary>
    /// Cache key prefix for brands
    /// </summary>
    public static string BrandPrefix => "Nop.trendyol.brand.";

    /// <summary>
    /// Cache key for brand by Trendyol brand ID
    /// </summary>
    public static CacheKey BrandByTrendyolIdCacheKey => new("Nop.trendyol.brand.bytrendyolid-{0}");

    /// <summary>
    /// Cache key for all brands
    /// </summary>
    public static CacheKey AllBrandsCacheKey => new("Nop.trendyol.brand.all");

    /// <summary>
    /// Cache key prefix for attributes
    /// </summary>
    public static string AttributePrefix => "Nop.trendyol.attribute.";

    /// <summary>
    /// Cache key for attributes by category ID
    /// </summary>
    public static CacheKey AttributesByCategoryCacheKey => new("Nop.trendyol.attribute.bycategory-{0}");

    /// <summary>
    /// Cache key prefix for products
    /// </summary>
    public static string ProductPrefix => "Nop.trendyol.product.";

    /// <summary>
    /// Cache key for product by barcode and vendor
    /// </summary>
    public static CacheKey ProductByBarcodeCacheKey => new("Nop.trendyol.product.bybarcode-{0}-{1}");

    #endregion

    #region API Endpoints (new gateway: apigw.trendyol.com/integration)

    /// <summary>
    /// Product list endpoint template (sellers replaces suppliers in new API)
    /// </summary>
    public const string ProductListEndpoint = "/product/sellers/{0}/products";

    /// <summary>
    /// Category list endpoint
    /// </summary>
    public const string CategoryListEndpoint = "/product/product-categories";

    /// <summary>
    /// Category attributes endpoint template
    /// </summary>
    public const string CategoryAttributesEndpoint = "/product/product-categories/{0}/attributes";

    /// <summary>
    /// Brand list endpoint
    /// </summary>
    public const string BrandListEndpoint = "/product/brands";

    /// <summary>
    /// Shipment providers endpoint
    /// </summary>
    public const string ShipmentProvidersEndpoint = "/product/shipment-providers";

    /// <summary>
    /// Batch request status endpoint template
    /// </summary>
    public const string BatchStatusEndpoint = "/product/sellers/{0}/products/batch-requests/{1}";

    /// <summary>
    /// Seller addresses endpoint template
    /// </summary>
    public const string SellerAddressesEndpoint = "/sellers/{0}/addresses";

    /// <summary>
    /// Price and inventory update endpoint template
    /// </summary>
    public const string PriceInventoryEndpoint = "/product/sellers/{0}/products/price-and-inventory";

    #endregion

    #region View Paths

    /// <summary>
    /// Base view path
    /// </summary>
    public const string ViewBasePath = "~/Plugins/Integration.TrendyolMarketplace/Views";

    /// <summary>
    /// Admin views path
    /// </summary>
    public const string AdminViewPath = ViewBasePath + "/TrendyolAdmin";

    /// <summary>
    /// Vendor views path
    /// </summary>
    public const string VendorViewPath = ViewBasePath + "/TrendyolVendor";

    #endregion

    #region Scheduled Tasks

    /// <summary>
    /// Product sync task name
    /// </summary>
    public const string ProductSyncTaskName = "Trendyol - Product Sync";

    /// <summary>
    /// Stock/Price sync task name
    /// </summary>
    public const string StockPriceSyncTaskName = "Trendyol - Stock/Price Sync";

    /// <summary>
    /// Category/Brand sync task name
    /// </summary>
    public const string CategoryBrandSyncTaskName = "Trendyol - Category/Brand Sync";

    #endregion
}
