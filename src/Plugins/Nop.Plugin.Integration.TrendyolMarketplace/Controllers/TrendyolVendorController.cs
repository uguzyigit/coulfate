using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core;
using Nop.Plugin.Integration.TrendyolMarketplace.Domain;
using Nop.Plugin.Integration.TrendyolMarketplace.Infrastructure;
using Nop.Plugin.Integration.TrendyolMarketplace.Models;
using Nop.Plugin.Integration.TrendyolMarketplace.Services;
using Nop.Plugin.Integration.TrendyolMarketplace.Services.Api;
using Nop.Plugin.Integration.TrendyolMarketplace.Services.Import;
using Nop.Plugin.Integration.TrendyolMarketplace.Services.Sync;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Vendors;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Integration.TrendyolMarketplace.Controllers;

[Area(AreaNames.ADMIN)]
[AuthorizeAdmin]
[AutoValidateAntiforgeryToken]
public class TrendyolVendorController : BasePluginController
{
    private readonly IWorkContext _workContext;
    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly IPermissionService _permissionService;
    private readonly IVendorCredentialService _vendorCredentialService;
    private readonly ITrendyolApiClient _apiClient;
    private readonly IProductImportService _productImportService;
    private readonly IStockSyncService _stockSyncService;
    private readonly IPriceSyncService _priceSyncService;
    private readonly ISyncLogService _syncLogService;
    private readonly IVendorService _vendorService;
    private readonly IProductService _productService;

    public TrendyolVendorController(
        IWorkContext workContext,
        ILocalizationService localizationService,
        INotificationService notificationService,
        IPermissionService permissionService,
        IVendorCredentialService vendorCredentialService,
        ITrendyolApiClient apiClient,
        IProductImportService productImportService,
        IStockSyncService stockSyncService,
        IPriceSyncService priceSyncService,
        ISyncLogService syncLogService,
        IVendorService vendorService,
        IProductService productService)
    {
        _workContext = workContext;
        _localizationService = localizationService;
        _notificationService = notificationService;
        _permissionService = permissionService;
        _vendorCredentialService = vendorCredentialService;
        _apiClient = apiClient;
        _productImportService = productImportService;
        _stockSyncService = stockSyncService;
        _priceSyncService = priceSyncService;
        _syncLogService = syncLogService;
        _vendorService = vendorService;
        _productService = productService;
    }

    #region Credentials

