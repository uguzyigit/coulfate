using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.Marketplace.Performance.Infrastructure.EventConsumers;

public class AdminMenuEventConsumer : IConsumer<AdminMenuCreatedEvent>
{
    private readonly ILocalizationService _localizationService;
    private readonly IPluginManager<IPlugin> _pluginManager;

    public AdminMenuEventConsumer(
        ILocalizationService localizationService,
        IPluginManager<IPlugin> pluginManager)
    {
        _localizationService = localizationService;
        _pluginManager = pluginManager;
    }

    public async Task HandleEventAsync(AdminMenuCreatedEvent eventMessage)
    {
        var plugin = await _pluginManager.LoadPluginBySystemNameAsync("Marketplace.Performance");
        if (plugin == null)
            return;

        // Create top-level Performans menu
        var performanceMenuItem = new AdminMenuItem
        {
            SystemName = "Marketplace.Performance",
            Title = await _localizationService.GetResourceAsync("Plugins.Marketplace.Performance.Menu.Performance"),
            IconClass = "fas fa-chart-line",
            Visible = true
        };

        performanceMenuItem.ChildNodes.Add(new AdminMenuItem
        {
            SystemName = "Marketplace.Performance.Product",
            Title = await _localizationService.GetResourceAsync("Plugins.Marketplace.Performance.Menu.ProductPerformance"),
            IconClass = "fas fa-chart-line",
            Url = "/Admin/MarketplacePerformance/ProductPerformance",
            Visible = true
        });

        performanceMenuItem.ChildNodes.Add(new AdminMenuItem
        {
            SystemName = "Marketplace.Performance.Sales",
            Title = await _localizationService.GetResourceAsync("Plugins.Marketplace.Performance.Menu.SalesPerformance"),
            IconClass = "fas fa-chart-bar",
            Url = "/Admin/MarketplacePerformance/SalesPerformance",
            Visible = true
        });

        // Insert after Catalog (or at root level)
        var catalogMenuItem = eventMessage.RootMenuItem.GetItemBySystemName("Catalog");
        if (catalogMenuItem != null)
            eventMessage.RootMenuItem.InsertAfter("Catalog", performanceMenuItem);
        else
            eventMessage.RootMenuItem.ChildNodes.Add(performanceMenuItem);
    }
}
