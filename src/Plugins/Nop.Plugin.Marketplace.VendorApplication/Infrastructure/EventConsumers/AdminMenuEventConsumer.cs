using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.Marketplace.VendorApplication.Infrastructure.EventConsumers;

/// <summary>
/// Admin menu event consumer - adds Vendor Application menu items
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

        // 1) Ensure "Marketplace" root node exists
        var marketplaceNode = root.GetItemBySystemName("Marketplace.Main");
        if (marketplaceNode is null)
        {
            marketplaceNode = new AdminMenuItem
            {
                SystemName = "Marketplace.Main",
                Title = "Marketplace",
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

        // 2) Ensure "Vendor Applications" submenu exists
        var applicationsNode = marketplaceNode.GetItemBySystemName("Marketplace.VendorApplications");
        if (applicationsNode is null)
        {
            applicationsNode = new AdminMenuItem
            {
                SystemName = "Marketplace.VendorApplications",
                Title = await _localizationService.GetResourceAsync("Plugins.Marketplace.VendorApplication.Menu.Applications"),
                IconClass = "fas fa-file-alt",
                Visible = true
            };
            marketplaceNode.ChildNodes.Add(applicationsNode);
        }

        // 3) Add leaf nodes
        AddIfMissing(applicationsNode, new AdminMenuItem
        {
            SystemName = "Marketplace.VendorApplication.List",
            Title = await _localizationService.GetResourceAsync("Plugins.Marketplace.VendorApplication.Admin.List.Title"),
            Url = eventMessage.GetMenuItemUrl("VendorApplicationAdmin", "List"),
            IconClass = "far fa-dot-circle",
            Visible = true
        });

        AddIfMissing(applicationsNode, new AdminMenuItem
        {
            SystemName = "Marketplace.VendorApplication.DocumentTypes",
            Title = await _localizationService.GetResourceAsync("Plugins.Marketplace.VendorApplication.Menu.DocumentTypes"),
            Url = eventMessage.GetMenuItemUrl("VendorApplicationAdmin", "DocumentTypes"),
            IconClass = "far fa-dot-circle",
            Visible = true
        });

        AddIfMissing(applicationsNode, new AdminMenuItem
        {
            SystemName = "Marketplace.VendorApplication.Settings",
            Title = await _localizationService.GetResourceAsync("Plugins.Marketplace.VendorApplication.Menu.Settings"),
            Url = eventMessage.GetMenuItemUrl("VendorApplicationAdmin", "Configure"),
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
