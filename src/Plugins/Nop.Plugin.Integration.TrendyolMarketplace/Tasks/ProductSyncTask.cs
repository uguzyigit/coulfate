using Nop.Plugin.Integration.TrendyolMarketplace.Domain;
using Nop.Plugin.Integration.TrendyolMarketplace.Services;
using Nop.Plugin.Integration.TrendyolMarketplace.Services.Import;
using Nop.Services.Logging;
using Nop.Services.ScheduleTasks;

namespace Nop.Plugin.Integration.TrendyolMarketplace.Tasks;

/// <summary>
/// Scheduled task for syncing products from Trendyol for all active vendors
/// </summary>
public class ProductSyncTask : IScheduleTask
{
    private readonly IVendorCredentialService _vendorCredentialService;
    private readonly IProductImportService _productImportService;
    private readonly ISyncLogService _syncLogService;
    private readonly ILogger _logger;

    public ProductSyncTask(
        IVendorCredentialService vendorCredentialService,
        IProductImportService productImportService,
        ISyncLogService syncLogService,
        ILogger logger)
    {
        _vendorCredentialService = vendorCredentialService;
        _productImportService = productImportService;
        _syncLogService = syncLogService;
        _logger = logger;
    }

    /// <summary>
    /// Executes the product sync task
    /// </summary>
    public async Task ExecuteAsync()
    {
        await _logger.InformationAsync("Trendyol Product Sync Task started");

        try
        {
            // Get all vendors that need sync
            var credentials = await _vendorCredentialService.GetCredentialsForSyncAsync();

            foreach (var credential in credentials)
            {
                // Skip if there's already a running sync for this vendor
                if (await _syncLogService.IsSyncRunningAsync(credential.VendorId, SyncType.Product))
                {
                    await _logger.WarningAsync($"Product sync already running for vendor {credential.VendorId}, skipping");
                    continue;
                }

                var syncLog = await _syncLogService.StartSyncAsync(SyncType.Product, credential.VendorId);

                try
                {
                    var (success, failed, skipped) = await _productImportService.ImportProductsAsync(credential, syncLog.Id);

                    var status = failed > 0 ? SyncStatus.CompletedWithErrors : SyncStatus.Completed;
                    await _syncLogService.CompleteSyncAsync(syncLog.Id, status);

                    // Update last sync time
                    await _vendorCredentialService.UpdateLastSyncTimeAsync(credential.VendorId);

                    await _logger.InformationAsync(
                        $"Trendyol Product Sync completed for vendor {credential.VendorId}: " +
                        $"{success} success, {failed} failed, {skipped} skipped");
                }
                catch (Exception ex)
                {
                    await _syncLogService.CompleteSyncAsync(syncLog.Id, SyncStatus.Failed, ex.Message);
                    await _logger.ErrorAsync($"Trendyol Product Sync failed for vendor {credential.VendorId}", ex);
                }
            }
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync("Trendyol Product Sync Task failed", ex);
        }

        await _logger.InformationAsync("Trendyol Product Sync Task completed");
    }
}
