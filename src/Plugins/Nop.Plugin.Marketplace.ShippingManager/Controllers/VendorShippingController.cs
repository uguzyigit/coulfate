using Marketplace.Abstractions.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Plugin.Marketplace.ShippingManager.Models.Vendor;
using Nop.Plugin.Marketplace.ShippingManager.Services;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Framework.UI;

namespace Nop.Plugin.Marketplace.ShippingManager.Controllers;

[Area(AreaNames.ADMIN)]
[AuthorizeAdmin]
[AutoValidateAntiforgeryToken]
public class VendorShippingController : BasePluginController
{
    private readonly IWorkContext _workContext;
    private readonly IShippingProviderService _shippingProviderService;
    private readonly IVendorShippingService _vendorShippingService;
    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly INopHtmlHelper _nopHtmlHelper;

    public VendorShippingController(
        IWorkContext workContext,
        IShippingProviderService shippingProviderService,
        IVendorShippingService vendorShippingService,
        ILocalizationService localizationService,
        INotificationService notificationService,
        INopHtmlHelper nopHtmlHelper)
    {
        _workContext = workContext;
        _shippingProviderService = shippingProviderService;
        _vendorShippingService = vendorShippingService;
        _localizationService = localizationService;
        _notificationService = notificationService;
        _nopHtmlHelper = nopHtmlHelper;
    }

    public async Task<IActionResult> Settings()
    {
        // Only vendors can access this page
        var vendor = await _workContext.GetCurrentVendorAsync();
        if (vendor == null)
            return AccessDeniedView();

        _nopHtmlHelper.SetActiveMenuItemSystemName("Vendor.Shipping.Settings");

        try
        {
            var model = await PrepareSettingsModelAsync(vendor.Id);
            return View("~/Plugins/Marketplace.ShippingManager/Views/VendorShipping/Settings.cshtml", model);
        }
        catch (Exception ex)
        {
            // Tables might not exist if plugin is not installed yet
            _notificationService.ErrorNotification($"Kargo yönetimi eklentisi hatası: {ex.Message}");
            return RedirectToAction("Index", "Home", new { area = AreaNames.ADMIN });
        }
    }

    [HttpPost]
    public async Task<IActionResult> SaveSettings(ShippingSettingsModel model)
    {
        var vendor = await _workContext.GetCurrentVendorAsync();
        if (vendor == null)
            return AccessDeniedView();

        // Validate provider selection
        if (model.ShippingProviderId <= 0)
        {
            _notificationService.ErrorNotification("Lütfen bir kargo şirketi seçin.");
            var reloadedModel = await PrepareSettingsModelAsync(vendor.Id);
            return View("~/Plugins/Marketplace.ShippingManager/Views/VendorShipping/Settings.cshtml", reloadedModel);
        }

        // Get or create preference
        var preference = await _vendorShippingService.GetVendorPreferenceAsync(vendor.Id);
        if (preference == null)
        {
            preference = new VendorShippingPreference
            {
                VendorId = vendor.Id,
                ShippingProviderId = model.ShippingProviderId,
                UseMarketplaceContract = model.UseMarketplaceContract,
                IsActive = true
            };
            await _vendorShippingService.InsertPreferenceAsync(preference);
        }
        else
        {
            preference.ShippingProviderId = model.ShippingProviderId;
            preference.UseMarketplaceContract = model.UseMarketplaceContract;
            await _vendorShippingService.UpdatePreferenceAsync(preference);
        }

        // Save credentials if using vendor contract
        if (!model.UseMarketplaceContract && model.Credentials != null)
        {
            foreach (var cred in model.Credentials)
            {
                // Only save if value is provided and not masked
                if (!string.IsNullOrWhiteSpace(cred.Value) && !cred.Value.StartsWith("***"))
                {
                    await _vendorShippingService.SetCredentialAsync(
                        vendor.Id,
                        model.ShippingProviderId,
                        cred.Key,
                        cred.Value);
                }
            }
        }

        _notificationService.SuccessNotification("Kargo ayarlarınız başarıyla kaydedildi.");

        return RedirectToAction("Settings");
    }

    [HttpPost]
    public async Task<IActionResult> GetProviderInfo(int providerId)
    {
        var vendor = await _workContext.GetCurrentVendorAsync();
        if (vendor == null)
            return Json(new { success = false, message = "Unauthorized" });

        var provider = await _shippingProviderService.GetByIdAsync(providerId);
        if (provider == null)
            return Json(new { success = false, message = "Provider not found" });

        // Get credential field definitions for this provider
        var credentialModels = GetProviderCredentialFields(provider.SystemName);

        // Get existing credentials and mask values
        var existingCredentials = await _vendorShippingService.GetVendorCredentialsAsync(vendor.Id, providerId);

        foreach (var credModel in credentialModels)
        {
            var existing = existingCredentials.FirstOrDefault(c => c.CredentialKey == credModel.Key);
            if (existing != null && !string.IsNullOrEmpty(existing.CredentialValue))
            {
                // Mask the value for security (show only last 4 chars)
                credModel.Value = existing.CredentialValue.Length > 4
                    ? new string('*', existing.CredentialValue.Length - 4) + existing.CredentialValue.Substring(existing.CredentialValue.Length - 4)
                    : "****";
            }
        }

        // Return JSON with camelCase property names (ASP.NET Core default)
        return Json(new
        {
            success = true,
            name = provider.Name,
            supportsMarketplace = provider.SupportsMarketplaceContract,
            supportsVendor = provider.SupportsVendorContract,
            credentials = credentialModels.Select(c => new
            {
                key = c.Key,
                label = c.Label,
                value = c.Value ?? "",
                isPassword = c.IsPassword,
                placeholder = c.Placeholder ?? ""
            }).ToList()
        });
    }

