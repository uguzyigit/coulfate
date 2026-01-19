using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Marketplace.Core.Domain;
using Nop.Plugin.Marketplace.Core.Models;
using Nop.Plugin.Marketplace.Core.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using NopVendorService = Nop.Services.Vendors.IVendorService;

namespace Nop.Plugin.Marketplace.Core.Controllers;

/// <summary>
/// Marketplace Vendor Extension controller for admin panel
/// Manages marketplace-specific vendor settings (extends NopCommerce native vendors)
/// </summary>
[Area(AreaNames.ADMIN)]
[AuthorizeAdmin]
[AutoValidateAntiforgeryToken]
public class MarketplaceVendorController : BasePluginController
{
    private readonly IVendorExtensionService _vendorExtensionService;
    private readonly NopVendorService _nopVendorService;
    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly ISettingService _settingService;

    public MarketplaceVendorController(
        IVendorExtensionService vendorExtensionService,
        NopVendorService nopVendorService,
        ILocalizationService localizationService,
        INotificationService notificationService,
        ISettingService settingService)
    {
        _vendorExtensionService = vendorExtensionService;
        _nopVendorService = nopVendorService;
        _localizationService = localizationService;
        _notificationService = notificationService;
        _settingService = settingService;
    }

    /// <summary>
    /// Configure plugin
    /// </summary>
    public async Task<IActionResult> Configure()
    {
        var settings = await _settingService.LoadSettingAsync<MarketplaceCoreSettings>();
        var model = new ConfigurationModel
        {
            Enabled = settings.Enabled,
            RequireVendorApproval = settings.RequireVendorApproval,
            MinimumPayoutAmount = settings.MinimumPayoutAmount
        };

        return View("~/Plugins/Marketplace.Core/Views/Configure.cshtml", model);
    }

    /// <summary>
    /// Configure plugin (POST)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!ModelState.IsValid)
            return await Configure();

        var settings = await _settingService.LoadSettingAsync<MarketplaceCoreSettings>();
        settings.Enabled = model.Enabled;
        settings.RequireVendorApproval = model.RequireVendorApproval;
        settings.MinimumPayoutAmount = model.MinimumPayoutAmount;

        await _settingService.SaveSettingAsync(settings);

        _notificationService.SuccessNotification(
            await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

        return await Configure();
    }

    /// <summary>
    /// List all vendor extensions
    /// </summary>
    public async Task<IActionResult> Index()
    {
        var vendorExtensions = await _vendorExtensionService.GetAllAsync();

        var models = new List<VendorExtensionModel>();
        foreach (var ve in vendorExtensions)
        {
            var vendor = await _nopVendorService.GetVendorByIdAsync(ve.VendorId);
            models.Add(new VendorExtensionModel
            {
                Id = ve.Id,
                VendorId = ve.VendorId,
                VendorName = vendor?.Name ?? "Unknown",
                Code = ve.Code,
                MarketplaceStatus = ve.MarketplaceStatus,
                Phone = ve.Phone,
                BankAccountName = ve.BankAccountName,
                BankIban = ve.BankIban,
                TaxNumber = ve.TaxNumber,
                TradeRegistryNumber = ve.TradeRegistryNumber,
                RequiresProductApproval = ve.RequiresProductApproval,
                MinimumPayoutAmount = ve.MinimumPayoutAmount,
                CreatedOnUtc = ve.CreatedOnUtc,
                UpdatedOnUtc = ve.UpdatedOnUtc
            });
        }

        return View("~/Plugins/Marketplace.Core/Views/Vendor/Index.cshtml", models);
    }

