using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Data;
using Nop.Plugin.Integration.TrendyolMarketplace.Data;
using Nop.Plugin.Integration.TrendyolMarketplace.Domain;
using Nop.Plugin.Integration.TrendyolMarketplace.Infrastructure;
using Nop.Plugin.Integration.TrendyolMarketplace.Models;
using Nop.Plugin.Integration.TrendyolMarketplace.Services;
using Nop.Plugin.Integration.TrendyolMarketplace.Services.Api;
using Nop.Plugin.Integration.TrendyolMarketplace.Services.Import;
using Nop.Plugin.Integration.TrendyolMarketplace.Services.Mapping;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
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
public class TrendyolAdminController : BasePluginController
{
    private readonly ISettingService _settingService;
    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly IPermissionService _permissionService;
    private readonly IVendorCredentialService _vendorCredentialService;
    private readonly ICategoryMappingService _categoryMappingService;
    private readonly IBrandMappingService _brandMappingService;
    private readonly IAttributeMappingService _attributeMappingService;
    private readonly ITrendyolApiClient _apiClient;
    private readonly ISyncLogService _syncLogService;
    private readonly IVendorService _vendorService;
    private readonly ICategoryService _categoryService;
    private readonly IManufacturerService _manufacturerService;
    private readonly IProductImportService _productImportService;
    private readonly INopDataProvider _dataProvider;

    public TrendyolAdminController(
        ISettingService settingService,
        ILocalizationService localizationService,
        INotificationService notificationService,
        IPermissionService permissionService,
        IVendorCredentialService vendorCredentialService,
        ICategoryMappingService categoryMappingService,
        IBrandMappingService brandMappingService,
        IAttributeMappingService attributeMappingService,
        ITrendyolApiClient apiClient,
        ISyncLogService syncLogService,
        IVendorService vendorService,
        ICategoryService categoryService,
        IManufacturerService manufacturerService,
        IProductImportService productImportService,
        INopDataProvider dataProvider)
    {
        _settingService = settingService;
        _localizationService = localizationService;
        _notificationService = notificationService;
        _permissionService = permissionService;
        _vendorCredentialService = vendorCredentialService;
        _categoryMappingService = categoryMappingService;
        _brandMappingService = brandMappingService;
        _attributeMappingService = attributeMappingService;
        _apiClient = apiClient;
        _syncLogService = syncLogService;
        _vendorService = vendorService;
        _categoryService = categoryService;
        _manufacturerService = manufacturerService;
        _productImportService = productImportService;
        _dataProvider = dataProvider;
    }

    #region Configuration

    public async Task<IActionResult> Configure()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        var settings = await _settingService.LoadSettingAsync<TrendyolSettings>();
        var model = new ConfigurationModel
        {
            Enabled = settings.Enabled,
            ApiBaseUrl = settings.ApiBaseUrl,
            DefaultProductSyncIntervalMinutes = settings.DefaultProductSyncIntervalMinutes,
            DefaultStockPriceSyncIntervalMinutes = settings.DefaultStockPriceSyncIntervalMinutes,
            CategoryBrandSyncIntervalHours = settings.CategoryBrandSyncIntervalHours,
            ApiRequestDelayMs = settings.ApiRequestDelayMs,
            MaxRetryCount = settings.MaxRetryCount,
            ApiPageSize = settings.ApiPageSize,
            AutoCreateManufacturers = settings.AutoCreateManufacturers,
            DownloadProductImages = settings.DownloadProductImages,
            PublishImportedProducts = settings.PublishImportedProducts,
            SkipUnmappedCategories = settings.SkipUnmappedCategories,
            ImportSpecificationAttributes = settings.ImportSpecificationAttributes
        };

        return View($"{TrendyolDefaults.AdminViewPath}/Configure.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        if (!ModelState.IsValid)
            return await Configure();

        var settings = await _settingService.LoadSettingAsync<TrendyolSettings>();
        settings.Enabled = model.Enabled;
        settings.ApiBaseUrl = model.ApiBaseUrl;
        settings.DefaultProductSyncIntervalMinutes = model.DefaultProductSyncIntervalMinutes;
        settings.DefaultStockPriceSyncIntervalMinutes = model.DefaultStockPriceSyncIntervalMinutes;
        settings.CategoryBrandSyncIntervalHours = model.CategoryBrandSyncIntervalHours;
        settings.ApiRequestDelayMs = model.ApiRequestDelayMs;
        settings.MaxRetryCount = model.MaxRetryCount;
        settings.ApiPageSize = model.ApiPageSize;
        settings.AutoCreateManufacturers = model.AutoCreateManufacturers;
        settings.DownloadProductImages = model.DownloadProductImages;
        settings.PublishImportedProducts = model.PublishImportedProducts;
        settings.SkipUnmappedCategories = model.SkipUnmappedCategories;
        settings.ImportSpecificationAttributes = model.ImportSpecificationAttributes;

        await _settingService.SaveSettingAsync(settings);

        _notificationService.SuccessNotification(
            await _localizationService.GetResourceAsync("Plugins.Integration.TrendyolMarketplace.Message.Saved"));

        return await Configure();
    }

