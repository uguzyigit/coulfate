using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Data;
using Nop.Plugin.Payments.Iyzico.Data;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Payments.Iyzico;

public class IyzicoPaymentProcessor : BasePlugin, IPaymentMethod, IWidgetPlugin
{
    private readonly IyzicoPaymentSettings _iyzicoPaymentSettings;
    private readonly ISettingService _settingService;
    private readonly ILocalizationService _localizationService;
    private readonly IWebHelper _webHelper;
    private readonly INopDataProvider _dataProvider;
    private readonly IOrderTotalCalculationService _orderTotalCalculationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    // Thread-safe storage
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<Guid, Dictionary<string, string>> _tempCardStorage = new();

    public IyzicoPaymentProcessor(
        IyzicoPaymentSettings iyzicoPaymentSettings,
        ISettingService settingService,
        ILocalizationService localizationService,
        IWebHelper webHelper,
        INopDataProvider dataProvider,
        IOrderTotalCalculationService orderTotalCalculationService,
        IHttpContextAccessor httpContextAccessor)
    {
        _iyzicoPaymentSettings = iyzicoPaymentSettings;
        _settingService = settingService;
        _localizationService = localizationService;
        _webHelper = webHelper;
        _dataProvider = dataProvider;
        _orderTotalCalculationService = orderTotalCalculationService;
        _httpContextAccessor = httpContextAccessor;
    }

    #region Payment Method

    public bool SupportCapture => false;
    public bool SupportPartiallyRefund => false;
    public bool SupportRefund => false;
    public bool SupportVoid => false;
    public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported;
    public PaymentMethodType PaymentMethodType => PaymentMethodType.Redirection;
    public bool SkipPaymentInfo => false;

    public async Task<string> GetPaymentMethodDescriptionAsync()
    {
        return await _localizationService.GetResourceAsync("Plugins.Payments.Iyzico.PaymentMethodDescription");
    }

    public Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
    {
        // Store card info in concurrent dictionary (won't be removed until ProcessPayment reads it)
        var cardInfo = new Dictionary<string, string>
        {
            ["CardholderName"] = processPaymentRequest.CustomValues["CardholderName"]?.ToString() ?? "",
            ["CardNumber"] = processPaymentRequest.CustomValues["CardNumber"]?.ToString() ?? "",
            ["ExpireMonth"] = processPaymentRequest.CustomValues["ExpireMonth"]?.ToString() ?? "",
            ["ExpireYear"] = processPaymentRequest.CustomValues["ExpireYear"]?.ToString() ?? "",
            ["CardCode"] = processPaymentRequest.CustomValues["CardCode"]?.ToString() ?? ""
        };

        _tempCardStorage[processPaymentRequest.OrderGuid] = cardInfo;

        return Task.FromResult(new ProcessPaymentResult { NewPaymentStatus = PaymentStatus.Pending });
    }

    public Task PostProcessPaymentAsync(PostProcessPaymentRequest postProcessPaymentRequest)
    {
        var order = postProcessPaymentRequest.Order;
        
        // Redirect to our ProcessPayment endpoint for 3DS flow
        var processUrl = $"{_webHelper.GetStoreLocation()}Plugins/PaymentIyzico/ProcessPayment?orderId={order.Id}";
        
        _httpContextAccessor.HttpContext.Response.Redirect(processUrl);
        
        return Task.CompletedTask;
    }

    public static Dictionary<string, string> GetCardInfo(Guid orderGuid)
    {
        // DON'T remove immediately - let ProcessPayment controller handle it
        if (_tempCardStorage.TryGetValue(orderGuid, out var cardInfo))
        {
            return cardInfo;
        }
        return null;
    }

    public static void RemoveCardInfo(Guid orderGuid)
    {
        _tempCardStorage.TryRemove(orderGuid, out _);
    }

    public Task<bool> HidePaymentMethodAsync(IList<ShoppingCartItem> cart)
    {
        return Task.FromResult(false);
    }

