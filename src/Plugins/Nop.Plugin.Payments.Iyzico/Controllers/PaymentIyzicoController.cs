using System;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Payments.Iyzico.Models;
using Nop.Plugin.Payments.Iyzico.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Payments.Iyzico.Controllers;

public class PaymentIyzicoController : BasePluginController
{
    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly ISettingService _settingService;
    private readonly IOrderService _orderService;
    private readonly IOrderProcessingService _orderProcessingService;
    private readonly IIyzicoPaymentService _iyzicoPaymentService;
    private readonly ILogger _logger;
    private readonly IyzicoPaymentSettings _iyzicoPaymentSettings;

    public PaymentIyzicoController(
        ILocalizationService localizationService,
        INotificationService notificationService,
        ISettingService settingService,
        IOrderService orderService,
        IOrderProcessingService orderProcessingService,
        IIyzicoPaymentService iyzicoPaymentService,
        ILogger logger,
        IyzicoPaymentSettings iyzicoPaymentSettings)
    {
        _localizationService = localizationService;
        _notificationService = notificationService;
        _settingService = settingService;
        _orderService = orderService;
        _orderProcessingService = orderProcessingService;
        _iyzicoPaymentService = iyzicoPaymentService;
        _logger = logger;
        _iyzicoPaymentSettings = iyzicoPaymentSettings;
    }

    #region Admin Configure
    
    [Area(AreaNames.ADMIN)]
    [AuthorizeAdmin]
    public IActionResult Configure()
    {
        var model = new ConfigurationModel
        {
            UseSandbox = _iyzicoPaymentSettings.UseSandbox,
            ApiKey = _iyzicoPaymentSettings.ApiKey,
            SecretKey = _iyzicoPaymentSettings.SecretKey,
            AdditionalFee = _iyzicoPaymentSettings.AdditionalFee,
            AdditionalFeePercentage = _iyzicoPaymentSettings.AdditionalFeePercentage,
            AutoApprovalDays = _iyzicoPaymentSettings.AutoApprovalDays,
            EnableAutoApproval = _iyzicoPaymentSettings.EnableAutoApproval,
            AutoCreateSubMerchant = _iyzicoPaymentSettings.AutoCreateSubMerchant
        };

        return View("~/Plugins/Payments.Iyzico/Views/Configure.cshtml", model);
    }

