using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.Accounting.Parasut.Infrastructure.EventConsumers;

public class VendorMenuEventConsumer : IConsumer<AdminMenuCreatedEvent>
{
    private readonly IWorkContext _workContext;
    private readonly ILocalizationService _localizationService;

    public VendorMenuEventConsumer(
        IWorkContext workContext,
        ILocalizationService localizationService)
    {
        _workContext = workContext;
        _localizationService = localizationService;
    }

    public async Task HandleEventAsync(AdminMenuCreatedEvent eventMessage)
    {
        var vendor = await _workContext.GetCurrentVendorAsync();
        if (vendor == null)
            return;

        var root = eventMessage.RootMenuItem;

        var financeNode = root.GetItemBySystemName("Finance");
        if (financeNode == null)
        {
            financeNode = new AdminMenuItem
            {
                SystemName = "Finance",
                Title = await _localizationService.GetResourceAsync("Plugins.Accounting.Parasut.Menu.Finance"),
                IconClass = "fas fa-money-bill-wave",
                Visible = true
            };
            root.ChildNodes.Add(financeNode);
        }

        var invoicesNode = financeNode.GetItemBySystemName("Finance.VendorInvoices");
        if (invoicesNode == null)
        {
            financeNode.ChildNodes.Add(new AdminMenuItem
            {
                SystemName = "Finance.VendorInvoices",
                Title = await _localizationService.GetResourceAsync("Plugins.Accounting.Parasut.Menu.MyInvoices"),
                Url = eventMessage.GetMenuItemUrl("VendorInvoice", "List"),
                IconClass = "far fa-file-invoice",
                Visible = true
            });
        }
    }
}
