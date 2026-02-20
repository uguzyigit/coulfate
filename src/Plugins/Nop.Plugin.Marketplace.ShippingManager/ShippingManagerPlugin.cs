using Nop.Core;
using Nop.Core.Domain.Shipping;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Tracking;

namespace Nop.Plugin.Marketplace.ShippingManager;

/// <summary>
/// Marketplace Shipping Manager Plugin
/// Integrates with NopCommerce's shipping system as a shipping rate computation method.
/// Combined with BypassShippingMethodSelectionIfOnlyOne setting, this auto-selects shipping.
/// Free shipping is handled by NopCommerce's built-in ShippingSettings.FreeShippingOverX.
/// </summary>
public class ShippingManagerPlugin : BasePlugin, IShippingRateComputationMethod
{
    private readonly ILocalizationService _localizationService;
    private readonly ISettingService _settingService;
    private readonly IWebHelper _webHelper;
    private readonly ShippingManagerSettings _settings;

    public ShippingManagerPlugin(
        ILocalizationService localizationService,
        ISettingService settingService,
        IWebHelper webHelper,
        ShippingManagerSettings settings)
    {
        _localizationService = localizationService;
        _settingService = settingService;
        _webHelper = webHelper;
        _settings = settings;
    }

    #region IShippingRateComputationMethod

    /// <summary>
    /// Gets available shipping options - returns a single option
    /// </summary>
    public async Task<GetShippingOptionResponse> GetShippingOptionsAsync(GetShippingOptionRequest getShippingOptionRequest)
    {
        ArgumentNullException.ThrowIfNull(getShippingOptionRequest);

        var response = new GetShippingOptionResponse();

        if (getShippingOptionRequest.Items == null || !getShippingOptionRequest.Items.Any())
        {
            response.AddError(await _localizationService.GetResourceAsync("Plugins.Shipping.Marketplace.NoShipmentItems"));
            return response;
        }

        // Return single shipping option with default rate
        // Free shipping threshold is handled by NopCommerce's built-in ShippingSettings
        // With BypassShippingMethodSelectionIfOnlyOne=true, this will be auto-selected
        response.ShippingOptions.Add(new ShippingOption
        {
            Name = _settings.ShippingMethodName ?? "Standart Kargo",
            Description = _settings.ShippingMethodDescription ?? "Satıcının tercih ettiği kargo ile gönderilir",
            Rate = _settings.DefaultShippingRate,
            TransitDays = null,
            ShippingRateComputationMethodSystemName = PluginDescriptor.SystemName
        });

        return response;
    }

    /// <summary>
    /// Gets fixed shipping rate for catalog display
    /// </summary>
    public Task<decimal?> GetFixedRateAsync(GetShippingOptionRequest getShippingOptionRequest)
    {
        return Task.FromResult<decimal?>(_settings.DefaultShippingRate);
    }

    /// <summary>
    /// Get associated shipment tracker
    /// </summary>
    public Task<IShipmentTracker> GetShipmentTrackerAsync()
    {
        // Tracking is handled by individual shipping providers (PTT Kargo, etc.)
        return Task.FromResult<IShipmentTracker>(null);
    }

    #endregion

    #region BasePlugin

    public override async Task InstallAsync()
    {
        // Note: Database tables are created by Marketplace.Core plugin
        // This plugin depends on Marketplace.Core

        // Save default settings
        await _settingService.SaveSettingAsync(new ShippingManagerSettings
        {
            DefaultShippingRate = 0,
            ShippingMethodName = "Standart Kargo",
            ShippingMethodDescription = "Satıcının tercih ettiği kargo ile gönderilir"
        });

        // Add localization resources
        await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
        {
            // Plugin info
            ["Plugins.Shipping.Marketplace.PluginName"] = "Marketplace Kargo",
            ["Plugins.Shipping.Marketplace.PluginDescription"] = "Pazaryeri kargo yöntemi - Satıcıların tercih ettiği kargo ile gönderim",

            // Settings labels
            ["Plugins.Shipping.Marketplace.DefaultShippingRate"] = "Varsayılan Kargo Ücreti",
            ["Plugins.Shipping.Marketplace.DefaultShippingRate.Hint"] = "Ücretsiz kargo koşulları sağlanmadığında uygulanacak kargo ücreti",
            ["Plugins.Shipping.Marketplace.ShippingMethodName"] = "Kargo Yöntemi Adı",
            ["Plugins.Shipping.Marketplace.ShippingMethodName.Hint"] = "Checkout'ta müşteriye gösterilecek kargo yöntemi adı",
            ["Plugins.Shipping.Marketplace.ShippingMethodDescription"] = "Kargo Yöntemi Açıklaması",
            ["Plugins.Shipping.Marketplace.ShippingMethodDescription.Hint"] = "Checkout'ta müşteriye gösterilecek açıklama",

            // Error messages
            ["Plugins.Shipping.Marketplace.NoShipmentItems"] = "Kargoya gönderilecek ürün bulunamadı",

            // Vendor panel
            ["Plugins.Shipping.Marketplace.Vendor.Settings"] = "Kargo Ayarları",
            ["Plugins.Shipping.Marketplace.Vendor.SelectProvider"] = "Kargo şirketi seçin",
            ["Plugins.Shipping.Marketplace.Vendor.UseMarketplaceContract"] = "Pazaryeri anlaşmasını kullan",
            ["Plugins.Shipping.Marketplace.Vendor.UseOwnContract"] = "Kendi anlaşmamı kullan",
            ["Plugins.Shipping.Marketplace.Vendor.SettingsSaved"] = "Kargo ayarları kaydedildi"
        });

        await base.InstallAsync();
    }

    public override async Task UninstallAsync()
    {
        // Delete settings
        await _settingService.DeleteSettingAsync<ShippingManagerSettings>();

        // Delete localization resources
        await _localizationService.DeleteLocaleResourcesAsync("Plugins.Shipping.Marketplace");

        // Note: Database tables are NOT deleted - they belong to Marketplace.Core
        // This ensures data is preserved if this plugin is accidentally uninstalled

        await base.UninstallAsync();
    }

    public override string GetConfigurationPageUrl()
    {
        return $"{_webHelper.GetStoreLocation()}Admin/ShippingManagerAdmin/Configure";
    }

    #endregion
}
