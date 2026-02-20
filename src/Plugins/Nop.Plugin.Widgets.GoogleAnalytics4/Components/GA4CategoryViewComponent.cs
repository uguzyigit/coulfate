using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
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
/// GA4 category/list view tracking component - view_item_list event
/// </summary>
public class GA4CategoryViewComponent : NopViewComponent
{
    private readonly IGA4TrackingService _ga4TrackingService;
    private readonly ISettingService _settingService;
    private readonly IStoreContext _storeContext;
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly ICurrencyService _currencyService;
    private readonly CurrencySettings _currencySettings;

    public GA4CategoryViewComponent(
        IGA4TrackingService ga4TrackingService,
        ISettingService settingService,
        IStoreContext storeContext,
        IProductService productService,
        ICategoryService categoryService,
        ICurrencyService currencyService,
        CurrencySettings currencySettings)
    {
        _ga4TrackingService = ga4TrackingService;
        _settingService = settingService;
        _storeContext = storeContext;
        _productService = productService;
        _categoryService = categoryService;
        _currencyService = currencyService;
        _currencySettings = currencySettings;
    }

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        var settings = await _settingService.LoadSettingAsync<GA4Settings>(store.Id);

        if (!settings.Enabled || !settings.EnableEcommerce || !settings.TrackViewItemList)
            return Content(string.Empty);

        // Get category ID from route
        var routeData = HttpContext.GetRouteData();
        if (!routeData.Values.TryGetValue("categoryId", out var categoryIdObj) ||
            !int.TryParse(categoryIdObj?.ToString(), out var categoryId))
            return Content(string.Empty);

        var category = await _categoryService.GetCategoryByIdAsync(categoryId);
        if (category == null || category.Deleted || !category.Published)
            return Content(string.Empty);

        var currency = await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId);

        // Get products in category (first 10 for tracking)
        var products = await _productService.SearchProductsAsync(
            categoryIds: new List<int> { categoryId },
            storeId: store.Id,
            visibleIndividuallyOnly: true,
            pageSize: 10);

        var items = new List<GA4Item>();
        var index = 0;
        decimal totalValue = 0;

        foreach (var product in products)
        {
            var ga4Item = await _ga4TrackingService.GetProductItemAsync(product, index: index);
            ga4Item.ItemListId = $"category_{categoryId}";
            ga4Item.ItemListName = category.Name;
            items.Add(ga4Item);
            totalValue += ga4Item.Price;
            index++;
        }

        var model = new GA4EventModel
        {
            EventName = GA4Defaults.EventViewItemList,
            Items = items,
            Currency = currency?.CurrencyCode ?? "TRY",
            Value = totalValue,
            ListId = $"category_{categoryId}",
            ListName = category.Name
        };

        return View("~/Plugins/Widgets.GoogleAnalytics4/Views/Components/GA4Event.cshtml", model);
    }
}
