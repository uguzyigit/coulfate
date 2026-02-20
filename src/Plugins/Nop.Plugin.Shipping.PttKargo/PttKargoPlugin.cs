using Marketplace.Abstractions.Domain;
using Nop.Core;
using Nop.Plugin.Marketplace.ShippingManager.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;

namespace Nop.Plugin.Shipping.PttKargo;

/// <summary>
/// PTT Kargo Plugin
/// </summary>
public class PttKargoPlugin : BasePlugin
{
    private readonly ILocalizationService _localizationService;
    private readonly ISettingService _settingService;
    private readonly IShippingProviderService _shippingProviderService;
    private readonly IWebHelper _webHelper;

    public PttKargoPlugin(
        ILocalizationService localizationService,
        ISettingService settingService,
        IShippingProviderService shippingProviderService,
        IWebHelper webHelper)
    {
        _localizationService = localizationService;
        _settingService = settingService;
        _shippingProviderService = shippingProviderService;
        _webHelper = webHelper;
    }

    public override async Task InstallAsync()
    {
        // Save default settings
        await _settingService.SaveSettingAsync(new PttKargoSettings
        {
            UseSandbox = true,
            MarketplaceContractEnabled = false,
            AllowVendorContracts = true
        });

        // Register as shipping provider
        var existingProvider = await _shippingProviderService.GetBySystemNameAsync("Shipping.PttKargo");
        if (existingProvider == null)
        {
            await _shippingProviderService.InsertAsync(new ShippingProvider
            {
                Name = "PTT Kargo",
                SystemName = "Shipping.PttKargo",
                SupportsMarketplaceContract = true,
                SupportsVendorContract = true,
                LogoUrl = "/Plugins/Shipping.PttKargo/logo.png",
                DisplayOrder = 1,
                IsActive = true
            });
        }

        // Add localization resources
        await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
        {
            // Plugin
            ["Plugins.Shipping.PttKargo.PluginName"] = "PTT Kargo",
            ["Plugins.Shipping.PttKargo.PluginDescription"] = "PTT Kargo entegrasyonu - Gönderi oluşturma ve takip",

            // Settings - Marketplace
            ["Plugins.Shipping.PttKargo.MarketplaceMusteriId"] = "Müşteri Numarası",
            ["Plugins.Shipping.PttKargo.MarketplaceMusteriId.Hint"] = "PTT tarafından verilen müşteri numarası",
            ["Plugins.Shipping.PttKargo.MarketplaceSifre"] = "Web Servis Şifresi",
            ["Plugins.Shipping.PttKargo.MarketplaceSifre.Hint"] = "PTT tarafından verilen web servis şifresi",
            ["Plugins.Shipping.PttKargo.UseSandbox"] = "Test Ortamı",
            ["Plugins.Shipping.PttKargo.UseSandbox.Hint"] = "PTT test ortamını kullan",
            ["Plugins.Shipping.PttKargo.MarketplaceContractEnabled"] = "Pazaryeri Anlaşması",
            ["Plugins.Shipping.PttKargo.MarketplaceContractEnabled.Hint"] = "Pazaryeri PTT anlaşması aktif",
            ["Plugins.Shipping.PttKargo.AllowVendorContracts"] = "Satıcı Anlaşması",
            ["Plugins.Shipping.PttKargo.AllowVendorContracts.Hint"] = "Satıcıların kendi PTT anlaşmalarını kullanmasına izin ver",

            // Settings - Sender
            ["Plugins.Shipping.PttKargo.DefaultSenderName"] = "Gönderici Adı",
            ["Plugins.Shipping.PttKargo.DefaultSenderName.Hint"] = "Varsayılan gönderici firma adı",
            ["Plugins.Shipping.PttKargo.DefaultSenderAddress"] = "Gönderici Adresi",
            ["Plugins.Shipping.PttKargo.DefaultSenderAddress.Hint"] = "Varsayılan gönderici adresi",
            ["Plugins.Shipping.PttKargo.DefaultSenderCityCode"] = "Gönderici İl Kodu",
            ["Plugins.Shipping.PttKargo.DefaultSenderCityCode.Hint"] = "Varsayılan gönderici il kodu",
            ["Plugins.Shipping.PttKargo.DefaultSenderDistrictCode"] = "Gönderici İlçe Kodu",
            ["Plugins.Shipping.PttKargo.DefaultSenderDistrictCode.Hint"] = "Varsayılan gönderici ilçe kodu",
            ["Plugins.Shipping.PttKargo.DefaultSenderPhone"] = "Gönderici Telefon",
            ["Plugins.Shipping.PttKargo.DefaultSenderPhone.Hint"] = "Varsayılan gönderici telefon numarası",

            // Messages
            ["Plugins.Shipping.PttKargo.ConnectionSuccess"] = "PTT API bağlantısı başarılı",
            ["Plugins.Shipping.PttKargo.ConnectionFailed"] = "PTT API bağlantısı başarısız",
            ["Plugins.Shipping.PttKargo.ShipmentCreated"] = "Gönderi oluşturuldu. Barkod: {0}",
            ["Plugins.Shipping.PttKargo.ShipmentNotFound"] = "Gönderi bulunamadı",
            ["Plugins.Shipping.PttKargo.CredentialsNotConfigured"] = "PTT credential'ları yapılandırılmamış"
        });

        await base.InstallAsync();
    }

    public override async Task UninstallAsync()
    {
        // Delete settings
        await _settingService.DeleteSettingAsync<PttKargoSettings>();

        // Deactivate provider (don't delete, as there might be historical data)
        var provider = await _shippingProviderService.GetBySystemNameAsync("Shipping.PttKargo");
        if (provider != null)
        {
            provider.IsActive = false;
            await _shippingProviderService.UpdateAsync(provider);
        }

        // Delete localization resources
        await _localizationService.DeleteLocaleResourcesAsync("Plugins.Shipping.PttKargo");

        await base.UninstallAsync();
    }

    public override string GetConfigurationPageUrl()
    {
        return $"{_webHelper.GetStoreLocation()}Admin/PttKargoAdmin/Configure";
    }
}
