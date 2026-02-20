using Nop.Web.Framework.Models;

namespace Nop.Plugin.Integration.TrendyolMarketplace.Models;

/// <summary>
/// Represents the vendor dashboard model
/// </summary>
public record VendorDashboardModel : BaseNopModel
{
    public int VendorId { get; set; }

    public string VendorName { get; set; }

    public bool HasCredentials { get; set; }

    public bool IsActive { get; set; }

    public string ConnectionStatus { get; set; }

    public DateTime? LastSyncOnUtc { get; set; }

    public string LastSyncOnDisplay { get; set; }

    // Product Statistics
    public int TotalProducts { get; set; }

    public int ImportedProducts { get; set; }

    public int PendingProducts { get; set; }

    public int FailedProducts { get; set; }

    public int SkippedProducts { get; set; }

    public int DeactivatedProducts { get; set; }

    // Recent Sync Logs
    public IList<SyncLogModel> RecentSyncLogs { get; set; } = new List<SyncLogModel>();

    // Quick Actions
    public bool CanStartProductSync { get; set; }

    public bool CanStartStockPriceSync { get; set; }
}

/// <summary>
/// Represents the admin dashboard model (overview of all vendors)
/// </summary>
public record AdminDashboardModel : BaseNopModel
{
    // Global Statistics
    public int TotalVendors { get; set; }

    public int ActiveVendors { get; set; }

    public int TotalProducts { get; set; }

    public int TotalImportedProducts { get; set; }

    // Category/Brand Statistics
    public int TotalCategories { get; set; }

    public int MappedCategories { get; set; }

    public int UnmappedCategories { get; set; }

    public int TotalBrands { get; set; }

    public int MappedBrands { get; set; }

    public int UnmappedBrands { get; set; }

    // Last Global Sync
    public DateTime? LastCategorySyncOnUtc { get; set; }

    public string LastCategorySyncOnDisplay { get; set; }

    public DateTime? LastBrandSyncOnUtc { get; set; }

    public string LastBrandSyncOnDisplay { get; set; }

    // Vendor List Summary
    public IList<VendorSummaryModel> VendorSummaries { get; set; } = new List<VendorSummaryModel>();

    // Recent Sync Logs (Global)
    public IList<SyncLogModel> RecentSyncLogs { get; set; } = new List<SyncLogModel>();
}

/// <summary>
/// Represents a vendor summary for admin dashboard
/// </summary>
public record VendorSummaryModel : BaseNopEntityModel
{
    public int VendorId { get; set; }

    public string VendorName { get; set; }

    public bool IsActive { get; set; }

    public int TotalProducts { get; set; }

    public int ImportedProducts { get; set; }

    public DateTime? LastSyncOnUtc { get; set; }

    public string LastSyncOnDisplay { get; set; }

    public string Status { get; set; }
}

/// <summary>
/// Represents manual sync model for vendor
/// </summary>
public record ManualSyncModel : BaseNopModel
{
    public int VendorId { get; set; }

    public bool CanStartProductSync { get; set; }

    public bool CanStartStockPriceSync { get; set; }

    public bool IsProductSyncRunning { get; set; }

    public bool IsStockSyncRunning { get; set; }

    public bool IsPriceSyncRunning { get; set; }

    public SyncLogModel LastProductSync { get; set; }

    public SyncLogModel LastStockSync { get; set; }

    public SyncLogModel LastPriceSync { get; set; }
}