    public async Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart)
    {
        return await _orderTotalCalculationService.CalculatePaymentAdditionalFeeAsync(cart,
            _iyzicoPaymentSettings.AdditionalFee, _iyzicoPaymentSettings.AdditionalFeePercentage);
    }

    public Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest capturePaymentRequest)
    {
        return Task.FromResult(new CapturePaymentResult { Errors = new[] { "Capture not supported" } });
    }

    public Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
    {
        return Task.FromResult(new RefundPaymentResult { Errors = new[] { "Refund not supported" } });
    }

    public Task<VoidPaymentResult> VoidAsync(VoidPaymentRequest voidPaymentRequest)
    {
        return Task.FromResult(new VoidPaymentResult { Errors = new[] { "Void not supported" } });
    }

    public Task<ProcessPaymentResult> ProcessRecurringPaymentAsync(ProcessPaymentRequest processPaymentRequest)
    {
        return Task.FromResult(new ProcessPaymentResult { Errors = new[] { "Recurring payment not supported" } });
    }

    public Task<CancelRecurringPaymentResult> CancelRecurringPaymentAsync(CancelRecurringPaymentRequest cancelPaymentRequest)
    {
        return Task.FromResult(new CancelRecurringPaymentResult { Errors = new[] { "Recurring payment not supported" } });
    }

    public Task<bool> CanRePostProcessPaymentAsync(Order order)
    {
        return Task.FromResult(false);
    }

    public Task<IList<string>> ValidatePaymentFormAsync(IFormCollection form)
    {
        return Task.FromResult<IList<string>>(new List<string>());
    }

    public Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
    {
        var request = new ProcessPaymentRequest();
        
        request.CustomValues["CardholderName"] = form["CardholderName"].ToString();
        request.CustomValues["CardNumber"] = form["CardNumber"].ToString();
        request.CustomValues["ExpireMonth"] = form["ExpireMonth"].ToString();
        request.CustomValues["ExpireYear"] = form["ExpireYear"].ToString();
        request.CustomValues["CardCode"] = form["CardCode"].ToString();
        
        return Task.FromResult(request);
    }

    public override string GetConfigurationPageUrl()
    {
        return $"{_webHelper.GetStoreLocation()}Admin/PaymentIyzico/Configure";
    }

    public Type GetPublicViewComponent()
    {
        return typeof(Components.PaymentIyzicoViewComponent);
    }

    #endregion

    #region Plugin

    public override async Task InstallAsync()
    {
        var settings = new IyzicoPaymentSettings
        {
            UseSandbox = true,
            ApiKey = "",
            SecretKey = "",
            BaseUrl = "https://sandbox-api.iyzipay.com",
            AdditionalFee = 0,
            AdditionalFeePercentage = false,
            AutoApprovalDays = 3,
            EnableAutoApproval = false,
            AutoCreateSubMerchant = false
        };

        await _settingService.SaveSettingAsync(settings);

        await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
        {
            ["Plugins.Payments.Iyzico.Fields.UseSandbox"] = "Use Sandbox",
            ["Plugins.Payments.Iyzico.Fields.ApiKey"] = "API Key",
            ["Plugins.Payments.Iyzico.Fields.SecretKey"] = "Secret Key",
            ["Plugins.Payments.Iyzico.Fields.AdditionalFee"] = "Additional Fee",
            ["Plugins.Payments.Iyzico.Fields.AdditionalFeePercentage"] = "Additional Fee Percentage",
            ["Plugins.Payments.Iyzico.PaymentMethodDescription"] = "Pay with Credit/Debit Card via iyzico"
        });

        await base.InstallAsync();
    }

    public override async Task UninstallAsync()
    {
        await _settingService.DeleteSettingAsync<IyzicoPaymentSettings>();
        await _localizationService.DeleteLocaleResourcesAsync("Plugins.Payments.Iyzico");
        
        // Drop iyzico tables if they exist
        try
        {
            await _dataProvider.ExecuteNonQueryAsync(InstallationData.DropTablesScript);
        }
        catch
        {
            // Ignore if tables don't exist
        }

        await base.UninstallAsync();
    }

    #endregion

    #region Widget

    public Task<IList<string>> GetWidgetZonesAsync()
    {
        return Task.FromResult<IList<string>>(new List<string>());
    }

    public Type GetWidgetViewComponent(string widgetZone)
    {
        return typeof(Components.PaymentIyzicoViewComponent);
    }

    public bool HideInWidgetList => true;

    #endregion
}
