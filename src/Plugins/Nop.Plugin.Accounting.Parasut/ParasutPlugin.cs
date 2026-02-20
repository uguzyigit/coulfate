using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.ScheduleTasks;
using Nop.Plugin.Accounting.Parasut.Domain;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.ScheduleTasks;

namespace Nop.Plugin.Accounting.Parasut;

public class ParasutPlugin : BasePlugin
{
    private readonly IWebHelper _webHelper;
    private readonly ISettingService _settingService;
    private readonly ILocalizationService _localizationService;
    private readonly IScheduleTaskService _scheduleTaskService;
    private readonly ILanguageService _languageService;

    public ParasutPlugin(
        IWebHelper webHelper,
        ISettingService settingService,
        ILocalizationService localizationService,
        IScheduleTaskService scheduleTaskService,
        ILanguageService languageService)
    {
        _webHelper = webHelper;
        _settingService = settingService;
        _localizationService = localizationService;
        _scheduleTaskService = scheduleTaskService;
        _languageService = languageService;
    }

    public override string GetConfigurationPageUrl()
    {
        return $"{_webHelper.GetStoreLocation()}Admin/Parasut/Configure";
    }

    public override async Task InstallAsync()
    {
        var settings = new ParasutSettings
        {
            Enabled = false,
            DefaultVatRate = 20m,
            PaymentTermDays = 15,
            
            InvoiceStrategy = InvoiceCreationStrategy.AfterReturnPeriod,
            ReturnPeriodDays = 14,
            StartFrom = ReturnPeriodStartFrom.DeliveryDate,
            AutoProcessExpiredReturnPeriods = true,
            
            AutoCreateCommissionInvoice = true,
            AutoCreateShippingInvoice = false,
            AutoCreatePenaltyInvoice = true,
            
            UseInternetSaleInfo = true,
            SendVendorNotifications = true
        };
        await _settingService.SaveSettingAsync(settings);

        await _localizationService.AddOrUpdateLocaleResourceAsync(
            LocalizationResources.GetEnglishResources());

        var turkishLanguage = (await _languageService.GetAllLanguagesAsync())
            .FirstOrDefault(l => l.LanguageCulture == "tr-TR");
        
        if (turkishLanguage != null)
        {
            await _localizationService.AddOrUpdateLocaleResourceAsync(
                LocalizationResources.GetTurkishResources(), 
                turkishLanguage.Id);
        }

        var existingTask = (await _scheduleTaskService.GetAllTasksAsync())
            .FirstOrDefault(t => t.Type.Contains("ReturnPeriodExpiredInvoiceTask"));
        
        if (existingTask == null)
        {
            await _scheduleTaskService.InsertTaskAsync(new ScheduleTask
            {
                Name = "Paraşüt - İade Süresi Dolmuş Faturalar",
                Seconds = 86400,
                Type = "Nop.Plugin.Accounting.Parasut.Tasks.ReturnPeriodExpiredInvoiceTask, Nop.Plugin.Accounting.Parasut",
                Enabled = true,
                StopOnError = false
            });
        }

        await base.InstallAsync();
    }

    public override async Task UninstallAsync()
    {
        await _settingService.DeleteSettingAsync<ParasutSettings>();
        await _localizationService.DeleteLocaleResourcesAsync("Plugins.Accounting.Parasut");

        var task = (await _scheduleTaskService.GetAllTasksAsync())
            .FirstOrDefault(t => t.Type.Contains("ReturnPeriodExpiredInvoiceTask"));
        
        if (task != null)
            await _scheduleTaskService.DeleteTaskAsync(task);

        await base.UninstallAsync();
    }
}
