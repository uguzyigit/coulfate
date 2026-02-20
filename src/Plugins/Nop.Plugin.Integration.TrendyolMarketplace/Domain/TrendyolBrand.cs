using LinqToDB.Mapping;
using Nop.Core;

namespace Nop.Plugin.Integration.TrendyolMarketplace.Domain;

/// <summary>
/// Represents Trendyol brand mapping to NopCommerce manufacturer
/// </summary>
[Table("TrendyolBrand")]
public class TrendyolBrand : BaseEntity
{
    /// <summary>
    /// Gets or sets the Trendyol brand ID
    /// </summary>
    [Column]
    public long TrendyolBrandId { get; set; }

    /// <summary>
    /// Gets or sets the Trendyol brand name
    /// </summary>
    [Column]
    public string TrendyolBrandName { get; set; }

    /// <summary>
    /// Gets or sets the mapped NopCommerce manufacturer ID
    /// </summary>
    [Column]
    public int? NopManufacturerId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this mapping was created automatically
    /// </summary>
    [Column]
    public bool IsAutoMapped { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to auto-create manufacturer if not exists
    /// </summary>
    [Column]
    public bool AutoCreateIfNotExists { get; set; } = true;

    /// <summary>
    /// Gets or sets the created date
    /// </summary>
    [Column]
    public DateTime CreatedOnUtc { get; set; }
}
