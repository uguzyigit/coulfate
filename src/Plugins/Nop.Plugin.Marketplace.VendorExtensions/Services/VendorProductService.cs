using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Tax;
using Nop.Data;
using Nop.Plugin.Marketplace.VendorExtensions.Models;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Date;
using Nop.Services.Tax;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Factories;

namespace Nop.Plugin.Marketplace.VendorExtensions.Services;

public class VendorProductService : IVendorProductService
{
    private readonly IRepository<Product> _productRepository;
    private readonly ICategoryService _categoryService;
    private readonly IManufacturerService _manufacturerService;
    private readonly ITaxCategoryService _taxCategoryService;
    private readonly IDateRangeService _dateRangeService;
    private readonly IWarehouseService _warehouseService;
    private readonly IProductService _productService;
    private readonly IDiscountService _discountService;
    private readonly ICurrencyService _currencyService;
    private readonly ILocalizationService _localizationService;
    private readonly ILocalizedModelFactory _localizedModelFactory;
    private readonly ILocalizedEntityService _localizedEntityService;
    private readonly ILanguageService _languageService;
    private readonly CurrencySettings _currencySettings;

    public VendorProductService(
        IRepository<Product> productRepository,
        ICategoryService categoryService,
        IManufacturerService manufacturerService,
        ITaxCategoryService taxCategoryService,
        IDateRangeService dateRangeService,
        IWarehouseService warehouseService,
        IProductService productService,
        IDiscountService discountService,
        ICurrencyService currencyService,
        ILocalizationService localizationService,
        ILocalizedModelFactory localizedModelFactory,
        ILocalizedEntityService localizedEntityService,
        ILanguageService languageService,
        CurrencySettings currencySettings)
    {
        _productRepository = productRepository;
        _categoryService = categoryService;
        _manufacturerService = manufacturerService;
        _taxCategoryService = taxCategoryService;
        _dateRangeService = dateRangeService;
        _warehouseService = warehouseService;
        _productService = productService;
        _discountService = discountService;
        _currencyService = currencyService;
        _localizationService = localizationService;
        _localizedModelFactory = localizedModelFactory;
        _localizedEntityService = localizedEntityService;
        _languageService = languageService;
        _currencySettings = currencySettings;
    }

    public Task<VendorProductDashboardModel> GetDashboardStatsAsync(int vendorId)
    {
        var products = _productRepository.Table
            .Where(p => p.VendorId == vendorId && !p.Deleted);

        var total = products.Count();
        var approved = products.Count(p => p.Published);
        var pending = products.Count(p => !p.Published);
        var outOfStock = products.Count(p =>
            p.ManageInventoryMethodId == (int)ManageInventoryMethod.ManageStock &&
            p.StockQuantity <= 0);
        var active = products.Count(p => p.Published && !p.DisableBuyButton);

        return Task.FromResult(new VendorProductDashboardModel
        {
            TotalProducts = total,
            ApprovedProducts = approved,
            PendingApproval = pending,
            OutOfStockProducts = outOfStock,
            ActiveProducts = active
        });
    }

    public Task ApplyCreateDefaultsAsync(Product product)
    {
        product.Published = false;
        product.ProductTypeId = (int)ProductType.SimpleProduct;
        product.ProductTemplateId = 1;
        product.VisibleIndividually = true;
        product.AllowCustomerReviews = true;
        product.MarkAsNew = true;
        product.MarkAsNewStartDateTimeUtc = DateTime.UtcNow;
        product.MarkAsNewEndDateTimeUtc = DateTime.UtcNow.AddDays(60);
        product.IsShipEnabled = true;
        product.OrderMinimumQuantity = 1;
        product.OrderMaximumQuantity = 10000;
        product.MinStockQuantity = 0;
        product.LowStockActivityId = (int)LowStockActivity.DisableBuyButton;
        product.NotifyAdminForQuantityBelow = 1;
        product.BackorderModeId = (int)BackorderMode.NoBackorders;
        product.DisplayStockAvailability = false;
        product.CreatedOnUtc = DateTime.UtcNow;
        product.UpdatedOnUtc = DateTime.UtcNow;

        return Task.CompletedTask;
    }

