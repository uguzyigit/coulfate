using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.Iyzico.Components;

[ViewComponent(Name = "PaymentIyzico")]
public class PaymentIyzicoViewComponent : NopViewComponent
{
    public IViewComponentResult Invoke()
    {
        return View("~/Plugins/Payments.Iyzico/Views/PaymentInfo.cshtml");
    }
}
