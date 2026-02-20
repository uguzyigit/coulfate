using Nop.Core;
using Nop.Services.Events;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.Marketplace.ShippingManager.Infrastructure.EventConsumers;

/// <summary>
/// Vendor shipping menu consumer - adds shipping settings menu for vendors
/// </summary>
public class VendorShippingMenuConsumer : IConsumer<AdminMenuCreatedEvent>
{
    private readonly IWorkContext _workContext;

    public VendorShippingMenuConsumer(IWorkContext workContext)
    {
        _workContext = workContext;
    }

    public async Task HandleEventAsync(AdminMenuCreatedEvent eventMessage)
    {
        // Only show to vendors (not admins)
        var vendor = await _workContext.GetCurrentVendorAsync();
        if (vendor == null)
            return; // Not a vendor, don't show menu

        var root = eventMessage.RootMenuItem;

        // Check if "Vendor.Shipping" menu already exists
        var existingNode = root.GetItemBySystemName("Vendor.Shipping");
        if (existingNode != null)
            return;

        // Create "Kargo Ayarları" menu node
        var shippingNode = new AdminMenuItem
        {
            SystemName = "Vendor.Shipping",
            Title = "Kargo Ayarları",
            IconClass = "fas fa-truck",
            Visible = true
        };

        // Add sub-item: Settings
        shippingNode.ChildNodes.Add(new AdminMenuItem
        {
            SystemName = "Vendor.Shipping.Settings",
            Title = "Ayarlar",
            Url = eventMessage.GetMenuItemUrl("VendorShipping", "Settings"),
            IconClass = "far fa-dot-circle",
            Visible = true
        });

        // Insert after Catalog menu (or at the beginning if Catalog not found)
        var catalogIndex = root.ChildNodes.ToList().FindIndex(x => x.SystemName == "Catalog");
        if (catalogIndex >= 0)
            root.ChildNodes.Insert(catalogIndex + 1, shippingNode);
        else
            root.ChildNodes.Insert(0, shippingNode);
    }
}
