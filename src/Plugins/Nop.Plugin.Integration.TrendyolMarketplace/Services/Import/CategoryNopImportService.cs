using Nop.Core.Domain.Catalog;
using Nop.Plugin.Integration.TrendyolMarketplace.Domain;
using Nop.Plugin.Integration.TrendyolMarketplace.Services.Mapping;
using Nop.Services.Catalog;

namespace Nop.Plugin.Integration.TrendyolMarketplace.Services.Import;

/// <summary>
/// Imports Trendyol categories into NopCommerce category hierarchy
/// </summary>
public class CategoryNopImportService : ICategoryNopImportService
{
    private readonly ICategoryMappingService _categoryMappingService;
    private readonly ICategoryService _categoryService;

    public CategoryNopImportService(
        ICategoryMappingService categoryMappingService,
        ICategoryService categoryService)
    {
        _categoryMappingService = categoryMappingService;
        _categoryService = categoryService;
    }

    public async Task<int> ImportCategoriesAsync()
    {
        // Load all TrendyolCategory records
        var allCategories = (await _categoryMappingService.GetAllAsync()).ToList();

        // Build dictionary: TrendyolCategoryId -> TrendyolCategory
        var dict = allCategories.ToDictionary(c => c.TrendyolCategoryId, c => c);

        // Find root categories (no Trendyol parent)
        var roots = allCategories.Where(c => c.TrendyolParentId == null).ToList();

        var counter = new ImportCounter();
        await ProcessCategoriesAsync(roots, dict, 0, counter);

        return counter.CreatedCount;
    }

    private async Task ProcessCategoriesAsync(
        IList<TrendyolCategory> categories,
        Dictionary<long, TrendyolCategory> dict,
        int parentNopCategoryId,
        ImportCounter counter)
    {
        foreach (var trendyolCategory in categories)
        {
            int currentNopCategoryId;

            if (trendyolCategory.NopCategoryId.HasValue && trendyolCategory.NopCategoryId.Value > 0)
            {
                // Already mapped — use existing NopCommerce category for child processing
                currentNopCategoryId = trendyolCategory.NopCategoryId.Value;
            }
            else
            {
                // Create NopCommerce category
                var nopCategory = new Category
                {
                    Name = trendyolCategory.TrendyolCategoryName,
                    ParentCategoryId = parentNopCategoryId,
                    Published = true,
                    DisplayOrder = ++counter.DisplayOrder,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow
                };

                await _categoryService.InsertCategoryAsync(nopCategory);
                counter.CreatedCount++;

                trendyolCategory.NopCategoryId = nopCategory.Id;
                trendyolCategory.IsAutoMapped = true;
                await _categoryMappingService.UpdateAsync(trendyolCategory);

                currentNopCategoryId = nopCategory.Id;
            }

            // Recurse into children
            var children = dict.Values
                .Where(c => c.TrendyolParentId == trendyolCategory.TrendyolCategoryId)
                .ToList();

            if (children.Any())
                await ProcessCategoriesAsync(children, dict, currentNopCategoryId, counter);
        }
    }

    private sealed class ImportCounter
    {
        public int CreatedCount;
        public int DisplayOrder;
    }
}
