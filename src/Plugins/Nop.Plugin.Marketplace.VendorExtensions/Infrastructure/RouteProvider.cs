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
    }

    public int Priority => 0;
}
