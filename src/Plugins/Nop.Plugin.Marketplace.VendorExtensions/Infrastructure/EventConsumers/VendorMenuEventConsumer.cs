using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.Marketplace.VendorExtensions.Infrastructure.EventConsumers;

/// <summary>
/// Vendor Finance Menu - Only visible to vendors (not admins)
/// Also reorders vendor panel menus
/// </summary>
public class VendorMenuEventConsumer : IConsumer<AdminMenuCreatedEvent>
{
    private readonly IWorkContext _workContext;
    private readonly ILocalizationService _localizationService;

    // Desired menu order for vendor panel
    private static readonly string[] VendorMenuOrder = new[]
    {
        "Catalog",           // Katalog
        "Sales",             // Satış
        "Promotions",        // Promosyonlar
        "Vendor.Finance",    // Finans
        "Accounting",        // Muhasebe (Parasut plugin)
        "Shipping",          // Kargo Ayarları
        "Vendor.Integration",// Urun Entegrasyonu
        "Reports",           // Raporlar
        "Help"               // Yardım
    };

    public VendorMenuEventConsumer(
        IWorkContext workContext,
        ILocalizationService localizationService)
    {
        _workContext = workContext;
        _localizationService = localizationService;
    }

    public async Task HandleEventAsync(AdminMenuCreatedEvent eventMessage)
    {
        // Only show to vendors (not admins)
        var vendor = await _workContext.GetCurrentVendorAsync();
        if (vendor == null)
            return; // Not a vendor, don't show vendor menu

        var root = eventMessage.RootMenuItem;

        // Create "Finans" menu node
        var financeNode = root.GetItemBySystemName("Vendor.Finance");
        if (financeNode == null)
        {
            financeNode = new AdminMenuItem
            {
                SystemName = "Vendor.Finance",
                Title = "Finans",
                IconClass = "fas fa-wallet",
                Visible = true
            };

            root.ChildNodes.Add(financeNode);
        }

        // Add submenu items under Finans
        AddIfMissing(financeNode, new AdminMenuItem
        {
            SystemName = "Vendor.Finance.AccountSummary",
            Title = "Hesap Özeti",
            Url = eventMessage.GetMenuItemUrl("VendorExtensions", "AccountSummary"),
            IconClass = "far fa-dot-circle",
            Visible = true
        });

        AddIfMissing(financeNode, new AdminMenuItem
        {
            SystemName = "Vendor.Finance.TransactionHistory",
            Title = "İşlem Geçmişi",
            Url = eventMessage.GetMenuItemUrl("VendorExtensions", "VendorTransactionHistory"),
            IconClass = "far fa-dot-circle",
            Visible = true
        });

        AddIfMissing(financeNode, new AdminMenuItem
        {
            SystemName = "Vendor.Finance.CommissionReports",
            Title = "Komisyon Raporları",
            Url = eventMessage.GetMenuItemUrl("VendorExtensions", "VendorCommissionReports"),
            IconClass = "far fa-dot-circle",
            Visible = true
        });

        // Add "Komisyon Oranları" (read-only commission rates) at the end of Finans menu
        AddIfMissing(financeNode, new AdminMenuItem
        {
            SystemName = "Vendor.Finance.CommissionRates",
            Title = "Komisyon Oranları",
            Url = eventMessage.GetMenuItemUrl("VendorExtensions", "CommissionRates"),
            IconClass = "fas fa-percentage",
            Visible = true
        });

        // Reorder vendor panel menus
        ReorderMenuItems(root);
    }

    private static void AddIfMissing(AdminMenuItem parent, AdminMenuItem item)
    {
        if (parent.ChildNodes.Any(x => x.SystemName == item.SystemName))
            return;
        parent.ChildNodes.Add(item);
    }

    /// <summary>
    /// Reorders menu items according to VendorMenuOrder
    /// </summary>
    private static void ReorderMenuItems(AdminMenuItem root)
    {
        var orderedItems = new List<AdminMenuItem>();
        var existingItems = root.ChildNodes.ToList();

        // First, add items in the desired order
        foreach (var systemName in VendorMenuOrder)
        {
            var item = existingItems.FirstOrDefault(x => x.SystemName == systemName);
            if (item != null)
            {
                orderedItems.Add(item);
                existingItems.Remove(item);
            }
        }

        // Then add any remaining items that weren't in our order list
        orderedItems.AddRange(existingItems);

        // Clear and re-add in order
        root.ChildNodes.Clear();
        foreach (var item in orderedItems)
        {
            root.ChildNodes.Add(item);
        }
    }
}
