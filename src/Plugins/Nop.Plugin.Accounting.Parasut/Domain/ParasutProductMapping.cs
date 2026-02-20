using System.ComponentModel.DataAnnotations.Schema;
using Nop.Core;

namespace Nop.Plugin.Accounting.Parasut.Domain;

/// <summary>
/// Represents a mapping between charge types and Paraşüt products
/// </summary>
[Table("ParasutProductMapping")]
public partial class ParasutProductMapping : BaseEntity
{
    /// <summary>
    /// Gets or sets the charge type
    /// 1 = Commission, 2 = Shipping, 3 = Penalty
    /// </summary>
    public int ChargeType { get; set; }

    /// <summary>
    /// Gets or sets the Paraşüt product identifier
    /// </summary>
    public string ParasutProductId { get; set; }

    /// <summary>
    /// Gets or sets the product name
    /// </summary>
    public string ProductName { get; set; }

    /// <summary>
    /// Gets or sets the product code
    /// </summary>
    public string ProductCode { get; set; }

    /// <summary>
    /// Gets or sets whether this mapping is active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets when this mapping was created (UTC)
    /// </summary>
    public DateTime CreatedOnUtc { get; set; }
}

/// <summary>
/// Charge type enumeration
/// </summary>
public enum ChargeType
{
    Commission = 1,
    Shipping = 2,
    Penalty = 3
}
