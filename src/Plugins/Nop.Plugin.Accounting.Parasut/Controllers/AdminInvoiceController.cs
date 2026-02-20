using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Data;
using Nop.Plugin.Accounting.Parasut.Domain;
using Nop.Plugin.Accounting.Parasut.Models.Admin;
using Nop.Services.Vendors;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Accounting.Parasut.Controllers;

[Area(AreaNames.ADMIN)]
[AuthorizeAdmin]
[AutoValidateAntiforgeryToken]
public class AdminInvoiceController : BasePluginController
{
    private readonly IRepository<ParasutInvoiceRecord> _invoiceRepository;
    private readonly IVendorService _vendorService;
    private readonly IWorkContext _workContext;

    public AdminInvoiceController(
        IRepository<ParasutInvoiceRecord> invoiceRepository,
        IVendorService vendorService,
        IWorkContext workContext)
    {
        _invoiceRepository = invoiceRepository;
        _vendorService = vendorService;
        _workContext = workContext;
    }

    public async Task<IActionResult> AllInvoices()
    {
        var vendor = await _workContext.GetCurrentVendorAsync();
        if (vendor != null)
            return AccessDeniedView();

        var model = new AdminInvoiceListModel();

        var vendors = await _vendorService.GetAllVendorsAsync();
        model.AvailableVendors.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
        {
            Text = "Tümü",
            Value = "0"
        });
        foreach (var v in vendors)
        {
            model.AvailableVendors.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Text = v.Name,
                Value = v.Id.ToString()
            });
        }

        return View("~/Plugins/Accounting.Parasut/Views/AdminInvoice/AllInvoices.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> GetInvoiceList(
        int vendorId = 0,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int? status = null)
    {
        var vendor = await _workContext.GetCurrentVendorAsync();
        if (vendor != null)
            return Json(new { success = false, message = "Access denied" });

        var query = _invoiceRepository.Table;

        if (vendorId > 0)
            query = query.Where(i => i.VendorId == vendorId);

        if (startDate.HasValue)
            query = query.Where(i => i.CreatedOnUtc >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(i => i.CreatedOnUtc <= endDate.Value.AddDays(1));

        if (status.HasValue)
            query = query.Where(i => i.Status == status.Value);

        var invoices = await query
            .OrderByDescending(i => i.CreatedOnUtc)
            .Take(500)
            .ToListAsync();

        var vendors = await _vendorService.GetAllVendorsAsync();
        var vendorDict = vendors.ToDictionary(v => v.Id, v => v.Name);

        var result = invoices.Select(i => new
        {
            i.Id,
            InvoiceNo = i.ParasutInvoiceNo ?? "N/A",
            i.OrderId,
            VendorName = vendorDict.ContainsKey(i.VendorId) ? vendorDict[i.VendorId] : "N/A",
            InvoiceType = GetInvoiceTypeText(i.InvoiceType),
            GrossAmount = i.GrossAmount,
            VatAmount = i.VatAmount,
            NetAmount = i.TotalAmount,
            i.Status,
            StatusText = GetStatusText(i.Status),
            StatusClass = GetStatusClass(i.Status),
            CreatedOn = i.CreatedOnUtc.ToString("dd.MM.yyyy HH:mm"),
            HasPdf = !string.IsNullOrEmpty(i.PdfLocalPath)
        }).ToList();

        return Json(new { success = true, data = result });
    }

    private string GetInvoiceTypeText(int type)
    {
        return type switch
        {
            1 => "Komisyon",
            2 => "Kargo",
            3 => "Ceza",
            4 => "Kombine",
            _ => "Diğer"
        };
    }

    private string GetStatusText(int status)
    {
        return status switch
        {
            1 => "Oluşturuldu",
            2 => "İşleniyor",
            3 => "Hazır",
            4 => "Hata",
            _ => "Bilinmiyor"
        };
    }

    private string GetStatusClass(int status)
    {
        return status switch
        {
            1 => "badge-info",
            2 => "badge-warning",
            3 => "badge-success",
            4 => "badge-danger",
            _ => "badge-secondary"
        };
    }
}
