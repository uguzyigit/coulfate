using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Marketplace.ShippingManager.Models.Admin;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Marketplace.ShippingManager.Controllers;

[Area(AreaNames.ADMIN)]
[AuthorizeAdmin]
[AutoValidateAntiforgeryToken]
public class ShippingManagerAdminController : BasePluginController
{
    private readonly IPermissionService _permissionService;
    private readonly ISettingService _settingService;
    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly ShippingManagerSettings _settings;

    public ShippingManagerAdminController(
        IPermissionService permissionService,
        ISettingService settingService,
        ILocalizationService localizationService,
        INotificationService notificationService,
        ShippingManagerSettings settings)
    {
        _permissionService = permissionService;
        _settingService = settingService;
        _localizationService = localizationService;
        _notificationService = notificationService;
        _settings = settings;
    }

    public async Task<IActionResult> Configure()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_SHIPPING_SETTINGS))
            return AccessDeniedView();

        var model = new ConfigurationModel
        {
            DefaultShippingRate = _settings.DefaultShippingRate,
            ShippingMethodName = _settings.ShippingMethodName,
            ShippingMethodDescription = _settings.ShippingMethodDescription
        };

        return View("~/Plugins/Marketplace.ShippingManager/Views/ShippingManagerAdmin/Configure.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_SHIPPING_SETTINGS))
            return AccessDeniedView();

        if (!ModelState.IsValid)
            return View("~/Plugins/Marketplace.ShippingManager/Views/ShippingManagerAdmin/Configure.cshtml", model);

        _settings.DefaultShippingRate = model.DefaultShippingRate;
        _settings.ShippingMethodName = model.ShippingMethodName;
        _settings.ShippingMethodDescription = model.ShippingMethodDescription;

        await _settingService.SaveSettingAsync(_settings);

        _notificationService.SuccessNotification(
            await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

        return RedirectToAction("Configure");
    }
}
