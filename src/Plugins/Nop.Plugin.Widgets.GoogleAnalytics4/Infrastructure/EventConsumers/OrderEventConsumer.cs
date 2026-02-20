using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Widgets.GoogleAnalytics4.Domain;
using Nop.Plugin.Widgets.GoogleAnalytics4.Services;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Events;
using Nop.Services.Stores;

namespace Nop.Plugin.Widgets.GoogleAnalytics4.Infrastructure.EventConsumers;

/// <summary>
/// Handles order-related events for GA4 tracking
/// </summary>
public class OrderEventConsumer :
    IConsumer<OrderPlacedEvent>,
    IConsumer<OrderPaidEvent>,
    IConsumer<OrderRefundedEvent>
{
    private readonly IGA4TrackingService _ga4TrackingService;
    private readonly ISettingService _settingService;
    private readonly IStoreService _storeService;
    private readonly IWidgetPluginManager _widgetPluginManager;

    public OrderEventConsumer(
        IGA4TrackingService ga4TrackingService,
        ISettingService settingService,
        IStoreService storeService,
        IWidgetPluginManager widgetPluginManager)
    {
        _ga4TrackingService = ga4TrackingService;
        _settingService = settingService;
        _storeService = storeService;
        _widgetPluginManager = widgetPluginManager;
    }

    private async Task<bool> IsPluginEnabledAsync(int storeId)
    {
        if (!await _widgetPluginManager.IsPluginActiveAsync(GA4Defaults.SystemName))
            return false;

        var settings = await _settingService.LoadSettingAsync<GA4Settings>(storeId);
        return settings.Enabled && settings.EnableEcommerce;
    }

    /// <summary>
    /// Handle order placed event - save cookies for later server-side tracking
    /// </summary>
    public async Task HandleEventAsync(OrderPlacedEvent eventMessage)
    {
        var order = eventMessage.Order;
        var store = await _storeService.GetStoreByIdAsync(order.StoreId);

        if (!await IsPluginEnabledAsync(store?.Id ?? 0))
            return;

        // Save GA cookies to order for server-side tracking when order is paid
        await _ga4TrackingService.SaveGACookiesToOrderAsync(order);
    }

    /// <summary>
    /// Handle order paid event - send purchase event via Measurement Protocol
    /// </summary>
    public async Task HandleEventAsync(OrderPaidEvent eventMessage)
    {
        var order = eventMessage.Order;
        var store = await _storeService.GetStoreByIdAsync(order.StoreId);

        if (!await IsPluginEnabledAsync(store?.Id ?? 0))
            return;

        var settings = await _settingService.LoadSettingAsync<GA4Settings>(store?.Id ?? 0);
        if (!settings.TrackPurchase)
            return;

        await _ga4TrackingService.SendPurchaseEventAsync(order);
    }

    /// <summary>
    /// Handle order refunded event - send refund event via Measurement Protocol
    /// </summary>
    public async Task HandleEventAsync(OrderRefundedEvent eventMessage)
    {
        var order = eventMessage.Order;
        var store = await _storeService.GetStoreByIdAsync(order.StoreId);

        if (!await IsPluginEnabledAsync(store?.Id ?? 0))
            return;

        var settings = await _settingService.LoadSettingAsync<GA4Settings>(store?.Id ?? 0);
        if (!settings.TrackRefund)
            return;

        // Only send refund if order was previously paid
        if (order.PaymentStatus == PaymentStatus.Paid || order.PaymentStatus == PaymentStatus.PartiallyRefunded)
        {
            await _ga4TrackingService.SendRefundEventAsync(order);
        }
    }
}
