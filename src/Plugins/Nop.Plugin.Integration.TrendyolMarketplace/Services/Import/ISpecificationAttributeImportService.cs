using Nop.Plugin.Integration.TrendyolMarketplace.Services.Api.Models;

namespace Nop.Plugin.Integration.TrendyolMarketplace.Services.Import;

/// <summary>
/// Service for importing Trendyol product attributes as NopCommerce specification attributes
/// </summary>
public interface ISpecificationAttributeImportService
{
    /// <summary>
    /// Assigns specification attributes from a Trendyol product DTO to a NopCommerce product.
    /// Skips variant attributes (Renk, Beden, etc.) that are handled by VariantService.
    /// Idempotent: will not create duplicates if called multiple times for the same product.
    /// </summary>
    /// <param name="productId">NopCommerce product identifier</param>
    /// <param name="productDto">Trendyol product DTO containing attributes</param>
    /// <returns>Number of specification attributes assigned</returns>
    Task<int> AssignSpecificationAttributesAsync(int productId, TrendyolProductDto productDto);
}
