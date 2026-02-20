using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Marketplace.ShippingManager.Infrastructure;

/// <summary>
/// Route provider
/// </summary>
public class RouteProvider : IRouteProvider
{
    public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
    {
        // Vendor shipping settings
        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Marketplace.ShippingManager.VendorSettings",
            pattern: "Admin/VendorShipping/Settings",
            defaults: new { controller = "VendorShipping", action = "Settings", area = "Admin" });

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Marketplace.ShippingManager.VendorSaveSettings",
            pattern: "Admin/VendorShipping/SaveSettings",
            defaults: new { controller = "VendorShipping", action = "SaveSettings", area = "Admin" });

        // Admin configuration
        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Marketplace.ShippingManager.Configure",
            pattern: "Admin/ShippingManagerAdmin/Configure",
            defaults: new { controller = "ShippingManagerAdmin", action = "Configure", area = "Admin" });

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Marketplace.ShippingManager.ProviderList",
            pattern: "Admin/ShippingManagerAdmin/ProviderList",
            defaults: new { controller = "ShippingManagerAdmin", action = "ProviderList", area = "Admin" });
    }

    public int Priority => 0;
}
