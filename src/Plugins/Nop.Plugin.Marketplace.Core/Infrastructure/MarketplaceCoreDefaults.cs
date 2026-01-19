using Nop.Core.Caching;

namespace Nop.Plugin.Marketplace.Core.Infrastructure;

/// <summary>
/// Represents default values related to marketplace core
/// </summary>
public static class MarketplaceCoreDefaults
{
    /// <summary>
    /// Cache key prefix for vendor extensions
    /// </summary>
    public static string VendorExtensionPrefix => "Nop.marketplace.vendorextension.";

    /// <summary>
    /// Cache key prefix for vendor products
    /// </summary>
    public static string VendorProductPrefix => "Nop.marketplace.vendorproduct.";

    /// <summary>
    /// Cache key for vendor extension by NopCommerce vendor id
    /// </summary>
    /// <remarks>
    /// {0} : vendor id
    /// </remarks>
    public static CacheKey VendorExtensionByVendorIdCacheKey => new("Nop.marketplace.vendorextension.byvendorid-{0}");

    /// <summary>
    /// Cache key for vendor extension by code
    /// </summary>
    /// <remarks>
    /// {0} : vendor code
    /// </remarks>
    public static CacheKey VendorExtensionByCodeCacheKey => new("Nop.marketplace.vendorextension.bycode-{0}");

    /// <summary>
    /// Cache key for vendor products by product id
    /// </summary>
    /// <remarks>
    /// {0} : product id
    /// </remarks>
    public static CacheKey VendorProductsByProductCacheKey => new("Nop.marketplace.vendorproduct.byproduct-{0}");
}
