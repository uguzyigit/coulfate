using Marketplace.Abstractions.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Plugin.Marketplace.Performance.Models;
using Nop.Services.Catalog;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Vendors;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Marketplace.Performance.Controllers;

[Area(AreaNames.ADMIN)]
[AuthorizeAdmin]
[AutoValidateAntiforgeryToken]
public class MarketplacePerformanceController : BasePluginController
{
    private readonly IProductPerformanceService _performanceService;
    private readonly IProductService _productService;
    private readonly IWorkContext _workContext;
    private readonly IPermissionService _permissionService;
    private readonly IVendorService _vendorService;
    private readonly IPictureService _pictureService;

    public MarketplacePerformanceController(
        IProductPerformanceService performanceService,
        IProductService productService,
        IWorkContext workContext,
        IPermissionService permissionService,
        IVendorService vendorService,
        IPictureService pictureService)
    {
        _performanceService = performanceService;
        _productService = productService;
        _workContext = workContext;
        _permissionService = permissionService;
        _vendorService = vendorService;
        _pictureService = pictureService;
    }

    private async Task<(bool isVendor, int vendorId, bool isAdmin)> GetCurrentUserContextAsync()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();

        // Check admin first
        var isAdmin = await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS);

        // Then check vendor
        var vendors = await _vendorService.GetVendorsByCustomerIdsAsync(new[] { customer.Id });
        var vendor = vendors.FirstOrDefault();

        if (vendor != null && !vendor.Deleted && vendor.Active)
            return (true, vendor.Id, isAdmin);

        return (false, 0, isAdmin);
    }

    private async Task<PerformanceListModel> PrepareListModelAsync(int vendorIdFilter, bool isAdmin)
    {
        var model = new PerformanceListModel
        {
            VendorId = vendorIdFilter,
            IsAdmin = isAdmin
        };

        if (isAdmin)
        {
            var vendors = await _vendorService.GetAllVendorsAsync();
            model.AvailableVendors.Add(new SelectListItem("Tumu", "0"));
            foreach (var v in vendors)
                model.AvailableVendors.Add(new SelectListItem(v.Name, v.Id.ToString()));
        }

        return model;
    }

    #region Diagnostics & Recalculate

    // GET: direct browser test at /Admin/MarketplacePerformance/TestData
    [HttpGet]
    public async Task<IActionResult> TestData(int vendorId = 0)
    {
        try
        {
            var (isVendor, currentVendorId, isAdmin) = await GetCurrentUserContextAsync();
            var filterVendorId = isVendor && !isAdmin ? currentVendorId : vendorId;

            var snapshots = await _performanceService.GetAllAsync(filterVendorId, 0, 25);

            var items = new List<object>();
            foreach (var s in snapshots)
            {
                var product = await _productService.GetProductByIdAsync(s.ProductId);
                items.Add(new
                {
                    snapshotId = s.Id,
                    productId = s.ProductId,
                    vendorId = s.VendorId,
                    productFound = product != null,
                    productName = product?.Name ?? "NULL",
                    views = s.Views30d,
                    finalScore = s.FinalScore
                });
            }

            return Json(new
            {
                isVendor,
                currentVendorId,
                isAdmin,
                filterVendorId,
                totalSnapshots = snapshots.TotalCount,
                itemCount = items.Count,
                items
            });
        }
        catch (Exception ex)
        {
            return Json(new { error = ex.Message, stack = ex.StackTrace });
        }
    }

    // GET: direct browser test at /Admin/MarketplacePerformance/Diagnose
    [HttpGet]
    public async Task<IActionResult> Diagnose()
    {
        try
        {
            var (isVendor, currentVendorId, isAdmin) = await GetCurrentUserContextAsync();
            var snapshots = await _performanceService.GetAllAsync(0, 0, 25);
            var interactionCount = await _performanceService.GetDiagnosticCountsAsync();

            // Diagnose product eligibility
            var allVendorProducts = (await _productService.SearchProductsAsync(
                pageIndex: 0, pageSize: int.MaxValue, vendorId: 0, visibleIndividuallyOnly: false))
                .Where(p => !p.Deleted && p.Published)
                .Select(p => new
                {
                    p.Id, p.Name, p.VendorId,
                    p.ManageInventoryMethodId, p.StockQuantity,
                    eligible = p.ManageInventoryMethodId == 0 || p.StockQuantity > 0
                }).ToList();

            var sampleItems = new List<object>();
            foreach (var s in snapshots.Take(3))
            {
                var product = await _productService.GetProductByIdAsync(s.ProductId);
                sampleItems.Add(new
                {
                    snapshotId = s.Id,
                    productId = s.ProductId,
                    vendorId = s.VendorId,
                    productFound = product != null,
                    productName = product?.Name ?? "NULL",
                    views30d = s.Views30d,
                    finalScore = s.FinalScore
                });
            }

            return Json(new
            {
                success = true,
                userContext = new { isVendor, currentVendorId, isAdmin },
                snapshotCount = snapshots.TotalCount,
                interactionCount = interactionCount,
                totalVendorProducts = allVendorProducts.Count,
                eligibleProducts = allVendorProducts.Count(x => x.eligible),
                excludedProducts = allVendorProducts.Where(x => !x.eligible).Select(x => new
                {
                    x.Id, x.Name, x.VendorId, x.ManageInventoryMethodId, x.StockQuantity
                }),
                sampleItems
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message, stackTrace = ex.StackTrace });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Recalculate()
    {
        var (_, _, isAdmin) = await GetCurrentUserContextAsync();
        if (!isAdmin)
            return Json(new { success = false, message = "Sadece admin bu islemi yapabilir." });

        try
        {
            await _performanceService.RecalculateAllAsync();
            var snapshots = await _performanceService.GetAllAsync();
            return Json(new { success = true, message = $"Hesaplama tamamlandi. {snapshots.TotalCount} urun guncellendi." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"Hata: {ex.Message}" });
        }
    }

    #endregion

    #region Product Performance

    public async Task<IActionResult> ProductPerformance()
    {
        var (isVendor, vendorId, isAdmin) = await GetCurrentUserContextAsync();
        if (!isVendor && !isAdmin)
            return AccessDeniedView();

        var model = await PrepareListModelAsync(isVendor && !isAdmin ? vendorId : 0, isAdmin);
        return View("~/Plugins/Marketplace.Performance/Views/Performance/ProductPerformance.cshtml", model);
    }

    [AcceptVerbs("GET", "POST")]
    public async Task<IActionResult> ProductPerformanceData(int vendorId = 0, int pageIndex = 0, int pageSize = 25, int draw = 1)
    {
        try
        {
            var (isVendor, currentVendorId, isAdmin) = await GetCurrentUserContextAsync();
            if (!isVendor && !isAdmin)
                return Json(new { draw = 1, recordsTotal = 0, recordsFiltered = 0, data = Array.Empty<object>() });

            var filterVendorId = isVendor && !isAdmin ? currentVendorId : vendorId;

            var snapshots = await _performanceService.GetAllAsync(filterVendorId, pageIndex, pageSize);

            var items = new List<ProductPerformanceItemModel>();
            foreach (var s in snapshots)
            {
                var product = await _productService.GetProductByIdAsync(s.ProductId);
                if (product == null) continue;

                var pictures = await _productService.GetProductPicturesByProductIdAsync(product.Id);
                var thumbUrl = string.Empty;
                if (pictures.Any())
                    thumbUrl = await _pictureService.GetPictureUrlAsync(pictures[0].PictureId, 75) ?? string.Empty;

                var convRate = s.Views30d > 0
                    ? ((double)s.GrossOrders30d / s.Views30d * 100).ToString("F2") + "%"
                    : "0.00%";

                items.Add(new ProductPerformanceItemModel
                {
                    ProductId = s.ProductId,
                    ProductName = product.Name,
                    Sku = product.Sku,
                    ThumbnailUrl = thumbUrl,
                    Views = s.Views30d,
                    Wishlist = s.Wishlist30d,
                    AddToCart = s.AddToCart30d,
                    GrossOrders = s.GrossOrders30d,
                    ConversionRate = convRate,
                    GrossQty = s.GrossQty30d,
                    GrossRevenue = s.GrossRevenue30d,
                    FinalScore = s.FinalScore
                });
            }

            return Json(new
            {
                draw,
                recordsTotal = snapshots.TotalCount,
                recordsFiltered = snapshots.TotalCount,
                data = items
            });
        }
        catch (Exception ex)
        {
            return Json(new { draw = 1, recordsTotal = 0, recordsFiltered = 0, data = Array.Empty<object>(), error = ex.Message });
        }
    }

    #endregion

    #region Sales Performance

    public async Task<IActionResult> SalesPerformance()
    {
        var (isVendor, vendorId, isAdmin) = await GetCurrentUserContextAsync();
        if (!isVendor && !isAdmin)
            return AccessDeniedView();

        var model = await PrepareListModelAsync(isVendor && !isAdmin ? vendorId : 0, isAdmin);
        return View("~/Plugins/Marketplace.Performance/Views/Performance/SalesPerformance.cshtml", model);
    }

    [AcceptVerbs("GET", "POST")]
    public async Task<IActionResult> SalesPerformanceData(int vendorId = 0, int pageIndex = 0, int pageSize = 25, int draw = 1)
    {
        try
        {
            var (isVendor, currentVendorId, isAdmin) = await GetCurrentUserContextAsync();
            if (!isVendor && !isAdmin)
                return Json(new { draw = 1, recordsTotal = 0, recordsFiltered = 0, data = Array.Empty<object>() });

            var filterVendorId = isVendor && !isAdmin ? currentVendorId : vendorId;

            var snapshots = await _performanceService.GetAllAsync(filterVendorId, pageIndex, pageSize);

            var items = new List<SalesPerformanceItemModel>();
            foreach (var s in snapshots)
            {
                var product = await _productService.GetProductByIdAsync(s.ProductId);
                if (product == null) continue;

                var pictures = await _productService.GetProductPicturesByProductIdAsync(product.Id);
                var thumbUrl = string.Empty;
                if (pictures.Any())
                    thumbUrl = await _pictureService.GetPictureUrlAsync(pictures[0].PictureId, 75) ?? string.Empty;

                var cancelRate = s.GrossQty30d > 0
                    ? ((double)s.CancelQty30d / s.GrossQty30d * 100).ToString("F2") + "%"
                    : "0.00%";
                var returnRate = s.GrossQty30d > 0
                    ? ((double)s.ReturnQty30d / s.GrossQty30d * 100).ToString("F2") + "%"
                    : "0.00%";
                var avgPrice = s.NetQty30d > 0 ? s.NetRevenue30d / s.NetQty30d : 0m;

                items.Add(new SalesPerformanceItemModel
                {
                    ProductId = s.ProductId,
                    ProductName = product.Name,
                    Sku = product.Sku,
                    ThumbnailUrl = thumbUrl,
                    GrossOrders = s.GrossOrders30d,
                    GrossQty = s.GrossQty30d,
                    CancelQty = s.CancelQty30d,
                    CancelRate = cancelRate,
                    ReturnQty = s.ReturnQty30d,
                    ReturnRate = returnRate,
                    NetQty = s.NetQty30d,
                    GrossRevenue = s.GrossRevenue30d,
                    DiscountAmount = s.DiscountAmount30d,
                    NetRevenue = s.NetRevenue30d,
                    AvgSalePrice = avgPrice
                });
            }

            return Json(new
            {
                draw,
                recordsTotal = snapshots.TotalCount,
                recordsFiltered = snapshots.TotalCount,
                data = items
            });
        }
        catch (Exception ex)
        {
            return Json(new { draw = 1, recordsTotal = 0, recordsFiltered = 0, data = Array.Empty<object>(), error = ex.Message });
        }
    }

    #endregion
}
