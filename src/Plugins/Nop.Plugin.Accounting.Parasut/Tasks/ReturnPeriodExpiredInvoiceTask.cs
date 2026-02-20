using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Data;
using Nop.Plugin.Accounting.Parasut.Domain;
using Nop.Plugin.Accounting.Parasut.Services.Invoice;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Shipping;
using Nop.Services.ScheduleTasks;

namespace Nop.Plugin.Accounting.Parasut.Tasks;

public class ReturnPeriodExpiredInvoiceTask : IScheduleTask
{
    private readonly IOrderService _orderService;
    private readonly IShipmentService _shipmentService;
    private readonly IParasutInvoiceService _invoiceService;
    private readonly ISettingService _settingService;
    private readonly IRepository<ParasutInvoiceRecord> _invoiceRepository;
    private readonly ILogger _logger;

    public ReturnPeriodExpiredInvoiceTask(
        IOrderService orderService,
        IShipmentService shipmentService,
        IParasutInvoiceService invoiceService,
        ISettingService settingService,
        IRepository<ParasutInvoiceRecord> invoiceRepository,
        ILogger logger)
    {
        _orderService = orderService;
        _shipmentService = shipmentService;
        _invoiceService = invoiceService;
        _settingService = settingService;
        _invoiceRepository = invoiceRepository;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        try
        {
            var settings = await _settingService.LoadSettingAsync<ParasutSettings>();
            
            if (settings.InvoiceStrategy != InvoiceCreationStrategy.AfterReturnPeriod)
            {
                await _logger.InformationAsync("Paraşüt: İade süresi task atlandı (strateji farklı)");
                return;
            }

            if (!settings.AutoProcessExpiredReturnPeriods)
            {
                await _logger.InformationAsync("Paraşüt: İade süresi task atlandı (otomatik işlem kapalı)");
                return;
            }

            await _logger.InformationAsync("Paraşüt: İade süresi dolmuş siparişler kontrol ediliyor...");

            var eligibleOrders = await GetOrdersWithExpiredReturnPeriodAsync(settings);

            await _logger.InformationAsync($"Paraşüt: {eligibleOrders.Count} sipariş bulundu");

            var processedCount = 0;
            var skippedCount = 0;
            var errorCount = 0;

            foreach (var order in eligibleOrders)
            {
                try
                {
                    var existingInvoice = await _invoiceRepository.Table
                        .AnyAsync(i => i.OrderId == order.Id);

                    if (existingInvoice)
                    {
                        skippedCount++;
                        continue;
                    }

                    var orderItems = await _orderService.GetOrderItemsAsync(order.Id);
                    
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
                        
                        decimal commissionAmount = 0;
                        decimal shippingAmount = 0;
                        decimal penaltyAmount = 0;

                        foreach (var (item, _) in vendorGroup)
                        {
                            commissionAmount += item.PriceInclTax * 0.15m;
                        }

                        if (settings.AutoCreateCommissionInvoice && commissionAmount > 0)
                        {
                            await _invoiceService.CreateVendorInvoiceAsync(
                                vendorId, 
                                order.Id, 
                                commissionAmount, 
                                shippingAmount, 
                                penaltyAmount
                            );

                            processedCount++;
                            await _logger.InformationAsync(
                                $"Paraşüt: Sipariş #{order.Id}, Vendor #{vendorId} için fatura oluşturuldu");
                        }
                    }
                }
                catch (Exception ex)
                {
                    errorCount++;
                    await _logger.ErrorAsync($"Paraşüt: Sipariş #{order.Id} için fatura oluşturulamadı", ex);
                }
            }

            await _logger.InformationAsync(
                $"Paraşüt: İade süresi task tamamlandı. " +
                $"İşlenen: {processedCount}, Atlanan: {skippedCount}, Hata: {errorCount}");
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync("Paraşüt: İade süresi task genel hatası", ex);
        }
    }

    private async Task<List<Order>> GetOrdersWithExpiredReturnPeriodAsync(ParasutSettings settings)
    {
        var allOrders = await _orderService.SearchOrdersAsync(
            psIds: new List<int> { (int)PaymentStatus.Paid },
            pageIndex: 0,
            pageSize: int.MaxValue
        );

        var eligibleOrders = new List<Order>();
        var today = DateTime.UtcNow.Date;

        foreach (var order in allOrders)
        {
            if (order.OrderStatus == OrderStatus.Cancelled)
                continue;

            if (order.RefundedAmount > 0)
                continue;

            DateTime? startDate = settings.StartFrom switch
            {
                ReturnPeriodStartFrom.OrderDate => order.CreatedOnUtc,
                ReturnPeriodStartFrom.PaymentDate => order.PaidDateUtc ?? order.CreatedOnUtc,
                ReturnPeriodStartFrom.ShippingDate => await GetShippingDateAsync(order),
                ReturnPeriodStartFrom.DeliveryDate => await GetDeliveryDateAsync(order),
                _ => order.CreatedOnUtc
            };

            if (startDate == null)
                continue;

            var returnPeriodEndDate = startDate.Value.Date.AddDays(settings.ReturnPeriodDays);

            if (today > returnPeriodEndDate)
            {
                eligibleOrders.Add(order);
            }
        }

        return eligibleOrders;
    }

    private async Task<DateTime?> GetShippingDateAsync(Order order)
    {
        var shipments = await _shipmentService.GetShipmentsByOrderIdAsync(order.Id);
        return shipments.FirstOrDefault()?.ShippedDateUtc;
    }

    private async Task<DateTime?> GetDeliveryDateAsync(Order order)
    {
        var shipments = await _shipmentService.GetShipmentsByOrderIdAsync(order.Id);
        return shipments.FirstOrDefault()?.DeliveryDateUtc;
    }
}
