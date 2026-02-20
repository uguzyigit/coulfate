using System;
using System.Threading.Tasks;
using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using Nop.Services.Logging;
using CustomModels = Nop.Plugin.Payments.Iyzico.Models.Api;

namespace Nop.Plugin.Payments.Iyzico.Services;

public interface IIyzicoApiService
{
    Task<CustomModels.ThreeDSInitializeResponse> Initialize3DSAsync(CustomModels.ThreeDSInitializeRequest request);
    Task<CustomModels.PaymentResponse> Complete3DSAsync(string token);
}

public class IyzicoApiService : IIyzicoApiService
{
    private readonly IyzicoPaymentSettings _settings;
    private readonly ILogger _logger;

    public IyzicoApiService(IyzicoPaymentSettings settings, ILogger logger)
    {
        _settings = settings;
        _logger = logger;
    }

    public async Task<CustomModels.ThreeDSInitializeResponse> Initialize3DSAsync(CustomModels.ThreeDSInitializeRequest customRequest)
    {
        try
        {
            var options = new Options
            {
                ApiKey = _settings.ApiKey,
                SecretKey = _settings.SecretKey,
                BaseUrl = _settings.BaseUrl
            };

            var request = new CreatePaymentRequest
            {
                Locale = Locale.TR.ToString(),
                ConversationId = customRequest.ConversationId,
                Price = customRequest.Price,
                PaidPrice = customRequest.PaidPrice,
                Currency = Currency.TRY.ToString(),
                Installment = 1,
                BasketId = customRequest.BasketId,
                PaymentChannel = PaymentChannel.WEB.ToString(),
                PaymentGroup = PaymentGroup.PRODUCT.ToString(),
                CallbackUrl = customRequest.CallbackUrl,
                PaymentCard = new PaymentCard
                {
                    CardHolderName = customRequest.PaymentCard.CardHolderName,
                    CardNumber = customRequest.PaymentCard.CardNumber,
                    ExpireMonth = customRequest.PaymentCard.ExpireMonth,
                    ExpireYear = customRequest.PaymentCard.ExpireYear,
                    Cvc = customRequest.PaymentCard.Cvc,
                    RegisterCard = 0
                },
                Buyer = new Buyer
                {
                    Id = customRequest.Buyer.Id,
                    Name = customRequest.Buyer.Name,
                    Surname = customRequest.Buyer.Surname,
                    GsmNumber = customRequest.Buyer.GsmNumber,
                    Email = customRequest.Buyer.Email,
                    IdentityNumber = customRequest.Buyer.IdentityNumber,
                    RegistrationAddress = customRequest.Buyer.RegistrationAddress,
                    Ip = customRequest.Buyer.Ip,
                    City = customRequest.Buyer.City,
                    Country = customRequest.Buyer.Country,
                    ZipCode = customRequest.Buyer.ZipCode
                },
                ShippingAddress = new Address
                {
                    ContactName = customRequest.ShippingAddress.ContactName,
                    City = customRequest.ShippingAddress.City,
                    Country = customRequest.ShippingAddress.Country,
                    Description = customRequest.ShippingAddress.AddressLine,
                    ZipCode = customRequest.ShippingAddress.ZipCode
                },
                BillingAddress = new Address
                {
                    ContactName = customRequest.BillingAddress.ContactName,
                    City = customRequest.BillingAddress.City,
                    Country = customRequest.BillingAddress.Country,
                    Description = customRequest.BillingAddress.AddressLine,
                    ZipCode = customRequest.BillingAddress.ZipCode
                },
                BasketItems = new System.Collections.Generic.List<BasketItem>()
            };

            foreach (var item in customRequest.BasketItems)
            {
                request.BasketItems.Add(new BasketItem
                {
                    Id = item.Id,
                    Name = item.Name,
                    Category1 = item.Category1,
                    ItemType = BasketItemType.PHYSICAL.ToString(),
                    Price = item.Price
                });
            }

            System.Console.WriteLine("=== CALLING IYZICO SDK ===");
            var payment = await Task.Run(() => ThreedsInitialize.Create(request, options));
            System.Console.WriteLine($"Status: {payment.Status}");
            
            if (payment.Status == "success")
            {
                return new CustomModels.ThreeDSInitializeResponse
                {
                    Status = "success",
                    HtmlContent = payment.HtmlContent,
                    PaymentId = payment.PaymentId
                };
            }
            else
            {
                System.Console.WriteLine($"Error: {payment.ErrorMessage}");
                return new CustomModels.ThreeDSInitializeResponse
                {
                    Status = "failure",
                    ErrorMessage = payment.ErrorMessage,
                    ErrorCode = payment.ErrorCode
                };
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"SDK ERROR: {ex.Message}");
            await _logger.ErrorAsync($"iyzico SDK error: {ex.Message}", ex);
            return new CustomModels.ThreeDSInitializeResponse { Status = "failure", ErrorMessage = ex.Message };
        }
    }

