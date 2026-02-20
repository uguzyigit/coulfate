using Nop.Plugin.Integration.TrendyolMarketplace.Domain;

namespace Nop.Plugin.Integration.TrendyolMarketplace.Services.Sync;

/// <summary>
/// Stock synchronization service interface
/// </summary>
public interface IStockSyncService
{
    /// <summary>
    /// Syncs stock for all products of a vendor from Trendyol
    /// </summary>
    Task<(int Success, int Failed)> SyncStockAsync(TrendyolVendorCredential credential, int syncLogId);

    /// <summary>
    /// Syncs stock for a single product
    /// </summary>
    Task<bool> SyncProductStockAsync(TrendyolProduct trendyolProduct, int newStock);
}
