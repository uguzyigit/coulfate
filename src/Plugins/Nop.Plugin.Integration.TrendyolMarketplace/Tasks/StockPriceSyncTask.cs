using Nop.Plugin.Integration.TrendyolMarketplace.Domain;
using Nop.Plugin.Integration.TrendyolMarketplace.Services;
using Nop.Plugin.Integration.TrendyolMarketplace.Services.Sync;
using Nop.Services.Logging;
using Nop.Services.ScheduleTasks;

namespace Nop.Plugin.Integration.TrendyolMarketplace.Tasks;

/// <summary>
/// Scheduled task for syncing stock and prices from Trendyol for all active vendors
/// </summary>
public class StockPriceSyncTask : IScheduleTask
{
    private readonly IVendorCredentialService _vendorCredentialService;
    private readonly IStockSyncService _stockSyncService;
    private readonly IPriceSyncService _priceSyncService;
    private readonly ISyncLogService _syncLogService;
    private readonly ILogger _logger;

    public StockPriceSyncTask(
        IVendorCredentialService vendorCredentialService,
        IStockSyncService stockSyncService,
        IPriceSyncService priceSyncService,
        ISyncLogService syncLogService,
        ILogger logger)
    {
        _vendorCredentialService = vendorCredentialService;
        _stockSyncService = stockSyncService;
        _priceSyncService = priceSyncService;
        _syncLogService = syncLogService;
        _logger = logger;
    }

    /// <summary>
    /// Executes the stock/price sync task
    /// </summary>
    public async Task ExecuteAsync()
    {
        await _logger.InformationAsync("Trendyol Stock/Price Sync Task started");

        try
        {
            // Get all active vendors
            var credentials = await _vendorCredentialService.GetAllActiveAsync();

            foreach (var credential in credentials)
            {
                // Stock Sync
                if (!await _syncLogService.IsSyncRunningAsync(credential.VendorId, SyncType.Stock))
                {
                    var stockSyncLog = await _syncLogService.StartSyncAsync(SyncType.Stock, credential.VendorId);

                    try
                    {
                        var (stockSuccess, stockFailed) = await _stockSyncService.SyncStockAsync(credential, stockSyncLog.Id);

                        var stockStatus = stockFailed > 0 ? SyncStatus.CompletedWithErrors : SyncStatus.Completed;
                        await _syncLogService.CompleteSyncAsync(stockSyncLog.Id, stockStatus);

                        await _logger.InformationAsync(
                            $"Trendyol Stock Sync completed for vendor {credential.VendorId}: " +
                            $"{stockSuccess} success, {stockFailed} failed");
                    }
                    catch (Exception ex)
                    {
                        await _syncLogService.CompleteSyncAsync(stockSyncLog.Id, SyncStatus.Failed, ex.Message);
                        await _logger.ErrorAsync($"Trendyol Stock Sync failed for vendor {credential.VendorId}", ex);
                    }
                }

                // Price Sync
                if (!await _syncLogService.IsSyncRunningAsync(credential.VendorId, SyncType.Price))
                {
                    var priceSyncLog = await _syncLogService.StartSyncAsync(SyncType.Price, credential.VendorId);

                    try
                    {
                        var (priceSuccess, priceFailed) = await _priceSyncService.SyncPricesAsync(credential, priceSyncLog.Id);

                        var priceStatus = priceFailed > 0 ? SyncStatus.CompletedWithErrors : SyncStatus.Completed;
                        await _syncLogService.CompleteSyncAsync(priceSyncLog.Id, priceStatus);

                        await _logger.InformationAsync(
                            $"Trendyol Price Sync completed for vendor {credential.VendorId}: " +
                            $"{priceSuccess} success, {priceFailed} failed");
                    }
                    catch (Exception ex)
                    {
                        await _syncLogService.CompleteSyncAsync(priceSyncLog.Id, SyncStatus.Failed, ex.Message);
                        await _logger.ErrorAsync($"Trendyol Price Sync failed for vendor {credential.VendorId}", ex);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync("Trendyol Stock/Price Sync Task failed", ex);
        }

        await _logger.InformationAsync("Trendyol Stock/Price Sync Task completed");
    }
}