    #endregion

    #region Dashboard

    public async Task<IActionResult> Dashboard()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        var model = new AdminDashboardModel();

        // Get vendor statistics
        var credentials = await _vendorCredentialService.GetAllAsync();
        model.TotalVendors = credentials.TotalCount;
        model.ActiveVendors = credentials.Count(c => c.IsActive);

        // Get category statistics
        var categories = await _categoryMappingService.GetAllAsync();
        model.TotalCategories = categories.TotalCount;
        model.MappedCategories = categories.Count(c => c.NopCategoryId.HasValue);
        model.UnmappedCategories = model.TotalCategories - model.MappedCategories;

        // Get brand statistics
        var brands = await _brandMappingService.GetAllAsync();
        model.TotalBrands = brands.TotalCount;
        model.MappedBrands = brands.Count(b => b.NopManufacturerId.HasValue);
        model.UnmappedBrands = model.TotalBrands - model.MappedBrands;

        // Get last global syncs
        var lastCategorySync = await _syncLogService.GetLatestAsync(null, SyncType.Category);
        if (lastCategorySync != null)
        {
            model.LastCategorySyncOnUtc = lastCategorySync.StartedOnUtc;
            model.LastCategorySyncOnDisplay = lastCategorySync.StartedOnUtc.ToString("g");
        }

        var lastBrandSync = await _syncLogService.GetLatestAsync(null, SyncType.Brand);
        if (lastBrandSync != null)
        {
            model.LastBrandSyncOnUtc = lastBrandSync.StartedOnUtc;
            model.LastBrandSyncOnDisplay = lastBrandSync.StartedOnUtc.ToString("g");
        }

        // Get vendor summaries with product statistics
        foreach (var credential in credentials)
        {
            var vendor = await _vendorService.GetVendorByIdAsync(credential.VendorId);
            if (vendor == null) continue;

            // Get product statistics for this vendor
            var stats = await _productImportService.GetStatisticsAsync(credential.VendorId);
            model.TotalProducts += stats.Total;
            model.TotalImportedProducts += stats.Imported;

            model.VendorSummaries.Add(new VendorSummaryModel
            {
                Id = credential.Id,
                VendorId = credential.VendorId,
                VendorName = vendor.Name,
                IsActive = credential.IsActive,
                TotalProducts = stats.Total,
                ImportedProducts = stats.Imported,
                LastSyncOnUtc = credential.LastSyncOnUtc,
                LastSyncOnDisplay = credential.LastSyncOnUtc?.ToString("g") ?? "-",
                Status = credential.IsActive ? "Active" : "Inactive"
            });
        }

        // Get recent sync logs
        var recentLogs = await _syncLogService.GetAllAsync(pageSize: 10);
        foreach (var log in recentLogs)
        {
            model.RecentSyncLogs.Add(await MapToSyncLogModelAsync(log));
        }

