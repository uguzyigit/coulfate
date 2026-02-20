using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.Marketplace.VendorExtensions.Infrastructure.EventConsumers;

/// <summary>
/// Vendor Finance Menu - Only visible to vendors
/// </summary>
public class VendorFinanceMenuConsumer : IConsumer<AdminMenuCreatedEvent>
{
    private readonly IWorkContext _workContext;
    private readonly ILocalizationService _localizationService;

    public VendorFinanceMenuConsumer(
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
            return; // Not a vendor, exit

        var root = eventMessage.RootMenuItem;

        // Get current language
        var languageId = (await _workContext.GetWorkingLanguageAsync()).Id;

        // Create Finance menu for vendors
        var financeMenu = new AdminMenuItem
        {
            SystemName = "Finance",
            Title = await _localizationService.GetResourceAsync("Plugins.Marketplace.VendorExtensions.Finance", languageId),
            IconClass = "fas fa-wallet",
            Visible = true
        };

        // Add My Commission Report
        financeMenu.ChildNodes.Add(new AdminMenuItem
        {
            SystemName = "Finance.MyCommissionReport",
            Title = await _localizationService.GetResourceAsync("Plugins.Marketplace.VendorExtensions.MyCommissionReport", languageId),
            Url = eventMessage.GetMenuItemUrl("VendorExtensions", "MyCommissionReport"),
            IconClass = "far fa-chart-bar",
            Visible = true
        });

        // Add My Transactions
        financeMenu.ChildNodes.Add(new AdminMenuItem
        {
            SystemName = "Finance.MyTransactions",
            Title = await _localizationService.GetResourceAsync("Plugins.Marketplace.VendorExtensions.MyTransactions", languageId),
            Url = eventMessage.GetMenuItemUrl("VendorExtensions", "MyTransactions"),
            IconClass = "far fa-list-alt",
            Visible = true
        });

        // Insert Finance menu before "Local plugins"
        if (root.ChildNodes.All(x => x.SystemName != "Finance"))
{
    // Configuration'dan önce ekle
    var configIndex = root.ChildNodes.ToList().FindIndex(x => x.SystemName == "Configuration");
    if (configIndex >= 0)
        root.ChildNodes.Insert(configIndex, financeMenu);
    else
        root.ChildNodes.Add(financeMenu);
}
    }
}