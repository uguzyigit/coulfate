using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Directory;
using Nop.Plugin.Widgets.GoogleAnalytics4.Domain;
using Nop.Plugin.Widgets.GoogleAnalytics4.Services;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Widgets.GoogleAnalytics4.Components;

/// <summary>
/// GA4 product view tracking component
/// </summary>
public class GA4ProductViewComponent : NopViewComponent
{
    private readonly IGA4TrackingService _ga4TrackingService;
    private readonly ISettingService _settingService;
    private readonly IStoreContext _storeContext;
    private readonly IProductService _productService;
    private readonly ICurrencyService _currencyService;
    private readonly IPriceCalculationService _priceCalculationService;
    private readonly IWorkContext _workContext;
    private readonly CurrencySettings _currencySettings;

    public GA4ProductViewComponent(
        IGA4TrackingService ga4TrackingService,
        ISettingService settingService,
        IStoreContext storeContext,
        IProductService productService,
        ICurrencyService currencyService,
        IPriceCalculationService priceCalculationService,
        IWorkContext workContext,
        CurrencySettings currencySettings)
    {
        _ga4TrackingService = ga4TrackingService;
        _settingService = settingService;
        _storeContext = storeContext;
        _productService = productService;
        _currencyService = currencyService;
        _priceCalculationService = priceCalculationService;
        _workContext = workContext;
        _currencySettings = currencySettings;
    }

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        var settings = await _settingService.LoadSettingAsync<GA4Settings>(store.Id);

        if (!settings.Enabled || !settings.EnableEcommerce || !settings.TrackViewItem)
            return Content(string.Empty);

        // Get product ID from route
        var routeData = HttpContext.GetRouteData();
        if (!routeData.Values.TryGetValue("productId", out var productIdObj) ||
            !int.TryParse(productIdObj?.ToString(), out var productId))
            return Content(string.Empty);

        var product = await _productService.GetProductByIdAsync(productId);
        if (product == null || product.Deleted || !product.Published)
            return Content(string.Empty);

        var customer = await _workContext.GetCurrentCustomerAsync();
        var currency = await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId);

        // Get product price
        var (_, finalPrice, _, _) = await _priceCalculationService.GetFinalPriceAsync(product, customer, store);

        var ga4Item = await _ga4TrackingService.GetProductItemAsync(product, finalPrice);

        var model = new GA4EventModel
        {
            EventName = GA4Defaults.EventViewItem,
            Item = ga4Item,
            Currency = currency?.CurrencyCode ?? "TRY",
            Value = ga4Item.Price
        };

        return View("~/Plugins/Widgets.GoogleAnalytics4/Views/Components/GA4Event.cshtml", model);
    }
}

/// <summary>
/// Model for GA4 events
/// </summary>
public class GA4EventModel
{
    public string EventName { get; set; }
    public GA4Item Item { get; set; }
    public List<GA4Item> Items { get; set; }
    public string Currency { get; set; }
    public decimal Value { get; set; }
    public string ListId { get; set; }
    public string ListName { get; set; }
    public string SearchTerm { get; set; }
}