    [HttpPost]
    [Area(AreaNames.ADMIN)]
    [AuthorizeAdmin]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!ModelState.IsValid)
            return Configure();

        _iyzicoPaymentSettings.UseSandbox = model.UseSandbox;
        _iyzicoPaymentSettings.ApiKey = model.ApiKey;
        _iyzicoPaymentSettings.SecretKey = model.SecretKey;
        _iyzicoPaymentSettings.BaseUrl = model.UseSandbox
            ? "https://sandbox-api.iyzipay.com"
            : "https://api.iyzipay.com";
        _iyzicoPaymentSettings.AdditionalFee = model.AdditionalFee;
        _iyzicoPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
        _iyzicoPaymentSettings.AutoApprovalDays = model.AutoApprovalDays;
        _iyzicoPaymentSettings.EnableAutoApproval = model.EnableAutoApproval;
        _iyzicoPaymentSettings.AutoCreateSubMerchant = model.AutoCreateSubMerchant;

        await _settingService.SaveSettingAsync(_iyzicoPaymentSettings);

        _notificationService.SuccessNotification(
            await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

        return Configure();
    }
    
    #endregion

    #region Public 3DS Flow
    
    public async Task<IActionResult> ProcessPayment(int orderId)
    {
        var order = await _orderService.GetOrderByIdAsync(orderId);
        if (order == null)
        {
            return Content("<h1>Order not found</h1>", "text/html");
        }

        try
        {
            var cardInfo = IyzicoPaymentProcessor.GetCardInfo(order.OrderGuid);
            
            if (cardInfo == null || cardInfo.Count == 0)
            {
                await _logger.InformationAsync($"[iyzico] Parsing CustomValuesXml for Order #{orderId}");
                
                cardInfo = new System.Collections.Generic.Dictionary<string, string>();
                
                if (!string.IsNullOrEmpty(order.CustomValuesXml))
                {
                    try
                    {
                        var doc = XDocument.Parse(order.CustomValuesXml);
                        var items = doc.Descendants("item");
                        
                        foreach (var item in items)
                        {
                            var key = item.Element("key")?.Value;
                            var value = item.Element("value")?.Value;
                            
                            if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                            {
                                cardInfo[key] = value;
                            }
                        }
                        
                        await _logger.InformationAsync($"[iyzico] Parsed {cardInfo.Count} values from CustomValuesXml");
                    }
                    catch (Exception ex)
                    {
                        await _logger.ErrorAsync($"[iyzico] Failed to parse CustomValuesXml: {ex.Message}");
                    }
                }
            }

            var cardNumber = cardInfo.ContainsKey("CardNumber") ? cardInfo["CardNumber"] : "";
            
            if (string.IsNullOrEmpty(cardNumber))
            {
                await _logger.ErrorAsync($"Card information missing for order #{orderId}");
                return Content("<h1>Payment Error</h1><p>Card information is missing.</p>", "text/html");
            }

            var response = await _iyzicoPaymentService.InitializePaymentAsync(
                order, 
                cardInfo.GetValueOrDefault("CardholderName", ""), 
                cardInfo.GetValueOrDefault("CardNumber", ""), 
                cardInfo.GetValueOrDefault("ExpireMonth", ""), 
                cardInfo.GetValueOrDefault("ExpireYear", ""), 
                cardInfo.GetValueOrDefault("CardCode", ""));

            if (response.Status == "success" && !string.IsNullOrEmpty(response.HtmlContent))
            {
                return Content(response.HtmlContent, "text/html");
            }
            else
            {
                await _logger.ErrorAsync($"3DS init failed for order #{orderId}: {response.ErrorMessage}");
                return Content($"<h1>Payment Error</h1><p>{response.ErrorMessage}</p>", "text/html");
            }
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync($"ProcessPayment error for order #{orderId}: {ex.Message}", ex);
            return Content($"<h1>Payment Error</h1><p>{ex.Message}</p>", "text/html");
        }
    }

    [HttpPost]
    public async Task<IActionResult> ThreeDSCallback()
    {
        try
        {
            // Log all form data
            System.Console.WriteLine("=== CALLBACK RECEIVED ===");
            foreach (var key in Request.Form.Keys)
            {
                System.Console.WriteLine($"{key}: {Request.Form[key]}");
            }
            System.Console.WriteLine("=========================");

            var token = Request.Form["paymentId"].ToString(); // iyzico sends paymentId
            var paymentId = Request.Form["paymentId"].ToString();
            var conversationId = Request.Form["conversationId"].ToString();
            
            await _logger.InformationAsync($"[iyzico] Callback: token={token}, paymentId={paymentId}, conversationId={conversationId}");
            
            if (string.IsNullOrEmpty(token))
            {
                await _logger.ErrorAsync("iyzico callback: token missing");
                return RedirectToRoute("Homepage");
            }

            var response = await _iyzicoPaymentService.CompletePaymentAsync(token);

            if (response.Status == "success")
            {
                var orderId = int.Parse(response.BasketId ?? "0");
                var order = await _orderService.GetOrderByIdAsync(orderId);

                if (order != null && order.PaymentStatus == Core.Domain.Payments.PaymentStatus.Pending)
                {
                    await _orderProcessingService.MarkOrderAsPaidAsync(order);
                    
                    order.CaptureTransactionId = response.PaymentId;
                    await _orderService.UpdateOrderAsync(order);

                    await _logger.InformationAsync($"Payment successful for order #{orderId}");

                    return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
                }
            }
            else
            {
                await _logger.ErrorAsync($"Payment failed: {response.ErrorMessage}");
            }

            return RedirectToRoute("Homepage");
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync($"Callback error: {ex.Message}", ex);
            return RedirectToRoute("Homepage");
        }
    }
    
    #endregion
}
