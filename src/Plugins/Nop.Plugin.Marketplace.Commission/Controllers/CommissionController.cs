using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Marketplace.Commission.Models;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Vendors;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Framework.UI;
using Nop.Data;
using LinqToDB;

namespace Nop.Plugin.Marketplace.Commission.Controllers;

[Area(AreaNames.ADMIN)]
[AuthorizeAdmin]
[AutoValidateAntiforgeryToken]
public class CommissionController : BasePluginController
{
    private readonly ISettingService _settingService;
    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly IPermissionService _permissionService;
    private readonly ICategoryService _categoryService;
    private readonly IProductService _productService;
    private readonly IVendorService _vendorService;
    private readonly IOrderService _orderService;
    private readonly INopDataProvider _dataProvider;
    private readonly INopHtmlHelper _nopHtmlHelper;
    private readonly IWorkContext _workContext;

    public CommissionController(
        ISettingService settingService,
        ILocalizationService localizationService,
        INotificationService notificationService,
        IPermissionService permissionService,
        ICategoryService categoryService,
        IProductService productService,
        IVendorService vendorService,
        IOrderService orderService,
        INopDataProvider dataProvider,
        INopHtmlHelper nopHtmlHelper,
        IWorkContext workContext)
    {
        _settingService = settingService;
        _localizationService = localizationService;
        _notificationService = notificationService;
        _permissionService = permissionService;
        _categoryService = categoryService;
        _productService = productService;
        _vendorService = vendorService;
        _orderService = orderService;
        _dataProvider = dataProvider;
        _nopHtmlHelper = nopHtmlHelper;
        _workContext = workContext;
    }

    #region Vendor Access Control Helpers

    private async Task<bool> IsVendorAsync()
    {
        var vendor = await _workContext.GetCurrentVendorAsync();
        return vendor != null;
    }

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
        var vendorCheck = await CheckVendorAccessAsync();
        if (vendorCheck != null) return vendorCheck;

        _nopHtmlHelper.SetActiveMenuItemSystemName("Marketplace.Commission.Settings");

        var settings = await _settingService.LoadSettingAsync<CommissionSettings>();
        var model = new ConfigurationModel
        {
            DefaultCommissionRate = settings.DefaultCommissionRate,
            MarketplaceFeePerOrder = settings.MarketplaceFeePerOrder,
            TaxWithholdingRate = settings.TaxWithholdingRate,
            AutoProcessOnPayment = settings.AutoProcessOnPayment
        };

