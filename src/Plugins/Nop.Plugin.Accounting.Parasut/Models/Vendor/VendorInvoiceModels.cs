using Nop.Web.Framework.Models;

namespace Nop.Plugin.Accounting.Parasut.Models.Vendor;

/// <summary>
/// Vendor invoice list model
/// </summary>
public record VendorInvoiceListModel : BaseNopModel
{
    public VendorInvoiceListModel()
    {
        Invoices = new List<VendorInvoiceModel>();
    }

    public IList<VendorInvoiceModel> Invoices { get; set; }
}

/// <summary>
/// Vendor invoice model
/// </summary>
public record VendorInvoiceModel : BaseNopModel
{
    public int Id { get; set; }
    
    public int? OrderId { get; set; }
    
    public string OrderNumber { get; set; }
    
    public string InvoiceNo { get; set; }
    
    public string InvoiceType { get; set; }
    
    public decimal TotalAmount { get; set; }
    
    public decimal VatAmount { get; set; }
    
    public decimal GrossAmount { get; set; }
    
    public string Status { get; set; }
    
    public string StatusClass { get; set; } // CSS class for badge
    
    public string EDocumentType { get; set; }
    
    public bool HasPdf { get; set; }
    
    public string CreatedOn { get; set; }
    
    // Breakdown details
    public decimal CommissionAmount { get; set; }
    
    public decimal MarketplaceFee { get; set; }
    
    public decimal TaxWithholding { get; set; }
    
    public decimal VendorNetAmount { get; set; }
    
    public string Description { get; set; }
}
