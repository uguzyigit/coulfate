namespace Nop.Plugin.Integration.TrendyolMarketplace.Services.Import;

/// <summary>
/// Imports Trendyol categories into NopCommerce category hierarchy
/// </summary>
public interface ICategoryNopImportService
{
    /// <summary>
    /// Creates NopCommerce categories from all synced Trendyol categories, preserving parent-child hierarchy.
    /// Already-mapped categories are skipped (idempotent).
    /// </summary>
    /// <returns>Number of new NopCommerce categories created</returns>
    Task<int> ImportCategoriesAsync();
}
