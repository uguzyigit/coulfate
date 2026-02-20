using Marketplace.Abstractions.Domain;
using Marketplace.Abstractions.Services;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Marketplace.Core.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Vendors;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Marketplace.Core.Controllers;

[Area(AreaNames.ADMIN)]
[AuthorizeAdmin]
[AutoValidateAntiforgeryToken]
public class MarketplaceCoreAdminController : BasePluginController
{
    private readonly ISettingService _settingService;
    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly IPermissionService _permissionService;
    private readonly IVendorExtensionService _vendorExtensionService;
    private readonly IVendorService _vendorService;

    public MarketplaceCoreAdminController(
        ISettingService settingService,
        ILocalizationService localizationService,
        INotificationService notificationService,
        IPermissionService permissionService,
        IVendorExtensionService vendorExtensionService,
        IVendorService vendorService)
    {
        _settingService = settingService;
        _localizationService = localizationService;
        _notificationService = notificationService;
        _permissionService = permissionService;
        _vendorExtensionService = vendorExtensionService;
        _vendorService = vendorService;
    }

    #region Configuration

    public async Task<IActionResult> Configure()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        var settings = await _settingService.LoadSettingAsync<MarketplaceCoreSettings>();

        var model = new ConfigurationModel
        {
            Enabled = settings.Enabled,
            RequireVendorApproval = settings.RequireVendorApproval,
            MinimumPayoutAmount = settings.MinimumPayoutAmount
        };

        return View("~/Plugins/Marketplace.Core/Views/Configure.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        var settings = await _settingService.LoadSettingAsync<MarketplaceCoreSettings>();

        settings.Enabled = model.Enabled;
        settings.RequireVendorApproval = model.RequireVendorApproval;
        settings.MinimumPayoutAmount = model.MinimumPayoutAmount;

        await _settingService.SaveSettingAsync(settings);

        _notificationService.SuccessNotification(
            await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

        return RedirectToAction("Configure");
    }

    #endregion

    #region Vendor Extension

    public async Task<IActionResult> VendorExtension(int vendorId)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        var vendor = await _vendorService.GetVendorByIdAsync(vendorId);
        if (vendor == null)
            return RedirectToAction("List", "Vendor");

        var extension = await _vendorExtensionService.GetByVendorIdAsync(vendorId);

        var model = new VendorExtensionModel
        {
            VendorId = vendorId,
            VendorName = vendor.Name
        };

        if (extension != null)
        {
            model.Id = extension.Id;
            model.Code = extension.Code;
            model.MarketplaceStatus = extension.MarketplaceStatus;
            model.Phone = extension.Phone;
            model.TaxNumber = extension.TaxNumber;
            model.TradeRegistryNumber = extension.TradeRegistryNumber;
            model.BankAccountName = extension.BankAccountName;
            model.BankIban = extension.BankIban;
            model.MinimumPayoutAmount = extension.MinimumPayoutAmount;
            model.RequiresProductApproval = extension.RequiresProductApproval;
            model.CreatedOnUtc = extension.CreatedOnUtc;
            model.UpdatedOnUtc = extension.UpdatedOnUtc;
        }

        return View("~/Plugins/Marketplace.Core/Views/VendorExtension.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> VendorExtension(VendorExtensionModel model)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        var vendor = await _vendorService.GetVendorByIdAsync(model.VendorId);
        if (vendor == null)
            return RedirectToAction("List", "Vendor");

        var extension = await _vendorExtensionService.GetByVendorIdAsync(model.VendorId);

        if (extension == null)
        {
            // Create new extension
            extension = new VendorExtension
            {
                VendorId = model.VendorId,
                Code = model.Code ?? await _vendorExtensionService.GenerateVendorCodeAsync(vendor.Name),
                MarketplaceStatus = model.MarketplaceStatus,
                Phone = model.Phone ?? string.Empty,
                TaxNumber = model.TaxNumber ?? string.Empty,
                TradeRegistryNumber = model.TradeRegistryNumber ?? string.Empty,
                BankAccountName = model.BankAccountName ?? string.Empty,
                BankIban = model.BankIban ?? string.Empty,
                MinimumPayoutAmount = model.MinimumPayoutAmount,
                RequiresProductApproval = model.RequiresProductApproval
            };

            await _vendorExtensionService.InsertAsync(extension);
        }
        else
        {
            // Update existing extension
            extension.Code = model.Code ?? extension.Code;
            extension.MarketplaceStatus = model.MarketplaceStatus;
            extension.Phone = model.Phone ?? string.Empty;
            extension.TaxNumber = model.TaxNumber ?? string.Empty;
            extension.TradeRegistryNumber = model.TradeRegistryNumber ?? string.Empty;
            extension.BankAccountName = model.BankAccountName ?? string.Empty;
            extension.BankIban = model.BankIban ?? string.Empty;
            extension.MinimumPayoutAmount = model.MinimumPayoutAmount;
            extension.RequiresProductApproval = model.RequiresProductApproval;

            await _vendorExtensionService.UpdateAsync(extension);
        }

        _notificationService.SuccessNotification(
            await _localizationService.GetResourceAsync("Admin.Common.DataEditSuccess"));

        return RedirectToAction("VendorExtension", new { vendorId = model.VendorId });
    }

    #endregion
}
