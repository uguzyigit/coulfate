using LinqToDB.Mapping;
using Nop.Core;

namespace Nop.Plugin.Integration.TrendyolMarketplace.Domain;

/// <summary>
/// Represents sync operation type
/// </summary>
public enum SyncType
{
    /// <summary>
    /// Product sync
    /// </summary>
    Product = 1,

    /// <summary>
    /// Stock sync
    /// </summary>
    Stock = 2,

    /// <summary>
    /// Price sync
    /// </summary>
    Price = 3,

    /// <summary>
    /// Category sync
    /// </summary>
    Category = 4,

    /// <summary>
    /// Brand sync
    /// </summary>
    Brand = 5,

    /// <summary>
    /// Full sync (all types)
    /// </summary>
    Full = 6
}

/// <summary>
/// Represents sync operation status
/// </summary>
public enum SyncStatus
{
    /// <summary>
    /// Currently running
    /// </summary>
    Running = 0,

    /// <summary>
    /// Completed successfully
    /// </summary>
    Completed = 1,

    /// <summary>
    /// Completed with errors
    /// </summary>
    CompletedWithErrors = 2,

    /// <summary>
    /// Failed
    /// </summary>
    Failed = 3,

    /// <summary>
    /// Cancelled
    /// </summary>
    Cancelled = 4
}

/// <summary>
/// Represents a sync operation log entry
/// </summary>
[Table("TrendyolSyncLog")]
public class TrendyolSyncLog : BaseEntity
{
    /// <summary>
    /// Gets or sets the sync type
    /// </summary>
    [Column]
    public SyncType SyncType { get; set; }

    /// <summary>
    /// Gets or sets the vendor ID (null for global syncs like category/brand)
    /// </summary>
    [Column]
    public int? VendorId { get; set; }

    /// <summary>
    /// Gets or sets the start time
    /// </summary>
    [Column]
    public DateTime StartedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the completion time
    /// </summary>
    [Column]
    public DateTime? CompletedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the total number of items processed
    /// </summary>
    [Column]
    public int TotalItems { get; set; }

    /// <summary>
    /// Gets or sets the number of successfully processed items
    /// </summary>
    [Column]
    public int SuccessCount { get; set; }

    /// <summary>
    /// Gets or sets the number of failed items
    /// </summary>
    [Column]
    public int FailedCount { get; set; }

    /// <summary>
    /// Gets or sets the number of skipped items
    /// </summary>
    [Column]
    public int SkippedCount { get; set; }

    /// <summary>
    /// Gets or sets the sync status
    /// </summary>
    [Column]
    public SyncStatus Status { get; set; } = SyncStatus.Running;

    /// <summary>
    /// Gets or sets the error message (if failed)
    /// </summary>
    [Column]
    public string ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the detailed log (JSON format)
    /// </summary>
    [Column]
    public string Details { get; set; }
}
