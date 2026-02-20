using Nop.Core;
using Nop.Plugin.Integration.TrendyolMarketplace.Domain;

namespace Nop.Plugin.Integration.TrendyolMarketplace.Services;

/// <summary>
/// Sync log service interface
/// </summary>
public interface ISyncLogService
{
    /// <summary>
    /// Gets a sync log by ID
    /// </summary>
    Task<TrendyolSyncLog> GetByIdAsync(int id);

    /// <summary>
    /// Gets all sync logs (paged)
    /// </summary>
    Task<IPagedList<TrendyolSyncLog>> GetAllAsync(
        int? vendorId = null,
        SyncType? syncType = null,
        SyncStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int pageIndex = 0,
        int pageSize = int.MaxValue);

    /// <summary>
    /// Gets the latest sync log for a vendor
    /// </summary>
    Task<TrendyolSyncLog> GetLatestAsync(int? vendorId = null, SyncType? syncType = null);

    /// <summary>
    /// Checks if there is a running sync for a vendor
    /// </summary>
    Task<bool> IsSyncRunningAsync(int? vendorId = null, SyncType? syncType = null);

    /// <summary>
    /// Creates a new sync log entry (starts a sync)
    /// </summary>
    Task<TrendyolSyncLog> StartSyncAsync(SyncType syncType, int? vendorId = null);

    /// <summary>
    /// Updates sync progress
    /// </summary>
    Task UpdateProgressAsync(int logId, int totalItems, int successCount, int failedCount, int skippedCount);

    /// <summary>
    /// Completes a sync operation
    /// </summary>
    Task CompleteSyncAsync(int logId, SyncStatus status, string errorMessage = null, string details = null);

    /// <summary>
    /// Inserts a sync log
    /// </summary>
    Task InsertAsync(TrendyolSyncLog syncLog);

    /// <summary>
    /// Updates a sync log
    /// </summary>
    Task UpdateAsync(TrendyolSyncLog syncLog);

    /// <summary>
    /// Deletes old sync logs
    /// </summary>
    Task DeleteOldLogsAsync(int daysToKeep = 30);
}