        return View("~/Plugins/Marketplace.Commission/Views/Configure.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        var vendorCheck = await CheckVendorAccessAsync();
        if (vendorCheck != null) return vendorCheck;

        if (!ModelState.IsValid)
            return await Configure();

        var settings = await _settingService.LoadSettingAsync<CommissionSettings>();
        settings.DefaultCommissionRate = model.DefaultCommissionRate;
        settings.MarketplaceFeePerOrder = model.MarketplaceFeePerOrder;
        settings.TaxWithholdingRate = model.TaxWithholdingRate;
        settings.AutoProcessOnPayment = model.AutoProcessOnPayment;

        await _settingService.SaveSettingAsync(settings);

        _notificationService.SuccessNotification(
            await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

        return await Configure();
    }

    #endregion

    #region Category Commissions

    public async Task<IActionResult> CategoryCommissions()
    {
        var vendorCheck = await CheckVendorAccessAsync();
        if (vendorCheck != null) return vendorCheck;

        _nopHtmlHelper.SetActiveMenuItemSystemName("Marketplace.Commission.CategoryRates");
        return View("~/Plugins/Marketplace.Commission/Views/CategoryCommissions.cshtml");
    }

    [HttpPost]
    public async Task<IActionResult> CategoryCommissionList()
    {
        var vendorCheck = await CheckVendorAccessAsync();
        if (vendorCheck != null) return vendorCheck;

        try
        {
            // Get commission data without tuple
            var sql = "SELECT CategoryId, Rate FROM `MarketplaceCategoryCommission` WHERE IsActive = 1";
            var commissionData = await _dataProvider.QueryAsync<dynamic>(sql);

            var commissionDict = new Dictionary<int, decimal>();
            foreach (var item in commissionData)
            {
                commissionDict[(int)item.CategoryId] = (decimal)item.Rate;
            }

            var allCategories = await _categoryService.GetAllCategoriesAsync(showHidden: true);
            var settings = await _settingService.LoadSettingAsync<CommissionSettings>();

            var result = allCategories.Select(c => new
            {
                categoryId = c.Id,
                categoryName = c.Name,
                rate = Math.Round(commissionDict.ContainsKey(c.Id) ? commissionDict[c.Id] : settings.DefaultCommissionRate),
                isActive = commissionDict.ContainsKey(c.Id)
            }).ToList();

            return Json(new { data = result });
        }
        catch (Exception ex)
        {
            return Json(new { error = ex.Message });
        }
    }
    [HttpPost]
    public async Task<IActionResult> SetCategoryCommission(int categoryId, decimal rate)
    {
        var vendorCheck = await CheckVendorAccessAsync();
        if (vendorCheck != null) return vendorCheck;

        try
        {
            // Check if exists using raw SQL
            var checkSql = "SELECT COUNT(*) FROM `MarketplaceCategoryCommission` WHERE CategoryId = @p0";
            var existsResult = await _dataProvider.QueryAsync<int>(checkSql, 
                new LinqToDB.Data.DataParameter("@p0", categoryId));
            var exists = existsResult.FirstOrDefault() > 0;

            if (exists)
            {
                var updateSql = @"UPDATE `MarketplaceCategoryCommission` 
                    SET Rate = @p0, UpdatedOnUtc = @p1 
                    WHERE CategoryId = @p2";
                    
                await _dataProvider.ExecuteNonQueryAsync(updateSql,
                    new LinqToDB.Data.DataParameter("@p0", rate),
                    new LinqToDB.Data.DataParameter("@p1", DateTime.UtcNow),
                    new LinqToDB.Data.DataParameter("@p2", categoryId));
            }
            else
            {
                var insertSql = @"INSERT INTO `MarketplaceCategoryCommission` 
                    (CategoryId, Rate, IsActive, CreatedOnUtc) 
                    VALUES (@p0, @p1, @p2, @p3)";
                    
                await _dataProvider.ExecuteNonQueryAsync(insertSql,
                    new LinqToDB.Data.DataParameter("@p0", categoryId),
                    new LinqToDB.Data.DataParameter("@p1", rate),
                    new LinqToDB.Data.DataParameter("@p2", true),
                    new LinqToDB.Data.DataParameter("@p3", DateTime.UtcNow));
            }

            return Json(new { success = true, message = "Commission rate updated successfully" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }

    #endregion

    #region Product Commissions

    public async Task<IActionResult> ProductCommissions()
    {
        var vendorCheck = await CheckVendorAccessAsync();
        if (vendorCheck != null) return vendorCheck;

        _nopHtmlHelper.SetActiveMenuItemSystemName("Marketplace.Commission.ProductRates");
        return View("~/Plugins/Marketplace.Commission/Views/ProductCommissions.cshtml");
    }

    [HttpPost]
    public async Task<IActionResult> ProductCommissionList()
    {
        var vendorCheck = await CheckVendorAccessAsync();
        if (vendorCheck != null) return vendorCheck;

        try
        {
            var sql = @"SELECT pc.Id, pc.ProductId, pc.Rate, pc.Reason, pc.CreatedOnUtc, pc.ExpiresOnUtc,
                    p.Name as ProductName
                FROM `MarketplaceProductCommission` pc
                INNER JOIN `Product` p ON pc.ProductId = p.Id
                WHERE pc.IsActive = 1
                ORDER BY pc.CreatedOnUtc DESC
                LIMIT 100";

            var commissions = await _dataProvider.QueryAsync<ProductCommissionModel>(sql);

            return Json(new { data = commissions });
        }
        catch (Exception ex)
        {
            return Json(new { error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> SetProductCommission(int productId, decimal rate, string reason, DateTime? expiresOnUtc)
    {
        var vendorCheck = await CheckVendorAccessAsync();
        if (vendorCheck != null) return vendorCheck;

        try
        {
            var checkSql = "SELECT COUNT(*) FROM `MarketplaceProductCommission` WHERE ProductId = @p0";
            var existsResult = await _dataProvider.QueryAsync<int>(checkSql,
                new LinqToDB.Data.DataParameter("@p0", productId));
            var exists = existsResult.FirstOrDefault() > 0;

            if (exists)
            {
                var updateSql = @"UPDATE `MarketplaceProductCommission` 
                    SET Rate = @p0, Reason = @p1, ExpiresOnUtc = @p2 
                    WHERE ProductId = @p3";
                    
                await _dataProvider.ExecuteNonQueryAsync(updateSql,
                    new LinqToDB.Data.DataParameter("@p0", rate),
                    new LinqToDB.Data.DataParameter("@p1", reason),
                    new LinqToDB.Data.DataParameter("@p2", expiresOnUtc),
                    new LinqToDB.Data.DataParameter("@p3", productId));
            }
            else
            {
                var insertSql = @"INSERT INTO `MarketplaceProductCommission` 
                    (ProductId, Rate, IsActive, Reason, CreatedOnUtc, ExpiresOnUtc) 
                    VALUES (@p0, @p1, @p2, @p3, @p4, @p5)";
                    
                await _dataProvider.ExecuteNonQueryAsync(insertSql,
                    new LinqToDB.Data.DataParameter("@p0", productId),
                    new LinqToDB.Data.DataParameter("@p1", rate),
                    new LinqToDB.Data.DataParameter("@p2", true),
                    new LinqToDB.Data.DataParameter("@p3", reason),
                    new LinqToDB.Data.DataParameter("@p4", DateTime.UtcNow),
                    new LinqToDB.Data.DataParameter("@p5", expiresOnUtc));
            }

            return Json(new { success = true, message = "Product commission updated successfully" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> DeleteProductCommission(int id)
    {
        var vendorCheck = await CheckVendorAccessAsync();
        if (vendorCheck != null) return vendorCheck;

        try
        {
            var deleteSql = "DELETE FROM `MarketplaceProductCommission` WHERE Id = @p0";
            await _dataProvider.ExecuteNonQueryAsync(deleteSql,
                new LinqToDB.Data.DataParameter("@p0", id));

            return Json(new { success = true, message = "Product commission deleted successfully" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }

    #endregion

    #region Reports

    public async Task<IActionResult> Reports()
    {
        var vendorCheck = await CheckVendorAccessAsync();
        if (vendorCheck != null) return vendorCheck;

        _nopHtmlHelper.SetActiveMenuItemSystemName("Marketplace.Commission.AllReports");
        return View("~/Plugins/Marketplace.Commission/Views/Reports.cshtml");
    }

    [HttpPost]
    public async Task<IActionResult> GetReportData(DateTime? startDate, DateTime? endDate)
    {
        var vendorCheck = await CheckVendorAccessAsync();
        if (vendorCheck != null) return vendorCheck;

        try
        {
            startDate ??= DateTime.UtcNow.AddMonths(-1);
            endDate ??= DateTime.UtcNow;

            var summarySql = @"SELECT 
                    COUNT(DISTINCT OrderId) as TotalOrders,
                    IFNULL(SUM(NetPrice), 0) as TotalRevenue,
                    IFNULL(SUM(CommissionAmount), 0) as TotalCommission,
                    IFNULL(SUM(MarketplaceFee), 0) as TotalFee,
                    IFNULL(SUM(TaxWithholding), 0) as TotalTax,
                    IFNULL(SUM(VendorNetAmount), 0) as TotalVendorNet
                FROM `MarketplaceOrderCommission`
                WHERE CreatedOnUtc >= @p0 AND CreatedOnUtc <= @p1";

            var summary = (await _dataProvider.QueryAsync<ReportSummary>(summarySql,
                new LinqToDB.Data.DataParameter("@p0", startDate),
                new LinqToDB.Data.DataParameter("@p1", endDate)
            )).FirstOrDefault() ?? new ReportSummary();

            var detailsSql = @"SELECT 
                    oc.OrderId, oc.VendorId, v.Name as VendorName, p.Name as ProductName,
                    oc.NetPrice, oc.CommissionRate, oc.CommissionAmount,
                    oc.MarketplaceFee, oc.TaxWithholding, oc.VendorNetAmount, oc.CreatedOnUtc
                FROM `MarketplaceOrderCommission` oc
                INNER JOIN `Vendor` v ON oc.VendorId = v.Id
                INNER JOIN `Product` p ON oc.ProductId = p.Id
                WHERE oc.CreatedOnUtc >= @p0 AND oc.CreatedOnUtc <= @p1
                ORDER BY oc.CreatedOnUtc DESC
                LIMIT 100";

            var details = await _dataProvider.QueryAsync<CommissionDetailModel>(detailsSql,
                new LinqToDB.Data.DataParameter("@p0", startDate),
                new LinqToDB.Data.DataParameter("@p1", endDate));

            return Json(new { summary = summary, details = details });
        }
        catch (Exception ex)
        {
            return Json(new { error = ex.Message });
        }
    }

    #endregion
}
