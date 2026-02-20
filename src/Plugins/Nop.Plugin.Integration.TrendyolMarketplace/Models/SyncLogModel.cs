using Nop.Plugin.Integration.TrendyolMarketplace.Domain;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Integration.TrendyolMarketplace.Models;

/// <summary>
/// Represents the sync log model
/// </summary>
public record SyncLogModel : BaseNopEntityModel
{
    [NopResourceDisplayName("Plugins.Integration.TrendyolMarketplace.SyncLogs.SyncType")]
    public SyncType SyncType { get; set; }

    public string SyncTypeDisplay { get; set; }

    public int? VendorId { get; set; }

    public string VendorName { get; set; }

    [NopResourceDisplayName("Plugins.Integration.TrendyolMarketplace.SyncLogs.StartedOnUtc")]
    public DateTime StartedOnUtc { get; set; }

    public string StartedOnDisplay { get; set; }

    [NopResourceDisplayName("Plugins.Integration.TrendyolMarketplace.SyncLogs.CompletedOnUtc")]
    public DateTime? CompletedOnUtc { get; set; }

    public string CompletedOnDisplay { get; set; }

    public string Duration { get; set; }

    [NopResourceDisplayName("Plugins.Integration.TrendyolMarketplace.SyncLogs.TotalItems")]
    public int TotalItems { get; set; }

    [NopResourceDisplayName("Plugins.Integration.TrendyolMarketplace.SyncLogs.SuccessCount")]
    public int SuccessCount { get; set; }

    [NopResourceDisplayName("Plugins.Integration.TrendyolMarketplace.SyncLogs.FailedCount")]
    public int FailedCount { get; set; }

    [NopResourceDisplayName("Plugins.Integration.TrendyolMarketplace.SyncLogs.SkippedCount")]
    public int SkippedCount { get; set; }

    [NopResourceDisplayName("Plugins.Integration.TrendyolMarketplace.SyncLogs.Status")]
    public SyncStatus Status { get; set; }

    public string StatusDisplay { get; set; }

    public string ErrorMessage { get; set; }

    public string Details { get; set; }
}

/// <summary>
/// Represents sync log list model
/// </summary>
public record SyncLogListModel : BasePagedListModel<SyncLogModel>
{
}

/// <summary>
/// Represents sync log search model
/// </summary>
public record SyncLogSearchModel : BaseSearchModel
{
    public int? VendorId { get; set; }

    public SyncType? SyncType { get; set; }

    public SyncStatus? Status { get; set; }

    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }
}
