using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Plugin.Widgets.GoogleAnalytics4.Domain;
using Nop.Plugin.Widgets.GoogleAnalytics4.Services;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Widgets.GoogleAnalytics4.Components;

/// <summary>
/// GA4 search tracking component - search and view_item_list events
/// </summary>
public class GA4SearchViewComponent : NopViewComponent
{
    private readonly IGA4TrackingService _ga4TrackingService;
    private readonly ISettingService _settingService;
    private readonly IStoreContext _storeContext;
    private readonly IProductService _productService;
    private readonly ICurrencyService _currencyService;
    private readonly CurrencySettings _currencySettings;

    public GA4SearchViewComponent(
        IGA4TrackingService ga4TrackingService,
        ISettingService settingService,
        IStoreContext storeContext,
        IProductService productService,
        ICurrencyService currencyService,
        CurrencySettings currencySettings)
    {
        _ga4TrackingService = ga4TrackingService;
        _settingService = settingService;
        _storeContext = storeContext;
        _productService = productService;
        _currencyService = currencyService;
        _currencySettings = currencySettings;
    }

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        var settings = await _settingService.LoadSettingAsync<GA4Settings>(store.Id);

        if (!settings.Enabled || !settings.EnableEcommerce)
            return Content(string.Empty);

        // Get search term from query string
        var searchTerm = HttpContext.Request.Query["q"].ToString();
        if (string.IsNullOrEmpty(searchTerm))
            searchTerm = HttpContext.Request.Query["searchterm"].ToString();

        if (string.IsNullOrEmpty(searchTerm))
            return Content(string.Empty);

        var currency = await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId);

        // Search for products
        var products = await _productService.SearchProductsAsync(
            storeId: store.Id,
            keywords: searchTerm,
            visibleIndividuallyOnly: true,
            pageSize: 10);

        var items = new List<GA4Item>();
        var index = 0;
        decimal totalValue = 0;

        foreach (var product in products)
        {
            var ga4Item = await _ga4TrackingService.GetProductItemAsync(product, index: index);
            ga4Item.ItemListId = "search_results";
            ga4Item.ItemListName = "Search Results";
            items.Add(ga4Item);
            totalValue += ga4Item.Price;
            index++;
        }

        var model = new GA4EventModel
        {
            EventName = settings.TrackSearch ? GA4Defaults.EventSearch : GA4Defaults.EventViewItemList,
            Items = items,
            Currency = currency?.CurrencyCode ?? "TRY",
            Value = totalValue,
            ListId = "search_results",
            ListName = "Search Results",
            SearchTerm = searchTerm
        };

        return View("~/Plugins/Widgets.GoogleAnalytics4/Views/Components/GA4Event.cshtml", model);
    }
}
