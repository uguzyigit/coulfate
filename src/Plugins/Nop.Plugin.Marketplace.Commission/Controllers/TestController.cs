using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Plugin.Marketplace.Commission.Services;
using Marketplace.Abstractions.Services;
using Marketplace.Abstractions.Domain;
using System.Collections.Generic;

namespace Nop.Plugin.Marketplace.Commission.Controllers;

[Area(AreaNames.ADMIN)]
[AuthorizeAdmin]
public class TestController : BasePluginController
{
    private readonly ICommissionService _commissionService;
    private readonly IVendorAccountService _vendorAccountService;

    public TestController(
        ICommissionService commissionService,
        IVendorAccountService vendorAccountService)
    {
        _commissionService = commissionService;
        _vendorAccountService = vendorAccountService;
    }

    public async Task<IActionResult> ProcessOrder(int orderItemId)
    {
        try
        {
            // Calculate commission
            var commissionResult = await _commissionService.CalculateAndCreateCommissionAsync(orderItemId);

            if (commissionResult == null)
                return Content($"ERROR: Commission calculation failed for OrderItem #{orderItemId}");

            // Create transactions
            var transactions = new List<(VendorTransactionType Type, decimal Amount, int? OrderId, int? OrderItemId, string Description, string ReferenceNumber)>
            {
                (VendorTransactionType.OrderRevenue, commissionResult.NetPrice, commissionResult.OrderId, orderItemId, 
                 $"Order #{commissionResult.OrderId} - Revenue", $"TEST-{commissionResult.OrderId}"),
                
                (VendorTransactionType.Commission, -commissionResult.CommissionAmount, commissionResult.OrderId, orderItemId, 
                 $"Order #{commissionResult.OrderId} - Commission ({commissionResult.CommissionRate}%)", $"TEST-{commissionResult.OrderId}"),
                
                (VendorTransactionType.MarketplaceFee, -commissionResult.MarketplaceFee, commissionResult.OrderId, orderItemId, 
                 $"Order #{commissionResult.OrderId} - Fee", $"TEST-{commissionResult.OrderId}"),
                
                (VendorTransactionType.TaxWithholding, -commissionResult.TaxWithholding, commissionResult.OrderId, orderItemId, 
                 $"Order #{commissionResult.OrderId} - Tax", $"TEST-{commissionResult.OrderId}")
            };

            await _vendorAccountService.AddTransactionsAsync(commissionResult.VendorId, transactions);

            return Content($"SUCCESS! OrderItem #{orderItemId} processed. Commission: {commissionResult.CommissionAmount} TL, Vendor Net: {commissionResult.VendorNetAmount} TL");
        }
        catch (System.Exception ex)
        {
            return Content($"ERROR: {ex.Message}\n\nStackTrace: {ex.StackTrace}");
        }
    }
}
