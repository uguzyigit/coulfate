using Marketplace.Abstractions.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core;
using Nop.Core.Domain.Cms;
using Nop.Core.Events;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Mapping;
using Nop.Plugin.Marketplace.VendorExtensions.Data;
using Nop.Plugin.Marketplace.VendorExtensions.Infrastructure.EventConsumers;
using Nop.Plugin.Marketplace.VendorExtensions.Services;
using Nop.Services.Configuration;
using Nop.Services.Events;
using Nop.Web.Framework.Events;

namespace Nop.Plugin.Marketplace.VendorExtensions.Infrastructure;

public class NopStartup : INopStartup
{
    private const string PLUGIN_SYSTEM_NAME = "Marketplace.VendorExtensions";

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Register services
        services.AddScoped<IVendorAccountService, VendorAccountService>();
        services.AddScoped<IVendorProductService, VendorProductService>();
        services.AddScoped<ICategorySpecMappingService, CategorySpecMappingService>();

        // Register table name compatibility (use AdditionalNameCompatibilities to avoid conflicts)
        if (!NameCompatibilityManager.AdditionalNameCompatibilities.Contains(typeof(Data.VendorExtensionsNameCompatibility)))
            NameCompatibilityManager.AdditionalNameCompatibilities.Add(typeof(Data.VendorExtensionsNameCompatibility));

        // Register event consumers
        services.AddScoped<IConsumer<EntityInsertedEvent<BaseEntity>>, OrderCommissionCreatedConsumer>();
        services.AddScoped<IConsumer<AdminMenuCreatedEvent>, AdminMenuEventConsumer>();
        services.AddScoped<IConsumer<AdminMenuCreatedEvent>, VendorMenuEventConsumer>();
    }

    public void Configure(IApplicationBuilder application)
    {
        // Ensure this plugin is registered as an active widget
        var settingService = EngineContext.Current.Resolve<ISettingService>();
        var widgetSettings = EngineContext.Current.Resolve<WidgetSettings>();

        if (!widgetSettings.ActiveWidgetSystemNames.Contains(PLUGIN_SYSTEM_NAME))
        {
            widgetSettings.ActiveWidgetSystemNames.Add(PLUGIN_SYSTEM_NAME);
            settingService.SaveSettingAsync(widgetSettings).GetAwaiter().GetResult();
        }

        // Ensure new tables exist (for existing installations that won't run InstallAsync again)
        try
        {
            var dataProvider = EngineContext.Current.Resolve<INopDataProvider>();
            dataProvider.ExecuteNonQueryAsync(InstallationData.CreateCategorySpecAttrTableScript).GetAwaiter().GetResult();
        }
        catch
        {
            // Table may already exist, ignore
        }
    }

    public int Order => 3000;
}
