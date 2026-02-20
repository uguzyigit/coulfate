using Nop.Plugin.Integration.TrendyolMarketplace.Services.Api.Models;

namespace Nop.Plugin.Integration.TrendyolMarketplace.Services.Import;

/// <summary>
/// Variant service interface for handling product attribute combinations
/// </summary>
public interface IVariantService
{
    /// <summary>
    /// Creates product attribute combinations from Trendyol variants
    /// </summary>
    Task CreateVariantsAsync(int productId, List<TrendyolProductDto> variants);

    /// <summary>
    /// Updates existing product attribute combinations
    /// </summary>
    Task UpdateVariantsAsync(int productId, List<TrendyolProductDto> variants);

    /// <summary>
    /// Updates stock and price for a specific combination by SKU
    /// </summary>
    Task UpdateCombinationAsync(int productId, string sku, int stock, decimal price);
}
