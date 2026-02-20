using System.Linq;
using Nop.Core;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.Accounting.Parasut.Infrastructure.EventConsumers;

/// <summary>
/// Admin menu event consumer - adds Parasut invoice menu for administrators
/// </summary>
public class AdminMenuEventConsumer : IConsumer<AdminMenuCreatedEvent>
{
    private readonly IWorkContext _workContext;
    private readonly ILocalizationService _localizationService;
    private readonly IPermissionService _permissionService;

    public AdminMenuEventConsumer(
        IWorkContext workContext,
        ILocalizationService localizationService,
        IPermissionService permissionService)
    {
        _workContext = workContext;
        _localizationService = localizationService;
        _permissionService = permissionService;
    }

    public async Task HandleEventAsync(AdminMenuCreatedEvent eventMessage)
    {
        // Only show for admins (not vendors)
        var vendor = await _workContext.GetCurrentVendorAsync();
        if (vendor != null)
            return;

        // Check admin permission
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return;

        var root = eventMessage.RootMenuItem;

        // 1) Find or create Marketplace.Main menu
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

            var configIndex = root.ChildNodes.ToList().FindIndex(x => x.SystemName == "Configuration");
            if (configIndex >= 0)
                root.ChildNodes.Insert(configIndex, marketplaceNode);
            else
                root.ChildNodes.Add(marketplaceNode);
        }

        // 2) Find or create "Finance" submenu under Marketplace.Main
        var financeNode = marketplaceNode.GetItemBySystemName("Marketplace.Finance");
        if (financeNode == null)
        {
            financeNode = new AdminMenuItem
            {
                SystemName = "Marketplace.Finance",
                Title = "Finans",
                IconClass = "fas fa-file-invoice-dollar",
                Visible = true
            };
            marketplaceNode.ChildNodes.Add(financeNode);
        }

        // 3) Add Vendor Invoices leaf node under Finance submenu
        AddIfMissing(financeNode, new AdminMenuItem
        {
            SystemName = "Marketplace.Finance.VendorInvoices",
            Title = "Satıcı Faturaları",
            Url = eventMessage.GetMenuItemUrl("AdminInvoice", "AllInvoices"),
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
