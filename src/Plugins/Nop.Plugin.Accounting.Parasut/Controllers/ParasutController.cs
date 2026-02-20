using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Accounting.Parasut.Domain;
using Nop.Plugin.Accounting.Parasut.Models.Configuration;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Accounting.Parasut.Controllers;

[Area(AreaNames.ADMIN)]
[AuthorizeAdmin]
[AutoValidateAntiforgeryToken]
public class ParasutController : BasePluginController
{
    private readonly ISettingService _settingService;
    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly IPermissionService _permissionService;

    public ParasutController(
        ISettingService settingService,
        ILocalizationService localizationService,
        INotificationService notificationService,
        IPermissionService permissionService)
    {
        _settingService = settingService;
        _localizationService = localizationService;
        _notificationService = notificationService;
        _permissionService = permissionService;
    }

    public async Task<IActionResult> Configure()
    {
        var settings = await _settingService.LoadSettingAsync<ParasutSettings>();

        var model = new ConfigurationModel
        {
            Enabled = settings.Enabled,
            CompanyId = settings.CompanyId,
            ClientId = settings.ClientId,
            ClientSecret = settings.ClientSecret,
            Username = settings.Username,
            Password = settings.Password,
            DefaultVatRate = settings.DefaultVatRate,
            
            // Invoice Strategy
            InvoiceStrategyId = (int)settings.InvoiceStrategy,
            ReturnPeriodDays = settings.ReturnPeriodDays,
            StartFromId = (int)settings.StartFrom,
            AutoProcessExpiredReturnPeriods = settings.AutoProcessExpiredReturnPeriods,
            
            // Auto-create flags
            AutoCreateCommissionInvoice = settings.AutoCreateCommissionInvoice,
            AutoCreateShippingInvoice = settings.AutoCreateShippingInvoice,
            AutoCreatePenaltyInvoice = settings.AutoCreatePenaltyInvoice
        };

        // Dropdown options
        model.AvailableInvoiceStrategies = new List<SelectListItem>
        {
            new SelectListItem 
            { 
                Text = await _localizationService.GetResourceAsync("Plugins.Accounting.Parasut.InvoiceStrategy.OnPayment"),
                Value = ((int)InvoiceCreationStrategy.OnPayment).ToString() 
            },
            new SelectListItem 
            { 
                Text = await _localizationService.GetResourceAsync("Plugins.Accounting.Parasut.InvoiceStrategy.OnDelivery"),
                Value = ((int)InvoiceCreationStrategy.OnDelivery).ToString() 
            },
            new SelectListItem 
            { 
                Text = await _localizationService.GetResourceAsync("Plugins.Accounting.Parasut.InvoiceStrategy.AfterReturnPeriod"),
                Value = ((int)InvoiceCreationStrategy.AfterReturnPeriod).ToString(),
                Selected = true
            },
            new SelectListItem 
            { 
                Text = await _localizationService.GetResourceAsync("Plugins.Accounting.Parasut.InvoiceStrategy.Manual"),
                Value = ((int)InvoiceCreationStrategy.Manual).ToString() 
            }
        };

        model.AvailableStartFromOptions = new List<SelectListItem>
        {
            new SelectListItem 
            { 
                Text = await _localizationService.GetResourceAsync("Plugins.Accounting.Parasut.StartFrom.OrderDate"),
                Value = ((int)ReturnPeriodStartFrom.OrderDate).ToString() 
            },
            new SelectListItem 
            { 
                Text = await _localizationService.GetResourceAsync("Plugins.Accounting.Parasut.StartFrom.PaymentDate"),
                Value = ((int)ReturnPeriodStartFrom.PaymentDate).ToString() 
            },
            new SelectListItem 
            { 
                Text = await _localizationService.GetResourceAsync("Plugins.Accounting.Parasut.StartFrom.ShippingDate"),
                Value = ((int)ReturnPeriodStartFrom.ShippingDate).ToString() 
            },
            new SelectListItem 
            { 
                Text = await _localizationService.GetResourceAsync("Plugins.Accounting.Parasut.StartFrom.DeliveryDate"),
                Value = ((int)ReturnPeriodStartFrom.DeliveryDate).ToString(),
                Selected = true
            }
        };

        return View("~/Plugins/Accounting.Parasut/Views/Configure.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!ModelState.IsValid)
            return await Configure();

        var settings = await _settingService.LoadSettingAsync<ParasutSettings>();
        
        settings.Enabled = model.Enabled;
        settings.CompanyId = model.CompanyId;
        settings.ClientId = model.ClientId;
        settings.ClientSecret = model.ClientSecret;
        settings.Username = model.Username;
        
        if (!string.IsNullOrEmpty(model.Password))
            settings.Password = model.Password;
        
        settings.DefaultVatRate = model.DefaultVatRate;
        
        // Invoice Strategy
        settings.InvoiceStrategy = (InvoiceCreationStrategy)model.InvoiceStrategyId;
        settings.ReturnPeriodDays = model.ReturnPeriodDays;
        settings.StartFrom = (ReturnPeriodStartFrom)model.StartFromId;
        settings.AutoProcessExpiredReturnPeriods = model.AutoProcessExpiredReturnPeriods;
        
        // Auto-create flags
        settings.AutoCreateCommissionInvoice = model.AutoCreateCommissionInvoice;
        settings.AutoCreateShippingInvoice = model.AutoCreateShippingInvoice;
        settings.AutoCreatePenaltyInvoice = model.AutoCreatePenaltyInvoice;

        await _settingService.SaveSettingAsync(settings);

        _notificationService.SuccessNotification(
            await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

        return await Configure();
    }
}
