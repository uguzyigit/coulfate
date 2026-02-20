using System.ComponentModel.DataAnnotations.Schema;
using Nop.Core;

namespace Nop.Plugin.Accounting.Parasut.Domain;

/// <summary>
/// Represents a mapping between NopCommerce vendor and Paraşüt contact
/// </summary>
[Table("ParasutVendorMapping")]
public partial class VendorParasutMapping : BaseEntity
{
    /// <summary>
    /// Gets or sets the vendor identifier
    /// </summary>
    public int VendorId { get; set; }

    /// <summary>
    /// Gets or sets the Paraşüt contact identifier
    /// </summary>
    public string ParasutContactId { get; set; }

    /// <summary>
    /// Gets or sets the contact name
    /// </summary>
    public string ContactName { get; set; }

    /// <summary>
    /// Gets or sets the tax office
    /// </summary>
    public string TaxOffice { get; set; }

    /// <summary>
    /// Gets or sets the tax number (VKN/TCKN)
    /// </summary>
    public string TaxNumber { get; set; }

    /// <summary>
    /// Gets or sets the email
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// Gets or sets whether this mapping is active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets when this mapping was last synced with Paraşüt (UTC)
    /// </summary>
    public DateTime LastSyncedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets when this mapping was created (UTC)
    /// </summary>
    public DateTime CreatedOnUtc { get; set; }
}