    public Task ApplyEditDefaultsAsync(Product product, Product existing)
    {
        // Vendors cannot change these fields
        product.Published = existing.Published;
        product.ProductTypeId = existing.ProductTypeId;
        product.ProductTemplateId = existing.ProductTemplateId;
        product.VisibleIndividually = existing.VisibleIndividually;
        product.VendorId = existing.VendorId;
        product.LowStockActivityId = existing.LowStockActivityId;
        product.BackorderModeId = existing.BackorderModeId;
        product.DisplayStockAvailability = existing.DisplayStockAvailability;
        product.MarkAsNew = existing.MarkAsNew;
        product.MarkAsNewStartDateTimeUtc = existing.MarkAsNewStartDateTimeUtc;
        product.MarkAsNewEndDateTimeUtc = existing.MarkAsNewEndDateTimeUtc;
        product.IsShipEnabled = existing.IsShipEnabled;
        product.AllowCustomerReviews = existing.AllowCustomerReviews;
        product.UpdatedOnUtc = DateTime.UtcNow;

        return Task.CompletedTask;
    }

    public Task HandlePriceChangeAsync(Product product, decimal previousPrice)
    {
        if (product.Price != previousPrice && previousPrice > 0)
            product.OldPrice = previousPrice;

        return Task.CompletedTask;
    }

    public async Task PrepareModelAsync(VendorProductModel model, Product product)
    {
        // Currency code
        var primaryCurrency = await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId);
        model.PrimaryStoreCurrencyCode = primaryCurrency?.CurrencyCode ?? string.Empty;

        // Categories
        model.AvailableCategories.Clear();
        model.AvailableCategories.Add(new SelectListItem
        {
            Text = await _localizationService.GetResourceAsync("Admin.Common.All"),
            Value = "0"
        });
        var categories = await _categoryService.GetAllCategoriesAsync(showHidden: true);
        foreach (var c in categories)
        {
            model.AvailableCategories.Add(new SelectListItem
            {
                Text = await _categoryService.GetFormattedBreadCrumbAsync(c),
                Value = c.Id.ToString(),
                Selected = c.Id == model.SelectedCategoryId
            });
        }

        // Manufacturers
        model.AvailableManufacturers.Clear();
        model.AvailableManufacturers.Add(new SelectListItem
        {
            Text = await _localizationService.GetResourceAsync("Admin.Common.All"),
            Value = "0"
        });
        var manufacturers = await _manufacturerService.GetAllManufacturersAsync(showHidden: true);
        foreach (var m in manufacturers)
        {
            model.AvailableManufacturers.Add(new SelectListItem
            {
                Text = m.Name,
                Value = m.Id.ToString(),
                Selected = m.Id == model.SelectedManufacturerId
            });
        }

        // Tax categories
        model.AvailableTaxCategories.Clear();
        model.AvailableTaxCategories.Add(new SelectListItem { Text = "---", Value = "0" });
        var taxCategories = await _taxCategoryService.GetAllTaxCategoriesAsync();
        foreach (var tc in taxCategories)
        {
            model.AvailableTaxCategories.Add(new SelectListItem
            {
                Text = tc.Name,
                Value = tc.Id.ToString(),
                Selected = tc.Id == model.TaxCategoryId
            });
        }

        // Delivery dates
        model.AvailableDeliveryDates.Clear();
        model.AvailableDeliveryDates.Add(new SelectListItem { Text = "---", Value = "0" });
        var deliveryDates = await _dateRangeService.GetAllDeliveryDatesAsync();
        foreach (var dd in deliveryDates)
        {
            model.AvailableDeliveryDates.Add(new SelectListItem
            {
                Text = dd.Name,
                Value = dd.Id.ToString(),
                Selected = dd.Id == model.DeliveryDateId
            });
        }

        // Warehouses
        model.AvailableWarehouses.Clear();
        model.AvailableWarehouses.Add(new SelectListItem { Text = "---", Value = "0" });
        var warehouses = await _warehouseService.GetAllWarehousesAsync();
        foreach (var w in warehouses)
        {
            model.AvailableWarehouses.Add(new SelectListItem
            {
                Text = w.Name,
                Value = w.Id.ToString(),
                Selected = w.Id == model.WarehouseId
            });
        }

        // Discounts
        model.AvailableDiscounts.Clear();
        var discounts = await _discountService.GetAllDiscountsAsync(DiscountType.AssignedToSkus, showHidden: true);
        foreach (var d in discounts)
        {
            model.AvailableDiscounts.Add(new SelectListItem
            {
                Text = d.Name,
                Value = d.Id.ToString(),
                Selected = model.SelectedDiscountIds.Contains(d.Id)
            });
        }

