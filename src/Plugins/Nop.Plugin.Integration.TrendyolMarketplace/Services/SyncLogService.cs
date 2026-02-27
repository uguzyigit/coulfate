using Nop.Core;
using Nop.Data;
using Nop.Plugin.Integration.TrendyolMarketplace.Domain;

namespace Nop.Plugin.Integration.TrendyolMarketplace.Services;

/// <summary>
/// Sync log service implementation
/// </summary>
public class SyncLogService : ISyncLogService
{
    private readonly IRepository<TrendyolSyncLog> _syncLogRepository;

    public SyncLogService(IRepository<TrendyolSyncLog> syncLogRepository)
    {
        _syncLogRepository = syncLogRepository;
    }

    /// <summary>
    /// Gets a sync log by ID
    /// </summary>
    public virtual async Task<TrendyolSyncLog> GetByIdAsync(int id)
    {
        if (id <= 0)
            return null;

        return await _syncLogRepository.GetByIdAsync(id);
    }

    /// <summary>
    /// Gets all sync logs (paged)
    /// </summary>
    public virtual async Task<IPagedList<TrendyolSyncLog>> GetAllAsync(
        int? vendorId = null,
        SyncType? syncType = null,
        SyncStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int pageIndex = 0,
        int pageSize = int.MaxValue)
    {
        var query = _syncLogRepository.Table;

        if (vendorId.HasValue)
            query = query.Where(sl => sl.VendorId == vendorId.Value);

        if (syncType.HasValue)
            query = query.Where(sl => sl.SyncType == syncType.Value);

        if (status.HasValue)
            query = query.Where(sl => sl.Status == status.Value);

        if (fromDate.HasValue)
            query = query.Where(sl => sl.StartedOnUtc >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(sl => sl.StartedOnUtc <= toDate.Value);

        query = query.OrderByDescending(sl => sl.StartedOnUtc);

        return await query.ToPagedListAsync(pageIndex, pageSize);
    }

    /// <summary>
    /// Gets the latest sync log for a vendor
    /// </summary>
    public virtual async Task<TrendyolSyncLog> GetLatestAsync(int? vendorId = null, SyncType? syncType = null)
    {
        var query = _syncLogRepository.Table;

        if (vendorId.HasValue)
            query = query.Where(sl => sl.VendorId == vendorId.Value);

        if (syncType.HasValue)
            query = query.Where(sl => sl.SyncType == syncType.Value);

        query = query.OrderByDescending(sl => sl.StartedOnUtc);

        return await query.FirstOrDefaultAsync();
    }

    /// <summary>
    /// Checks if there is a running sync for a vendor.
    /// Auto-expires syncs stuck in Running state for more than 30 minutes.
    /// </summary>
    public virtual async Task<bool> IsSyncRunningAsync(int? vendorId = null, SyncType? syncType = null)
    {
        var query = _syncLogRepository.Table.Where(sl => sl.Status == SyncStatus.Running);

        if (vendorId.HasValue)
            query = query.Where(sl => sl.VendorId == vendorId.Value);

        if (syncType.HasValue)
            query = query.Where(sl => sl.SyncType == syncType.Value);

        var stuckSyncs = await query.ToListAsync();

        if (!stuckSyncs.Any())
            return false;

        // 30 dakikadan uzun süre Running olan sync'leri otomatik Failed yap
        var cutoff = DateTime.UtcNow.AddMinutes(-30);
        var hasActive = false;

        foreach (var sync in stuckSyncs)
        {
            if (sync.StartedOnUtc < cutoff)
            {
                sync.Status = SyncStatus.Failed;
                sync.CompletedOnUtc = DateTime.UtcNow;
                sync.ErrorMessage = "Timeout: 30 dakikadan uzun süre Running durumunda kaldı";
                await _syncLogRepository.UpdateAsync(sync);
            }
            else
            {
                hasActive = true;
            }
        }

        return hasActive;
    }

    /// <summary>
    /// Creates a new sync log entry (starts a sync)
    /// </summary>
    public virtual async Task<TrendyolSyncLog> StartSyncAsync(SyncType syncType, int? vendorId = null)
    {
        var syncLog = new TrendyolSyncLog
        {
            SyncType = syncType,
            VendorId = vendorId,
            StartedOnUtc = DateTime.UtcNow,
            Status = SyncStatus.Running,
            TotalItems = 0,
            SuccessCount = 0,
            FailedCount = 0,
            SkippedCount = 0
        };

        await _syncLogRepository.InsertAsync(syncLog);
        return syncLog;
    }

    /// <summary>
    /// Updates sync progress
    /// </summary>
    public virtual async Task UpdateProgressAsync(int logId, int totalItems, int successCount, int failedCount, int skippedCount)
    {
        var syncLog = await GetByIdAsync(logId);
        if (syncLog == null)
            return;

        syncLog.TotalItems = totalItems;
        syncLog.SuccessCount = successCount;
        syncLog.FailedCount = failedCount;
        syncLog.SkippedCount = skippedCount;

        await _syncLogRepository.UpdateAsync(syncLog);
    }

    /// <summary>
    /// Completes a sync operation
    /// </summary>
    public virtual async Task CompleteSyncAsync(int logId, SyncStatus status, string errorMessage = null, string details = null)
    {
        var syncLog = await GetByIdAsync(logId);
        if (syncLog == null)
            return;

        syncLog.Status = status;
        syncLog.CompletedOnUtc = DateTime.UtcNow;
        syncLog.ErrorMessage = errorMessage;
        syncLog.Details = details;

        await _syncLogRepository.UpdateAsync(syncLog);
    }

    /// <summary>
    /// Inserts a sync log
    /// </summary>
    public virtual async Task InsertAsync(TrendyolSyncLog syncLog)
    {
        ArgumentNullException.ThrowIfNull(syncLog);
        await _syncLogRepository.InsertAsync(syncLog);
    }

    /// <summary>
    /// Updates a sync log
    /// </summary>
    public virtual async Task UpdateAsync(TrendyolSyncLog syncLog)
    {
        ArgumentNullException.ThrowIfNull(syncLog);
        await _syncLogRepository.UpdateAsync(syncLog);
    }

    /// <summary>
    /// Deletes old sync logs
    /// </summary>
    public virtual async Task DeleteOldLogsAsync(int daysToKeep = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);

        var query = _syncLogRepository.Table
            .Where(sl => sl.StartedOnUtc < cutoffDate && sl.Status != SyncStatus.Running);

        var oldLogs = await query.ToListAsync();
        await _syncLogRepository.DeleteAsync(oldLogs);
    }
}