    /// <summary>
    /// Edit vendor extension
    /// </summary>
    public async Task<IActionResult> Edit(int id)
    {
        var vendorExtension = await _vendorExtensionService.GetByIdAsync(id);
        if (vendorExtension == null)
            return RedirectToAction("Index");

        var vendor = await _nopVendorService.GetVendorByIdAsync(vendorExtension.VendorId);

        var model = new VendorExtensionModel
        {
            Id = vendorExtension.Id,
            VendorId = vendorExtension.VendorId,
            VendorName = vendor?.Name ?? "Unknown",
            Code = vendorExtension.Code,
            MarketplaceStatus = vendorExtension.MarketplaceStatus,
            Phone = vendorExtension.Phone,
            BankAccountName = vendorExtension.BankAccountName,
            BankIban = vendorExtension.BankIban,
            TaxNumber = vendorExtension.TaxNumber,
            TradeRegistryNumber = vendorExtension.TradeRegistryNumber,
            RequiresProductApproval = vendorExtension.RequiresProductApproval,
            MinimumPayoutAmount = vendorExtension.MinimumPayoutAmount,
            CreatedOnUtc = vendorExtension.CreatedOnUtc,
            UpdatedOnUtc = vendorExtension.UpdatedOnUtc
        };

        return View("~/Plugins/Marketplace.Core/Views/Vendor/Edit.cshtml", model);
    }

    /// <summary>
    /// Edit vendor extension (POST)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Edit(VendorExtensionModel model)
    {
        var vendorExtension = await _vendorExtensionService.GetByIdAsync(model.Id);
        if (vendorExtension == null)
            return RedirectToAction("Index");

        if (!ModelState.IsValid)
        {
            var vendor = await _nopVendorService.GetVendorByIdAsync(vendorExtension.VendorId);
            model.VendorName = vendor?.Name ?? "Unknown";
            return View("~/Plugins/Marketplace.Core/Views/Vendor/Edit.cshtml", model);
        }

        vendorExtension.Code = model.Code;
        vendorExtension.MarketplaceStatus = model.MarketplaceStatus;
        vendorExtension.Phone = model.Phone;
        vendorExtension.BankAccountName = model.BankAccountName;
        vendorExtension.BankIban = model.BankIban;
        vendorExtension.TaxNumber = model.TaxNumber;
        vendorExtension.TradeRegistryNumber = model.TradeRegistryNumber;
        vendorExtension.RequiresProductApproval = model.RequiresProductApproval;
        vendorExtension.MinimumPayoutAmount = model.MinimumPayoutAmount;

        await _vendorExtensionService.UpdateAsync(vendorExtension);

        _notificationService.SuccessNotification(
            await _localizationService.GetResourceAsync("Plugins.Marketplace.Core.Vendor.Updated"));

        return RedirectToAction("Index");
    }

    /// <summary>
    /// View vendor extension details
    /// </summary>
    public async Task<IActionResult> Details(int id)
    {
        var vendorExtension = await _vendorExtensionService.GetByIdAsync(id);
        if (vendorExtension == null)
            return RedirectToAction("Index");

        var vendor = await _nopVendorService.GetVendorByIdAsync(vendorExtension.VendorId);

        var model = new VendorExtensionModel
        {
            Id = vendorExtension.Id,
            VendorId = vendorExtension.VendorId,
            VendorName = vendor?.Name ?? "Unknown",
            Code = vendorExtension.Code,
            MarketplaceStatus = vendorExtension.MarketplaceStatus,
            Phone = vendorExtension.Phone,
            BankAccountName = vendorExtension.BankAccountName,
            BankIban = vendorExtension.BankIban,
            TaxNumber = vendorExtension.TaxNumber,
            TradeRegistryNumber = vendorExtension.TradeRegistryNumber,
            RequiresProductApproval = vendorExtension.RequiresProductApproval,
            MinimumPayoutAmount = vendorExtension.MinimumPayoutAmount,
            CreatedOnUtc = vendorExtension.CreatedOnUtc,
            UpdatedOnUtc = vendorExtension.UpdatedOnUtc
        };

        return View("~/Plugins/Marketplace.Core/Views/Vendor/Details.cshtml", model);
    }
}
