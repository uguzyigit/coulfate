using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Services.Events;
using Nop.Services.Security;
using Nop.Services.Localization;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.Marketplace.VendorExtensions.Infrastructure.EventConsumers;

/// <summary>
/// Admin Marketplace Menu - Only visible to admins (not vendors)
/// </summary>
public class AdminMenuEventConsumer : IConsumer<AdminMenuCreatedEvent>
{
    private readonly IPermissionService _permissionService;
    private readonly IWorkContext _workContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILocalizationService _localizationService;

    public AdminMenuEventConsumer(
        IPermissionService permissionService,
        IWorkContext workContext,
        IHttpContextAccessor httpContextAccessor,
        ILocalizationService localizationService)
    {
        _permissionService = permissionService;
        _workContext = workContext;
        _httpContextAccessor = httpContextAccessor;
        _localizationService = localizationService;
    }

    public async Task HandleEventAsync(AdminMenuCreatedEvent eventMessage)
    {
        // Only show to admins (not vendors)
        var vendor = await _workContext.GetCurrentVendorAsync();
        if (vendor != null)
            return; // Is a vendor, don't show admin menu

        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return;

        var root = eventMessage.RootMenuItem;
        var currentPath = _httpContextAccessor.HttpContext?.Request.Path.Value?.ToLowerInvariant() ?? "";

        // 1) Ensure "Marketplace" root node exists
        var marketplaceNode = root.GetItemBySystemName("Marketplace.Main");
        if (marketplaceNode is null)
        {
            marketplaceNode = new AdminMenuItem
            {
                SystemName = "Marketplace.Main",
                Title = await _localizationService.GetResourceAsync("Plugins.Marketplace.Commission.Menu.Marketplace"),
                IconClass = "fas fa-store",
                Visible = true
            };
            
            // Insert at root level, before Configuration
            var configIndex = root.ChildNodes.ToList().FindIndex(x => x.SystemName == "Configuration");
            if (configIndex >= 0)
                root.ChildNodes.Insert(configIndex, marketplaceNode);
            else
                root.ChildNodes.Add(marketplaceNode);
        }

        // 2) Ensure "Vendor Account" submenu exists
        var vendorNode = marketplaceNode.GetItemBySystemName("Marketplace.VendorAccount");
        if (vendorNode is null)
        {
            vendorNode = new AdminMenuItem
            {
                SystemName = "Marketplace.VendorAccount",
                Title = await _localizationService.GetResourceAsync("Plugins.Marketplace.VendorExtensions.Menu.VendorAccount"),
                IconClass = "far fa-user",
                Visible = true
            };
            marketplaceNode.ChildNodes.Add(vendorNode);
        }

        // 3) Add leaf nodes with localization
        AddIfMissing(vendorNode, new AdminMenuItem
        {
            SystemName = "Marketplace.VendorExtensions.Settings",
            Title = await _localizationService.GetResourceAsync("Plugins.Marketplace.VendorExtensions.Menu.Settings"),
            Url = eventMessage.GetMenuItemUrl("VendorExtensions", "Configure"),
            IconClass = "far fa-dot-circle",
            Visible = true
        });

        AddIfMissing(vendorNode, new AdminMenuItem
        {
            SystemName = "Marketplace.VendorExtensions.Transactions",
            Title = await _localizationService.GetResourceAsync("Plugins.Marketplace.VendorExtensions.Menu.Transactions"),
            Url = eventMessage.GetMenuItemUrl("VendorExtensions", "TransactionHistory"),
            IconClass = "far fa-dot-circle",
            Visible = true
        });
    }

    private static void AddIfMissing(AdminMenuItem parent, AdminMenuItem item)
    {
        if (parent.ChildNodes.Any(x => x.SystemName == item.SystemName))
            return;
        parent.ChildNodes.Add(item);
    }
}
