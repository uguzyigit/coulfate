using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marketplace.Abstractions.Domain;
using Marketplace.Abstractions.Services;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Marketplace.VendorExtensions.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Vendors;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Framework.UI;

namespace Nop.Plugin.Marketplace.VendorExtensions.Controllers;

[Area(AreaNames.ADMIN)]
[AuthorizeAdmin]
[AutoValidateAntiforgeryToken]
public class VendorExtensionsController : BasePluginController
{
    private readonly ISettingService _settingService;
    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly IPermissionService _permissionService;
    private readonly IVendorService _vendorService;
    private readonly IVendorAccountService _vendorAccountService;
    private readonly INopHtmlHelper _nopHtmlHelper;
    private readonly IWorkContext _workContext;

    public VendorExtensionsController(
        ISettingService settingService,
        ILocalizationService localizationService,
        INotificationService notificationService,
        IPermissionService permissionService,
        IVendorService vendorService,
        IVendorAccountService vendorAccountService,
        INopHtmlHelper nopHtmlHelper,
        IWorkContext workContext)
    {
        _settingService = settingService;
        _localizationService = localizationService;
        _notificationService = notificationService;
        _permissionService = permissionService;
        _vendorService = vendorService;
        _vendorAccountService = vendorAccountService;
        _nopHtmlHelper = nopHtmlHelper;
        _workContext = workContext;
    }

    #region Vendor Access Control Helpers

    /// <summary>
    /// Check if current user is a vendor
    /// </summary>
    private async Task<bool> IsVendorAsync()
    {
        var vendor = await _workContext.GetCurrentVendorAsync();
        return vendor != null;
    }

    /// <summary>
    /// Checks vendor access and returns AccessDenied view if user is a vendor
    /// </summary>
    private async Task<IActionResult> CheckVendorAccessAsync()
    {
        if (await IsVendorAsync())
            return AccessDeniedView();
        return null;
    }

    #endregion

    #region Configure

