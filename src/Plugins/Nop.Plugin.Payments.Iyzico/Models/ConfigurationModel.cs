using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Payments.Iyzico.Models;

public record ConfigurationModel : BaseNopModel
{
    [NopResourceDisplayName("Plugins.Payments.Iyzico.Fields.UseSandbox")]
    public bool UseSandbox { get; set; }

    [NopResourceDisplayName("Plugins.Payments.Iyzico.Fields.ApiKey")]
    public string ApiKey { get; set; } = string.Empty;

    [NopResourceDisplayName("Plugins.Payments.Iyzico.Fields.SecretKey")]
    public string SecretKey { get; set; } = string.Empty;

    [NopResourceDisplayName("Plugins.Payments.Iyzico.Fields.AdditionalFee")]
    public decimal AdditionalFee { get; set; }

    [NopResourceDisplayName("Plugins.Payments.Iyzico.Fields.AdditionalFeePercentage")]
    public bool AdditionalFeePercentage { get; set; }

    [NopResourceDisplayName("Plugins.Payments.Iyzico.Fields.AutoApprovalDays")]
    public int AutoApprovalDays { get; set; }

    [NopResourceDisplayName("Plugins.Payments.Iyzico.Fields.EnableAutoApproval")]
    public bool EnableAutoApproval { get; set; }

    [NopResourceDisplayName("Plugins.Payments.Iyzico.Fields.AutoCreateSubMerchant")]
    public bool AutoCreateSubMerchant { get; set; }
}
