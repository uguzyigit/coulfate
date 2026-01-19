using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Marketplace.Core.Infrastructure;

/// <summary>
/// Represents plugin route provider
/// </summary>
public class RouteProvider : IRouteProvider
{
    public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
    {
        // Configure route (for plugin configuration page)
        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Marketplace.Core.Configure",
            pattern: "Admin/MarketplaceVendor/Configure",
            defaults: new { controller = "MarketplaceVendor", action = "Configure", area = "Admin" });

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Marketplace.Core.Vendor.List",
            pattern: "Admin/Marketplace/Vendor/List",
            defaults: new { controller = "MarketplaceVendor", action = "Index", area = "Admin" });

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Marketplace.Core.Vendor.Create",
            pattern: "Admin/Marketplace/Vendor/Create",
            defaults: new { controller = "MarketplaceVendor", action = "Create", area = "Admin" });

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Marketplace.Core.Vendor.Edit",
            pattern: "Admin/Marketplace/Vendor/Edit/{id}",
            defaults: new { controller = "MarketplaceVendor", action = "Edit", area = "Admin" });
    }

    public int Priority => 0;
}
