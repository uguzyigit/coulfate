using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Marketplace.ShippingManager.Models.Vendor;

/// <summary>
/// Vendor shipping settings model
/// </summary>
public record ShippingSettingsModel : BaseNopModel
{
    public ShippingSettingsModel()
    {
        AvailableProviders = new List<SelectListItem>();
        Credentials = new List<CredentialModel>();
    }

    /// <summary>
    /// Selected shipping provider ID
    /// </summary>
    public int ShippingProviderId { get; set; }

    /// <summary>
    /// Whether to use marketplace contract
    /// </summary>
    public bool UseMarketplaceContract { get; set; }

    /// <summary>
    /// Available shipping providers
    /// </summary>
    public IList<SelectListItem> AvailableProviders { get; set; }

    /// <summary>
    /// Selected provider name
    /// </summary>
    public string SelectedProviderName { get; set; }

    /// <summary>
    /// Whether selected provider supports marketplace contract
    /// </summary>
    public bool ProviderSupportsMarketplaceContract { get; set; }

    /// <summary>
    /// Whether selected provider supports vendor contract
    /// </summary>
    public bool ProviderSupportsVendorContract { get; set; }

    /// <summary>
    /// Credentials for vendor contract
    /// </summary>
    public IList<CredentialModel> Credentials { get; set; }

    /// <summary>
    /// Whether the vendor has configured shipping
    /// </summary>
    public bool HasConfiguration { get; set; }
}

/// <summary>
/// Credential model for vendor API keys
/// </summary>
public record CredentialModel
{
    public string Key { get; set; }
    public string Label { get; set; }
    public string Value { get; set; }
    public bool IsPassword { get; set; }
    public string Placeholder { get; set; }
}
