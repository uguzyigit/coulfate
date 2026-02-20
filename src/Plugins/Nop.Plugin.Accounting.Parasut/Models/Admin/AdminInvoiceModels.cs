using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Accounting.Parasut.Models.Admin;

public record AdminInvoiceListModel : BaseNopModel
{
    public AdminInvoiceListModel()
    {
        Invoices = new List<AdminInvoiceModel>();
        AvailableVendors = new List<SelectListItem>();
    }

    public List<AdminInvoiceModel> Invoices { get; set; }
    public List<SelectListItem> AvailableVendors { get; set; }
    
    // Filtreler
    public int SelectedVendorId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? SelectedStatus { get; set; }
}

public record AdminInvoiceModel : BaseNopModel
{
    public int Id { get; set; }
    public string InvoiceNo { get; set; }
    public int OrderId { get; set; }
    public int VendorId { get; set; }
    public string VendorName { get; set; }
    public string InvoiceType { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal VatAmount { get; set; }
    public decimal NetAmount { get; set; }
    public string Status { get; set; }
    public string StatusClass { get; set; }
    public string CreatedOn { get; set; }
    public bool HasPdf { get; set; }
}
