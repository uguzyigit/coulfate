using Nop.Services.Events;
using Nop.Services.Orders;
using Nop.Services.Logging;
using Nop.Plugin.Accounting.Parasut.Services.Invoice;
using Nop.Services.Configuration;
using Nop.Core.Domain.Orders;

namespace Nop.Plugin.Accounting.Parasut.Infrastructure.EventConsumers;

public class OrderPaidEventConsumer : IConsumer<OrderPaidEvent>
{
    private readonly IParasutInvoiceService _invoiceService;
    private readonly IOrderService _orderService;
    private readonly ILogger _logger;
    private readonly ParasutSettings _settings;

    public OrderPaidEventConsumer(
        IParasutInvoiceService invoiceService,
        IOrderService orderService,
        ILogger logger,
        ISettingService settingService)
    {
        _invoiceService = invoiceService;
        _orderService = orderService;
        _logger = logger;
        _settings = settingService.LoadSettingAsync<ParasutSettings>().Result;
    }

    public async Task HandleEventAsync(OrderPaidEvent eventMessage)
    {
        if (!_settings.Enabled)
            return;

        var order = eventMessage.Order;

        try
        {
            var orderItems = await _orderService.GetOrderItemsAsync(order.Id);
            
            // Group by vendor - ProductId üzerinden vendor ID'yi alacağız
            var itemsWithVendors = new List<(OrderItem item, int vendorId)>();
            
            foreach (var item in orderItems)
            {
                var product = await _orderService.GetProductByOrderItemIdAsync(item.Id);
                if (product != null && product.VendorId > 0)
                {
                    itemsWithVendors.Add((item, product.VendorId));
                }
            }
            
            var vendorGroups = itemsWithVendors.GroupBy(x => x.vendorId);

            foreach (var vendorGroup in vendorGroups)
            {
                var vendorId = vendorGroup.Key;
                
                // Calculate charges (simplified - integrate with Commission plugin)
                decimal commissionAmount = 0;
                decimal shippingAmount = 0;
                decimal penaltyAmount = 0;

                foreach (var (item, _) in vendorGroup)
                {
                    // TODO: Integrate with ICommissionService
                    commissionAmount += item.PriceInclTax * 0.15m; // 15% example
                }

                if (commissionAmount > 0 || shippingAmount > 0 || penaltyAmount > 0)
                {
                    await _invoiceService.CreateVendorInvoiceAsync(
                        vendorId, order.Id, commissionAmount, shippingAmount, penaltyAmount);
                }
            }
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync($"Paraşüt invoice creation failed for order {order.Id}", ex);
            // Don't throw - order processing should continue
        }
    }
}