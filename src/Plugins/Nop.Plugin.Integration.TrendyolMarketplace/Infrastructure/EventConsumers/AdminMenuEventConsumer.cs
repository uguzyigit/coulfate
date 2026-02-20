using Nop.Core;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Vendors;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.Integration.TrendyolMarketplace.Infrastructure.EventConsumers;

/// <summary>
/// Handles admin menu creation event to add plugin menu items
/// </summary>
public class AdminMenuEventConsumer : IConsumer<AdminMenuCreatedEvent>
{
    private readonly IPermissionService _permissionService;
    private readonly ILocalizationService _localizationService;
    private readonly IWorkContext _workContext;
    private readonly IVendorService _vendorService;

    public AdminMenuEventConsumer(
        IPermissionService permissionService,
        ILocalizationService localizationService,
        IWorkContext workContext,
        IVendorService vendorService)
    {
        _permissionService = permissionService;
        _localizationService = localizationService;
        _workContext = workContext;
        _vendorService = vendorService;
    }

    /// <summary>
    /// Handle the admin menu created event
    /// </summary>
    public async Task HandleEventAsync(AdminMenuCreatedEvent eventMessage)
    {
        var root = eventMessage.RootMenuItem;
        var isAdmin = await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS);

        // Check if user is a vendor
        var currentCustomer = await _workContext.GetCurrentCustomerAsync();
        var vendor = await _vendorService.GetVendorByIdAsync(currentCustomer.VendorId);
        var isVendor = vendor != null && !vendor.Deleted && vendor.Active;

        // Add Admin menu items (only for admins)
        if (isAdmin)
        {
            await AddAdminMenuItemsAsync(root, eventMessage);
        }

