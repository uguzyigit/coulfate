using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Widgets.GoogleAnalytics4.Domain;
using Nop.Plugin.Widgets.GoogleAnalytics4.Services;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Orders;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Widgets.GoogleAnalytics4.Components;

/// <summary>
/// GA4 cart view tracking component - view_cart and begin_checkout events
/// </summary>
public class GA4CartViewComponent : NopViewComponent
{
    private readonly IGA4TrackingService _ga4TrackingService;
    private readonly ISettingService _settingService;
    private readonly IStoreContext _storeContext;
    private readonly IWorkContext _workContext;
    private readonly IShoppingCartService _shoppingCartService;
    private readonly IProductService _productService;
    private readonly ICurrencyService _currencyService;
    private readonly IPriceCalculationService _priceCalculationService;
    private readonly CurrencySettings _currencySettings;

    public GA4CartViewComponent(
        IGA4TrackingService ga4TrackingService,
        ISettingService settingService,
        IStoreContext storeContext,
        IWorkContext workContext,
        IShoppingCartService shoppingCartService,
        IProductService productService,
        ICurrencyService currencyService,
        IPriceCalculationService priceCalculationService,
        CurrencySettings currencySettings)
    {
        _ga4TrackingService = ga4TrackingService;
        _settingService = settingService;
        _storeContext = storeContext;
        _workContext = workContext;
        _shoppingCartService = shoppingCartService;
        _productService = productService;
        _currencyService = currencyService;
        _priceCalculationService = priceCalculationService;
        _currencySettings = currencySettings;
    }

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        var settings = await _settingService.LoadSettingAsync<GA4Settings>(store.Id);

        if (!settings.Enabled || !settings.EnableEcommerce)
            return Content(string.Empty);

        // Determine event type based on current page
        var path = HttpContext.Request.Path.Value?.ToLowerInvariant() ?? "";
        string eventName;

        if (path.Contains("/checkout") || path.Contains("/onepagecheckout"))
        {
            if (!settings.TrackBeginCheckout)
                return Content(string.Empty);
            eventName = GA4Defaults.EventBeginCheckout;
        }
        else if (path.Contains("/cart"))
        {
            if (!settings.TrackViewCart)
                return Content(string.Empty);
            eventName = GA4Defaults.EventViewCart;
        }
        else
        {
            return Content(string.Empty);
        }

        var customer = await _workContext.GetCurrentCustomerAsync();
        var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

        if (!cart.Any())
            return Content(string.Empty);

        var currency = await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId);
        var items = new List<GA4Item>();
        var index = 0;
        decimal totalValue = 0;

        foreach (var cartItem in cart)
        {
            var product = await _productService.GetProductByIdAsync(cartItem.ProductId);
            if (product == null)
                continue;

            var (_, finalPrice, _, _) = await _priceCalculationService.GetFinalPriceAsync(product, customer, store);

            var ga4Item = await _ga4TrackingService.GetProductItemAsync(product, finalPrice, cartItem.Quantity, index);
            items.Add(ga4Item);
            totalValue += finalPrice * cartItem.Quantity;
            index++;
        }

        var model = new GA4EventModel
        {
            EventName = eventName,
            Items = items,
            Currency = currency?.CurrencyCode ?? "TRY",
            Value = totalValue
        };

        return View("~/Plugins/Widgets.GoogleAnalytics4/Views/Components/GA4Event.cshtml", model);
    }
}
