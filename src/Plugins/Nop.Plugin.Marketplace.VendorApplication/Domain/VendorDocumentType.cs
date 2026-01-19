using System.ComponentModel.DataAnnotations.Schema;
using Nop.Core;

namespace Nop.Plugin.Marketplace.VendorApplication.Domain;

/// <summary>
/// Represents a vendor document type (admin-configurable)
/// </summary>
[Table("MarketplaceVendorDocumentType")]
public partial class VendorDocumentType : BaseEntity
{
    /// <summary>
    /// Gets or sets the document type name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the description
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this document is required
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Gets or sets the display order
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this document type is active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the allowed file extensions (comma-separated, e.g., ".pdf,.jpg,.png")
    /// </summary>
    public string AllowedExtensions { get; set; }

    /// <summary>
    /// Gets or sets the maximum file size in KB
    /// </summary>
    public int MaxFileSizeKb { get; set; }

    /// <summary>
    /// Gets or sets the date and time of creation (UTC)
    /// </summary>
    public DateTime CreatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the date and time of last update (UTC)
    /// </summary>
    public DateTime? UpdatedOnUtc { get; set; }
}
