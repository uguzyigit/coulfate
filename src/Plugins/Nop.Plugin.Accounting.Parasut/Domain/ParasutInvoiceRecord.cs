using System.ComponentModel.DataAnnotations.Schema;
using Nop.Core;

namespace Nop.Plugin.Accounting.Parasut.Domain;

/// <summary>
/// Represents a Paraşüt invoice record
/// </summary>
[Table("ParasutInvoiceRecord")]
public partial class ParasutInvoiceRecord : BaseEntity
{
    /// <summary>
    /// Gets or sets the vendor identifier
    /// </summary>
    public int VendorId { get; set; }

    /// <summary>
    /// Gets or sets the order identifier (nullable)
    /// </summary>
    public int? OrderId { get; set; }

    /// <summary>
    /// Gets or sets the invoice type
    /// 1 = Commission, 2 = Shipping, 3 = Penalty, 4 = Combined
    /// </summary>
    public int InvoiceType { get; set; }

    /// <summary>
    /// Gets or sets the Paraşüt sales invoice identifier
    /// </summary>
    public string ParasutSalesInvoiceId { get; set; }

    /// <summary>
    /// Gets or sets the Paraşüt invoice number
    /// </summary>
    public string ParasutInvoiceNo { get; set; }

    /// <summary>
    /// Gets or sets the total amount (before VAT)
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Gets or sets the VAT amount
    /// </summary>
    public decimal VatAmount { get; set; }

    /// <summary>
    /// Gets or sets the gross amount (total + VAT)
    /// </summary>
    public decimal GrossAmount { get; set; }

    /// <summary>
    /// Gets or sets the e-document type (e_invoice, e_archive, e_smm)
    /// </summary>
    public string EDocumentType { get; set; }

    /// <summary>
    /// Gets or sets the e-document identifier
    /// </summary>
    public string EDocumentId { get; set; }

    /// <summary>
    /// Gets or sets the trackable job identifier (for async processing)
    /// </summary>
    public string TrackableJobId { get; set; }

    /// <summary>
    /// Gets or sets the status
    /// 1 = Created, 2 = Formalizing, 3 = PdfReady, 4 = Failed
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// Gets or sets the status message
    /// </summary>
    public string StatusMessage { get; set; }

    /// <summary>
    /// Gets or sets the error message (if any)
    /// </summary>
    public string ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the PDF storage identifier
    /// </summary>
    public string PdfStorageId { get; set; }

    /// <summary>
    /// Gets or sets the PDF local file path
    /// </summary>
    public string PdfLocalPath { get; set; }

    /// <summary>
    /// Gets or sets when the PDF was downloaded (UTC)
    /// </summary>
    public DateTime? PdfDownloadedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets when this record was created (UTC)
    /// </summary>
    public DateTime CreatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets when this record was last updated (UTC)
    /// </summary>
    public DateTime? UpdatedOnUtc { get; set; }
}

/// <summary>
/// Invoice type enumeration
/// </summary>
public enum InvoiceType
{
    Commission = 1,
    Shipping = 2,
    Penalty = 3,
    Combined = 4
}

/// <summary>
/// Invoice status enumeration
/// </summary>
public enum InvoiceStatus
{
    Created = 1,
    Formalizing = 2,
    PdfReady = 3,
    Failed = 4
}
