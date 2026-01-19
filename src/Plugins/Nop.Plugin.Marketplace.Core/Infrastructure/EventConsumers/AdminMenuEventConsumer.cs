using System.Threading.Tasks;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.Marketplace.Core.Infrastructure.EventConsumers;

/// <summary>
/// Event consumer for adding Marketplace menu to admin panel
/// </summary>
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

    /// <summary>
    /// Handle admin menu created event
    /// </summary>
    public async Task HandleEventAsync(AdminMenuCreatedEvent eventMessage)
    {
        var plugin = await _pluginManager.LoadPluginBySystemNameAsync("Marketplace.Core");
        
        // Plugin not installed yet or not active
        if (plugin == null)
            return;

        // Get the Catalog menu item to insert after it
        var catalogMenuItem = eventMessage.RootMenuItem.GetItemBySystemName("Catalog");
        
        if (catalogMenuItem == null)
            return;

        // Create main Marketplace menu item
        var marketplaceMenuItem = new AdminMenuItem
        {
            SystemName = "Marketplace",
            Title = await _localizationService.GetResourceAsync("Plugins.Marketplace.Core.Menu.Marketplace"),
            IconClass = "far fa-dot-circle",
            Visible = true
        };

        // Add Vendors submenu
        var vendorsMenuItem = new AdminMenuItem
        {
            SystemName = "Marketplace.Vendors",
            Title = await _localizationService.GetResourceAsync("Plugins.Marketplace.Core.Menu.Vendors"),
            Url = eventMessage.GetMenuItemUrl("MarketplaceVendor", "Index"),
            IconClass = "far fa-circle",
            Visible = true
        };

        marketplaceMenuItem.ChildNodes.Add(vendorsMenuItem);

        // Insert after Catalog
        eventMessage.RootMenuItem.InsertAfter("Catalog", marketplaceMenuItem);
    }
}
