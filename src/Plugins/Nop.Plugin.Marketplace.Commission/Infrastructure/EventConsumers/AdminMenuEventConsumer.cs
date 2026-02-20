using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Services.Events;
using Nop.Services.Security;
using Nop.Services.Localization;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.Marketplace.Commission.Infrastructure.EventConsumers;

/// <summary>
/// Admin Commission Menu - Only visible to admins (not vendors)
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

        // 2) Ensure "Commission" submenu exists
        var commissionNode = marketplaceNode.GetItemBySystemName("Marketplace.Commission");
        if (commissionNode is null)
        {
            commissionNode = new AdminMenuItem
            {
                SystemName = "Marketplace.Commission",
                Title = await _localizationService.GetResourceAsync("Plugins.Marketplace.Commission.Menu.Commission"),
                IconClass = "far fa-money-bill-alt",
                Visible = true
            };
            marketplaceNode.ChildNodes.Add(commissionNode);
        }

        // 3) Add leaf nodes with localization
        AddIfMissing(commissionNode, new AdminMenuItem
        {
            SystemName = "Marketplace.Commission.Settings",
            Title = await _localizationService.GetResourceAsync("Plugins.Marketplace.Commission.Menu.Settings"),
            Url = eventMessage.GetMenuItemUrl("Commission", "Configure"),
            IconClass = "far fa-dot-circle",
            Visible = true
        });

        AddIfMissing(commissionNode, new AdminMenuItem
        {
            SystemName = "Marketplace.Commission.CategoryRates",
            Title = await _localizationService.GetResourceAsync("Plugins.Marketplace.Commission.Menu.CategoryRates"),
            Url = eventMessage.GetMenuItemUrl("Commission", "CategoryCommissions"),
            IconClass = "far fa-dot-circle",
            Visible = true
        });

        AddIfMissing(commissionNode, new AdminMenuItem
        {
            SystemName = "Marketplace.Commission.ProductRates",
            Title = await _localizationService.GetResourceAsync("Plugins.Marketplace.Commission.Menu.ProductRates"),
            Url = eventMessage.GetMenuItemUrl("Commission", "ProductCommissions"),
            IconClass = "far fa-dot-circle",
            Visible = true
        });

        AddIfMissing(commissionNode, new AdminMenuItem
        {
            SystemName = "Marketplace.Commission.AllReports",
            Title = await _localizationService.GetResourceAsync("Plugins.Marketplace.Commission.Menu.Reports"),
            Url = eventMessage.GetMenuItemUrl("Commission", "Reports"),
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
