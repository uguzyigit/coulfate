using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Marketplace.VendorExtensions.Infrastructure;

public class RouteProvider : IRouteProvider
{
    public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Marketplace.VendorExtensions.Configure",
            pattern: "Admin/VendorExtensions/Configure",
            defaults: new { controller = "VendorExtensions", action = "Configure", area = "Admin" });

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Marketplace.VendorExtensions.TransactionHistory",
            pattern: "Admin/VendorExtensions/TransactionHistory",
            defaults: new { controller = "VendorExtensions", action = "TransactionHistory", area = "Admin" });

        // VendorProduct routes
        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Marketplace.VendorExtensions.VendorProduct.List",
            pattern: "Admin/VendorProduct/List",
            defaults: new { controller = "VendorProduct", action = "List", area = "Admin" });

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Marketplace.VendorExtensions.VendorProduct.Create",
            pattern: "Admin/VendorProduct/Create",
            defaults: new { controller = "VendorProduct", action = "Create", area = "Admin" });

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Marketplace.VendorExtensions.VendorProduct.Edit",
            pattern: "Admin/VendorProduct/Edit/{id:int}",
            defaults: new { controller = "VendorProduct", action = "Edit", area = "Admin" });

        // CategorySpecMapping routes (admin)
        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Marketplace.VendorExtensions.CategorySpecMapping.Add",
            pattern: "Admin/CategorySpecMapping/Add",
            defaults: new { controller = "CategorySpecMapping", action = "Add", area = "Admin" });

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Marketplace.VendorExtensions.CategorySpecMapping.Delete",
            pattern: "Admin/CategorySpecMapping/Delete",
            defaults: new { controller = "CategorySpecMapping", action = "Delete", area = "Admin" });
    }

    public int Priority => 0;
}
