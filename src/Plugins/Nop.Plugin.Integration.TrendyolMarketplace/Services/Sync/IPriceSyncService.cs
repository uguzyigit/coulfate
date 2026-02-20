using Nop.Plugin.Integration.TrendyolMarketplace.Domain;

namespace Nop.Plugin.Integration.TrendyolMarketplace.Services.Sync;

/// <summary>
/// Price synchronization service interface
/// </summary>
public interface IPriceSyncService
{
    /// <summary>
    /// Syncs prices for all products of a vendor from Trendyol
    /// </summary>
    Task<(int Success, int Failed)> SyncPricesAsync(TrendyolVendorCredential credential, int syncLogId);

    /// <summary>
    /// Syncs price for a single product
    /// </summary>
    Task<bool> SyncProductPriceAsync(TrendyolProduct trendyolProduct, decimal salePrice, decimal listPrice);
}