    public async Task<IActionResult> Credentials()
    {
        var vendorId = await GetCurrentVendorIdAsync();
        if (vendorId == 0)
            return AccessDeniedView();

        var vendor = await _vendorService.GetVendorByIdAsync(vendorId);
        var credential = await _vendorCredentialService.GetByVendorIdAsync(vendorId);

        var model = new VendorCredentialModel
        {
            VendorId = vendorId,
            VendorName = vendor?.Name,
            HasCredentials = credential != null
        };

        if (credential != null)
        {
            model.Id = credential.Id;
            model.TrendyolSupplierId = credential.TrendyolSupplierId;
            model.ApiKey = credential.ApiKey;
            model.IntegrationReferenceCode = credential.IntegrationReferenceCode;
            model.IsActive = credential.IsActive;
            model.AutoSyncEnabled = credential.AutoSyncEnabled;
            model.SyncIntervalMinutes = credential.SyncIntervalMinutes;
            model.LastSyncOnUtc = credential.LastSyncOnUtc;
            model.LastSyncOnDisplay = credential.LastSyncOnUtc?.ToString("g") ?? "-";
        }
        else
        {
            model.SyncIntervalMinutes = 60;
            model.IsActive = true;
            model.AutoSyncEnabled = true;
        }

        return View($"{TrendyolDefaults.VendorViewPath}/Credentials.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> SaveCredentials(VendorCredentialModel model)
    {
        var vendorId = await GetCurrentVendorIdAsync();
        if (vendorId == 0)
            return AccessDeniedView();

        if (!ModelState.IsValid)
            return await Credentials();

        var credential = await _vendorCredentialService.GetByVendorIdAsync(vendorId);

        if (credential == null)
        {
            // Create new credential
            credential = new TrendyolVendorCredential
            {
                VendorId = vendorId,
                TrendyolSupplierId = model.TrendyolSupplierId,
                ApiKey = model.ApiKey,
                ApiSecret = model.ApiSecret ?? string.Empty, // InsertAsync will encrypt
                IntegrationReferenceCode = model.IntegrationReferenceCode,
                IsActive = model.IsActive,
                AutoSyncEnabled = model.AutoSyncEnabled,
                SyncIntervalMinutes = model.SyncIntervalMinutes
            };

            await _vendorCredentialService.InsertAsync(credential);
        }
        else
        {
            // Update existing credential
            credential.TrendyolSupplierId = model.TrendyolSupplierId;
            credential.ApiKey = model.ApiKey;
            credential.IntegrationReferenceCode = model.IntegrationReferenceCode;
            credential.IsActive = model.IsActive;
            credential.AutoSyncEnabled = model.AutoSyncEnabled;
            credential.SyncIntervalMinutes = model.SyncIntervalMinutes;

            // Only update API secret if provided
            if (!string.IsNullOrEmpty(model.ApiSecret))
            {
                // Note: Encryption disabled for debugging
                credential.ApiSecret = model.ApiSecret;
            }

            await _vendorCredentialService.UpdateAsync(credential);
        }

        _notificationService.SuccessNotification(
            await _localizationService.GetResourceAsync("Plugins.Integration.TrendyolMarketplace.Message.Saved"));

        return RedirectToAction("Credentials");
    }

    [HttpPost]
    public async Task<IActionResult> TestConnection()
    {
        var vendorId = await GetCurrentVendorIdAsync();
        if (vendorId == 0)
            return Json(new { success = false, message = "Vendor not found" });

        var credential = await _vendorCredentialService.GetByVendorIdAsync(vendorId);
        if (credential == null)
            return Json(new { success = false, message = await _localizationService.GetResourceAsync("Plugins.Integration.TrendyolMarketplace.Message.NoCredentials") });

        // Debug: show what we're using
        var decryptedSecret = _vendorCredentialService.DecryptApiSecret(credential.ApiSecret);
        var debugInfo = $"SupplierId: {credential.TrendyolSupplierId}, ApiKey: {credential.ApiKey?.Substring(0, Math.Min(5, credential.ApiKey?.Length ?? 0))}..., " +
                        $"Secret length: {decryptedSecret?.Length ?? 0}, IntegRef: {credential.IntegrationReferenceCode?.Substring(0, Math.Min(8, credential.IntegrationReferenceCode?.Length ?? 0))}...";

        var (success, message) = await _apiClient.TestConnectionAsync(credential);

        return Json(new { success, message = success ? message : $"{message} | Debug: {debugInfo}" });
    }

    #endregion

    #region Dashboard

    public async Task<IActionResult> Dashboard()
    {
        var vendorId = await GetCurrentVendorIdAsync();
        if (vendorId == 0)
            return AccessDeniedView();

        var vendor = await _vendorService.GetVendorByIdAsync(vendorId);
        var credential = await _vendorCredentialService.GetByVendorIdAsync(vendorId);

        var model = new VendorDashboardModel
        {
            VendorId = vendorId,
            VendorName = vendor?.Name,
            HasCredentials = credential != null,
            IsActive = credential?.IsActive ?? false,
            LastSyncOnUtc = credential?.LastSyncOnUtc,
            LastSyncOnDisplay = credential?.LastSyncOnUtc?.ToString("g") ?? "-"
        };

        if (credential != null)
        {
            // Test connection status
            var (success, _) = await _apiClient.TestConnectionAsync(credential);
            model.ConnectionStatus = success ? "Connected" : "Disconnected";

            // Get product statistics
            var stats = await _productImportService.GetStatisticsAsync(vendorId);
            model.TotalProducts = stats.Total;
            model.ImportedProducts = stats.Imported;
            model.PendingProducts = stats.Pending;
            model.FailedProducts = stats.Failed;
            model.SkippedProducts = stats.Skipped;
            model.DeactivatedProducts = stats.Deactivated;

            // Check if syncs can be started
            model.CanStartProductSync = !await _syncLogService.IsSyncRunningAsync(vendorId, SyncType.Product);
            model.CanStartStockPriceSync = !await _syncLogService.IsSyncRunningAsync(vendorId, SyncType.Stock) &&
                                           !await _syncLogService.IsSyncRunningAsync(vendorId, SyncType.Price);

            // Get recent sync logs
            var recentLogs = await _syncLogService.GetAllAsync(vendorId: vendorId, pageSize: 5);
            foreach (var log in recentLogs)
            {
                model.RecentSyncLogs.Add(await MapToSyncLogModelAsync(log));
            }
        }
        else
        {
            model.ConnectionStatus = "Not Configured";
        }

        return View($"{TrendyolDefaults.VendorViewPath}/Dashboard.cshtml", model);
    }

    #endregion

    #region Products

    public async Task<IActionResult> Products()
    {
        var vendorId = await GetCurrentVendorIdAsync();
        if (vendorId == 0)
            return AccessDeniedView();

        var model = new TrendyolProductSearchModel
        {
            VendorId = vendorId
        };
        model.SetGridPageSize();

        return View($"{TrendyolDefaults.VendorViewPath}/Products.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> ProductList(TrendyolProductSearchModel searchModel)
    {
        var vendorId = await GetCurrentVendorIdAsync();
        if (vendorId == 0)
            return AccessDeniedView();

        var products = await _productImportService.GetAllAsync(
            vendorId: vendorId,
            status: searchModel.ImportStatus,
            searchTerm: searchModel.SearchTerm,
            pageIndex: searchModel.Page - 1,
            pageSize: searchModel.PageSize);

        var model = new TrendyolProductListModel().PrepareToGrid(searchModel, products, () =>
        {
            return products.Select(p =>
            {
                string nopProductName = null;
                if (p.NopProductId.HasValue)
                {
                    var nopProduct = _productService.GetProductByIdAsync(p.NopProductId.Value).Result;
                    nopProductName = nopProduct?.Name;
                }

                return new TrendyolProductModel
                {
                    Id = p.Id,
                    TrendyolBarcode = p.TrendyolBarcode,
                    TrendyolProductCode = p.TrendyolProductCode,
                    TrendyolStockCode = p.TrendyolStockCode,
                    TrendyolTitle = p.TrendyolTitle,
                    TrendyolCategoryId = p.TrendyolCategoryId,
                    TrendyolBrandId = p.TrendyolBrandId,
                    NopProductId = p.NopProductId,
                    NopProductName = nopProductName ?? "-",
                    VendorId = p.VendorId,
                    IsVariant = p.IsVariant,
                    LastTrendyolPrice = p.LastTrendyolPrice,
                    LastTrendyolStock = p.LastTrendyolStock,
                    LastSyncOnUtc = p.LastSyncOnUtc,
                    LastSyncOnDisplay = p.LastSyncOnUtc?.ToString("g") ?? "-",
                    ImportStatus = p.ImportStatus,
                    ImportStatusDisplay = p.ImportStatus.ToString(),
                    ImportMessage = p.ImportMessage,
                    TrendyolOnSale = p.TrendyolOnSale
                };
            });
        });

        return Json(model);
    }

    #endregion

    #region Sync Logs

    public async Task<IActionResult> SyncLogs()
    {
        var vendorId = await GetCurrentVendorIdAsync();
        if (vendorId == 0)
            return AccessDeniedView();

        var model = new SyncLogSearchModel
        {
            VendorId = vendorId
        };
        model.SetGridPageSize();

        return View($"{TrendyolDefaults.VendorViewPath}/SyncLogs.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> SyncLogList(SyncLogSearchModel searchModel)
    {
        var vendorId = await GetCurrentVendorIdAsync();
        if (vendorId == 0)
            return AccessDeniedView();

        var logs = await _syncLogService.GetAllAsync(
            vendorId: vendorId,
            syncType: searchModel.SyncType,
            status: searchModel.Status,
            fromDate: searchModel.FromDate,
            toDate: searchModel.ToDate,
            pageIndex: searchModel.Page - 1,
            pageSize: searchModel.PageSize);

        var model = new SyncLogListModel().PrepareToGrid(searchModel, logs, () =>
        {
            var result = new List<SyncLogModel>();
            foreach (var log in logs)
            {
                result.Add(MapToSyncLogModelAsync(log).Result);
            }
            return result;
        });

        return Json(model);
    }

    #endregion

    #region Manual Sync

    public async Task<IActionResult> ManualSync()
    {
        var vendorId = await GetCurrentVendorIdAsync();
        if (vendorId == 0)
            return AccessDeniedView();

        var credential = await _vendorCredentialService.GetByVendorIdAsync(vendorId);
        if (credential == null)
        {
            _notificationService.WarningNotification(
                await _localizationService.GetResourceAsync("Plugins.Integration.TrendyolMarketplace.Message.NoCredentials"));
            return RedirectToAction("Credentials");
        }

        var model = new ManualSyncModel
        {
            VendorId = vendorId,
            IsProductSyncRunning = await _syncLogService.IsSyncRunningAsync(vendorId, SyncType.Product),
            IsStockSyncRunning = await _syncLogService.IsSyncRunningAsync(vendorId, SyncType.Stock),
            IsPriceSyncRunning = await _syncLogService.IsSyncRunningAsync(vendorId, SyncType.Price)
        };

        model.CanStartProductSync = !model.IsProductSyncRunning;
        model.CanStartStockPriceSync = !model.IsStockSyncRunning && !model.IsPriceSyncRunning;

        // Get last syncs
        var lastProductSync = await _syncLogService.GetLatestAsync(vendorId, SyncType.Product);
        if (lastProductSync != null)
            model.LastProductSync = await MapToSyncLogModelAsync(lastProductSync);

        var lastStockSync = await _syncLogService.GetLatestAsync(vendorId, SyncType.Stock);
        if (lastStockSync != null)
            model.LastStockSync = await MapToSyncLogModelAsync(lastStockSync);

        var lastPriceSync = await _syncLogService.GetLatestAsync(vendorId, SyncType.Price);
        if (lastPriceSync != null)
            model.LastPriceSync = await MapToSyncLogModelAsync(lastPriceSync);

        return View($"{TrendyolDefaults.VendorViewPath}/ManualSync.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> StartProductSync()
    {
        var vendorId = await GetCurrentVendorIdAsync();
        if (vendorId == 0)
            return Json(new { success = false, message = "Vendor not found" });

        var credential = await _vendorCredentialService.GetByVendorIdAsync(vendorId);
        if (credential == null)
            return Json(new { success = false, message = await _localizationService.GetResourceAsync("Plugins.Integration.TrendyolMarketplace.Message.NoCredentials") });

        if (await _syncLogService.IsSyncRunningAsync(vendorId, SyncType.Product))
            return Json(new { success = false, message = await _localizationService.GetResourceAsync("Plugins.Integration.TrendyolMarketplace.Message.AlreadySyncing") });

        // Start sync in background
        var syncLog = await _syncLogService.StartSyncAsync(SyncType.Product, vendorId);

        try
        {
            var (success, failed, skipped) = await _productImportService.ImportProductsAsync(credential, syncLog.Id);

            var status = failed > 0 ? SyncStatus.CompletedWithErrors : SyncStatus.Completed;
            await _syncLogService.CompleteSyncAsync(syncLog.Id, status);
            await _vendorCredentialService.UpdateLastSyncTimeAsync(vendorId);

            var message = $"Product sync completed: {success} success, {failed} failed, {skipped} skipped";

            // If there were failures, include the error details from the product records
            if (failed > 0)
            {
                var failedProducts = await _productImportService.GetAllAsync(vendorId: vendorId, status: ImportStatus.Failed, pageSize: 5);
                var errors = failedProducts
                    .Where(p => !string.IsNullOrEmpty(p.ImportMessage))
                    .Select(p => $"[{p.TrendyolBarcode}] {p.ImportMessage}")
                    .Distinct()
                    .Take(3);
                var errorSummary = string.Join(" | ", errors);
                if (!string.IsNullOrEmpty(errorSummary))
                    message += $" -- Errors: {errorSummary}";
            }

            return Json(new { success = failed == 0, message });
        }
        catch (Exception ex)
        {
            var innerMsg = ex.InnerException != null ? $" -> {ex.InnerException.Message}" : "";
            await _syncLogService.CompleteSyncAsync(syncLog.Id, SyncStatus.Failed, $"{ex.Message}{innerMsg}");
            return Json(new { success = false, message = $"Sync error: {ex.Message}{innerMsg}" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> StartStockPriceSync()
    {
        var vendorId = await GetCurrentVendorIdAsync();
        if (vendorId == 0)
            return Json(new { success = false, message = "Vendor not found" });

        var credential = await _vendorCredentialService.GetByVendorIdAsync(vendorId);
        if (credential == null)
            return Json(new { success = false, message = await _localizationService.GetResourceAsync("Plugins.Integration.TrendyolMarketplace.Message.NoCredentials") });

        if (await _syncLogService.IsSyncRunningAsync(vendorId, SyncType.Stock) ||
            await _syncLogService.IsSyncRunningAsync(vendorId, SyncType.Price))
            return Json(new { success = false, message = await _localizationService.GetResourceAsync("Plugins.Integration.TrendyolMarketplace.Message.AlreadySyncing") });

        // Start stock sync
        var stockSyncLog = await _syncLogService.StartSyncAsync(SyncType.Stock, vendorId);
        try
        {
            var (stockSuccess, stockFailed) = await _stockSyncService.SyncStockAsync(credential, stockSyncLog.Id);
            var stockStatus = stockFailed > 0 ? SyncStatus.CompletedWithErrors : SyncStatus.Completed;
            await _syncLogService.CompleteSyncAsync(stockSyncLog.Id, stockStatus);
        }
        catch (Exception ex)
        {
            await _syncLogService.CompleteSyncAsync(stockSyncLog.Id, SyncStatus.Failed, ex.Message);
        }

        // Start price sync
        var priceSyncLog = await _syncLogService.StartSyncAsync(SyncType.Price, vendorId);
        try
        {
            var (priceSuccess, priceFailed) = await _priceSyncService.SyncPricesAsync(credential, priceSyncLog.Id);
            var priceStatus = priceFailed > 0 ? SyncStatus.CompletedWithErrors : SyncStatus.Completed;
            await _syncLogService.CompleteSyncAsync(priceSyncLog.Id, priceStatus);
        }
        catch (Exception ex)
        {
            await _syncLogService.CompleteSyncAsync(priceSyncLog.Id, SyncStatus.Failed, ex.Message);
        }

        return Json(new { success = true, message = "Stock/Price sync completed" });
    }

    #endregion

    #region Debug

    [HttpPost]
    public async Task<IActionResult> DebugApiProducts()
    {
        var vendorId = await GetCurrentVendorIdAsync();
        if (vendorId == 0)
            return Json(new { success = false, message = "Vendor not found" });

        var credential = await _vendorCredentialService.GetByVendorIdAsync(vendorId);
        if (credential == null)
            return Json(new { success = false, message = "No credentials" });

        try
        {
            // Test 1: Fetch with approved=true&onSale=true (what import now uses)
            var onSaleProducts = await _apiClient.GetAllProductsAsync(credential, approved: true, onSale: true);

            // Test 2: Fetch with approved=true (all approved)
            var approvedProducts = await _apiClient.GetAllProductsAsync(credential, approved: true);

            // Test 3: Try specific barcode
            var specificProduct = await _apiClient.GetProductByBarcodeAsync(credential, "86951121742");

            // Load settings to show current config
            var settingService = HttpContext.RequestServices.GetRequiredService<Nop.Services.Configuration.ISettingService>();
            var settings = await settingService.LoadSettingAsync<TrendyolSettings>();

            var result = new
            {
                success = true,
                settings = new { settings.SkipUnmappedCategories, settings.PublishImportedProducts, settings.AutoCreateManufacturers },
                onSaleCount = onSaleProducts.Count,
                onSaleProducts = onSaleProducts.Take(10).Select(p => new { p.Barcode, p.Title, p.BrandName, p.BrandId, p.CategoryName, p.CategoryId, p.OnSale, p.Quantity, p.SalePrice }),
                approvedCount = approvedProducts.Count,
                specificProduct = specificProduct != null ? new { specificProduct.Barcode, specificProduct.Title, specificProduct.BrandName, specificProduct.BrandId, specificProduct.CategoryName, specificProduct.CategoryId, specificProduct.OnSale, specificProduct.Quantity } : null,
                specificFound = specificProduct != null
            };

            return Json(result);
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message, stackTrace = ex.StackTrace });
        }
    }

    #endregion

    #region Helper Methods

    private async Task<int> GetCurrentVendorIdAsync()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (customer == null)
            return 0;

        // Check if customer is associated with a vendor
        var vendor = await _vendorService.GetVendorByIdAsync(customer.VendorId);
        if (vendor == null || vendor.Deleted || !vendor.Active)
            return 0;

        return vendor.Id;
    }

    private async Task<SyncLogModel> MapToSyncLogModelAsync(TrendyolSyncLog log)
    {
        string vendorName = null;
        if (log.VendorId.HasValue)
        {
            var vendor = await _vendorService.GetVendorByIdAsync(log.VendorId.Value);
            vendorName = vendor?.Name;
        }

        var duration = log.CompletedOnUtc.HasValue
            ? (log.CompletedOnUtc.Value - log.StartedOnUtc).ToString(@"hh\:mm\:ss")
            : "-";

        return new SyncLogModel
        {
            Id = log.Id,
            SyncType = log.SyncType,
            SyncTypeDisplay = log.SyncType.ToString(),
            VendorId = log.VendorId,
            VendorName = vendorName ?? "Global",
            StartedOnUtc = log.StartedOnUtc,
            StartedOnDisplay = log.StartedOnUtc.ToString("g"),
            CompletedOnUtc = log.CompletedOnUtc,
            CompletedOnDisplay = log.CompletedOnUtc?.ToString("g") ?? "-",
            Duration = duration,
            TotalItems = log.TotalItems,
            SuccessCount = log.SuccessCount,
            FailedCount = log.FailedCount,
            SkippedCount = log.SkippedCount,
            Status = log.Status,
            StatusDisplay = log.Status.ToString(),
            ErrorMessage = log.ErrorMessage,
            Details = log.Details
        };
    }

    #endregion
}