        // Add Vendor menu items (only for vendors)
        if (isVendor)
        {
            await AddVendorMenuItemsAsync(root, eventMessage);
        }
    }

    private async Task AddAdminMenuItemsAsync(AdminMenuItem root, AdminMenuCreatedEvent eventMessage)
    {
        // Find or create the Marketplace parent menu
        var marketplaceNode = root.GetItemBySystemName("Marketplace.Main");
        if (marketplaceNode is null)
        {
            marketplaceNode = new AdminMenuItem
            {
                SystemName = "Marketplace.Main",
                Title = await _localizationService.GetResourceAsync("Plugins.Integration.TrendyolMarketplace.Menu.Marketplace"),
                IconClass = "fas fa-store",
                Visible = true
            };

            // Find Configuration menu and insert before it
            var configIndex = root.ChildNodes.ToList().FindIndex(x => x.SystemName == "Configuration");
            if (configIndex >= 0)
                root.ChildNodes.Insert(configIndex, marketplaceNode);
            else
                root.ChildNodes.Add(marketplaceNode);
        }

        // Add Trendyol Integration submenu
        var trendyolNode = marketplaceNode.GetItemBySystemName("Marketplace.Trendyol");
        if (trendyolNode is null)
        {
            trendyolNode = new AdminMenuItem
            {
                SystemName = "Marketplace.Trendyol",
                Title = await _localizationService.GetResourceAsync("Plugins.Integration.TrendyolMarketplace.Menu.Trendyol"),
                IconClass = "fas fa-sync-alt",
                Visible = true
            };
            marketplaceNode.ChildNodes.Add(trendyolNode);
        }

        // Add Admin menu items
        AddMenuItemIfMissing(trendyolNode, new AdminMenuItem
        {
            SystemName = "Marketplace.Trendyol.Dashboard",
            Title = await _localizationService.GetResourceAsync("Plugins.Integration.TrendyolMarketplace.Menu.Admin.Dashboard"),
            IconClass = "fas fa-tachometer-alt",
            Url = eventMessage.GetMenuItemUrl("TrendyolAdmin", "Dashboard"),
            Visible = true
        });

        AddMenuItemIfMissing(trendyolNode, new AdminMenuItem
        {
            SystemName = "Marketplace.Trendyol.Configure",
            Title = await _localizationService.GetResourceAsync("Plugins.Integration.TrendyolMarketplace.Menu.Admin.Configure"),
            IconClass = "fas fa-cog",
            Url = eventMessage.GetMenuItemUrl("TrendyolAdmin", "Configure"),
            Visible = true
        });

        AddMenuItemIfMissing(trendyolNode, new AdminMenuItem
        {
            SystemName = "Marketplace.Trendyol.CategoryMapping",
            Title = await _localizationService.GetResourceAsync("Plugins.Integration.TrendyolMarketplace.Menu.Admin.CategoryMapping"),
            IconClass = "fas fa-sitemap",
            Url = eventMessage.GetMenuItemUrl("TrendyolAdmin", "CategoryMapping"),
            Visible = true
        });

        AddMenuItemIfMissing(trendyolNode, new AdminMenuItem
        {
            SystemName = "Marketplace.Trendyol.BrandMapping",
            Title = await _localizationService.GetResourceAsync("Plugins.Integration.TrendyolMarketplace.Menu.Admin.BrandMapping"),
            IconClass = "fas fa-tags",
            Url = eventMessage.GetMenuItemUrl("TrendyolAdmin", "BrandMapping"),
            Visible = true
        });

        AddMenuItemIfMissing(trendyolNode, new AdminMenuItem
        {
            SystemName = "Marketplace.Trendyol.VendorList",
            Title = await _localizationService.GetResourceAsync("Plugins.Integration.TrendyolMarketplace.Menu.Admin.VendorList"),
            IconClass = "fas fa-users",
            Url = eventMessage.GetMenuItemUrl("TrendyolAdmin", "VendorList"),
            Visible = true
        });
    }

    private async Task AddVendorMenuItemsAsync(AdminMenuItem root, AdminMenuCreatedEvent eventMessage)
    {
        // Find or create Vendor Panel node (Ürün Entegrasyonu)
        var vendorNode = root.GetItemBySystemName("VendorPanel");
        if (vendorNode is null)
        {
            vendorNode = new AdminMenuItem
            {
                SystemName = "VendorPanel",
                Title = await _localizationService.GetResourceAsync("Plugins.Integration.TrendyolMarketplace.Menu.VendorPanel"),
                IconClass = "fas fa-boxes",
                Visible = true
            };
            root.ChildNodes.Add(vendorNode);
        }

        // Add Trendyol submenu for vendors
        var vendorTrendyolNode = vendorNode.GetItemBySystemName("VendorPanel.Trendyol");
        if (vendorTrendyolNode is null)
        {
            vendorTrendyolNode = new AdminMenuItem
            {
                SystemName = "VendorPanel.Trendyol",
                Title = await _localizationService.GetResourceAsync("Plugins.Integration.TrendyolMarketplace.Menu.Vendor.Trendyol"),
                IconClass = "fas fa-sync-alt",
                Visible = true
            };
            vendorNode.ChildNodes.Add(vendorTrendyolNode);
        }

        AddMenuItemIfMissing(vendorTrendyolNode, new AdminMenuItem
        {
            SystemName = "VendorPanel.Trendyol.Dashboard",
            Title = await _localizationService.GetResourceAsync("Plugins.Integration.TrendyolMarketplace.Menu.Vendor.Dashboard"),
            IconClass = "fas fa-tachometer-alt",
            Url = eventMessage.GetMenuItemUrl("TrendyolVendor", "Dashboard"),
            Visible = true
        });

        AddMenuItemIfMissing(vendorTrendyolNode, new AdminMenuItem
        {
            SystemName = "VendorPanel.Trendyol.Credentials",
            Title = await _localizationService.GetResourceAsync("Plugins.Integration.TrendyolMarketplace.Menu.Vendor.Credentials"),
            IconClass = "fas fa-key",
            Url = eventMessage.GetMenuItemUrl("TrendyolVendor", "Credentials"),
            Visible = true
        });

        AddMenuItemIfMissing(vendorTrendyolNode, new AdminMenuItem
        {
            SystemName = "VendorPanel.Trendyol.Products",
            Title = await _localizationService.GetResourceAsync("Plugins.Integration.TrendyolMarketplace.Menu.Vendor.Products"),
            IconClass = "fas fa-box",
            Url = eventMessage.GetMenuItemUrl("TrendyolVendor", "Products"),
            Visible = true
        });

        AddMenuItemIfMissing(vendorTrendyolNode, new AdminMenuItem
        {
            SystemName = "VendorPanel.Trendyol.SyncLogs",
            Title = await _localizationService.GetResourceAsync("Plugins.Integration.TrendyolMarketplace.Menu.Vendor.SyncLogs"),
            IconClass = "fas fa-history",
            Url = eventMessage.GetMenuItemUrl("TrendyolVendor", "SyncLogs"),
            Visible = true
        });

        AddMenuItemIfMissing(vendorTrendyolNode, new AdminMenuItem
        {
            SystemName = "VendorPanel.Trendyol.ManualSync",
            Title = await _localizationService.GetResourceAsync("Plugins.Integration.TrendyolMarketplace.Menu.Vendor.ManualSync"),
            IconClass = "fas fa-play",
            Url = eventMessage.GetMenuItemUrl("TrendyolVendor", "ManualSync"),
            Visible = true
        });
    }

    private static void AddMenuItemIfMissing(AdminMenuItem parent, AdminMenuItem item)
    {
        if (parent.ChildNodes.Any(x => x.SystemName == item.SystemName))
            return;
        parent.ChildNodes.Add(item);
    }
}
