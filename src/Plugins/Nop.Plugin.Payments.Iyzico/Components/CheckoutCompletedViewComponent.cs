using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Orders;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.Iyzico.Components;

[ViewComponent(Name = "IyzicoCheckoutCompleted")]
public class CheckoutCompletedViewComponent : NopViewComponent
{
    private readonly IOrderService _orderService;
    private readonly IWorkContext _workContext;

    public CheckoutCompletedViewComponent(
        IOrderService orderService,
        IWorkContext workContext)
    {
        _orderService = orderService;
        _workContext = workContext;
    }

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        // Get order ID from route
        if (!int.TryParse(Request.Query["orderId"], out var orderId))
            return Content(string.Empty);

        var order = await _orderService.GetOrderByIdAsync(orderId);
        if (order == null)
            return Content(string.Empty);

        // Only for iyzico payments
        if (order.PaymentMethodSystemName != "Payments.Iyzico")
            return Content(string.Empty);

        // Only if payment is pending
        if (order.PaymentStatus != Core.Domain.Payments.PaymentStatus.Pending)
            return Content(string.Empty);

        // Get card info from static storage
        var cardInfo = IyzicoPaymentProcessor.GetCardInfo(order.OrderGuid);
        if (cardInfo == null)
            return Content(string.Empty);

        // Pass data to view
        ViewBag.OrderId = order.Id;
        ViewBag.OrderGuid = order.OrderGuid;
        ViewBag.CardholderName = cardInfo["CardholderName"];
        ViewBag.CardNumber = cardInfo["CardNumber"];
        ViewBag.ExpireMonth = cardInfo["ExpireMonth"];
        ViewBag.ExpireYear = cardInfo["ExpireYear"];
        ViewBag.CardCode = cardInfo["CardCode"];

        return View("~/Plugins/Payments.Iyzico/Views/CheckoutCompleted.cshtml");
    }
}
