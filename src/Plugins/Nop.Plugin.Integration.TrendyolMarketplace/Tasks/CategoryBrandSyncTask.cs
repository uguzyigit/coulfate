using Nop.Plugin.Integration.TrendyolMarketplace.Domain;
using Nop.Plugin.Integration.TrendyolMarketplace.Services;
using Nop.Plugin.Integration.TrendyolMarketplace.Services.Api;
using Nop.Plugin.Integration.TrendyolMarketplace.Services.Mapping;
using Nop.Services.Logging;
using Nop.Services.ScheduleTasks;

namespace Nop.Plugin.Integration.TrendyolMarketplace.Tasks;

/// <summary>
/// Scheduled task for syncing categories and brands from Trendyol (global, no auth required)
/// </summary>
public class CategoryBrandSyncTask : IScheduleTask
{
    private readonly ITrendyolApiClient _apiClient;
    private readonly ICategoryMappingService _categoryMappingService;
    private readonly IBrandMappingService _brandMappingService;
    private readonly ISyncLogService _syncLogService;
    private readonly ILogger _logger;

    public CategoryBrandSyncTask(
        ITrendyolApiClient apiClient,
        ICategoryMappingService categoryMappingService,
        IBrandMappingService brandMappingService,
        ISyncLogService syncLogService,
        ILogger logger)
    {
        _apiClient = apiClient;
        _categoryMappingService = categoryMappingService;
        _brandMappingService = brandMappingService;
        _syncLogService = syncLogService;
        _logger = logger;
    }

    /// <summary>
    /// Executes the category/brand sync task
    /// </summary>
    public async Task ExecuteAsync()
    {
        await _logger.InformationAsync("Trendyol Category/Brand Sync Task started");

        // Sync Categories
        await SyncCategoriesAsync();

        // Sync Brands
        await SyncBrandsAsync();

        await _logger.InformationAsync("Trendyol Category/Brand Sync Task completed");
    }

    private async Task SyncCategoriesAsync()
    {
        // Skip if there's already a running category sync
        if (await _syncLogService.IsSyncRunningAsync(null, SyncType.Category))
        {
            await _logger.WarningAsync("Category sync already running, skipping");
            return;
        }

        var syncLog = await _syncLogService.StartSyncAsync(SyncType.Category, null);

        try
        {
            // Fetch categories from Trendyol
            var categories = await _apiClient.GetCategoriesAsync();

            // Sync to database
            var syncCount = await _categoryMappingService.SyncFromApiAsync(categories);

            // Auto-map categories by exact name match
            var mappedCount = await _categoryMappingService.AutoMapCategoriesAsync();

            await _syncLogService.UpdateProgressAsync(syncLog.Id, categories.Count, syncCount, 0, 0);
            await _syncLogService.CompleteSyncAsync(syncLog.Id, SyncStatus.Completed,
                details: $"Categories synced: {syncCount}, Auto-mapped: {mappedCount}");

            await _logger.InformationAsync(
                $"Trendyol Category Sync completed: {categories.Count} categories fetched, {syncCount} synced, {mappedCount} auto-mapped");
        }
        catch (Exception ex)
        {
            await _syncLogService.CompleteSyncAsync(syncLog.Id, SyncStatus.Failed, ex.Message);
            await _logger.ErrorAsync("Trendyol Category Sync failed", ex);
        }
    }

    private async Task SyncBrandsAsync()
    {
        // Skip if there's already a running brand sync
        if (await _syncLogService.IsSyncRunningAsync(null, SyncType.Brand))
        {
            await _logger.WarningAsync("Brand sync already running, skipping");
            return;
        }

        var syncLog = await _syncLogService.StartSyncAsync(SyncType.Brand, null);

        try
        {
            // Fetch brands from Trendyol
            var brands = await _apiClient.GetAllBrandsAsync();

            // Sync to database
            var syncCount = await _brandMappingService.SyncFromApiAsync(brands);

            // Auto-map brands by exact name match
            var mappedCount = await _brandMappingService.AutoMapBrandsAsync();

            await _syncLogService.UpdateProgressAsync(syncLog.Id, brands.Count, syncCount, 0, 0);
            await _syncLogService.CompleteSyncAsync(syncLog.Id, SyncStatus.Completed,
                details: $"Brands synced: {syncCount}, Auto-mapped: {mappedCount}");

            await _logger.InformationAsync(
                $"Trendyol Brand Sync completed: {brands.Count} brands fetched, {syncCount} synced, {mappedCount} auto-mapped");
        }
        catch (Exception ex)
        {
            await _syncLogService.CompleteSyncAsync(syncLog.Id, SyncStatus.Failed, ex.Message);
            await _logger.ErrorAsync("Trendyol Brand Sync failed", ex);
        }
    }
}