    public async Task<CustomModels.PaymentResponse> Complete3DSAsync(string paymentId)
    {
        try
        {
            System.Console.WriteLine($"=== COMPLETING 3DS PAYMENT ===");
            System.Console.WriteLine($"PaymentId: {paymentId}");
            
            var options = new Options 
            { 
                ApiKey = _settings.ApiKey, 
                SecretKey = _settings.SecretKey, 
                BaseUrl = _settings.BaseUrl 
            };
            
            var request = new CreateThreedsPaymentRequest 
            { 
                Locale = Locale.TR.ToString(), 
                ConversationId = Guid.NewGuid().ToString(), 
                PaymentId = paymentId 
            };
            
            System.Console.WriteLine($"Calling ThreedsPayment.Create...");
            var payment = await Task.Run(() => ThreedsPayment.Create(request, options));
            
            System.Console.WriteLine($"=== 3DS COMPLETE RESPONSE ===");
            System.Console.WriteLine($"Status: {payment.Status}");
            System.Console.WriteLine($"PaymentId: {payment.PaymentId}");
            System.Console.WriteLine($"BasketId: {payment.BasketId}");
            System.Console.WriteLine($"ErrorMessage: {payment.ErrorMessage}");
            System.Console.WriteLine($"ErrorCode: {payment.ErrorCode}");
            System.Console.WriteLine($"==============================");

            if (payment.Status == "success")
            {
                var response = new CustomModels.PaymentResponse
                {
                    Status = "success",
                    PaymentId = payment.PaymentId,
                    BasketId = payment.BasketId,
                    ConversationId = payment.ConversationId,
                    ItemTransactions = new System.Collections.Generic.List<CustomModels.ItemTransaction>()
                };

                if (payment.PaymentItems != null)
                {
                    System.Console.WriteLine($"PaymentItems count: {payment.PaymentItems.Count}");
                    foreach (var item in payment.PaymentItems)
                    {
                        response.ItemTransactions.Add(new CustomModels.ItemTransaction
                        {
                            ItemId = item.ItemId,
                            PaymentTransactionId = item.PaymentTransactionId,
                            SubMerchantKey = item.SubMerchantKey ?? "",
                            SubMerchantPrice = decimal.TryParse(item.SubMerchantPrice, out var price) ? price : 0m
                        });
                    }
                }
                
                return response;
            }
            else
            {
                System.Console.WriteLine($"Payment failed: {payment.ErrorMessage}");
                return new CustomModels.PaymentResponse { Status = "failure", ErrorMessage = payment.ErrorMessage };
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"=== 3DS COMPLETE ERROR ===");
            System.Console.WriteLine($"Error: {ex.Message}");
            System.Console.WriteLine($"StackTrace: {ex.StackTrace}");
            System.Console.WriteLine($"==========================");
            
            await _logger.ErrorAsync($"iyzico 3DS complete error: {ex.Message}", ex);
            return new CustomModels.PaymentResponse { Status = "failure", ErrorMessage = ex.Message };
        }
    }
}
