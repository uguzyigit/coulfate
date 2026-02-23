using Nop.Core;
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
    private readonly IWorkContext _workContext;

    public AdminMenuEventConsumer(
        ILocalizationService localizationService,
        IPluginManager<IPlugin> pluginManager,
        IWorkContext workContext)
    {
        _localizationService = localizationService;
        _pluginManager = pluginManager;
        _workContext = workContext;
    }

    public async Task HandleEventAsync(AdminMenuCreatedEvent eventMessage)
    {
        var plugin = await _pluginManager.LoadPluginBySystemNameAsync("Marketplace.Performance");
        if (plugin == null)
            return;

        var root = eventMessage.RootMenuItem;

        // For vendors, add as top-level (reordering handled by VendorMenuEventConsumer)
        var vendor = await _workContext.GetCurrentVendorAsync();
        if (vendor != null)
        {
            AddPerformanceMenu(root, null);
            return;
        }

        // For admins, add under "Pazar Yeri" (Marketplace.Main)
        var marketplaceNode = root.GetItemBySystemName("Marketplace.Main");
        if (marketplaceNode == null)
        {
            marketplaceNode = new AdminMenuItem
            {
                SystemName = "Marketplace.Main",
                Title = "Pazar Yeri",
                IconClass = "fas fa-store",
                Visible = true
            };
            root.InsertBefore("Configuration", marketplaceNode);
        }

        AddPerformanceMenu(marketplaceNode, null);
    }

    private void AddPerformanceMenu(AdminMenuItem parent, string? insertAfter)
    {
        if (parent.GetItemBySystemName("Marketplace.Performance") != null)
            return;

        var performanceMenuItem = new AdminMenuItem
        {
            SystemName = "Marketplace.Performance",
            Title = "Performans",
            IconClass = "fas fa-chart-line",
            Visible = true
        };

        performanceMenuItem.ChildNodes.Add(new AdminMenuItem
        {
            SystemName = "Marketplace.Performance.Product",
            Title = "Ürün Performansı",
            IconClass = "fas fa-chart-line",
            Url = "/Admin/MarketplacePerformance/ProductPerformance",
            Visible = true
        });

        performanceMenuItem.ChildNodes.Add(new AdminMenuItem
        {
            SystemName = "Marketplace.Performance.Sales",
            Title = "Satış Performansı",
            IconClass = "fas fa-chart-bar",
            Url = "/Admin/MarketplacePerformance/SalesPerformance",
            Visible = true
        });

        parent.ChildNodes.Add(performanceMenuItem);
    }
}
