using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Data;
using Nop.Plugin.Accounting.Parasut.Domain;
using Nop.Plugin.Accounting.Parasut.Models.Vendor;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Vendors;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Accounting.Parasut.Controllers;

[Area(AreaNames.ADMIN)]
[AuthorizeAdmin]
[AutoValidateAntiforgeryToken]
public class VendorInvoiceController : BasePluginController
{
    private readonly IWorkContext _workContext;
    private readonly IVendorService _vendorService;
    private readonly IRepository<ParasutInvoiceRecord> _invoiceRepository;
    private readonly IOrderService _orderService;
    private readonly ILocalizationService _localizationService;
    private readonly IWebHelper _webHelper;

    public VendorInvoiceController(
        IWorkContext workContext,
        IVendorService vendorService,
        IRepository<ParasutInvoiceRecord> invoiceRepository,
        IOrderService orderService,
        ILocalizationService localizationService,
        IWebHelper webHelper)
    {
        _workContext = workContext;
        _vendorService = vendorService;
        _invoiceRepository = invoiceRepository;
        _orderService = orderService;
        _localizationService = localizationService;
        _webHelper = webHelper;
    }

    /// <summary>
    /// List vendor invoices
    /// </summary>
    public async Task<IActionResult> List()
    {
        // Get current vendor
        var customer = await _workContext.GetCurrentCustomerAsync();
        var vendor = await _vendorService.GetVendorByIdAsync(customer.VendorId);
        
        if (vendor == null)
            return Challenge();

        // Get vendor invoices
        var invoices = await _invoiceRepository.Table
            .Where(i => i.VendorId == vendor.Id)
            .OrderByDescending(i => i.CreatedOnUtc)
            .ToListAsync();

        var model = new VendorInvoiceListModel();

        foreach (var invoice in invoices)
        {
            var order = invoice.OrderId.HasValue 
                ? await _orderService.GetOrderByIdAsync(invoice.OrderId.Value) 
                : null;

            var invoiceModel = new VendorInvoiceModel
            {
                Id = invoice.Id,
                OrderId = invoice.OrderId,
                OrderNumber = order?.CustomOrderNumber ?? invoice.OrderId?.ToString(),
                InvoiceNo = invoice.ParasutInvoiceNo ?? "-",
                InvoiceType = GetInvoiceTypeText(invoice.InvoiceType),
                TotalAmount = invoice.TotalAmount,
                VatAmount = invoice.VatAmount,
                GrossAmount = invoice.GrossAmount,
                Status = GetStatusText(invoice.Status),
                StatusClass = GetStatusClass(invoice.Status),
                EDocumentType = invoice.EDocumentType ?? "-",
                HasPdf = !string.IsNullOrEmpty(invoice.PdfLocalPath) && System.IO.File.Exists(invoice.PdfLocalPath),
                CreatedOn = invoice.CreatedOnUtc.ToString("dd.MM.yyyy HH:mm"),
                Description = GetInvoiceDescription(invoice, order)
            };

            model.Invoices.Add(invoiceModel);
        }

        return View("~/Plugins/Accounting.Parasut/Views/VendorInvoice/List.cshtml", model);
    }

    /// <summary>
    /// Get invoice details
    /// </summary>
    public async Task<IActionResult> Details(int id)
    {
        // Get current vendor
        var customer = await _workContext.GetCurrentCustomerAsync();
        var vendor = await _vendorService.GetVendorByIdAsync(customer.VendorId);
        
        if (vendor == null)
            return Challenge();

        // Get invoice
        var invoice = await _invoiceRepository.GetByIdAsync(id);
        
        if (invoice == null || invoice.VendorId != vendor.Id)
            return Challenge();

        var order = invoice.OrderId.HasValue 
            ? await _orderService.GetOrderByIdAsync(invoice.OrderId.Value) 
            : null;

        // Get order items to show breakdown
        var orderItems = order != null 
            ? await _orderService.GetOrderItemsAsync(order.Id)
            : new List<Nop.Core.Domain.Orders.OrderItem>();

        var vendorOrderItems = new List<string>();
        foreach (var item in orderItems)
        {
            var product = await _orderService.GetProductByOrderItemIdAsync(item.Id);
            if (product?.VendorId == vendor.Id)
            {
                vendorOrderItems.Add($"{product.Name} x{item.Quantity} - {item.PriceInclTax:C}");
            }
        }

        var model = new VendorInvoiceModel
        {
            Id = invoice.Id,
            OrderId = invoice.OrderId,
            OrderNumber = order?.CustomOrderNumber ?? invoice.OrderId?.ToString(),
            InvoiceNo = invoice.ParasutInvoiceNo ?? "-",
            InvoiceType = GetInvoiceTypeText(invoice.InvoiceType),
            TotalAmount = invoice.TotalAmount,
            VatAmount = invoice.VatAmount,
            GrossAmount = invoice.GrossAmount,
            Status = GetStatusText(invoice.Status),
            StatusClass = GetStatusClass(invoice.Status),
            EDocumentType = invoice.EDocumentType ?? "-",
            HasPdf = !string.IsNullOrEmpty(invoice.PdfLocalPath) && System.IO.File.Exists(invoice.PdfLocalPath),
            CreatedOn = invoice.CreatedOnUtc.ToString("dd.MM.yyyy HH:mm"),
            Description = string.Join("<br/>", vendorOrderItems)
        };

        return View("~/Plugins/Accounting.Parasut/Views/VendorInvoice/Details.cshtml", model);
    }

    /// <summary>
    /// Download PDF
    /// </summary>
    public async Task<IActionResult> DownloadPdf(int id)
    {
        // Get current vendor
        var customer = await _workContext.GetCurrentCustomerAsync();
        var vendor = await _vendorService.GetVendorByIdAsync(customer.VendorId);
        
        if (vendor == null)
            return Challenge();

        // Get invoice
        var invoice = await _invoiceRepository.GetByIdAsync(id);
        
        if (invoice == null || invoice.VendorId != vendor.Id)
            return Challenge();

        if (string.IsNullOrEmpty(invoice.PdfLocalPath) || !System.IO.File.Exists(invoice.PdfLocalPath))
            return NotFound();

        var pdfBytes = await System.IO.File.ReadAllBytesAsync(invoice.PdfLocalPath);
        var fileName = $"Fatura_{invoice.ParasutInvoiceNo ?? invoice.Id.ToString()}.pdf";

        return File(pdfBytes, "application/pdf", fileName);
    }

    #region Helpers

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

    private string GetInvoiceDescription(ParasutInvoiceRecord invoice, Nop.Core.Domain.Orders.Order order)
    {
        if (order == null)
            return $"Fatura #{invoice.ParasutInvoiceNo ?? invoice.Id.ToString()}";

        var parts = new List<string>();
        
        if (invoice.InvoiceType == 1 || invoice.InvoiceType == 4)
            parts.Add($"Komisyon");
        
        if (invoice.InvoiceType == 2 || invoice.InvoiceType == 4)
            parts.Add($"Kargo");
        
        if (invoice.InvoiceType == 3 || invoice.InvoiceType == 4)
            parts.Add($"Ceza");

        return $"Sipariş #{order.CustomOrderNumber} - {string.Join(", ", parts)}";
    }

    #endregion
}
