using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Plugin.Integration.TrendyolMarketplace.Domain;
using Nop.Plugin.Integration.TrendyolMarketplace.Infrastructure;
using Nop.Plugin.Integration.TrendyolMarketplace.Services.Api.Models;
using Nop.Services.Catalog;

namespace Nop.Plugin.Integration.TrendyolMarketplace.Services.Mapping;

/// <summary>
/// Category mapping service implementation
/// </summary>
public class CategoryMappingService : ICategoryMappingService
{
    private readonly IRepository<TrendyolCategory> _categoryRepository;
    private readonly IStaticCacheManager _staticCacheManager;
    private readonly ICategoryService _nopCategoryService;

    public CategoryMappingService(
        IRepository<TrendyolCategory> categoryRepository,
        IStaticCacheManager staticCacheManager,
        ICategoryService nopCategoryService)
    {
        _categoryRepository = categoryRepository;
        _staticCacheManager = staticCacheManager;
        _nopCategoryService = nopCategoryService;
    }

    /// <summary>
    /// Gets a category mapping by ID
    /// </summary>
    public virtual async Task<TrendyolCategory> GetByIdAsync(int id)
    {
        if (id <= 0)
            return null;

        return await _categoryRepository.GetByIdAsync(id);
    }

    /// <summary>
    /// Gets a category mapping by Trendyol category ID
    /// </summary>
    public virtual async Task<TrendyolCategory> GetByTrendyolIdAsync(long trendyolCategoryId)
    {
        var key = _staticCacheManager.PrepareKeyForDefaultCache(
            TrendyolDefaults.CategoryByTrendyolIdCacheKey, trendyolCategoryId);

        return await _staticCacheManager.GetAsync(key, async () =>
        {
            var query = from c in _categoryRepository.Table
                        where c.TrendyolCategoryId == trendyolCategoryId
                        select c;

            return await query.FirstOrDefaultAsync();
        });
    }

    /// <summary>
    /// Gets all category mappings (paged)
    /// </summary>
    public virtual async Task<IPagedList<TrendyolCategory>> GetAllAsync(
        string searchTerm = null,
        bool? isMapped = null,
        bool? isLeaf = null,
        int pageIndex = 0,
        int pageSize = int.MaxValue)
    {
        var query = _categoryRepository.Table;

        if (!string.IsNullOrEmpty(searchTerm))
        {
            searchTerm = searchTerm.ToLower();
            query = query.Where(c => c.TrendyolCategoryName.ToLower().Contains(searchTerm) ||
                                     c.TrendyolCategoryPath.ToLower().Contains(searchTerm));
        }

        if (isMapped.HasValue)
        {
            if (isMapped.Value)
                query = query.Where(c => c.NopCategoryId.HasValue && c.NopCategoryId.Value > 0);
            else
                query = query.Where(c => !c.NopCategoryId.HasValue || c.NopCategoryId.Value <= 0);
        }

        if (isLeaf.HasValue)
            query = query.Where(c => c.IsLeaf == isLeaf.Value);

        query = query.OrderBy(c => c.TrendyolCategoryPath);

        return await query.ToPagedListAsync(pageIndex, pageSize);
    }

    /// <summary>
    /// Gets all mapped categories
    /// </summary>
    public virtual async Task<IList<TrendyolCategory>> GetAllMappedAsync()
    {
        var query = from c in _categoryRepository.Table
                    where c.NopCategoryId.HasValue && c.NopCategoryId.Value > 0
                    select c;

        return await query.ToListAsync();
    }

    /// <summary>
    /// Inserts a category mapping
    /// </summary>
    public virtual async Task InsertAsync(TrendyolCategory category)
    {
        ArgumentNullException.ThrowIfNull(category);

        category.CreatedOnUtc = DateTime.UtcNow;
        category.UpdatedOnUtc = DateTime.UtcNow;

        await _categoryRepository.InsertAsync(category);
        await _staticCacheManager.RemoveByPrefixAsync(TrendyolDefaults.CategoryPrefix);
    }

    /// <summary>
    /// Updates a category mapping
    /// </summary>
    public virtual async Task UpdateAsync(TrendyolCategory category)
    {
        ArgumentNullException.ThrowIfNull(category);

        category.UpdatedOnUtc = DateTime.UtcNow;

        await _categoryRepository.UpdateAsync(category);
        await _staticCacheManager.RemoveByPrefixAsync(TrendyolDefaults.CategoryPrefix);
    }

