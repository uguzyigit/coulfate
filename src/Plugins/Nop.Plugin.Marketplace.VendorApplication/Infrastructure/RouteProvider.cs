using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Marketplace.VendorApplication.Infrastructure;

public class RouteProvider : IRouteProvider
{
    public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
    {
        // Public routes
        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Marketplace.VendorApplication.Apply",
            pattern: "VendorApplication/Apply",
            defaults: new { controller = "VendorApplication", action = "Apply" });

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Marketplace.VendorApplication.ApplySuccess",
            pattern: "VendorApplication/ApplySuccess",
            defaults: new { controller = "VendorApplication", action = "ApplySuccess" });

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Marketplace.VendorApplication.CheckStatus",
            pattern: "VendorApplication/CheckStatus",
            defaults: new { controller = "VendorApplication", action = "CheckStatus" });

        // Admin routes
        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Marketplace.VendorApplication.Admin.Configure",
            pattern: "Admin/VendorApplicationAdmin/Configure",
            defaults: new { controller = "VendorApplicationAdmin", action = "Configure", area = "Admin" });

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Marketplace.VendorApplication.Admin.List",
            pattern: "Admin/VendorApplicationAdmin/List",
            defaults: new { controller = "VendorApplicationAdmin", action = "List", area = "Admin" });

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Marketplace.VendorApplication.Admin.Detail",
            pattern: "Admin/VendorApplicationAdmin/Detail/{id}",
            defaults: new { controller = "VendorApplicationAdmin", action = "Detail", area = "Admin" });

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Marketplace.VendorApplication.Admin.DocumentTypes",
            pattern: "Admin/VendorApplicationAdmin/DocumentTypes",
            defaults: new { controller = "VendorApplicationAdmin", action = "DocumentTypes", area = "Admin" });

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Marketplace.VendorApplication.Admin.DocumentTypeCreate",
            pattern: "Admin/VendorApplicationAdmin/DocumentTypeCreate",
            defaults: new { controller = "VendorApplicationAdmin", action = "DocumentTypeCreate", area = "Admin" });

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Marketplace.VendorApplication.Admin.DocumentTypeEdit",
            pattern: "Admin/VendorApplicationAdmin/DocumentTypeEdit/{id}",
            defaults: new { controller = "VendorApplicationAdmin", action = "DocumentTypeEdit", area = "Admin" });
    }

    public int Priority => 0;
}