    public async Task<IActionResult> Configure()
    {
        // Vendor access check
        var vendorCheck = await CheckVendorAccessAsync();
        if (vendorCheck != null) return vendorCheck;

        _nopHtmlHelper.SetActiveMenuItemSystemName("Marketplace.VendorExtensions.Settings");

        var model = new ConfigurationModel
        {
            Enabled = true,
            MinimumPayoutAmount = 100,
            AllowVendorPayoutRequest = true
        };

        return View("~/Plugins/Marketplace.VendorExtensions/Views/Configure.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        // Vendor access check
        var vendorCheck = await CheckVendorAccessAsync();
        if (vendorCheck != null) return vendorCheck;

        if (!ModelState.IsValid)
            return await Configure();

        // Save settings here if needed

        _notificationService.SuccessNotification(
            await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

        return await Configure();
    }

    #endregion

    #region Transaction History

    public async Task<IActionResult> TransactionHistory()
    {
        // Vendor access check
        var vendorCheck = await CheckVendorAccessAsync();
        if (vendorCheck != null) return vendorCheck;

        _nopHtmlHelper.SetActiveMenuItemSystemName("Marketplace.VendorExtensions.Transactions");

        var vendors = await _vendorService.GetAllVendorsAsync(showHidden: false);
        var model = new VendorTransactionHistoryModel
        {
            AvailableVendors = vendors.Select(v => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Text = v.Name,
                Value = v.Id.ToString()
            }).ToList()
        };

        model.AvailableVendors.Insert(0, new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
        {
            Text = await _localizationService.GetResourceAsync("Admin.Common.All"),
            Value = "0"
        });

        return View("~/Plugins/Marketplace.VendorExtensions/Views/TransactionHistory.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> GetTransactionList(int vendorId, DateTime? startDate, DateTime? endDate, int? transactionType)
    {
        // Vendor access check
        var vendorCheck = await CheckVendorAccessAsync();
        if (vendorCheck != null) return vendorCheck;

        try
        {
            // Get account summary if vendor is selected
            object accountSummary = null;
            if (vendorId > 0)
            {
                var account = await _vendorAccountService.GetOrCreateCurrentAccountAsync(vendorId);
                accountSummary = new
                {
                    balance = account.Balance,
                    totalCredit = account.TotalCredit,
                    totalDebit = account.TotalDebit,
                    lastUpdated = account.LastUpdatedUtc
                };
            }

            // Get transactions
            VendorTransactionType? typeFilter = transactionType.HasValue 
                ? (VendorTransactionType)transactionType.Value 
                : null;

            // FIXED: Service expects int, not int?
            var transactions = vendorId > 0
                ? await _vendorAccountService.GetTransactionsAsync(vendorId, null, typeFilter)
                : new Nop.Core.PagedList<VendorTransaction>(new List<VendorTransaction>(), 0, 100);

            // Apply date filters manually
            var filteredTransactions = transactions.AsQueryable();
            
            if (startDate.HasValue)
                filteredTransactions = filteredTransactions.Where(t => t.CreatedOnUtc >= startDate.Value);
            
            if (endDate.HasValue)
                filteredTransactions = filteredTransactions.Where(t => t.CreatedOnUtc <= endDate.Value);

            var transactionModels = filteredTransactions.Select(t => new TransactionItemModel
            {
                Id = t.Id,
                VendorId = t.VendorId,
                OrderId = t.OrderId,
                Type = (int)t.Type,
                TypeName = GetTransactionTypeName(t.Type),
                Amount = t.Amount,
                BalanceAfter = t.BalanceAfter,
                Description = t.Description,
                ReferenceNumber = t.ReferenceNumber,
                CreatedOnUtc = t.CreatedOnUtc
            }).ToList();

            return Json(new
            {
                success = true,
                accountSummary = accountSummary,
                transactions = transactionModels
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }

    #endregion

    #region Helpers

    private string GetTransactionTypeName(VendorTransactionType type)
    {
        return type switch
        {
            VendorTransactionType.OrderRevenue => "Order Revenue",
            VendorTransactionType.Commission => "Commission",
            VendorTransactionType.MarketplaceFee => "Marketplace Fee",
            VendorTransactionType.TaxWithholding => "Tax Withholding",
            VendorTransactionType.ShippingCost => "Shipping Cost",
            VendorTransactionType.ReturnShippingCost => "Return Shipping Cost",
            VendorTransactionType.LatePenalty => "Late Penalty",
            VendorTransactionType.CancellationPenalty => "Cancellation Penalty",
            VendorTransactionType.ManualAdjustment => "Manual Adjustment",
            VendorTransactionType.Refund => "Refund",
            VendorTransactionType.PaymentReceived => "Payment Received",
            VendorTransactionType.VendorDiscount => "Vendor Discount",
            VendorTransactionType.PlatformDiscount => "Platform Discount",
            _ => "Unknown"
        };
    }


    #region Vendor Panel Actions

    /// <summary>
    /// Vendor Account Summary
    /// </summary>
    [AuthorizeAdmin]
    public async Task<IActionResult> AccountSummary()
    {
        var vendor = await _workContext.GetCurrentVendorAsync();
        if (vendor == null)
            return AccessDeniedView();

        _nopHtmlHelper.SetActiveMenuItemSystemName("Vendor.Finance.AccountSummary");

        var account = await _vendorAccountService.GetOrCreateCurrentAccountAsync(vendor.Id);
        
        ViewBag.VendorName = vendor.Name;
        ViewBag.Balance = account.Balance;
        ViewBag.TotalCredit = account.TotalCredit;
        ViewBag.TotalDebit = account.TotalDebit;
        ViewBag.LastUpdated = account.LastUpdatedUtc.ToLocalTime();

        return View("~/Plugins/Marketplace.VendorExtensions/Views/Vendor/AccountSummary.cshtml");
    }

    /// <summary>
    /// Vendor Transaction History
    /// </summary>
    [AuthorizeAdmin]
    public async Task<IActionResult> VendorTransactionHistory()
    {
        var vendor = await _workContext.GetCurrentVendorAsync();
        if (vendor == null)
            return AccessDeniedView();

        _nopHtmlHelper.SetActiveMenuItemSystemName("Vendor.Finance.TransactionHistory");

        var model = new VendorTransactionHistoryModel();

        return View("~/Plugins/Marketplace.VendorExtensions/Views/Vendor/TransactionHistory.cshtml", model);
    }

    /// <summary>
    /// Vendor Transaction History - AJAX
    /// </summary>
    [HttpPost]
    [AuthorizeAdmin]
    public async Task<IActionResult> GetVendorTransactionList(DateTime? startDate, DateTime? endDate, int? type)
    {
        var vendor = await _workContext.GetCurrentVendorAsync();
        if (vendor == null)
            return Json(new { success = false, message = "Unauthorized" });

        try
        {
            var account = await _vendorAccountService.GetOrCreateCurrentAccountAsync(vendor.Id);
            
            // GetTransactionsAsync signature: (vendorId, orderId, type, pageIndex, pageSize)
            // We need to pass null for orderId
            var allTransactions = new List<VendorTransaction>();
            
            // Simple filtering in memory for now
            var query = await _vendorAccountService.GetTransactionsAsync(vendor.Id, null, null, 0, 1000);
            
            foreach (var t in query)
            {
                if (startDate.HasValue && t.CreatedOnUtc < startDate.Value)
                    continue;
                if (endDate.HasValue && t.CreatedOnUtc > endDate.Value)
                    continue;
                if (type.HasValue && (int)t.Type != type.Value)
                    continue;
                    
                allTransactions.Add(t);
            }

            var result = allTransactions.Select(t => new
            {
                t.Id,
                Type = (int)t.Type,
                TypeName = GetTransactionTypeName(t.Type),
                t.Amount,
                t.BalanceAfter,
                t.Description,
                t.ReferenceNumber,
                t.OrderId,
                CreatedOnUtc = t.CreatedOnUtc,
                CreatedOnStr = t.CreatedOnUtc.ToLocalTime().ToString("dd.MM.yyyy HH:mm")
            }).ToList();

            return Json(new
            {
                success = true,
                account = new
                {
                    account.Balance,
                    account.TotalCredit,
                    account.TotalDebit,
                    LastUpdated = account.LastUpdatedUtc
                },
                transactions = result
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Vendor Commission Reports
    /// </summary>
    [AuthorizeAdmin]
    public async Task<IActionResult> VendorCommissionReports()
    {
        var vendor = await _workContext.GetCurrentVendorAsync();
        if (vendor == null)
            return AccessDeniedView();

        _nopHtmlHelper.SetActiveMenuItemSystemName("Vendor.Finance.CommissionReports");

        return View("~/Plugins/Marketplace.VendorExtensions/Views/Vendor/CommissionReports.cshtml");
    }

    #endregion

    #region Commission Rates (Read-Only for Vendors)

    /// <summary>
    /// Vendor Commission Rates - Read Only View
    /// </summary>
    [AuthorizeAdmin]
    public async Task<IActionResult> CommissionRates()
    {
        var vendor = await _workContext.GetCurrentVendorAsync();
        if (vendor == null)
            return AccessDeniedView();

        _nopHtmlHelper.SetActiveMenuItemSystemName("Vendor.CommissionRates");

        return View("~/Plugins/Marketplace.VendorExtensions/Views/Vendor/CommissionRates.cshtml");
    }

    /// <summary>
    /// Get Category Commission List - Read Only for Vendors
    /// </summary>
    [HttpPost]
    [AuthorizeAdmin]
    public async Task<IActionResult> GetCommissionRatesList()
    {
        var vendor = await _workContext.GetCurrentVendorAsync();
        if (vendor == null)
            return Json(new { success = false, message = "Unauthorized" });

        try
        {
            // Get default commission rate from settings (without referencing Commission plugin)
            var defaultRate = await _settingService.GetSettingByKeyAsync<decimal>("commissionsettings.defaultcommissionrate", 15.0m);

            var categories = await Nop.Core.Infrastructure.EngineContext.Current.Resolve<Nop.Services.Catalog.ICategoryService>()
                .GetAllCategoriesAsync(showHidden: false);

            // Get commission data from database
            var dataProvider = Nop.Core.Infrastructure.EngineContext.Current.Resolve<Nop.Data.INopDataProvider>();
            var sql = "SELECT CategoryId, Rate FROM MarketplaceCategoryCommission WHERE IsActive = 1";
            var commissionData = await dataProvider.QueryAsync<dynamic>(sql);

            var commissionDict = new Dictionary<int, decimal>();
            foreach (var item in commissionData)
            {
                commissionDict[(int)item.CategoryId] = (decimal)item.Rate;
            }

            var result = categories.Select(c => new
            {
                categoryId = c.Id,
                categoryName = c.Name,
                rate = Math.Round(commissionDict.ContainsKey(c.Id) ? commissionDict[c.Id] : defaultRate, 2)
            }).ToList();

            return Json(new { success = true, data = result, defaultRate = defaultRate });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }

    #endregion

    #endregion
}