        return View($"{TrendyolDefaults.AdminViewPath}/Dashboard.cshtml", model);
    }

    #endregion

    #region Category Mapping

    public async Task<IActionResult> CategoryMapping()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        var model = new CategoryMappingSearchModel();
        model.SetGridPageSize();

        return View($"{TrendyolDefaults.AdminViewPath}/CategoryMapping.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> CategoryMappingList(CategoryMappingSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        var categories = await _categoryMappingService.GetAllAsync(
            searchTerm: searchModel.SearchTrendyolCategory,
            isMapped: searchModel.UnmappedOnly ? false : null,
            isLeaf: searchModel.LeafOnly ? true : null,
            pageIndex: searchModel.Page - 1,
            pageSize: searchModel.PageSize);

        var nopCategories = await _categoryService.GetAllCategoriesAsync(showHidden: true);

        var model = new CategoryMappingListModel().PrepareToGrid(searchModel, categories, () =>
        {
            return categories.Select(c =>
            {
                var nopCategory = c.NopCategoryId.HasValue
                    ? nopCategories.FirstOrDefault(nc => nc.Id == c.NopCategoryId.Value)
                    : null;

                return new CategoryMappingModel
                {
                    Id = c.Id,
                    TrendyolCategoryId = c.TrendyolCategoryId,
                    TrendyolCategoryName = c.TrendyolCategoryName,
                    TrendyolCategoryPath = c.TrendyolCategoryPath,
                    NopCategoryId = c.NopCategoryId,
                    NopCategoryName = nopCategory?.Name ?? "-",
                    IsAutoMapped = c.IsAutoMapped,
                    IsLeaf = c.IsLeaf
                };
            });
        });

        return Json(model);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateCategoryMapping(int id, int? nopCategoryId)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        var category = await _categoryMappingService.GetByIdAsync(id);
        if (category == null)
            return Json(new { success = false, message = "Category not found" });

        category.NopCategoryId = nopCategoryId;
        category.IsAutoMapped = false;
        await _categoryMappingService.UpdateAsync(category);

        return Json(new { success = true });
    }

    [HttpPost]
    public async Task<IActionResult> SyncCategories()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        try
        {
            var categories = await _apiClient.GetCategoriesAsync();
            var syncCount = await _categoryMappingService.SyncFromApiAsync(categories);
            var mappedCount = await _categoryMappingService.AutoMapCategoriesAsync();

            _notificationService.SuccessNotification(
                string.Format(await _localizationService.GetResourceAsync("Plugins.Integration.TrendyolMarketplace.Message.CategorySyncCompleted"), syncCount));

            return Json(new { success = true, message = $"Synced {syncCount} categories, auto-mapped {mappedCount}" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    #endregion

    #region Brand Mapping

    public async Task<IActionResult> BrandMapping()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        var model = new BrandMappingSearchModel();
        model.SetGridPageSize();

        return View($"{TrendyolDefaults.AdminViewPath}/BrandMapping.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> BrandMappingList(BrandMappingSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        var brands = await _brandMappingService.GetAllAsync(
            searchTerm: searchModel.SearchTrendyolBrand,
            isMapped: searchModel.UnmappedOnly ? false : null,
            pageIndex: searchModel.Page - 1,
            pageSize: searchModel.PageSize);

        var manufacturers = await _manufacturerService.GetAllManufacturersAsync(showHidden: true);

        var model = new BrandMappingListModel().PrepareToGrid(searchModel, brands, () =>
        {
            return brands.Select(b =>
            {
                var manufacturer = b.NopManufacturerId.HasValue
                    ? manufacturers.FirstOrDefault(m => m.Id == b.NopManufacturerId.Value)
                    : null;

                return new BrandMappingModel
                {
                    Id = b.Id,
                    TrendyolBrandId = b.TrendyolBrandId,
                    TrendyolBrandName = b.TrendyolBrandName,
                    NopManufacturerId = b.NopManufacturerId,
                    NopManufacturerName = manufacturer?.Name ?? "-",
                    IsAutoMapped = b.IsAutoMapped,
                    AutoCreateIfNotExists = b.AutoCreateIfNotExists
                };
            });
        });

        return Json(model);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateBrandMapping(int id, int? nopManufacturerId)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        var brand = await _brandMappingService.GetByIdAsync(id);
        if (brand == null)
            return Json(new { success = false, message = "Brand not found" });

        brand.NopManufacturerId = nopManufacturerId;
        brand.IsAutoMapped = false;
        await _brandMappingService.UpdateAsync(brand);

        return Json(new { success = true });
    }

    [HttpPost]
    public async Task<IActionResult> SyncBrands()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        try
        {
            var brands = await _apiClient.GetAllBrandsAsync();
            var syncCount = await _brandMappingService.SyncFromApiAsync(brands);
            var mappedCount = await _brandMappingService.AutoMapBrandsAsync();

            _notificationService.SuccessNotification(
                string.Format(await _localizationService.GetResourceAsync("Plugins.Integration.TrendyolMarketplace.Message.BrandSyncCompleted"), syncCount));

            return Json(new { success = true, message = $"Synced {syncCount} brands, auto-mapped {mappedCount}" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    #endregion

    #region Attribute Mapping

    public async Task<IActionResult> AttributeMapping()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        var categories = await _categoryMappingService.GetAllAsync(isLeaf: true);

        var model = new AttributeMappingSearchModel();
        model.SetGridPageSize();
        model.AvailableCategories = categories.Select(c => new SelectListItem
        {
            Value = c.TrendyolCategoryId.ToString(),
            Text = c.TrendyolCategoryPath ?? c.TrendyolCategoryName
        }).ToList();

        return View($"{TrendyolDefaults.AdminViewPath}/AttributeMapping.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> AttributeMappingList(AttributeMappingSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        var attributes = await _attributeMappingService.GetAllAsync(
            trendyolCategoryId: searchModel.TrendyolCategoryId,
            searchTerm: searchModel.SearchTerm,
            isMapped: searchModel.IsMapped,
            isVariant: searchModel.IsVariant,
            pageIndex: searchModel.Page - 1,
            pageSize: searchModel.PageSize);

        var model = new AttributeMappingListModel().PrepareToGrid(searchModel, attributes, () =>
        {
            return attributes.Select(a => new AttributeMappingModel
            {
                Id = a.Id,
                TrendyolCategoryId = a.TrendyolCategoryId,
                TrendyolAttributeId = a.TrendyolAttributeId,
                TrendyolAttributeName = a.TrendyolAttributeName,
                IsRequired = a.IsRequired,
                IsVariantAttribute = a.IsVariantAttribute,
                AllowCustomValue = a.AllowCustomValue,
                MappingType = a.MappingType,
                MappingTypeDisplay = a.MappingType.ToString(),
                NopSpecificationAttributeId = a.NopSpecificationAttributeId,
                NopProductAttributeId = a.NopProductAttributeId
            });
        });

        return Json(model);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateAttributeMapping(int id, AttributeMappingType mappingType, int? nopSpecificationAttributeId, int? nopProductAttributeId)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        var attribute = await _attributeMappingService.GetByIdAsync(id);
        if (attribute == null)
            return Json(new { success = false, message = "Attribute not found" });

        attribute.MappingType = mappingType;
        attribute.NopSpecificationAttributeId = mappingType == AttributeMappingType.SpecificationAttribute ? nopSpecificationAttributeId : null;
        attribute.NopProductAttributeId = mappingType == AttributeMappingType.ProductAttribute ? nopProductAttributeId : null;

        await _attributeMappingService.UpdateAsync(attribute);

        return Json(new { success = true });
    }

    #endregion

    #region Vendor List

    public async Task<IActionResult> VendorList()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        var model = new VendorCredentialSearchModel();
        model.SetGridPageSize();

        return View($"{TrendyolDefaults.AdminViewPath}/VendorList.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> VendorCredentialList(VendorCredentialSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        var credentials = await _vendorCredentialService.GetAllAsync(
            isActive: searchModel.IsActive,
            pageIndex: searchModel.Page - 1,
            pageSize: searchModel.PageSize);

        var model = new VendorCredentialListModel().PrepareToGrid(searchModel, credentials, () =>
        {
            return credentials.Select(c =>
            {
                var vendor = _vendorService.GetVendorByIdAsync(c.VendorId).Result;

                return new VendorCredentialModel
                {
                    Id = c.Id,
                    VendorId = c.VendorId,
                    VendorName = vendor?.Name ?? "-",
                    TrendyolSupplierId = c.TrendyolSupplierId,
                    ApiKey = c.ApiKey,
                    IsActive = c.IsActive,
                    AutoSyncEnabled = c.AutoSyncEnabled,
                    SyncIntervalMinutes = c.SyncIntervalMinutes,
                    LastSyncOnUtc = c.LastSyncOnUtc,
                    LastSyncOnDisplay = c.LastSyncOnUtc?.ToString("g") ?? "-",
                    HasCredentials = true
                };
            });
        });

        return Json(model);
    }

    #endregion

    #region Data Management

    /// <summary>
    /// Tüm Trendyol verilerini kalıcı olarak siler (DİKKAT: GERİ ALINAMAZ!)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> PurgeAllData(string confirmText)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        // Güvenlik kontrolü - kullanıcı "SIL" yazmalı
        if (confirmText != "SIL")
        {
            return Json(new
            {
                success = false,
                message = await _localizationService.GetResourceAsync("Plugins.Integration.TrendyolMarketplace.PurgeData.ConfirmationRequired")
            });
        }

        try
        {
            // Tüm Trendyol tablolarını sil
            await _dataProvider.ExecuteNonQueryAsync(InstallationData.DropTablesScript);

            // Tabloları yeniden oluştur (boş olarak)
            foreach (var statement in InstallationData.CreateTableScripts)
            {
                try
                {
                    await _dataProvider.ExecuteNonQueryAsync(statement);
                }
                catch
                {
                    // Ignore - table might already exist
                }
            }

            return Json(new
            {
                success = true,
                message = await _localizationService.GetResourceAsync("Plugins.Integration.TrendyolMarketplace.PurgeData.Success")
            });
        }
        catch (Exception ex)
        {
            return Json(new
            {
                success = false,
                message = string.Format(
                    await _localizationService.GetResourceAsync("Plugins.Integration.TrendyolMarketplace.PurgeData.Failed"),
                    ex.Message)
            });
        }
    }

    #endregion

    #region Helper Methods

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
