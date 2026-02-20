using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Accounting.Parasut.Infrastructure;

public class RouteProvider : IRouteProvider
{
    public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
    {
        // Parasut Configure
        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Accounting.Parasut.Configure",
            pattern: "Admin/Parasut/Configure",
            defaults: new { controller = "Parasut", action = "Configure", area = "Admin" });

        // Vendor Invoice Routes
        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Accounting.Parasut.VendorInvoiceList",
            pattern: "Admin/VendorInvoice/List",
            defaults: new { controller = "VendorInvoice", action = "List", area = "Admin" });

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Accounting.Parasut.VendorInvoiceDetails",
            pattern: "Admin/VendorInvoice/Details/{id}",
            defaults: new { controller = "VendorInvoice", action = "Details", area = "Admin" });

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Accounting.Parasut.VendorInvoiceDownloadPdf",
            pattern: "Admin/VendorInvoice/DownloadPdf/{id}",
            defaults: new { controller = "VendorInvoice", action = "DownloadPdf", area = "Admin" });

        // Admin Invoice Routes
        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Accounting.Parasut.AdminInvoices",
            pattern: "Admin/AdminInvoice/AllInvoices",
            defaults: new { controller = "AdminInvoice", action = "AllInvoices", area = "Admin" });

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Accounting.Parasut.AdminInvoiceList",
            pattern: "Admin/AdminInvoice/GetInvoiceList",
            defaults: new { controller = "AdminInvoice", action = "GetInvoiceList", area = "Admin" });
    }

    public int Priority => 0;
}