    /// <summary>
    /// Deletes a category mapping
    /// </summary>
    public virtual async Task DeleteAsync(TrendyolCategory category)
    {
        ArgumentNullException.ThrowIfNull(category);

        await _categoryRepository.DeleteAsync(category);
        await _staticCacheManager.RemoveByPrefixAsync(TrendyolDefaults.CategoryPrefix);
    }

    /// <summary>
    /// Syncs categories from Trendyol API
    /// </summary>
    public virtual async Task<int> SyncFromApiAsync(List<TrendyolCategoryDto> categories)
    {
        var syncCount = 0;
        var processedCategories = new Dictionary<long, (TrendyolCategoryDto Category, string Path, bool IsLeaf)>();

        // Build category paths and determine leaf status
        BuildCategoryPaths(categories, processedCategories, null, "");

        foreach (var item in processedCategories.Values)
        {
            var existing = await GetByTrendyolIdAsync(item.Category.Id);
            if (existing == null)
            {
                // Insert new category
                var newCategory = new TrendyolCategory
                {
                    TrendyolCategoryId = item.Category.Id,
                    TrendyolCategoryName = item.Category.Name,
                    TrendyolParentId = item.Category.ParentId,
                    TrendyolCategoryPath = item.Path,
                    IsLeaf = item.IsLeaf,
                    IsAutoMapped = false,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow
                };

                await _categoryRepository.InsertAsync(newCategory);
                syncCount++;
            }
            else
            {
                // Update existing category
                var needsUpdate = false;

                if (existing.TrendyolCategoryName != item.Category.Name)
                {
                    existing.TrendyolCategoryName = item.Category.Name;
                    needsUpdate = true;
                }

                if (existing.TrendyolCategoryPath != item.Path)
                {
                    existing.TrendyolCategoryPath = item.Path;
                    needsUpdate = true;
                }

                if (existing.IsLeaf != item.IsLeaf)
                {
                    existing.IsLeaf = item.IsLeaf;
                    needsUpdate = true;
                }

                if (needsUpdate)
                {
                    existing.UpdatedOnUtc = DateTime.UtcNow;
                    await _categoryRepository.UpdateAsync(existing);
                    syncCount++;
                }
            }
        }

        await _staticCacheManager.RemoveByPrefixAsync(TrendyolDefaults.CategoryPrefix);
        return syncCount;
    }

    /// <summary>
    /// Auto-maps categories by exact name match
    /// </summary>
    public virtual async Task<int> AutoMapCategoriesAsync()
    {
        var mappedCount = 0;

        // Get all NopCommerce categories
        var nopCategories = await _nopCategoryService.GetAllCategoriesAsync(showHidden: true);
        var nopCategoryDict = nopCategories.ToDictionary(c => c.Name.ToLower().Trim(), c => c.Id);

        // Get unmapped Trendyol categories
        var unmappedCategories = await GetAllAsync(isMapped: false);

        foreach (var trendyolCategory in unmappedCategories)
        {
            var searchName = trendyolCategory.TrendyolCategoryName?.ToLower().Trim();
            if (string.IsNullOrEmpty(searchName))
                continue;

            if (nopCategoryDict.TryGetValue(searchName, out var nopCategoryId))
            {
                trendyolCategory.NopCategoryId = nopCategoryId;
                trendyolCategory.IsAutoMapped = true;
                trendyolCategory.UpdatedOnUtc = DateTime.UtcNow;
                await _categoryRepository.UpdateAsync(trendyolCategory);
                mappedCount++;
            }
        }

        await _staticCacheManager.RemoveByPrefixAsync(TrendyolDefaults.CategoryPrefix);
        return mappedCount;
    }

    /// <summary>
    /// Gets the NopCommerce category ID for a Trendyol category
    /// </summary>
    public virtual async Task<int?> GetNopCategoryIdAsync(long trendyolCategoryId)
    {
        var category = await GetByTrendyolIdAsync(trendyolCategoryId);
        return category?.NopCategoryId;
    }

    #region Private Methods

    private void BuildCategoryPaths(
        List<TrendyolCategoryDto> categories,
        Dictionary<long, (TrendyolCategoryDto Category, string Path, bool IsLeaf)> result,
        long? parentId,
        string parentPath)
    {
        foreach (var category in categories)
        {
            var path = string.IsNullOrEmpty(parentPath) ? category.Name : $"{parentPath} > {category.Name}";
            var isLeaf = category.SubCategories == null || !category.SubCategories.Any();

            result[category.Id] = (category, path, isLeaf);

            if (!isLeaf)
            {
                BuildCategoryPaths(category.SubCategories, result, category.Id, path);
            }
        }
    }

    #endregion
}
