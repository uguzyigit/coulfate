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
            pattern: "Admin/MarketplaceCore/Configure",
            defaults: new { controller = "MarketplaceCoreAdmin", action = "Configure", area = "Admin" });

        // Vendor Extension route (marketplace-specific vendor info)
        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Marketplace.Core.VendorExtension",
            pattern: "Admin/MarketplaceCore/VendorExtension/{vendorId}",
            defaults: new { controller = "MarketplaceCoreAdmin", action = "VendorExtension", area = "Admin" });
    }

    public int Priority => 0;
}
