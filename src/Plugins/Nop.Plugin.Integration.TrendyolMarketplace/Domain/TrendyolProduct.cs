using LinqToDB.Mapping;
using Nop.Core;

namespace Nop.Plugin.Integration.TrendyolMarketplace.Domain;

/// <summary>
/// Represents product import status
/// </summary>
public enum ImportStatus
{
    /// <summary>
    /// Waiting to be imported
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Successfully imported
    /// </summary>
    Imported = 1,

    /// <summary>
    /// Import failed
    /// </summary>
    Failed = 2,

    /// <summary>
    /// Skipped (e.g., unmapped category)
    /// </summary>
    Skipped = 3,

    /// <summary>
    /// Deactivated (was imported but no longer on sale in Trendyol)
    /// </summary>
    Deactivated = 4
}

/// <summary>
/// Represents Trendyol product tracking and mapping to NopCommerce product
/// </summary>
[Table("TrendyolProduct")]
public class TrendyolProduct : BaseEntity
{
    /// <summary>
    /// Gets or sets the Trendyol barcode (unique identifier)
    /// </summary>
    [Column]
    public string TrendyolBarcode { get; set; }

    /// <summary>
    /// Gets or sets the Trendyol product code (productMainId - groups variants)
    /// </summary>
    [Column]
    public string TrendyolProductCode { get; set; }

    /// <summary>
    /// Gets or sets the Trendyol stock code
    /// </summary>
    [Column]
    public string TrendyolStockCode { get; set; }

    /// <summary>
    /// Gets or sets the Trendyol product title
    /// </summary>
    [Column]
    public string TrendyolTitle { get; set; }

    /// <summary>
    /// Gets or sets the Trendyol category ID
    /// </summary>
    [Column]
    public long? TrendyolCategoryId { get; set; }

    /// <summary>
    /// Gets or sets the Trendyol brand ID
    /// </summary>
    [Column]
    public long? TrendyolBrandId { get; set; }

    /// <summary>
    /// Gets or sets the created NopCommerce product ID
    /// </summary>
    [Column]
    public int? NopProductId { get; set; }

    /// <summary>
    /// Gets or sets the parent NopCommerce product ID (for variants)
    /// </summary>
    [Column]
    public int? NopParentProductId { get; set; }

    /// <summary>
    /// Gets or sets the vendor ID (Marketplace.Core vendor)
    /// </summary>
    [Column]
    public int VendorId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this is a variant
    /// </summary>
    [Column]
    public bool IsVariant { get; set; }

    /// <summary>
    /// Gets or sets the last known Trendyol price
    /// </summary>
    [Column]
    public decimal? LastTrendyolPrice { get; set; }

    /// <summary>
    /// Gets or sets the last known Trendyol stock quantity
    /// </summary>
    [Column]
    public int? LastTrendyolStock { get; set; }

    /// <summary>
    /// Gets or sets the last sync date
    /// </summary>
    [Column]
    public DateTime? LastSyncOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the import status
    /// </summary>
    [Column]
    public ImportStatus ImportStatus { get; set; } = ImportStatus.Pending;

    /// <summary>
    /// Gets or sets the import message (error or info)
    /// </summary>
    [Column]
    public string ImportMessage { get; set; }

    /// <summary>
    /// Gets or sets the raw JSON data from Trendyol (for debugging)
    /// </summary>
    [Column]
    public string TrendyolJsonData { get; set; }

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

    /// <summary>
    /// Gets or sets a value indicating whether the product is currently on sale in Trendyol
    /// </summary>
    [Column]
    public bool TrendyolOnSale { get; set; } = true;
}
