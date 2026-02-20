using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Marketplace.Performance.Infrastructure;

public class RouteProvider : IRouteProvider
{
    public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Marketplace.Performance.ProductPerformance",
            pattern: "Admin/MarketplacePerformance/ProductPerformance",
            defaults: new { controller = "MarketplacePerformance", action = "ProductPerformance", area = "Admin" });

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Marketplace.Performance.SalesPerformance",
            pattern: "Admin/MarketplacePerformance/SalesPerformance",
            defaults: new { controller = "MarketplacePerformance", action = "SalesPerformance", area = "Admin" });

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Marketplace.Performance.ProductPerformanceData",
            pattern: "Admin/MarketplacePerformance/ProductPerformanceData",
            defaults: new { controller = "MarketplacePerformance", action = "ProductPerformanceData", area = "Admin" });

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Marketplace.Performance.SalesPerformanceData",
            pattern: "Admin/MarketplacePerformance/SalesPerformanceData",
            defaults: new { controller = "MarketplacePerformance", action = "SalesPerformanceData", area = "Admin" });

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Marketplace.Performance.Recalculate",
            pattern: "Admin/MarketplacePerformance/Recalculate",
            defaults: new { controller = "MarketplacePerformance", action = "Recalculate", area = "Admin" });

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Marketplace.Performance.Diagnose",
            pattern: "Admin/MarketplacePerformance/Diagnose",
            defaults: new { controller = "MarketplacePerformance", action = "Diagnose", area = "Admin" });

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Marketplace.Performance.TestData",
            pattern: "Admin/MarketplacePerformance/TestData",
            defaults: new { controller = "MarketplacePerformance", action = "TestData", area = "Admin" });
    }

    public int Priority => 0;
}
