using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Plugin.Widgets.GoogleAnalytics4.Domain;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Widgets.GoogleAnalytics4.Infrastructure;

/// <summary>
/// Represents plugin route provider
/// </summary>
public class RouteProvider : IRouteProvider
{
    /// <summary>
    /// Register routes
    /// </summary>
    public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapControllerRoute(
            name: GA4Defaults.ConfigurationRouteName,
            pattern: "Admin/GA4Admin/Configure",
            defaults: new { controller = "GA4Admin", action = "Configure" });
    }

    /// <summary>
    /// Gets a priority of route provider
    /// </summary>
    public int Priority => 0;
}
