using System.Text;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Widgets.GoogleAnalytics4.Domain;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Logging;
using Nop.Services.Orders;

namespace Nop.Plugin.Widgets.GoogleAnalytics4.Services;

/// <summary>
/// GA4 tracking service implementation
/// </summary>
public class GA4TrackingService : IGA4TrackingService
{
    private readonly ISettingService _settingService;
    private readonly IStoreContext _storeContext;
    private readonly IWorkContext _workContext;
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly IManufacturerService _manufacturerService;
    private readonly ICurrencyService _currencyService;
    private readonly IOrderService _orderService;
    private readonly ICustomerService _customerService;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger _logger;
    private readonly CurrencySettings _currencySettings;
    private readonly HttpClient _httpClient;

    public GA4TrackingService(
        ISettingService settingService,
        IStoreContext storeContext,
        IWorkContext workContext,
        IProductService productService,
        ICategoryService categoryService,
        IManufacturerService manufacturerService,
        ICurrencyService currencyService,
        IOrderService orderService,
        ICustomerService customerService,
        IGenericAttributeService genericAttributeService,
        IHttpContextAccessor httpContextAccessor,
        ILogger logger,
        CurrencySettings currencySettings,
        IHttpClientFactory httpClientFactory)
    {
        _settingService = settingService;
        _storeContext = storeContext;
        _workContext = workContext;
        _productService = productService;
        _categoryService = categoryService;
        _manufacturerService = manufacturerService;
        _currencyService = currencyService;
        _orderService = orderService;
        _customerService = customerService;
        _genericAttributeService = genericAttributeService;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _currencySettings = currencySettings;
        _httpClient = httpClientFactory.CreateClient();
    }

    /// <summary>
    /// Gets the GA4 base script
    /// </summary>
    public async Task<string> GetGA4ScriptAsync()
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        var settings = await _settingService.LoadSettingAsync<GA4Settings>(store.Id);

        if (!settings.Enabled || string.IsNullOrEmpty(settings.MeasurementId))
            return string.Empty;

        var customer = await _workContext.GetCurrentCustomerAsync();
        var isGuest = await _customerService.IsGuestAsync(customer);

        var sb = new StringBuilder();

