using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Widgets.GoogleAnalytics4.Domain;
using Nop.Plugin.Widgets.GoogleAnalytics4.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Widgets.GoogleAnalytics4.Controllers;

[Area(AreaNames.ADMIN)]
[AuthorizeAdmin]
[AutoValidateAntiforgeryToken]
public class GA4AdminController : BasePluginController
{
    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly IPermissionService _permissionService;
    private readonly ISettingService _settingService;
    private readonly IStoreContext _storeContext;

    public GA4AdminController(
        ILocalizationService localizationService,
        INotificationService notificationService,
        IPermissionService permissionService,
        ISettingService settingService,
        IStoreContext storeContext)
    {
        _localizationService = localizationService;
        _notificationService = notificationService;
        _permissionService = permissionService;
        _settingService = settingService;
        _storeContext = storeContext;
    }

    [CheckPermission(StandardPermission.Configuration.MANAGE_WIDGETS)]
    public async Task<IActionResult> Configure()
    {
        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var settings = await _settingService.LoadSettingAsync<GA4Settings>(storeScope);

        var model = new ConfigurationModel
        {
            ActiveStoreScopeConfiguration = storeScope,
            MeasurementId = settings.MeasurementId,
            ApiSecret = settings.ApiSecret,
            Enabled = settings.Enabled,
            EnableEcommerce = settings.EnableEcommerce,
            IncludeTax = settings.IncludeTax,
            IncludeCustomerId = settings.IncludeCustomerId,
            EnableDebugMode = settings.EnableDebugMode,
            TrackViewItemList = settings.TrackViewItemList,
            TrackViewItem = settings.TrackViewItem,
            TrackAddToCart = settings.TrackAddToCart,
            TrackRemoveFromCart = settings.TrackRemoveFromCart,
            TrackViewCart = settings.TrackViewCart,
            TrackBeginCheckout = settings.TrackBeginCheckout,
            TrackAddShippingInfo = settings.TrackAddShippingInfo,
            TrackAddPaymentInfo = settings.TrackAddPaymentInfo,
            TrackPurchase = settings.TrackPurchase,
            TrackRefund = settings.TrackRefund,
            TrackSearch = settings.TrackSearch
        };

        if (storeScope > 0)
        {
            model.MeasurementId_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.MeasurementId, storeScope);
            model.ApiSecret_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.ApiSecret, storeScope);
            model.Enabled_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.Enabled, storeScope);
            model.EnableEcommerce_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.EnableEcommerce, storeScope);
            model.IncludeTax_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.IncludeTax, storeScope);
            model.IncludeCustomerId_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.IncludeCustomerId, storeScope);
            model.EnableDebugMode_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.EnableDebugMode, storeScope);
            model.TrackViewItemList_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.TrackViewItemList, storeScope);
            model.TrackViewItem_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.TrackViewItem, storeScope);
            model.TrackAddToCart_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.TrackAddToCart, storeScope);
            model.TrackRemoveFromCart_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.TrackRemoveFromCart, storeScope);
            model.TrackViewCart_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.TrackViewCart, storeScope);
            model.TrackBeginCheckout_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.TrackBeginCheckout, storeScope);
            model.TrackAddShippingInfo_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.TrackAddShippingInfo, storeScope);
            model.TrackAddPaymentInfo_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.TrackAddPaymentInfo, storeScope);
            model.TrackPurchase_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.TrackPurchase, storeScope);
            model.TrackRefund_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.TrackRefund, storeScope);
            model.TrackSearch_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.TrackSearch, storeScope);
        }

        return View("~/Plugins/Widgets.GoogleAnalytics4/Views/Configure.cshtml", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Configuration.MANAGE_WIDGETS)]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var settings = await _settingService.LoadSettingAsync<GA4Settings>(storeScope);

        settings.MeasurementId = model.MeasurementId;
        settings.ApiSecret = model.ApiSecret;
        settings.Enabled = model.Enabled;
        settings.EnableEcommerce = model.EnableEcommerce;
        settings.IncludeTax = model.IncludeTax;
        settings.IncludeCustomerId = model.IncludeCustomerId;
        settings.EnableDebugMode = model.EnableDebugMode;
        settings.TrackViewItemList = model.TrackViewItemList;
        settings.TrackViewItem = model.TrackViewItem;
        settings.TrackAddToCart = model.TrackAddToCart;
        settings.TrackRemoveFromCart = model.TrackRemoveFromCart;
        settings.TrackViewCart = model.TrackViewCart;
        settings.TrackBeginCheckout = model.TrackBeginCheckout;
        settings.TrackAddShippingInfo = model.TrackAddShippingInfo;
        settings.TrackAddPaymentInfo = model.TrackAddPaymentInfo;
        settings.TrackPurchase = model.TrackPurchase;
        settings.TrackRefund = model.TrackRefund;
        settings.TrackSearch = model.TrackSearch;

        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.MeasurementId, model.MeasurementId_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.ApiSecret, model.ApiSecret_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.Enabled, model.Enabled_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.EnableEcommerce, model.EnableEcommerce_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.IncludeTax, model.IncludeTax_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.IncludeCustomerId, model.IncludeCustomerId_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.EnableDebugMode, model.EnableDebugMode_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.TrackViewItemList, model.TrackViewItemList_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.TrackViewItem, model.TrackViewItem_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.TrackAddToCart, model.TrackAddToCart_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.TrackRemoveFromCart, model.TrackRemoveFromCart_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.TrackViewCart, model.TrackViewCart_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.TrackBeginCheckout, model.TrackBeginCheckout_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.TrackAddShippingInfo, model.TrackAddShippingInfo_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.TrackAddPaymentInfo, model.TrackAddPaymentInfo_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.TrackPurchase, model.TrackPurchase_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.TrackRefund, model.TrackRefund_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.TrackSearch, model.TrackSearch_OverrideForStore, storeScope, false);

        await _settingService.ClearCacheAsync();

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

        return await Configure();
    }
}
