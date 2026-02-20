using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Payments.Iyzico.Infrastructure;

public class RouteProvider : IRouteProvider
{
    public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
    {
        // Public routes for 3DS flow
        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Payments.Iyzico.ProcessPayment",
            pattern: "Plugins/PaymentIyzico/ProcessPayment",
            defaults: new { controller = "PaymentIyzico", action = "ProcessPayment" });

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Payments.Iyzico.ThreeDSCallback",
            pattern: "Plugins/PaymentIyzico/ThreeDSCallback",
            defaults: new { controller = "PaymentIyzico", action = "ThreeDSCallback" });
    }

    public int Priority => 0;
}
