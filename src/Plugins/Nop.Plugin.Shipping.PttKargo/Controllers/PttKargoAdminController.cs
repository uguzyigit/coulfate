using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Shipping.PttKargo.Models;
using Nop.Plugin.Shipping.PttKargo.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Shipping.PttKargo.Controllers;

[Area(AreaNames.ADMIN)]
[AuthorizeAdmin]
[AutoValidateAntiforgeryToken]
public class PttKargoAdminController : BasePluginController
{
    private readonly IPermissionService _permissionService;
    private readonly ISettingService _settingService;
    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly IPttApiService _pttApiService;
    private readonly PttKargoSettings _settings;

    public PttKargoAdminController(
        IPermissionService permissionService,
        ISettingService settingService,
        ILocalizationService localizationService,
        INotificationService notificationService,
        IPttApiService pttApiService,
        PttKargoSettings settings)
    {
        _permissionService = permissionService;
        _settingService = settingService;
        _localizationService = localizationService;
        _notificationService = notificationService;
        _pttApiService = pttApiService;
        _settings = settings;
    }

    public async Task<IActionResult> Configure()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_SHIPPING_SETTINGS))
            return AccessDeniedView();

        var model = new ConfigurationModel
        {
            MarketplaceMusteriId = _settings.MarketplaceMusteriId,
            MarketplaceSifre = _settings.MarketplaceSifre,
            UseSandbox = _settings.UseSandbox,
            MarketplaceContractEnabled = _settings.MarketplaceContractEnabled,
            AllowVendorContracts = _settings.AllowVendorContracts,
            DefaultSenderName = _settings.DefaultSenderName,
            DefaultSenderAddress = _settings.DefaultSenderAddress,
            DefaultSenderCityCode = _settings.DefaultSenderCityCode,
            DefaultSenderDistrictCode = _settings.DefaultSenderDistrictCode,
            DefaultSenderPhone = _settings.DefaultSenderPhone
        };

        return View("~/Plugins/Shipping.PttKargo/Views/PttKargoAdmin/Configure.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_SHIPPING_SETTINGS))
            return AccessDeniedView();

        if (!ModelState.IsValid)
            return View("~/Plugins/Shipping.PttKargo/Views/PttKargoAdmin/Configure.cshtml", model);

        _settings.MarketplaceMusteriId = model.MarketplaceMusteriId;
        _settings.MarketplaceSifre = model.MarketplaceSifre;
        _settings.UseSandbox = model.UseSandbox;
        _settings.MarketplaceContractEnabled = model.MarketplaceContractEnabled;
        _settings.AllowVendorContracts = model.AllowVendorContracts;
        _settings.DefaultSenderName = model.DefaultSenderName;
        _settings.DefaultSenderAddress = model.DefaultSenderAddress;
        _settings.DefaultSenderCityCode = model.DefaultSenderCityCode;
        _settings.DefaultSenderDistrictCode = model.DefaultSenderDistrictCode;
        _settings.DefaultSenderPhone = model.DefaultSenderPhone;

        await _settingService.SaveSettingAsync(_settings);

        _notificationService.SuccessNotification(
            await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

        return RedirectToAction("Configure");
    }

    [HttpPost]
    public async Task<IActionResult> TestConnection()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_SHIPPING_SETTINGS))
            return Json(new { success = false, message = "Yetkisiz erişim" });

        try
        {
            var credentials = await _pttApiService.GetMarketplaceCredentialsAsync();
            if (credentials == null)
            {
                return Json(new { success = false, message = "Pazaryeri credential'ları yapılandırılmamış" });
            }

            // Basit bir test sorgusu yap (var olmayan bir barkod ile)
            var result = await _pttApiService.TrackByBarcodeAsync("TEST123456789", credentials);

            // PTT API'den yanıt geldiyse (hata olsa bile) bağlantı başarılıdır
            if (result.RawResponse != null)
            {
                return Json(new { success = true, message = "PTT API bağlantısı başarılı" });
            }

            return Json(new { success = false, message = result.ErrorMessage ?? "Bağlantı test edilemedi" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"Hata: {ex.Message}" });
        }
    }
}