        // Base GA4 script
        sb.AppendLine($@"<!-- Google Analytics 4 Enhanced by Coulfate -->
<script async src=""https://www.googletagmanager.com/gtag/js?id={settings.MeasurementId}""></script>
<script>
window.dataLayer = window.dataLayer || [];
function gtag(){{dataLayer.push(arguments);}}
gtag('js', new Date());
gtag('config', '{settings.MeasurementId}'");

        // Add user ID if enabled and not guest
        if (settings.IncludeCustomerId && !isGuest)
        {
            sb.Append($", {{'user_id': '{customer.Id}'}}");
        }

        sb.AppendLine(");");

        // Debug mode
        if (settings.EnableDebugMode)
        {
            sb.AppendLine(@"
// GA4 Debug Mode
window.ga4Debug = true;
function ga4Log(eventName, params) {
    if (window.ga4Debug) {
        console.log('%c[GA4] ' + eventName, 'color: #4285f4; font-weight: bold;', params);
    }
}");
        }
        else
        {
            sb.AppendLine("function ga4Log(eventName, params) {}");
        }

        // Helper function for sending events
        sb.AppendLine(@"
// GA4 Event Helper
function ga4Event(eventName, params) {
    ga4Log(eventName, params);
    gtag('event', eventName, params);
}

// GA4 E-commerce Event Tracking - Vanilla JS (no jQuery dependency)
(function() {
    // Intercept XMLHttpRequest to track AJAX calls
    var originalXHROpen = XMLHttpRequest.prototype.open;
    var originalXHRSend = XMLHttpRequest.prototype.send;

    XMLHttpRequest.prototype.open = function(method, url) {
        this._ga4Url = url;
        this._ga4Method = method;
        return originalXHROpen.apply(this, arguments);
    };

    XMLHttpRequest.prototype.send = function(body) {
        var xhr = this;
        var url = this._ga4Url || '';

        xhr.addEventListener('load', function() {
            if (xhr.status !== 200) return;

            try {
                // Add to Cart
                if (url.toLowerCase().indexOf('/addproducttocart/') > -1) {
                    var response = JSON.parse(xhr.responseText);
                    if (response.success && window.ga4ProductData) {
                        ga4Event('add_to_cart', {
                            currency: window.ga4ProductData.currency || 'TRY',
                            value: window.ga4ProductData.price || 0,
                            items: [{
                                item_id: window.ga4ProductData.id,
                                item_name: window.ga4ProductData.name,
                                item_brand: window.ga4ProductData.brand,
                                item_category: window.ga4ProductData.category,
                                price: window.ga4ProductData.price,
                                quantity: 1
                            }]
                        });
                    }
                }

                // Shipping Method Selection (One Page Checkout)
                if (url.indexOf('OpcSaveShippingMethod') > -1 || url.toLowerCase().indexOf('saveshippingmethod') > -1) {
                    try {
                        var response = JSON.parse(xhr.responseText);
                        if (!response.error && window.ga4CheckoutData) {
                            var shippingInput = document.querySelector('input[name=""shippingoption""]:checked');
                            var shippingTier = 'Standard';
                            if (shippingInput) {
                                var label = shippingInput.parentElement ? shippingInput.parentElement.textContent.trim() : '';
                                if (!label) {
                                    var nextLabel = shippingInput.nextElementSibling;
                                    label = nextLabel ? nextLabel.textContent.trim() : '';
                                }
                                shippingTier = label || 'Standard';
                            }
                            ga4Event('add_shipping_info', {
                                currency: window.ga4CheckoutData.currency || 'TRY',
                                value: window.ga4CheckoutData.value || 0,
                                shipping_tier: shippingTier,
                                items: window.ga4CheckoutData.items || []
                            });
                        }
                    } catch(e) { ga4Log('add_shipping_info error', e); }
                }

                // Payment Method Selection (One Page Checkout)
                if (url.indexOf('OpcSavePaymentMethod') > -1 || url.toLowerCase().indexOf('savepaymentmethod') > -1) {
                    try {
                        var response = JSON.parse(xhr.responseText);
                        if (!response.error && window.ga4CheckoutData) {
                            var paymentInput = document.querySelector('input[name=""paymentmethod""]:checked');
                            var paymentType = 'Unknown';
                            if (paymentInput) {
                                paymentType = paymentInput.value || paymentInput.id || 'Unknown';
                            }
                            ga4Event('add_payment_info', {
                                currency: window.ga4CheckoutData.currency || 'TRY',
                                value: window.ga4CheckoutData.value || 0,
                                payment_type: paymentType,
                                items: window.ga4CheckoutData.items || []
                            });
                        }
                    } catch(e) { ga4Log('add_payment_info error', e); }
                }

                // Cart Update
                if (url.toLowerCase().indexOf('/updatecart') > -1) {
                    ga4Log('cart_updated', 'Cart was updated');
                }
            } catch(e) {
                console.warn('[GA4] Error tracking event:', e);
            }
        });

        return originalXHRSend.apply(this, arguments);
    };

    // Track remove from cart button clicks using event delegation
    document.addEventListener('click', function(e) {
        var target = e.target;
        if (target.classList.contains('remove-btn') ||
            target.classList.contains('qty-remove') ||
            (target.name === 'updatecart' && target.value === 'removefromcart')) {

            var row = target.closest('tr') || target.closest('.cart-item');
            if (row) {
                var productNameEl = row.querySelector('.product-name') || row.querySelector('.product a');
                var productName = productNameEl ? productNameEl.textContent.trim() : '';
                if (productName) {
                    ga4Event('remove_from_cart', {
                        currency: 'TRY',
                        items: [{
                            item_name: productName
                        }]
                    });
                }
            }
        }
    });
})();
</script>");

        return sb.ToString();
    }

    /// <summary>
    /// Gets product item data for GA4 event
    /// </summary>
    public async Task<GA4Item> GetProductItemAsync(Product product, decimal? price = null, int quantity = 1, int? index = null)
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        var settings = await _settingService.LoadSettingAsync<GA4Settings>(store.Id);

        // Get category hierarchy
        var categories = await _categoryService.GetProductCategoriesByProductIdAsync(product.Id);
        var categoryNames = new List<string>();

        foreach (var pc in categories.Take(3))
        {
            var category = await _categoryService.GetCategoryByIdAsync(pc.CategoryId);
            if (category != null)
                categoryNames.Add(category.Name);
        }

        // Get manufacturer (brand)
        var manufacturers = await _manufacturerService.GetProductManufacturersByProductIdAsync(product.Id);
        var brand = string.Empty;
        if (manufacturers.Any())
        {
            var manufacturer = await _manufacturerService.GetManufacturerByIdAsync(manufacturers.First().ManufacturerId);
            brand = manufacturer?.Name ?? string.Empty;
        }

        // Get price
        var itemPrice = price ?? (settings.IncludeTax ? product.Price : product.Price);

        var item = new GA4Item
        {
            ItemId = !string.IsNullOrEmpty(product.Sku) ? product.Sku : product.Id.ToString(),
            ItemName = product.Name,
            ItemBrand = brand,
            Price = itemPrice,
            Quantity = quantity,
            Index = index
        };

        if (categoryNames.Count > 0) item.ItemCategory = categoryNames[0];
        if (categoryNames.Count > 1) item.ItemCategory2 = categoryNames[1];
        if (categoryNames.Count > 2) item.ItemCategory3 = categoryNames[2];

        return item;
    }

    /// <summary>
    /// Gets product item data for GA4 event from order item
    /// </summary>
    public async Task<GA4Item> GetOrderItemAsync(OrderItem orderItem)
    {
        var product = await _productService.GetProductByIdAsync(orderItem.ProductId);
        var store = await _storeContext.GetCurrentStoreAsync();
        var settings = await _settingService.LoadSettingAsync<GA4Settings>(store.Id);

        var price = settings.IncludeTax ? orderItem.UnitPriceInclTax : orderItem.UnitPriceExclTax;

        return await GetProductItemAsync(product, price, orderItem.Quantity);
    }

    /// <summary>
    /// Parses client ID from _ga cookie value
    /// GA cookie format: GA1.1.XXXXXXXXXX.XXXXXXXXXX
    /// Client ID is: XXXXXXXXXX.XXXXXXXXXX (last two parts)
    /// </summary>
    public string ParseClientId(string gaCookieValue)
    {
        if (string.IsNullOrEmpty(gaCookieValue))
            return null;

        var parts = gaCookieValue.Split('.');
        if (parts.Length >= 4)
        {
            // Return last two parts: timestamp.random
            return $"{parts[parts.Length - 2]}.{parts[parts.Length - 1]}";
        }

        return gaCookieValue;
    }

    /// <summary>
    /// Parses session ID from _ga_XXXXX cookie value
    /// Session cookie format: GS1.1.XXXXXXXXXX.N.X.XXXXXXXXXX.X.X.X
    /// Session ID is the timestamp (3rd part)
    /// </summary>
    public string ParseSessionId(string gaSessionCookieValue)
    {
        if (string.IsNullOrEmpty(gaSessionCookieValue))
            return null;

        var parts = gaSessionCookieValue.Split('.');
        if (parts.Length >= 3)
        {
            return parts[2];
        }

        return null;
    }

    /// <summary>
    /// Saves GA cookies to order generic attributes for server-side tracking
    /// </summary>
    public async Task SaveGACookiesToOrderAsync(Order order)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return;

        var store = await _storeContext.GetCurrentStoreAsync();
        var settings = await _settingService.LoadSettingAsync<GA4Settings>(store.Id);

        // Get and parse client ID
        if (httpContext.Request.Cookies.TryGetValue(GA4Defaults.ClientIdCookieName, out var gaCookie))
        {
            var clientId = ParseClientId(gaCookie);
            if (!string.IsNullOrEmpty(clientId))
            {
                await _genericAttributeService.SaveAttributeAsync(order, GA4Defaults.ClientIdAttributeKey, clientId, store.Id);
            }
        }

        // Get and parse session ID
        if (!string.IsNullOrEmpty(settings.MeasurementId))
        {
            var measurementIdSuffix = settings.MeasurementId.Replace("G-", "");
            var sessionCookieName = $"{GA4Defaults.SessionIdCookiePrefix}{measurementIdSuffix}";

            if (httpContext.Request.Cookies.TryGetValue(sessionCookieName, out var sessionCookie))
            {
                var sessionId = ParseSessionId(sessionCookie);
                if (!string.IsNullOrEmpty(sessionId))
                {
                    await _genericAttributeService.SaveAttributeAsync(order, GA4Defaults.SessionIdAttributeKey, sessionId, store.Id);
                }
            }
        }
    }

    /// <summary>
    /// Sends purchase event via Measurement Protocol (server-side)
    /// </summary>
    public async Task SendPurchaseEventAsync(Order order)
    {
        await SendOrderEventAsync(order, GA4Defaults.EventPurchase);
    }

    /// <summary>
    /// Sends refund event via Measurement Protocol (server-side)
    /// </summary>
    public async Task SendRefundEventAsync(Order order)
    {
        await SendOrderEventAsync(order, GA4Defaults.EventRefund);
    }

    private async Task SendOrderEventAsync(Order order, string eventName)
    {
        try
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            var settings = await _settingService.LoadSettingAsync<GA4Settings>(store.Id);

            if (!settings.Enabled || !settings.EnableEcommerce)
                return;

            if (string.IsNullOrEmpty(settings.MeasurementId) || string.IsNullOrEmpty(settings.ApiSecret))
            {
                await _logger.WarningAsync("GA4: MeasurementId or ApiSecret is not configured");
                return;
            }

            // Get client ID from order attributes
            var clientId = await _genericAttributeService.GetAttributeAsync<string>(order, GA4Defaults.ClientIdAttributeKey, store.Id);
            if (string.IsNullOrEmpty(clientId))
            {
                await _logger.WarningAsync($"GA4: Client ID not found for order {order.Id}. Server-side event cannot be sent.");
                return;
            }

            var sessionId = await _genericAttributeService.GetAttributeAsync<string>(order, GA4Defaults.SessionIdAttributeKey, store.Id);
            var currency = (await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId))?.CurrencyCode ?? "TRY";

            // Build items array
            var items = new List<object>();
            var orderItems = await _orderService.GetOrderItemsAsync(order.Id);
            var index = 0;

            foreach (var orderItem in orderItems)
            {
                var product = await _productService.GetProductByIdAsync(orderItem.ProductId);
                var ga4Item = await GetOrderItemAsync(orderItem);

                items.Add(new
                {
                    item_id = ga4Item.ItemId,
                    item_name = ga4Item.ItemName,
                    item_brand = ga4Item.ItemBrand,
                    item_category = ga4Item.ItemCategory,
                    item_category2 = ga4Item.ItemCategory2,
                    item_category3 = ga4Item.ItemCategory3,
                    price = ga4Item.Price,
                    quantity = ga4Item.Quantity,
                    index = index++
                });
            }

            var orderShipping = settings.IncludeTax ? order.OrderShippingInclTax : order.OrderShippingExclTax;

            // Build request payload
            var payload = new
            {
                client_id = clientId,
                user_id = order.CustomerId.ToString(),
                timestamp_micros = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() * 1000,
                events = new[]
                {
                    new
                    {
                        name = eventName,
                        @params = new
                        {
                            transaction_id = order.CustomOrderNumber,
                            value = order.OrderTotal,
                            currency = currency,
                            tax = order.OrderTax,
                            shipping = orderShipping,
                            items = items,
                            session_id = sessionId,
                            engagement_time_msec = 100
                        }
                    }
                }
            };

            var jsonPayload = JsonConvert.SerializeObject(payload, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            // Send to Measurement Protocol
            var url = $"{GA4Defaults.MeasurementProtocolUrl}?measurement_id={settings.MeasurementId}&api_secret={settings.ApiSecret}";

            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                await _logger.ErrorAsync($"GA4: Failed to send {eventName} event. Status: {response.StatusCode}, Response: {responseBody}");
            }
            else
            {
                await _logger.InformationAsync($"GA4: {eventName} event sent for order {order.CustomOrderNumber}");
            }
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync($"GA4: Error sending {eventName} event", ex);
        }
    }
}
