using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Payments.Iyzico.Models;

public record PaymentInfoModel : BaseNopModel
{
    [NopResourceDisplayName("Payment.CardholderName")]
    public string CardholderName { get; set; } = string.Empty;

    [NopResourceDisplayName("Payment.CardNumber")]
    public string CardNumber { get; set; } = string.Empty;

    [NopResourceDisplayName("Payment.ExpirationDate")]
    public string ExpireMonth { get; set; } = string.Empty;

    [NopResourceDisplayName("Payment.ExpirationDate")]
    public string ExpireYear { get; set; } = string.Empty;

    [NopResourceDisplayName("Payment.CardCode")]
    public string CardCode { get; set; } = string.Empty;
}
