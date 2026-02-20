using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;

namespace Nop.Plugin.Marketplace.Core;

/// <summary>
/// Marketplace Core plugin
/// </summary>
public class MarketplaceCorePlugin : BasePlugin
{
    private readonly IWebHelper _webHelper;
    private readonly ISettingService _settingService;
    private readonly ILocalizationService _localizationService;
    private readonly INopDataProvider _dataProvider;

    public MarketplaceCorePlugin(
        IWebHelper webHelper,
        ISettingService settingService,
        ILocalizationService localizationService,
        INopDataProvider dataProvider)
    {
        _webHelper = webHelper;
        _settingService = settingService;
        _localizationService = localizationService;
        _dataProvider = dataProvider;
    }

    public override string GetConfigurationPageUrl()
    {
        return $"{_webHelper.GetStoreLocation()}Admin/MarketplaceCoreAdmin/Configure";
    }

    public override async Task InstallAsync()
    {
        // Install settings
        var settings = new MarketplaceCoreSettings
        {
            Enabled = true,
            RequireVendorApproval = true,
            MinimumPayoutAmount = 100m,
            MaxPendingDays = 30
        };
        await _settingService.SaveSettingAsync(settings);

        // Create all database tables using InstallationData
        await _dataProvider.ExecuteNonQueryAsync(Data.InstallationData.CreateTablesScript);

        // Install localization resources
        await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
        {
            ["Plugins.Marketplace.Core.FriendlyName"] = "Marketplace Core",
            ["Plugins.Marketplace.Core.Description"] = "Core functionality for marketplace operations",
            ["Plugins.Marketplace.Core.Fields.Enabled"] = "Enabled",
            ["Plugins.Marketplace.Core.Fields.RequireVendorApproval"] = "Require Vendor Approval",
            ["Plugins.Marketplace.Core.Fields.MinimumPayoutAmount"] = "Minimum Payout Amount",
            ["Plugins.Marketplace.Core.Fields.MaxPendingDays"] = "Maximum Pending Days",
            ["Plugins.Marketplace.Core.Menu.Marketplace"] = "Marketplace",
            ["Enums.ProductSortingEnum.BestSelling"] = "En Çok Satan",
            ["Enums.ProductSortingEnum.MostWishlisted"] = "En Favoriler",
            ["Enums.ProductSortingEnum.MostReviewed"] = "En Çok Değerlendirilen"
        });

        await base.InstallAsync();
    }

    public override async Task UninstallAsync()
    {
        // Delete settings
        await _settingService.DeleteSettingAsync<MarketplaceCoreSettings>();

        // Delete localization resources
        await _localizationService.DeleteLocaleResourcesAsync("Plugins.Marketplace.Core");

        // Drop all tables (WARNING: This will delete all marketplace data!)
        await _dataProvider.ExecuteNonQueryAsync(Data.InstallationData.DropTablesScript);

        await base.UninstallAsync();
    }
}
