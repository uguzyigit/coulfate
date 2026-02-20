using System;
using System.Linq;
using System.Threading.Tasks;
using Marketplace.Abstractions.Domain;
using Marketplace.Abstractions.Services;
using Nop.Core;
using Nop.Core.Events;
using Nop.Services.Events;
using Nop.Services.Logging;

namespace Nop.Plugin.Marketplace.VendorExtensions.Infrastructure.EventConsumers;

/// <summary>
/// Event consumer for OrderCommission entity insertions
/// Automatically creates vendor transactions when commission is calculated
/// </summary>
public class OrderCommissionCreatedConsumer : IConsumer<EntityInsertedEvent<BaseEntity>>
{
    private readonly IVendorAccountService _vendorAccountService;
    private readonly ILogger _logger;

    public OrderCommissionCreatedConsumer(
        IVendorAccountService vendorAccountService,
        ILogger logger)
    {
        _vendorAccountService = vendorAccountService;
        _logger = logger;
    }

    public async Task HandleEventAsync(EntityInsertedEvent<BaseEntity> eventMessage)
    {
        try
        {
            var entity = eventMessage.Entity;
            
            // Check if this is OrderCommission entity
            var entityType = entity.GetType();
            if (entityType.Name != "OrderCommission" || !entityType.FullName.Contains("Marketplace.Commission"))
                return;

            // Use reflection to get properties
            var orderIdProp = entityType.GetProperty("OrderId");
            var orderItemIdProp = entityType.GetProperty("OrderItemId");
            var vendorIdProp = entityType.GetProperty("VendorId");
            var netPriceProp = entityType.GetProperty("NetPrice");
            var commissionAmountProp = entityType.GetProperty("CommissionAmount");
            var marketplaceFeeProp = entityType.GetProperty("MarketplaceFee");
            var taxWithholdingProp = entityType.GetProperty("TaxWithholding");

            if (orderIdProp == null || vendorIdProp == null)
                return;

            var orderId = (int)orderIdProp.GetValue(entity);
            var orderItemId = (int)orderItemIdProp.GetValue(entity);
            var vendorId = (int)vendorIdProp.GetValue(entity);
            var netPrice = (decimal)netPriceProp.GetValue(entity);
            var commissionAmount = (decimal)commissionAmountProp.GetValue(entity);
            var marketplaceFee = (decimal)marketplaceFeeProp.GetValue(entity);
            var taxWithholding = (decimal)taxWithholdingProp.GetValue(entity);

            // Check if transactions already exist for this order item
            var existingTransactions = await _vendorAccountService.GetTransactionsAsync(
                vendorId: vendorId,
                orderId: orderId,
                type: null);
            
            if (existingTransactions.Any(t => t.OrderItemId == orderItemId))
            {
                Console.WriteLine($"[VENDOR TRANSACTIONS] Transactions already exist for Order #{orderId}, OrderItem #{orderItemId}, skipping");
                return;
            }

            Console.WriteLine("==========================================");
            Console.WriteLine($"[VENDOR TRANSACTIONS] Processing commission for Order #{orderId}");
            Console.WriteLine("==========================================");

            await _logger.InformationAsync($"[VENDOR TRANSACTIONS] Processing commission for Order #{orderId}");

            // 1. Order Revenue
            await _vendorAccountService.AddTransactionAsync(
                vendorId: vendorId,
                type: VendorTransactionType.OrderRevenue,
                amount: netPrice,
                orderId: orderId,
                orderItemId: orderItemId,
                description: $"Sipariş #{orderId} - Satış Geliri",
                referenceNumber: $"ORD-{orderId}-{orderItemId}");

            Console.WriteLine($"[VENDOR TRANSACTIONS] Revenue recorded: {netPrice} TL");
            await _logger.InformationAsync($"[VENDOR TRANSACTIONS] Revenue recorded: {netPrice} TL");

            // 2. Commission Deduction
            await _vendorAccountService.AddTransactionAsync(
                vendorId: vendorId,
                type: VendorTransactionType.Commission,
                amount: -commissionAmount,
                orderId: orderId,
                orderItemId: orderItemId,
                description: $"Komisyon Kesintisi",
                referenceNumber: $"COM-{orderId}-{orderItemId}");

            Console.WriteLine($"[VENDOR TRANSACTIONS] Commission recorded: -{commissionAmount} TL");
            await _logger.InformationAsync($"[VENDOR TRANSACTIONS] Commission recorded: -{commissionAmount} TL");

            // 3. Marketplace Fee
            await _vendorAccountService.AddTransactionAsync(
                vendorId: vendorId,
                type: VendorTransactionType.MarketplaceFee,
                amount: -marketplaceFee,
                orderId: orderId,
                orderItemId: orderItemId,
                description: $"Platform Ücreti",
                referenceNumber: $"FEE-{orderId}-{orderItemId}");

            Console.WriteLine($"[VENDOR TRANSACTIONS] Fee recorded: -{marketplaceFee} TL");
            await _logger.InformationAsync($"[VENDOR TRANSACTIONS] Fee recorded: -{marketplaceFee} TL");

            // 4. Tax Withholding
            await _vendorAccountService.AddTransactionAsync(
                vendorId: vendorId,
                type: VendorTransactionType.TaxWithholding,
                amount: -taxWithholding,
                orderId: orderId,
                orderItemId: orderItemId,
                description: $"Stopaj Kesintisi",
                referenceNumber: $"TAX-{orderId}-{orderItemId}");

            Console.WriteLine($"[VENDOR TRANSACTIONS] Tax recorded: -{taxWithholding} TL");
            await _logger.InformationAsync($"[VENDOR TRANSACTIONS] Tax recorded: -{taxWithholding} TL");

            Console.WriteLine("==========================================");
            Console.WriteLine($"[VENDOR TRANSACTIONS] All transactions created for Order #{orderId}");
            Console.WriteLine("==========================================");
            
            await _logger.InformationAsync($"[VENDOR TRANSACTIONS] All transactions created for Order #{orderId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine("==========================================");
            Console.WriteLine($"[VENDOR TRANSACTIONS ERROR] {ex.Message}");
            Console.WriteLine($"[VENDOR TRANSACTIONS ERROR] Stack: {ex.StackTrace}");
            Console.WriteLine("==========================================");
            
            await _logger.ErrorAsync($"[VENDOR TRANSACTIONS] Error creating transactions", ex);
        }
    }
}