        // Manage inventory methods
        model.AvailableManageInventoryMethods.Clear();
        foreach (var mim in Enum.GetValues<ManageInventoryMethod>())
        {
            model.AvailableManageInventoryMethods.Add(new SelectListItem
            {
                Text = await _localizationService.GetLocalizedEnumAsync(mim),
                Value = ((int)mim).ToString(),
                Selected = (int)mim == model.ManageInventoryMethodId
            });
        }

        // Locales
        if (product != null)
        {
            model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync<VendorProductLocalizedModel>(async (locale, languageId) =>
            {
                locale.LanguageId = languageId;
                locale.Name = await _localizedEntityService.GetLocalizedValueAsync(languageId, product.Id, "Product", nameof(Product.Name));
                locale.ShortDescription = await _localizedEntityService.GetLocalizedValueAsync(languageId, product.Id, "Product", nameof(Product.ShortDescription));
                locale.FullDescription = await _localizedEntityService.GetLocalizedValueAsync(languageId, product.Id, "Product", nameof(Product.FullDescription));
            });
        }
        else
        {
            model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync<VendorProductLocalizedModel>();
        }

        // Warehouse inventory models
        if (product != null)
        {
            foreach (var warehouse in await _warehouseService.GetAllWarehousesAsync())
            {
                var whModel = new ProductWarehouseInventoryModel
                {
                    WarehouseId = warehouse.Id,
                    WarehouseName = warehouse.Name
                };

                var records = await _productService.GetAllProductWarehouseInventoryRecordsAsync(product.Id);
                var record = records?.FirstOrDefault(r => r.WarehouseId == warehouse.Id);
                if (record != null)
                {
                    whModel.WarehouseUsed = true;
                    whModel.StockQuantity = record.StockQuantity;
                    whModel.ReservedQuantity = record.ReservedQuantity;
                }

                model.ProductWarehouseInventoryModels.Add(whModel);
            }
        }

        // Search models for edit
        if (product != null)
        {
            model.ProductPictureSearchModel.ProductId = product.Id;
            model.ProductPictureSearchModel.SetGridPageSize();
            model.ProductVideoSearchModel.ProductId = product.Id;
            model.ProductVideoSearchModel.SetGridPageSize();
            model.ProductAttributeMappingSearchModel.ProductId = product.Id;
            model.ProductAttributeMappingSearchModel.SetGridPageSize();
            model.ProductAttributeCombinationSearchModel.ProductId = product.Id;
            model.ProductAttributeCombinationSearchModel.SetGridPageSize();
            model.ProductSpecificationAttributeSearchModel.ProductId = product.Id;
            model.ProductSpecificationAttributeSearchModel.SetGridPageSize();
            model.TierPriceSearchModel.ProductId = product.Id;
            model.TierPriceSearchModel.SetGridPageSize();
        }
    }

    public Task<(bool isValid, string errorMessage)> ValidateVideoAsync(string fileName, decimal? durationSeconds)
    {
        if (string.IsNullOrEmpty(fileName))
            return Task.FromResult((false, "Dosya adı boş olamaz."));

        var ext = Path.GetExtension(fileName)?.ToLowerInvariant();
        if (ext != ".mp4")
            return Task.FromResult((false, "Sadece mp4 formatında video yükleyebilirsiniz."));

        if (durationSeconds.HasValue)
        {
            if (durationSeconds.Value < 8)
                return Task.FromResult((false, "Video en az 8 saniye uzunluğunda olmalıdır."));
            if (durationSeconds.Value > 60)
                return Task.FromResult((false, "Video en fazla 60 saniye uzunluğunda olabilir."));
        }

        return Task.FromResult((true, string.Empty));
    }

    public Task<bool> IsGtinUniqueAsync(string gtin, int excludeProductId = 0)
    {
        if (string.IsNullOrWhiteSpace(gtin))
            return Task.FromResult(true);

        var query = _productRepository.Table.Where(p => p.Gtin == gtin && !p.Deleted);
        if (excludeProductId > 0)
            query = query.Where(p => p.Id != excludeProductId);

        return Task.FromResult(!query.Any());
    }
}
