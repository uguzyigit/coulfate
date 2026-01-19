using System.ComponentModel.DataAnnotations.Schema;
using Nop.Core;

namespace Nop.Plugin.Marketplace.VendorApplication.Domain;

/// <summary>
/// Represents a vendor application document (uploaded file)
/// </summary>
[Table("MarketplaceVendorApplicationDocument")]
public partial class VendorApplicationDocument : BaseEntity
{
    /// <summary>
    /// Gets or sets the vendor application identifier
    /// </summary>
    public int VendorApplicationId { get; set; }

    /// <summary>
    /// Gets or sets the document type identifier
    /// </summary>
    public int DocumentTypeId { get; set; }

    /// <summary>
    /// Gets or sets the download identifier (NopCommerce Download table)
    /// </summary>
    public int DownloadId { get; set; }

    /// <summary>
    /// Gets or sets the original file name
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// Gets or sets the content type (MIME type)
    /// </summary>
    public string ContentType { get; set; }

    /// <summary>
    /// Gets or sets the file size in bytes
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// Gets or sets the date and time of upload (UTC)
    /// </summary>
    public DateTime CreatedOnUtc { get; set; }
}
