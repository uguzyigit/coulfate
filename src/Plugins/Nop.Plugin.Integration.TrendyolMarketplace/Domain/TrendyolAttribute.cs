using LinqToDB.Mapping;
using Nop.Core;

namespace Nop.Plugin.Integration.TrendyolMarketplace.Domain;

/// <summary>
/// Represents attribute mapping type
/// </summary>
public enum AttributeMappingType
{
    /// <summary>
    /// Not mapped
    /// </summary>
    None = 0,

    /// <summary>
    /// Mapped to specification attribute
    /// </summary>
    SpecificationAttribute = 1,

    /// <summary>
    /// Mapped to product attribute (for variants)
    /// </summary>
    ProductAttribute = 2
}

/// <summary>
/// Represents Trendyol attribute mapping to NopCommerce attributes
/// </summary>
[Table("TrendyolAttribute")]
public class TrendyolAttribute : BaseEntity
{
    /// <summary>
    /// Gets or sets the Trendyol category ID this attribute belongs to
    /// </summary>
    [Column]
    public long TrendyolCategoryId { get; set; }

    /// <summary>
    /// Gets or sets the Trendyol attribute ID
    /// </summary>
    [Column]
    public long TrendyolAttributeId { get; set; }

    /// <summary>
    /// Gets or sets the Trendyol attribute name
    /// </summary>
    [Column]
    public string TrendyolAttributeName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this attribute is required
    /// </summary>
    [Column]
    public bool IsRequired { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this is a variant attribute (e.g., size, color)
    /// </summary>
    [Column]
    public bool IsVariantAttribute { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether custom values are allowed
    /// </summary>
    [Column]
    public bool AllowCustomValue { get; set; }

    /// <summary>
    /// Gets or sets the mapped NopCommerce specification attribute ID
    /// </summary>
    [Column]
    public int? NopSpecificationAttributeId { get; set; }

    /// <summary>
    /// Gets or sets the mapped NopCommerce product attribute ID (for variants)
    /// </summary>
    [Column]
    public int? NopProductAttributeId { get; set; }

    /// <summary>
    /// Gets or sets the mapping type
    /// </summary>
    [Column]
    public AttributeMappingType MappingType { get; set; } = AttributeMappingType.None;

    /// <summary>
    /// Gets or sets the created date
    /// </summary>
    [Column]
    public DateTime CreatedOnUtc { get; set; }
}

/// <summary>
/// Represents Trendyol attribute value mapping to NopCommerce attribute options
/// </summary>
[Table("TrendyolAttributeValue")]
public class TrendyolAttributeValue : BaseEntity
{
    /// <summary>
    /// Gets or sets the TrendyolAttribute ID (foreign key)
    /// </summary>
    [Column]
    public int TrendyolAttributeId { get; set; }

    /// <summary>
    /// Gets or sets the Trendyol value ID
    /// </summary>
    [Column]
    public long TrendyolValueId { get; set; }

    /// <summary>
    /// Gets or sets the Trendyol value name
    /// </summary>
    [Column]
    public string TrendyolValueName { get; set; }

    /// <summary>
    /// Gets or sets the mapped NopCommerce specification attribute option ID
    /// </summary>
    [Column]
    public int? NopSpecificationAttributeOptionId { get; set; }

    /// <summary>
    /// Gets or sets the mapped NopCommerce product attribute value ID
    /// </summary>
    [Column]
    public int? NopProductAttributeValueId { get; set; }

    /// <summary>
    /// Gets or sets the created date
    /// </summary>
    [Column]
    public DateTime CreatedOnUtc { get; set; }
}
