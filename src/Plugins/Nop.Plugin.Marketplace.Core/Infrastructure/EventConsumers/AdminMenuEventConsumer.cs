using Nop.Services.Events;
using Nop.Services.Plugins;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.Marketplace.Core.Infrastructure.EventConsumers;

/// <summary>
/// Event consumer for adding Marketplace menu to admin panel
/// </summary>
public class AdminMenuEventConsumer : IConsumer<AdminMenuCreatedEvent>
{
    private readonly IPluginManager<IPlugin> _pluginManager;

    public AdminMenuEventConsumer(IPluginManager<IPlugin> pluginManager)
    {
        _pluginManager = pluginManager;
    }

    /// <summary>
    /// Handle admin menu created event
    /// </summary>
    public async Task HandleEventAsync(AdminMenuCreatedEvent eventMessage)
    {
        var plugin = await _pluginManager.LoadPluginBySystemNameAsync("Marketplace.Core");

        if (plugin == null)
            return;

        var catalogMenuItem = eventMessage.RootMenuItem.GetItemBySystemName("Catalog");

        if (catalogMenuItem == null)
            return;

        // Create main Marketplace menu item (other plugins will add their items here)
        var marketplaceMenuItem = new AdminMenuItem
        {
            SystemName = "Marketplace",
            Title = "Marketplace",
            IconClass = "far fa-dot-circle",
            Visible = true
        };

        eventMessage.RootMenuItem.InsertAfter("Catalog", marketplaceMenuItem);
    }
}
