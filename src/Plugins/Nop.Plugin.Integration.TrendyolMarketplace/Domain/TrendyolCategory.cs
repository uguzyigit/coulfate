using LinqToDB.Mapping;
using Nop.Core;

namespace Nop.Plugin.Integration.TrendyolMarketplace.Domain;

/// <summary>
/// Represents Trendyol category mapping to NopCommerce category
/// </summary>
[Table("TrendyolCategory")]
public class TrendyolCategory : BaseEntity
{
    /// <summary>
    /// Gets or sets the Trendyol category ID
    /// </summary>
    [Column]
    public long TrendyolCategoryId { get; set; }

    /// <summary>
    /// Gets or sets the Trendyol category name
    /// </summary>
    [Column]
    public string TrendyolCategoryName { get; set; }

    /// <summary>
    /// Gets or sets the Trendyol parent category ID
    /// </summary>
    [Column]
    public long? TrendyolParentId { get; set; }

    /// <summary>
    /// Gets or sets the full category path (e.g., "Elektronik > Telefon > Akıllı Telefon")
    /// </summary>
    [Column]
    public string TrendyolCategoryPath { get; set; }

    /// <summary>
    /// Gets or sets the mapped NopCommerce category ID
    /// </summary>
    [Column]
    public int? NopCategoryId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this mapping was created automatically
    /// </summary>
    [Column]
    public bool IsAutoMapped { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this is a leaf category (no children)
    /// </summary>
    [Column]
    public bool IsLeaf { get; set; }

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
