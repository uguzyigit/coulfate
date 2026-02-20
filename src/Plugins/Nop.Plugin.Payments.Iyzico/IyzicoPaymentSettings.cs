using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.Iyzico;

/// <summary>
/// iyzico payment settings
/// </summary>
public class IyzicoPaymentSettings : ISettings
{
    /// <summary>
    /// Gets or sets API key
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets secret key
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets base URL (sandbox or production)
    /// </summary>
    public string BaseUrl { get; set; } = "https://sandbox-api.iyzipay.com";

    /// <summary>
    /// Gets or sets a value indicating whether to use sandbox
    /// </summary>
    public bool UseSandbox { get; set; } = true;

    /// <summary>
    /// Gets or sets additional fee
    /// </summary>
    public decimal AdditionalFee { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to "additional fee" is specified as percentage
    /// </summary>
    public bool AdditionalFeePercentage { get; set; }

    /// <summary>
    /// Gets or sets auto approval days (default: 14)
    /// </summary>
    public int AutoApprovalDays { get; set; } = 14;

    /// <summary>
    /// Gets or sets a value indicating whether to enable auto approval
    /// </summary>
    public bool EnableAutoApproval { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to auto create submerchant on vendor creation
    /// </summary>
    public bool AutoCreateSubMerchant { get; set; } = true;
}
