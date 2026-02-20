using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Plugin.Payments.Iyzico.Models.Api;

namespace Nop.Plugin.Payments.Iyzico.Services;

public interface IIyzicoPaymentService
{
    Task<ThreeDSInitializeResponse> InitializePaymentAsync(Order order, string cardHolderName, string cardNumber, string expireMonth, string expireYear, string cvc);
    Task<PaymentResponse> CompletePaymentAsync(string token);
}

public class IyzicoPaymentService : IIyzicoPaymentService
{
    private readonly IIyzicoApiService _iyzicoApiService;
    private readonly IIyzicoTransactionService _transactionService;
    private readonly IProductService _productService;
    private readonly IOrderService _orderService;
    private readonly IWorkContext _workContext;
    private readonly IWebHelper _webHelper;
    private readonly ILogger _logger;

    public IyzicoPaymentService(
        IIyzicoApiService iyzicoApiService,
        IIyzicoTransactionService transactionService,
        IProductService productService,
        IOrderService orderService,
        IWorkContext workContext,
        IWebHelper webHelper,
        ILogger logger)
    {
        _iyzicoApiService = iyzicoApiService;
        _transactionService = transactionService;
        _productService = productService;
        _orderService = orderService;
        _workContext = workContext;
        _webHelper = webHelper;
        _logger = logger;
    }

    public async Task<ThreeDSInitializeResponse> InitializePaymentAsync(
        Order order, 
        string cardHolderName, 
        string cardNumber, 
        string expireMonth, 
        string expireYear, 
        string cvc)
    {
        try
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var basketItems = new List<BasketItem>();

            var orderItems = await _orderService.GetOrderItemsAsync(order.Id);

            foreach (var orderItem in orderItems)
            {
                var product = await _productService.GetProductByIdAsync(orderItem.ProductId);

                basketItems.Add(new BasketItem
                {
                    Id = orderItem.Id.ToString(),
                    Name = product?.Name ?? "Product",
                    Category1 = "Product",
                    Category2 = "",
                    Price = orderItem.PriceInclTax.ToString("0.00", CultureInfo.InvariantCulture),
                    ItemType = "PHYSICAL"
                });
            }

            var request = new ThreeDSInitializeRequest
            {
                Locale = "tr",
                ConversationId = order.OrderGuid.ToString(),
                Price = order.OrderTotal.ToString("0.00", CultureInfo.InvariantCulture),
                PaidPrice = order.OrderTotal.ToString("0.00", CultureInfo.InvariantCulture),
                Currency = "TRY",
                Installment = 1,
                BasketId = order.Id.ToString(),
                PaymentChannel = "WEB",
                PaymentGroup = "PRODUCT",
                CallbackUrl = $"{_webHelper.GetStoreLocation()}Plugins/PaymentIyzico/ThreeDSCallback",
                PaymentCard = new PaymentCard
                {
                    CardHolderName = cardHolderName,
                    CardNumber = cardNumber.Replace(" ", ""),
                    ExpireMonth = expireMonth,
                    ExpireYear = expireYear,
                    Cvc = cvc,
                    RegisterCard = 0
                },
                Buyer = new Buyer
                {
                    Id = customer.Id.ToString(),
                    Name = customer.FirstName ?? "Customer",
                    Surname = customer.LastName ?? "Name",
                    Email = customer.Email,
                    GsmNumber = "+905551234567",
                    IdentityNumber = "11111111111",
                    RegistrationAddress = "Test Address",
                    Ip = customer.LastIpAddress ?? "127.0.0.1",
                    City = "Istanbul",
                    Country = "Turkey",
                    ZipCode = "34000"
                },
                ShippingAddress = new Address
                {
                    ContactName = "Test User",
                    City = "Istanbul",
                    Country = "Turkey",
                    AddressLine = "Test Address Line 1",
                    ZipCode = "34000"
                },
                BillingAddress = new Address
                {
                    ContactName = "Test User",
                    City = "Istanbul",
                    Country = "Turkey",
                    AddressLine = "Test Address Line 1",
                    ZipCode = "34000"
                },
                BasketItems = basketItems
            };

            return await _iyzicoApiService.Initialize3DSAsync(request);
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync($"iyzico payment initialization error: {ex.Message}", ex);
            return new ThreeDSInitializeResponse 
            { 
                Status = "failure", 
                ErrorMessage = ex.Message 
            };
        }
    }

    public async Task<PaymentResponse> CompletePaymentAsync(string token)
    {
        var response = await _iyzicoApiService.Complete3DSAsync(token);

        if (response.Status == "success" && response.ItemTransactions != null)
        {
            foreach (var item in response.ItemTransactions)
            {
                var transaction = new Domain.IyzicoPaymentTransaction
                {
                    OrderId = int.Parse(response.BasketId ?? "0"),
                    OrderItemId = int.Parse(item.ItemId ?? "0"),
                    PaymentTransactionId = item.PaymentTransactionId ?? "",
                    PaymentId = response.PaymentId ?? "",
                    ConversationId = response.ConversationId ?? "",
                    SubMerchantKey = item.SubMerchantKey ?? "",
                    SubMerchantPrice = item.SubMerchantPrice,
                    Status = "Paid",
                    CreatedOnUtc = DateTime.UtcNow,
                    RawResponse = System.Text.Json.JsonSerializer.Serialize(response)
                };

                await _transactionService.InsertAsync(transaction);
            }
        }

        return response;
    }
}
