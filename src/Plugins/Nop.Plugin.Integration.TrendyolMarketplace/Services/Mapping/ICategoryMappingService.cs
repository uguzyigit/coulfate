using Nop.Core;
using Nop.Plugin.Integration.TrendyolMarketplace.Domain;
using Nop.Plugin.Integration.TrendyolMarketplace.Services.Api.Models;

namespace Nop.Plugin.Integration.TrendyolMarketplace.Services.Mapping;

/// <summary>
/// Category mapping service interface
/// </summary>
public interface ICategoryMappingService
{
    /// <summary>
    /// Gets a category mapping by ID
    /// </summary>
    Task<TrendyolCategory> GetByIdAsync(int id);

    /// <summary>
    /// Gets a category mapping by Trendyol category ID
    /// </summary>
    Task<TrendyolCategory> GetByTrendyolIdAsync(long trendyolCategoryId);

    /// <summary>
    /// Gets all category mappings (paged)
    /// </summary>
    Task<IPagedList<TrendyolCategory>> GetAllAsync(
        string searchTerm = null,
        bool? isMapped = null,
        bool? isLeaf = null,
        int pageIndex = 0,
        int pageSize = int.MaxValue);

    /// <summary>
    /// Gets all mapped categories
    /// </summary>
    Task<IList<TrendyolCategory>> GetAllMappedAsync();

    /// <summary>
    /// Inserts a category mapping
    /// </summary>
    Task InsertAsync(TrendyolCategory category);

    /// <summary>
    /// Updates a category mapping
    /// </summary>
    Task UpdateAsync(TrendyolCategory category);

    /// <summary>
    /// Deletes a category mapping
    /// </summary>
    Task DeleteAsync(TrendyolCategory category);

    /// <summary>
    /// Syncs categories from Trendyol API
    /// </summary>
    Task<int> SyncFromApiAsync(List<TrendyolCategoryDto> categories);

    /// <summary>
    /// Auto-maps categories by exact name match
    /// </summary>
    Task<int> AutoMapCategoriesAsync();

    /// <summary>
    /// Gets the NopCommerce category ID for a Trendyol category
    /// </summary>
    Task<int?> GetNopCategoryIdAsync(long trendyolCategoryId);
}
