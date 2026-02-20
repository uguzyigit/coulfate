using LinqToDB.Mapping;
using Nop.Core;

namespace Nop.Plugin.Integration.TrendyolMarketplace.Domain;

/// <summary>
/// Represents vendor's Trendyol API credentials
/// </summary>
[Table("TrendyolVendorCredential")]
public class TrendyolVendorCredential : BaseEntity
{
    /// <summary>
    /// Gets or sets the NopCommerce vendor ID
    /// </summary>
    [Column]
    public int VendorId { get; set; }

    /// <summary>
    /// Gets or sets the Trendyol supplier ID
    /// </summary>
    [Column]
    public long TrendyolSupplierId { get; set; }

    /// <summary>
    /// Gets or sets the API key (username)
    /// </summary>
    [Column]
    public string ApiKey { get; set; }

    /// <summary>
    /// Gets or sets the API secret (encrypted password)
    /// </summary>
    [Column]
    public string ApiSecret { get; set; }

    /// <summary>
    /// Gets or sets the integration reference code (used in User-Agent header)
    /// </summary>
    [Column]
    public string IntegrationReferenceCode { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the integration is active
    /// </summary>
    [Column]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the last successful sync date
    /// </summary>
    [Column]
    public DateTime? LastSyncOnUtc { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether auto sync is enabled
    /// </summary>
    [Column]
    public bool AutoSyncEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the sync interval in minutes
    /// </summary>
    [Column]
    public int SyncIntervalMinutes { get; set; } = 60;

    /// <summary>
    /// Gets or sets the created date
    /// </summary>
    [Column]
    public DateTime CreatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the updated date
    /// </summary>
    [Column]
    public DateTime? UpdatedOnUtc { get; set; }
}