    private async Task<ShippingSettingsModel> PrepareSettingsModelAsync(int vendorId)
    {
        var model = new ShippingSettingsModel
        {
            UseMarketplaceContract = true // Default to marketplace contract
        };

        // Get available providers
        var providers = await _shippingProviderService.GetAllAsync(activeOnly: true);
        model.AvailableProviders = providers.Select(p => new SelectListItem
        {
            Value = p.Id.ToString(),
            Text = p.Name
        }).ToList();

        // Add empty option
        model.AvailableProviders.Insert(0, new SelectListItem
        {
            Value = "0",
            Text = "-- Kargo şirketi seçiniz --"
        });

        // Get vendor's current preference
        var preference = await _vendorShippingService.GetVendorPreferenceAsync(vendorId);
        if (preference != null)
        {
            model.HasConfiguration = true;
            model.ShippingProviderId = preference.ShippingProviderId;
            model.UseMarketplaceContract = preference.UseMarketplaceContract;

            var selectedProvider = providers.FirstOrDefault(p => p.Id == preference.ShippingProviderId);
            if (selectedProvider != null)
            {
                model.SelectedProviderName = selectedProvider.Name;
                model.ProviderSupportsMarketplaceContract = selectedProvider.SupportsMarketplaceContract;
                model.ProviderSupportsVendorContract = selectedProvider.SupportsVendorContract;

                // Always load credential fields for the selected provider
                model.Credentials = GetProviderCredentialFields(selectedProvider.SystemName);

                // If using vendor contract, load existing credential values (masked)
                if (!preference.UseMarketplaceContract)
                {
                    var existingCredentials = await _vendorShippingService.GetVendorCredentialsAsync(vendorId, preference.ShippingProviderId);

                    foreach (var credModel in model.Credentials)
                    {
                        var existing = existingCredentials.FirstOrDefault(c => c.CredentialKey == credModel.Key);
                        if (existing != null && !string.IsNullOrEmpty(existing.CredentialValue))
                        {
                            // Mask for display
                            credModel.Value = "********";
                        }
                    }
                }
            }
        }
        else
        {
            // No preference yet - if there's only one provider, pre-select it
            if (providers.Count == 1)
            {
                var singleProvider = providers.First();
                model.ShippingProviderId = singleProvider.Id;
                model.SelectedProviderName = singleProvider.Name;
                model.ProviderSupportsMarketplaceContract = singleProvider.SupportsMarketplaceContract;
                model.ProviderSupportsVendorContract = singleProvider.SupportsVendorContract;
                model.Credentials = GetProviderCredentialFields(singleProvider.SystemName);
            }
        }

        return model;
    }

    /// <summary>
    /// Returns credential field definitions for each shipping provider
    /// </summary>
    private List<CredentialModel> GetProviderCredentialFields(string systemName)
    {
        return systemName switch
        {
            "Shipping.PttKargo" => new List<CredentialModel>
            {
                new() { Key = "MusteriId", Label = "Müşteri Numarası", IsPassword = false, Placeholder = "PTT tarafından verilen müşteri numaranız" },
                new() { Key = "Sifre", Label = "Web Servis Şifresi", IsPassword = true, Placeholder = "PTT web servis şifreniz" }
            },
            "Shipping.ArasKargo" => new List<CredentialModel>
            {
                new() { Key = "UserName", Label = "Kullanıcı Adı", IsPassword = false, Placeholder = "Aras Kargo kullanıcı adınız" },
                new() { Key = "Password", Label = "Şifre", IsPassword = true, Placeholder = "Aras Kargo şifreniz" },
                new() { Key = "CustomerCode", Label = "Müşteri Kodu", IsPassword = false, Placeholder = "Aras Kargo müşteri kodunuz" }
            },
            "Shipping.YurticiKargo" => new List<CredentialModel>
            {
                new() { Key = "UserName", Label = "Kullanıcı Adı", IsPassword = false, Placeholder = "Yurtiçi Kargo kullanıcı adınız" },
                new() { Key = "Password", Label = "Şifre", IsPassword = true, Placeholder = "Yurtiçi Kargo şifreniz" },
                new() { Key = "UserLanguage", Label = "Dil Kodu", IsPassword = false, Placeholder = "TR" }
            },
            "Shipping.UPS" => new List<CredentialModel>
            {
                new() { Key = "AccessKey", Label = "Access Key", IsPassword = false, Placeholder = "UPS Access Key" },
                new() { Key = "Username", Label = "Kullanıcı Adı", IsPassword = false, Placeholder = "UPS kullanıcı adınız" },
                new() { Key = "Password", Label = "Şifre", IsPassword = true, Placeholder = "UPS şifreniz" },
                new() { Key = "AccountNumber", Label = "Hesap Numarası", IsPassword = false, Placeholder = "UPS hesap numaranız" }
            },
            "Shipping.DHL" => new List<CredentialModel>
            {
                new() { Key = "SiteId", Label = "Site ID", IsPassword = false, Placeholder = "DHL Site ID" },
                new() { Key = "Password", Label = "Şifre", IsPassword = true, Placeholder = "DHL şifreniz" },
                new() { Key = "AccountNumber", Label = "Hesap Numarası", IsPassword = false, Placeholder = "DHL hesap numaranız" }
            },
            _ => new List<CredentialModel>()
        };
    }
}
