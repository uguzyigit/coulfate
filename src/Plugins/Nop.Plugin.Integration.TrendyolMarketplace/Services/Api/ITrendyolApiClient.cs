using Nop.Plugin.Integration.TrendyolMarketplace.Domain;
using Nop.Plugin.Integration.TrendyolMarketplace.Services.Api.Models;

namespace Nop.Plugin.Integration.TrendyolMarketplace.Services.Api;

/// <summary>
/// Trendyol API client interface
/// </summary>
public interface ITrendyolApiClient
{
    /// <summary>
    /// Tests the API connection with given credentials
    /// </summary>
    Task<(bool Success, string Message)> TestConnectionAsync(TrendyolVendorCredential credential);

    /// <summary>
    /// Gets all categories from Trendyol (no auth required)
    /// </summary>
    Task<List<TrendyolCategoryDto>> GetCategoriesAsync();

    /// <summary>
    /// Gets category attributes from Trendyol (no auth required)
    /// </summary>
    Task<TrendyolCategoryAttributesResponse> GetCategoryAttributesAsync(long categoryId);

    /// <summary>
    /// Gets brands from Trendyol (no auth required)
    /// </summary>
    Task<List<TrendyolBrandDto>> GetBrandsAsync(int page = 0, int size = 1000);

    /// <summary>
    /// Gets all brands from Trendyol (paginated, no auth required)
    /// </summary>
    Task<List<TrendyolBrandDto>> GetAllBrandsAsync();

    /// <summary>
    /// Gets products for a vendor
    /// </summary>
    Task<TrendyolProductsResponse> GetProductsAsync(TrendyolVendorCredential credential, int page = 0, int size = 50, bool? approved = null, string barcode = null, bool? onSale = null);

    /// <summary>
    /// Gets all products for a vendor (paginated)
    /// </summary>
    Task<List<TrendyolProductDto>> GetAllProductsAsync(TrendyolVendorCredential credential, bool? approved = null, bool? onSale = null);

    /// <summary>
    /// Gets a single product by barcode
    /// </summary>
    Task<TrendyolProductDto> GetProductByBarcodeAsync(TrendyolVendorCredential credential, string barcode);
}
