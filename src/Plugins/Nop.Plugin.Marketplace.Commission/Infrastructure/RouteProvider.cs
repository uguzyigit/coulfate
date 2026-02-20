using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Marketplace.Commission.Infrastructure;

public class RouteProvider : IRouteProvider
{
    public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Marketplace.Commission.Configure",
            pattern: "Admin/Commission/Configure",
            defaults: new { controller = "Commission", action = "Configure", area = "Admin" });

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Marketplace.Commission.CategoryCommissions",
            pattern: "Admin/Commission/CategoryCommissions",
            defaults: new { controller = "Commission", action = "CategoryCommissions", area = "Admin" });

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Marketplace.Commission.ProductCommissions",
            pattern: "Admin/Commission/ProductCommissions",
            defaults: new { controller = "Commission", action = "ProductCommissions", area = "Admin" });

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Marketplace.Commission.Reports",
            pattern: "Admin/Commission/Reports",
            defaults: new { controller = "Commission", action = "Reports", area = "Admin" });
    }

    public int Priority => 0;
}
