using Nop.Core;
using Nop.Plugin.Integration.TrendyolMarketplace.Domain;
using Nop.Plugin.Integration.TrendyolMarketplace.Services.Api.Models;

namespace Nop.Plugin.Integration.TrendyolMarketplace.Services.Mapping;

/// <summary>
/// Brand mapping service interface
/// </summary>
public interface IBrandMappingService
{
    /// <summary>
    /// Gets a brand mapping by ID
    /// </summary>
    Task<TrendyolBrand> GetByIdAsync(int id);

    /// <summary>
    /// Gets a brand mapping by Trendyol brand ID
    /// </summary>
    Task<TrendyolBrand> GetByTrendyolIdAsync(long trendyolBrandId);

    /// <summary>
    /// Gets all brand mappings (paged)
    /// </summary>
    Task<IPagedList<TrendyolBrand>> GetAllAsync(
        string searchTerm = null,
        bool? isMapped = null,
        int pageIndex = 0,
        int pageSize = int.MaxValue);

    /// <summary>
    /// Gets all mapped brands
    /// </summary>
    Task<IList<TrendyolBrand>> GetAllMappedAsync();

    /// <summary>
    /// Inserts a brand mapping
    /// </summary>
    Task InsertAsync(TrendyolBrand brand);

    /// <summary>
    /// Updates a brand mapping
    /// </summary>
    Task UpdateAsync(TrendyolBrand brand);

    /// <summary>
    /// Deletes a brand mapping
    /// </summary>
    Task DeleteAsync(TrendyolBrand brand);

    /// <summary>
    /// Syncs brands from Trendyol API
    /// </summary>
    Task<int> SyncFromApiAsync(List<TrendyolBrandDto> brands);

    /// <summary>
    /// Auto-maps brands by exact name match
    /// </summary>
    Task<int> AutoMapBrandsAsync();

    /// <summary>
    /// Gets the NopCommerce manufacturer ID for a Trendyol brand
    /// Returns null if not mapped, creates manufacturer if auto-create is enabled
    /// </summary>
    Task<int?> GetOrCreateNopManufacturerIdAsync(long trendyolBrandId, string brandName);
}
