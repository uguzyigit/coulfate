using Marketplace.Abstractions.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Media;
using Nop.Core.Infrastructure;
using Nop.Plugin.Marketplace.VendorExtensions.Models;
using Nop.Plugin.Marketplace.VendorExtensions.Services;
using Nop.Services.Catalog;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Seo;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Framework.UI;

namespace Nop.Plugin.Marketplace.VendorExtensions.Controllers;

[Area(AreaNames.ADMIN)]
[AuthorizeAdmin]
[AutoValidateAntiforgeryToken]
public class VendorProductController : BasePluginController
{
    private readonly IWorkContext _workContext;
    private readonly IProductService _productService;
    private readonly IProductModelFactory _productModelFactory;
    private readonly IProductAttributeService _productAttributeService;
    private readonly IProductAttributeParser _productAttributeParser;
    private readonly ISpecificationAttributeService _specificationAttributeService;
    private readonly ICategoryService _categoryService;
    private readonly IManufacturerService _manufacturerService;
    private readonly IPictureService _pictureService;
    private readonly IVideoService _videoService;
    private readonly IUrlRecordService _urlRecordService;
    private readonly ILocalizedEntityService _localizedEntityService;
    private readonly ILocalizationService _localizationService;
    private readonly ILanguageService _languageService;
    private readonly INotificationService _notificationService;
    private readonly ICustomerActivityService _customerActivityService;
    private readonly IDiscountService _discountService;
    private readonly INopHtmlHelper _nopHtmlHelper;
    private readonly IVendorProductService _vendorProductService;
    private readonly INopFileProvider _fileProvider;
    private readonly ICategorySpecMappingService _categorySpecMappingService;

    public VendorProductController(
        IWorkContext workContext,
        IProductService productService,
        IProductModelFactory productModelFactory,
        IProductAttributeService productAttributeService,
        IProductAttributeParser productAttributeParser,
        ISpecificationAttributeService specificationAttributeService,
        ICategoryService categoryService,
        IManufacturerService manufacturerService,
        IPictureService pictureService,
        IVideoService videoService,
        IUrlRecordService urlRecordService,
        ILocalizedEntityService localizedEntityService,
        ILocalizationService localizationService,
        ILanguageService languageService,
        INotificationService notificationService,
        ICustomerActivityService customerActivityService,
        IDiscountService discountService,
        INopHtmlHelper nopHtmlHelper,
        IVendorProductService vendorProductService,
        INopFileProvider fileProvider,
        ICategorySpecMappingService categorySpecMappingService)
    {
        _workContext = workContext;
        _productService = productService;
        _productModelFactory = productModelFactory;
        _productAttributeService = productAttributeService;
        _productAttributeParser = productAttributeParser;
        _specificationAttributeService = specificationAttributeService;
        _categoryService = categoryService;
        _manufacturerService = manufacturerService;
        _pictureService = pictureService;
        _videoService = videoService;
        _urlRecordService = urlRecordService;
        _localizedEntityService = localizedEntityService;
        _localizationService = localizationService;
        _languageService = languageService;
        _notificationService = notificationService;
        _customerActivityService = customerActivityService;
        _discountService = discountService;
        _nopHtmlHelper = nopHtmlHelper;
        _vendorProductService = vendorProductService;
        _fileProvider = fileProvider;
        _categorySpecMappingService = categorySpecMappingService;
    }

    #region Utilities

    private async Task<Nop.Core.Domain.Vendors.Vendor> GetCurrentVendorOrDenyAsync()
    {
        return await _workContext.GetCurrentVendorAsync();
    }

    private async Task<Product> GetVendorProductAsync(int productId, int vendorId)
    {
        var product = await _productService.GetProductByIdAsync(productId);
        if (product == null || product.Deleted || product.VendorId != vendorId)
            return null;
        return product;
    }

    private async Task UpdateLocalesAsync(Product product, VendorProductModel model)
    {
        foreach (var localized in model.Locales)
        {
            await _localizedEntityService.SaveLocalizedValueAsync(product, x => x.Name, localized.Name, localized.LanguageId);
            await _localizedEntityService.SaveLocalizedValueAsync(product, x => x.ShortDescription, localized.ShortDescription, localized.LanguageId);
            await _localizedEntityService.SaveLocalizedValueAsync(product, x => x.FullDescription, localized.FullDescription, localized.LanguageId);

            var seName = await _urlRecordService.ValidateSeNameAsync(product, string.Empty, localized.Name, false);
            await _urlRecordService.SaveSlugAsync(product, seName, localized.LanguageId);
        }
    }

    private async Task SaveCategoryMappingAsync(Product product, int categoryId)
    {
        var existingCategories = await _categoryService.GetProductCategoriesByProductIdAsync(product.Id, true);

        // Remove existing
        if (existingCategories.Any())
            await _categoryService.DeleteProductCategoriesAsync(existingCategories.ToList());

        // Add new
        if (categoryId > 0)
        {
            await _categoryService.InsertProductCategoryAsync(new ProductCategory
            {
                ProductId = product.Id,
                CategoryId = categoryId,
                DisplayOrder = 1
            });
        }
    }

    private async Task SaveManufacturerMappingAsync(Product product, int manufacturerId)
    {
        var existingManufacturers = await _manufacturerService.GetProductManufacturersByProductIdAsync(product.Id, true);

        // Remove existing
        if (existingManufacturers.Any())
            await _manufacturerService.DeleteProductManufacturersAsync(existingManufacturers.ToList());

        // Add new
        if (manufacturerId > 0)
        {
            await _manufacturerService.InsertProductManufacturerAsync(new ProductManufacturer
            {
                ProductId = product.Id,
                ManufacturerId = manufacturerId,
                DisplayOrder = 1
            });
        }
    }

    private async Task SaveDiscountMappingsAsync(Product product, IList<int> selectedDiscountIds)
    {
        var allDiscounts = await _discountService.GetAllDiscountsAsync(DiscountType.AssignedToSkus, showHidden: true);

        // Remove mappings no longer selected
        foreach (var discount in allDiscounts)
        {
            if (selectedDiscountIds.Contains(discount.Id))
                continue;

            var mapping = await _productService.GetDiscountAppliedToProductAsync(product.Id, discount.Id);
            if (mapping != null)
                await _productService.DeleteDiscountProductMappingAsync(mapping);
        }

        // Add new mappings
        foreach (var discountId in selectedDiscountIds)
        {
            if (await _productService.GetDiscountAppliedToProductAsync(product.Id, discountId) is null)
                await _productService.InsertDiscountProductMappingAsync(new DiscountProductMapping { EntityId = product.Id, DiscountId = discountId });
        }

        await _productService.UpdateProductAsync(product);
    }

    private void MapModelToProduct(VendorProductModel model, Product product)
    {
        product.Name = model.Name;
        product.ShortDescription = model.ShortDescription;
        product.FullDescription = model.FullDescription;
        product.Sku = model.Sku;
        product.Gtin = model.Gtin;
        product.ManufacturerPartNumber = model.ManufacturerPartNumber;
        product.Price = model.Price;
        product.IsTaxExempt = model.IsTaxExempt;
        product.TaxCategoryId = model.TaxCategoryId;
        product.DeliveryDateId = model.DeliveryDateId;
        product.ManageInventoryMethodId = model.ManageInventoryMethodId;
        product.StockQuantity = model.StockQuantity;
        product.WarehouseId = model.WarehouseId;
        product.UseMultipleWarehouses = model.UseMultipleWarehouses;
        product.AllowedQuantities = model.AllowedQuantities;
    }

    private void MapProductToModel(Product product, VendorProductModel model)
    {
        model.Id = product.Id;
        model.Name = product.Name;
        model.ShortDescription = product.ShortDescription;
        model.FullDescription = product.FullDescription;
        model.Sku = product.Sku;
        model.Gtin = product.Gtin;
        model.ManufacturerPartNumber = product.ManufacturerPartNumber;
        model.Price = product.Price;
        model.OldPrice = product.OldPrice;
        model.IsTaxExempt = product.IsTaxExempt;
        model.TaxCategoryId = product.TaxCategoryId;
        model.DeliveryDateId = product.DeliveryDateId;
        model.ManageInventoryMethodId = product.ManageInventoryMethodId;
        model.StockQuantity = product.StockQuantity;
        model.LastStockQuantity = product.StockQuantity;
        model.WarehouseId = product.WarehouseId;
        model.UseMultipleWarehouses = product.UseMultipleWarehouses;
        model.AllowedQuantities = product.AllowedQuantities;
        model.Published = product.Published;
    }

    #endregion

    #region List / Dashboard

    public async Task<IActionResult> List()
    {
        var vendor = await GetCurrentVendorOrDenyAsync();
        if (vendor == null)
            return AccessDeniedView();

        _nopHtmlHelper.SetActiveMenuItemSystemName("Vendor.ProductCatalog.Products");

        var model = new VendorProductListModel();

        // Categories
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
                Value = c.Id.ToString()
            });
        }

        // Manufacturers
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
                Value = m.Id.ToString()
            });
        }

        // Product status options
        model.AvailablePublishedOptions.Add(new SelectListItem { Text = "Tümü", Value = "0" });
        model.AvailablePublishedOptions.Add(new SelectListItem { Text = "Onaylanan Ürünler", Value = "1" });
        model.AvailablePublishedOptions.Add(new SelectListItem { Text = "Onay Bekleyen", Value = "2" });
        model.AvailablePublishedOptions.Add(new SelectListItem { Text = "Tükenen Ürünler", Value = "3" });
        model.AvailablePublishedOptions.Add(new SelectListItem { Text = "Satıştaki Ürünler", Value = "4" });

        model.SetGridPageSize();

        return View("~/Plugins/Marketplace.VendorExtensions/Views/VendorProduct/List.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> ProductListData(VendorProductListModel searchModel)
    {
        var vendor = await GetCurrentVendorOrDenyAsync();
        if (vendor == null)
            return AccessDeniedView();

        // Filter: 0=All, 1=Published, 2=Unpublished, 3=OutOfStock, 4=Active(OnSale)
        bool? published = searchModel.SearchPublishedId switch
        {
            1 => true,
            2 => false,
            _ => null
        };

        var products = await _productService.SearchProductsAsync(
            pageIndex: searchModel.Page - 1,
            pageSize: searchModel.PageSize,
            vendorId: vendor.Id,
            categoryIds: searchModel.SearchCategoryId > 0 ? new List<int> { searchModel.SearchCategoryId } : null,
            manufacturerIds: searchModel.SearchManufacturerId > 0 ? new List<int> { searchModel.SearchManufacturerId } : null,
            keywords: searchModel.SearchProductName,
            showHidden: true,
            overridePublished: published);

        // Apply additional filters for out-of-stock and active products
        if (searchModel.SearchPublishedId == 3) // Tükenen Ürünler
        {
            products = await _productService.SearchProductsAsync(
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize,
                vendorId: vendor.Id,
                categoryIds: searchModel.SearchCategoryId > 0 ? new List<int> { searchModel.SearchCategoryId } : null,
                keywords: searchModel.SearchProductName,
                showHidden: true,
                overridePublished: null);

            var outOfStockIds = products.Where(p =>
                p.ManageInventoryMethodId == (int)ManageInventoryMethod.ManageStock &&
                p.StockQuantity <= 0).Select(p => p.Id).ToList();

            var allOutOfStock = products.Where(p => outOfStockIds.Contains(p.Id)).ToList();
            var pagedResult = new Nop.Core.PagedList<Product>(allOutOfStock, 0, allOutOfStock.Count, allOutOfStock.Count);
            products = pagedResult;
        }
        else if (searchModel.SearchPublishedId == 4) // Satıştaki Ürünler
        {
            products = await _productService.SearchProductsAsync(
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize,
                vendorId: vendor.Id,
                categoryIds: searchModel.SearchCategoryId > 0 ? new List<int> { searchModel.SearchCategoryId } : null,
                keywords: searchModel.SearchProductName,
                showHidden: true,
                overridePublished: true);

            var activeIds = products.Where(p => !p.DisableBuyButton).Select(p => p.Id).ToList();
            var allActive = products.Where(p => activeIds.Contains(p.Id)).ToList();
            var pagedResult = new Nop.Core.PagedList<Product>(allActive, 0, allActive.Count, allActive.Count);
            products = pagedResult;
        }

        var data = await Task.WhenAll(products.Select(async p =>
        {
            var defaultPicture = (await _productService.GetProductPicturesByProductIdAsync(p.Id)).FirstOrDefault();
            var pictureUrl = defaultPicture != null
                ? await _pictureService.GetPictureUrlAsync(defaultPicture.PictureId, 75) ?? string.Empty
                : string.Empty;

            return (object)new
            {
                p.Id,
                PictureUrl = pictureUrl,
                p.Name,
                p.Sku,
                p.Price,
                p.StockQuantity,
                p.Published
            };
        }));

        return Json(new
        {
            draw = searchModel.Draw,
            recordsTotal = products.TotalCount,
            recordsFiltered = products.TotalCount,
            Data = data
        });
    }

    [HttpPost]
    public async Task<IActionResult> DashboardStats()
    {
        var vendor = await GetCurrentVendorOrDenyAsync();
        if (vendor == null)
            return Json(new { success = false });

        var stats = await _vendorProductService.GetDashboardStatsAsync(vendor.Id);
        return Json(stats);
    }

    #endregion

    #region Create / Edit / Delete

    public async Task<IActionResult> Create()
    {
        var vendor = await GetCurrentVendorOrDenyAsync();
        if (vendor == null)
            return AccessDeniedView();

        _nopHtmlHelper.SetActiveMenuItemSystemName("Vendor.ProductCatalog.AddNew");

        var model = new VendorProductModel();
        await _vendorProductService.PrepareModelAsync(model, null);

        return View("~/Plugins/Marketplace.VendorExtensions/Views/VendorProduct/Create.cshtml", model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    public async Task<IActionResult> Create(VendorProductModel model, bool continueEditing)
    {
        var vendor = await GetCurrentVendorOrDenyAsync();
        if (vendor == null)
            return AccessDeniedView();

        // GTIN uniqueness check
        if (!string.IsNullOrWhiteSpace(model.Gtin) && !await _vendorProductService.IsGtinUniqueAsync(model.Gtin))
            ModelState.AddModelError(nameof(model.Gtin), "Bu GTIN numarası zaten başka bir ürüne ait. Lütfen farklı bir GTIN girin.");

        if (ModelState.IsValid)
        {
            var product = new Product();
            MapModelToProduct(model, product);

            // Force vendor ownership
            product.VendorId = vendor.Id;

            // Apply all defaults
            await _vendorProductService.ApplyCreateDefaultsAsync(product);

            await _productService.InsertProductAsync(product);

            // SEO slug
            var seName = await _urlRecordService.ValidateSeNameAsync(product, string.Empty, product.Name, true);
            await _urlRecordService.SaveSlugAsync(product, seName, 0);

            // Locales
            await UpdateLocalesAsync(product, model);

            // Category (single)
            await SaveCategoryMappingAsync(product, model.SelectedCategoryId);

            // Auto-add spec attributes from category mapping
            if (model.SelectedCategoryId > 0)
                await _categorySpecMappingService.AutoAddSpecAttributesToProductAsync(product.Id, model.SelectedCategoryId);

            // Manufacturer (single)
            await SaveManufacturerMappingAsync(product, model.SelectedManufacturerId);

            // Discounts
            await SaveDiscountMappingsAsync(product, model.SelectedDiscountIds ?? new List<int>());

            // Stock history
            await _productService.AddStockQuantityHistoryEntryAsync(product, product.StockQuantity, product.StockQuantity, product.WarehouseId,
                await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.Edit"));

            // Activity log
            await _customerActivityService.InsertActivityAsync("AddNewProduct",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.AddNewProduct"), product.Name), product);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Added"));

            if (!continueEditing)
                return RedirectToAction("List");

            return RedirectToAction("Edit", new { id = product.Id });
        }

        // Redisplay form
        await _vendorProductService.PrepareModelAsync(model, null);
        return View("~/Plugins/Marketplace.VendorExtensions/Views/VendorProduct/Create.cshtml", model);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var vendor = await GetCurrentVendorOrDenyAsync();
        if (vendor == null)
            return AccessDeniedView();

        var product = await GetVendorProductAsync(id, vendor.Id);
        if (product == null)
            return RedirectToAction("List");

        _nopHtmlHelper.SetActiveMenuItemSystemName("Vendor.ProductCatalog.Products");

        var model = new VendorProductModel();
        MapProductToModel(product, model);

        // Load existing category
        var productCategories = await _categoryService.GetProductCategoriesByProductIdAsync(product.Id, true);
        model.SelectedCategoryId = productCategories.FirstOrDefault()?.CategoryId ?? 0;

        // Load existing manufacturer
        var productManufacturers = await _manufacturerService.GetProductManufacturersByProductIdAsync(product.Id, true);
        model.SelectedManufacturerId = productManufacturers.FirstOrDefault()?.ManufacturerId ?? 0;

        // Load existing discounts
        var appliedDiscounts = await _discountService.GetAppliedDiscountsAsync(product);
        model.SelectedDiscountIds = appliedDiscounts.Select(d => d.Id).ToList();

        await _vendorProductService.PrepareModelAsync(model, product);

        // Product preview URL
        var seName = await _urlRecordService.GetSeNameAsync(product);
        ViewBag.PreviewUrl = $"/{seName}";

        return View("~/Plugins/Marketplace.VendorExtensions/Views/VendorProduct/Edit.cshtml", model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    public async Task<IActionResult> Edit(VendorProductModel model, bool continueEditing)
    {
        var vendor = await GetCurrentVendorOrDenyAsync();
        if (vendor == null)
            return AccessDeniedView();

        var product = await GetVendorProductAsync(model.Id, vendor.Id);
        if (product == null)
            return RedirectToAction("List");

        // Stock concurrency check
        if (product.StockQuantity != model.LastStockQuantity)
        {
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Fields.StockQuantity.ChangedWarning"));
            return RedirectToAction("Edit", new { id = product.Id });
        }

        // Check at least 1 picture exists
        var productPictures = await _productService.GetProductPicturesByProductIdAsync(product.Id);
        if (!productPictures.Any())
        {
            ModelState.AddModelError(string.Empty, "En az 1 adet ürün resmi eklemeniz zorunludur.");
        }

        // GTIN uniqueness check
        if (!string.IsNullOrWhiteSpace(model.Gtin) && !await _vendorProductService.IsGtinUniqueAsync(model.Gtin, product.Id))
            ModelState.AddModelError(nameof(model.Gtin), "Bu GTIN numarası zaten başka bir ürüne ait. Lütfen farklı bir GTIN girin.");

        if (ModelState.IsValid)
        {
            var previousPrice = product.Price;
            var previousStockQuantity = product.StockQuantity;
            var previousWarehouseId = product.WarehouseId;

            // Save immutable values before mapping overwrites them
            var savedPublished = product.Published;
            var savedProductTypeId = product.ProductTypeId;
            var savedProductTemplateId = product.ProductTemplateId;
            var savedVisibleIndividually = product.VisibleIndividually;
            var savedVendorId = product.VendorId;
            var savedLowStockActivityId = product.LowStockActivityId;
            var savedBackorderModeId = product.BackorderModeId;
            var savedDisplayStockAvailability = product.DisplayStockAvailability;
            var savedMarkAsNew = product.MarkAsNew;
            var savedMarkAsNewStart = product.MarkAsNewStartDateTimeUtc;
            var savedMarkAsNewEnd = product.MarkAsNewEndDateTimeUtc;
            var savedIsShipEnabled = product.IsShipEnabled;
            var savedAllowCustomerReviews = product.AllowCustomerReviews;

            // Map visible fields only
            MapModelToProduct(model, product);

            // Restore immutable fields (vendors cannot change these)
            product.Published = savedPublished;
            product.ProductTypeId = savedProductTypeId;
            product.ProductTemplateId = savedProductTemplateId;
            product.VisibleIndividually = savedVisibleIndividually;
            product.VendorId = savedVendorId;
            product.LowStockActivityId = savedLowStockActivityId;
            product.BackorderModeId = savedBackorderModeId;
            product.DisplayStockAvailability = savedDisplayStockAvailability;
            product.MarkAsNew = savedMarkAsNew;
            product.MarkAsNewStartDateTimeUtc = savedMarkAsNewStart;
            product.MarkAsNewEndDateTimeUtc = savedMarkAsNewEnd;
            product.IsShipEnabled = savedIsShipEnabled;
            product.AllowCustomerReviews = savedAllowCustomerReviews;
            product.UpdatedOnUtc = DateTime.UtcNow;

            // Handle OldPrice
            await _vendorProductService.HandlePriceChangeAsync(product, previousPrice);

            await _productService.UpdateProductAsync(product);

            // SEO slug
            var seName = await _urlRecordService.ValidateSeNameAsync(product, string.Empty, product.Name, true);
            await _urlRecordService.SaveSlugAsync(product, seName, 0);

            // Locales
            await UpdateLocalesAsync(product, model);

            // Category — handle category change for spec attributes
            var oldCategoryId = (await _categoryService.GetProductCategoriesByProductIdAsync(product.Id)).FirstOrDefault()?.CategoryId ?? 0;
            await SaveCategoryMappingAsync(product, model.SelectedCategoryId);

            if (model.SelectedCategoryId != oldCategoryId && model.SelectedCategoryId > 0)
            {
                // Remove auto-added spec attrs from old category
                if (oldCategoryId > 0)
                    await _categorySpecMappingService.RemoveAutoAddedSpecAttributesAsync(product.Id, oldCategoryId);

                // Auto-add spec attrs from new category
                await _categorySpecMappingService.AutoAddSpecAttributesToProductAsync(product.Id, model.SelectedCategoryId);
            }

            // Manufacturer
            await SaveManufacturerMappingAsync(product, model.SelectedManufacturerId);

            // Discounts
            await SaveDiscountMappingsAsync(product, model.SelectedDiscountIds ?? new List<int>());

            // Stock history
            if (previousWarehouseId != product.WarehouseId)
            {
                await _productService.AddStockQuantityHistoryEntryAsync(product, -previousStockQuantity, 0, previousWarehouseId,
                    await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.Edit"));
                await _productService.AddStockQuantityHistoryEntryAsync(product, product.StockQuantity, product.StockQuantity, product.WarehouseId,
                    await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.Edit"));
            }
            else
            {
                await _productService.AddStockQuantityHistoryEntryAsync(product, product.StockQuantity - previousStockQuantity, product.StockQuantity,
                    product.WarehouseId, await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.Edit"));
            }

            // Activity log
            await _customerActivityService.InsertActivityAsync("EditProduct",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditProduct"), product.Name), product);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Updated"));

            if (!continueEditing)
                return RedirectToAction("List");

            return RedirectToAction("Edit", new { id = product.Id });
        }

        // Redisplay form
        await _vendorProductService.PrepareModelAsync(model, product);
        return View("~/Plugins/Marketplace.VendorExtensions/Views/VendorProduct/Edit.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var vendor = await GetCurrentVendorOrDenyAsync();
        if (vendor == null)
            return AccessDeniedView();

        var product = await GetVendorProductAsync(id, vendor.Id);
        if (product == null)
            return RedirectToAction("List");

        await _productService.DeleteProductAsync(product);

        await _customerActivityService.InsertActivityAsync("DeleteProduct",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteProduct"), product.Name), product);

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Deleted"));

        return RedirectToAction("List");
    }

    #endregion

    #region Pictures

    [HttpPost]
    public async Task<IActionResult> ProductPictureAdd(int productId, IFormCollection form)
    {
        var vendor = await GetCurrentVendorOrDenyAsync();
        if (vendor == null)
            return Json(new { success = false });

        var product = await GetVendorProductAsync(productId, vendor.Id);
        if (product == null)
            return Json(new { success = false, message = "Product not found" });

        var files = form.Files.ToList();
        if (!files.Any())
            return Json(new { success = false });

        try
        {
            foreach (var file in files)
            {
                var picture = await _pictureService.InsertPictureAsync(file);
                await _pictureService.SetSeoFilenameAsync(picture.Id, await _pictureService.GetPictureSeNameAsync(product.Name));
                await _productService.InsertProductPictureAsync(new ProductPicture
                {
                    PictureId = picture.Id,
                    ProductId = product.Id,
                    DisplayOrder = 0
                });
            }
        }
        catch (Exception exc)
        {
            return Json(new { success = false, message = exc.Message });
        }

        return Json(new { success = true });
    }

    [HttpPost]
    public async Task<IActionResult> ProductPictureList(ProductPictureSearchModel searchModel)
    {
        var vendor = await GetCurrentVendorOrDenyAsync();
        if (vendor == null)
            return Content("Access denied");

        var product = await GetVendorProductAsync(searchModel.ProductId, vendor.Id);
        if (product == null)
            return Content("This is not your product");

        var model = await _productModelFactory.PrepareProductPictureListModelAsync(searchModel, product);
        return Json(model);
    }

    [HttpPost]
    public async Task<IActionResult> ProductPictureUpdate(ProductPictureModel model)
    {
        var vendor = await GetCurrentVendorOrDenyAsync();
        if (vendor == null)
            return Content("Access denied");

        var productPicture = await _productService.GetProductPictureByIdAsync(model.Id);
        if (productPicture == null)
            return Content("Not found");

        var product = await GetVendorProductAsync(productPicture.ProductId, vendor.Id);
        if (product == null)
            return Content("This is not your product");

        var picture = await _pictureService.GetPictureByIdAsync(productPicture.PictureId);
        if (picture != null)
        {
            await _pictureService.UpdatePictureAsync(picture.Id,
                await _pictureService.LoadPictureBinaryAsync(picture),
                picture.MimeType, picture.SeoFilename,
                model.OverrideAltAttribute, model.OverrideTitleAttribute);
        }

        productPicture.DisplayOrder = model.DisplayOrder;
        await _productService.UpdateProductPictureAsync(productPicture);

        return new NullJsonResult();
    }

    [HttpPost]
    public async Task<IActionResult> ProductPictureDelete(int id)
    {
        var vendor = await GetCurrentVendorOrDenyAsync();
        if (vendor == null)
            return Content("Access denied");

        var productPicture = await _productService.GetProductPictureByIdAsync(id);
        if (productPicture == null)
            return Content("Not found");

        var product = await GetVendorProductAsync(productPicture.ProductId, vendor.Id);
        if (product == null)
            return Content("This is not your product");

        var pictureId = productPicture.PictureId;
        await _productService.DeleteProductPictureAsync(productPicture);

        var picture = await _pictureService.GetPictureByIdAsync(pictureId);
        if (picture != null)
            await _pictureService.DeletePictureAsync(picture);

        return new NullJsonResult();
    }

    #endregion

    #region Videos

    [HttpPost]
    public async Task<IActionResult> ProductVideoUpload(int productId, IFormFile videoFile, string videoDuration)
    {
        var vendor = await GetCurrentVendorOrDenyAsync();
        if (vendor == null)
            return Json(new { success = false });

        var product = await GetVendorProductAsync(productId, vendor.Id);
        if (product == null)
            return Json(new { success = false, message = "Product not found" });

        if (videoFile == null || videoFile.Length == 0)
            return Json(new { success = false, message = "Video dosyası gereklidir." });

        // Parse duration with InvariantCulture to avoid locale issues (JS sends "18.5" with dot)
        decimal? parsedDuration = null;
        if (!string.IsNullOrEmpty(videoDuration) && decimal.TryParse(videoDuration, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var dur))
            parsedDuration = dur;

        // Validate
        var (isValid, errorMessage) = await _vendorProductService.ValidateVideoAsync(videoFile.FileName, parsedDuration);
        if (!isValid)
            return Json(new { success = false, message = errorMessage });

        try
        {
            // Save file to wwwroot/videos/products/
            var videosDir = _fileProvider.MapPath("~/wwwroot/videos/products");
            _fileProvider.CreateDirectory(videosDir);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(videoFile.FileName)}";
            var filePath = _fileProvider.Combine(videosDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await videoFile.CopyToAsync(stream);
            }

            var videoUrl = $"/videos/products/{fileName}";

            // Create Video entity
            var video = new Video { VideoUrl = videoUrl };
            await _videoService.InsertVideoAsync(video);

            // Create ProductVideo mapping
            await _productService.InsertProductVideoAsync(new ProductVideo
            {
                VideoId = video.Id,
                ProductId = product.Id,
                DisplayOrder = 0
            });

            return Json(new { success = true });
        }
        catch (Exception exc)
        {
            return Json(new { success = false, message = exc.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> ProductVideoList(ProductVideoSearchModel searchModel)
    {
        var vendor = await GetCurrentVendorOrDenyAsync();
        if (vendor == null)
            return Content("Access denied");

        var product = await GetVendorProductAsync(searchModel.ProductId, vendor.Id);
        if (product == null)
            return Content("This is not your product");

        var model = await _productModelFactory.PrepareProductVideoListModelAsync(searchModel, product);
        return Json(model);
    }

    [HttpPost]
    public async Task<IActionResult> ProductVideoDelete(int id)
    {
        var vendor = await GetCurrentVendorOrDenyAsync();
        if (vendor == null)
            return Content("Access denied");

        var productVideo = await _productService.GetProductVideoByIdAsync(id);
        if (productVideo == null)
            return Content("Not found");

        var product = await GetVendorProductAsync(productVideo.ProductId, vendor.Id);
        if (product == null)
            return Content("This is not your product");

        var videoId = productVideo.VideoId;
        await _productService.DeleteProductVideoAsync(productVideo);

        var video = await _videoService.GetVideoByIdAsync(videoId);
        if (video != null)
            await _videoService.DeleteVideoAsync(video);

        return new NullJsonResult();
    }

    #endregion

    #region Product Attributes

    [HttpPost]
    public async Task<IActionResult> ProductAttributeMappingList(ProductAttributeMappingSearchModel searchModel)
    {
        var vendor = await GetCurrentVendorOrDenyAsync();
        if (vendor == null)
            return Content("Access denied");

        var product = await GetVendorProductAsync(searchModel.ProductId, vendor.Id);
        if (product == null)
            return Content("This is not your product");

        var model = await _productModelFactory.PrepareProductAttributeMappingListModelAsync(searchModel, product);
        return Json(model);
    }

    public async Task<IActionResult> ProductAttributeMappingCreate(int productId)
    {
        var vendor = await GetCurrentVendorOrDenyAsync();
        if (vendor == null)
            return AccessDeniedView();

        var product = await GetVendorProductAsync(productId, vendor.Id);
        if (product == null)
            return RedirectToAction("List");

        var model = await _productModelFactory.PrepareProductAttributeMappingModelAsync(new ProductAttributeMappingModel(), product, null);
        return View("~/Plugins/Marketplace.VendorExtensions/Views/VendorProduct/ProductAttributeMappingCreate.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> ProductAttributeMappingCreate(ProductAttributeMappingModel model)
    {
        var vendor = await GetCurrentVendorOrDenyAsync();
        if (vendor == null)
            return AccessDeniedView();

        var product = await GetVendorProductAsync(model.ProductId, vendor.Id);
        if (product == null)
            return RedirectToAction("List");

        // Check already mapped
        if ((await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id))
            .Any(x => x.ProductAttributeId == model.ProductAttributeId))
        {
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.Attributes.AlreadyExists"));
            model = await _productModelFactory.PrepareProductAttributeMappingModelAsync(model, product, null, true);
            return View("~/Plugins/Marketplace.VendorExtensions/Views/VendorProduct/ProductAttributeMappingCreate.cshtml", model);
        }

        // Determine control type: check if attribute is "color" type
        var productAttribute = await _productAttributeService.GetProductAttributeByIdAsync(model.ProductAttributeId);
        var isColor = productAttribute?.Name?.Contains("renk", StringComparison.OrdinalIgnoreCase) == true ||
                      productAttribute?.Name?.Contains("color", StringComparison.OrdinalIgnoreCase) == true;

        var pam = new ProductAttributeMapping
        {
            ProductId = model.ProductId,
            ProductAttributeId = model.ProductAttributeId,
            TextPrompt = string.Empty,
            IsRequired = true,
            AttributeControlTypeId = isColor ? (int)AttributeControlType.ColorSquares : (int)AttributeControlType.DropdownList,
            DisplayOrder = model.DisplayOrder
        };

        await _productAttributeService.InsertProductAttributeMappingAsync(pam);

        // Predefined values
        var predefinedValues = await _productAttributeService.GetPredefinedProductAttributeValuesAsync(model.ProductAttributeId);
        foreach (var predefinedValue in predefinedValues)
        {
            var pav = new ProductAttributeValue
            {
                ProductAttributeMappingId = pam.Id,
                AttributeValueType = AttributeValueType.Simple,
                Name = predefinedValue.Name,
                PriceAdjustment = predefinedValue.PriceAdjustment,
                PriceAdjustmentUsePercentage = predefinedValue.PriceAdjustmentUsePercentage,
                WeightAdjustment = predefinedValue.WeightAdjustment,
                Cost = predefinedValue.Cost,
                IsPreSelected = predefinedValue.IsPreSelected,
                DisplayOrder = predefinedValue.DisplayOrder
            };
            await _productAttributeService.InsertProductAttributeValueAsync(pav);

            var languages = await _languageService.GetAllLanguagesAsync(true);
            foreach (var lang in languages)
            {
                var name = await _localizationService.GetLocalizedAsync(predefinedValue, x => x.Name, lang.Id, false, false);
                if (!string.IsNullOrEmpty(name))
                    await _localizedEntityService.SaveLocalizedValueAsync(pav, x => x.Name, name, lang.Id);
            }
        }

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.Attributes.Added"));
        return RedirectToAction("Edit", new { id = product.Id });
    }

    public async Task<IActionResult> ProductAttributeMappingCreatePopup(int productId, string btnId, string formId)
    {
        var vendor = await GetCurrentVendorOrDenyAsync();
        if (vendor == null)
            return AccessDeniedView();

        var product = await GetVendorProductAsync(productId, vendor.Id);
        if (product == null)
            return RedirectToAction("List");

        var model = await _productModelFactory.PrepareProductAttributeMappingModelAsync(new ProductAttributeMappingModel(), product, null);
        return View("~/Plugins/Marketplace.VendorExtensions/Views/VendorProduct/ProductAttributeMappingCreatePopup.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> ProductAttributeMappingCreatePopup(ProductAttributeMappingModel model, string btnId, string formId)
    {
        var vendor = await GetCurrentVendorOrDenyAsync();
        if (vendor == null)
            return AccessDeniedView();

        var product = await GetVendorProductAsync(model.ProductId, vendor.Id);
        if (product == null)
            return RedirectToAction("List");

        // Check already mapped
        if ((await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id))
            .Any(x => x.ProductAttributeId == model.ProductAttributeId))
        {
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.Attributes.AlreadyExists"));
            model = await _productModelFactory.PrepareProductAttributeMappingModelAsync(model, product, null, true);
            return View("~/Plugins/Marketplace.VendorExtensions/Views/VendorProduct/ProductAttributeMappingCreatePopup.cshtml", model);
        }

        // Determine control type: check if attribute is "color" type
        var productAttribute = await _productAttributeService.GetProductAttributeByIdAsync(model.ProductAttributeId);
        var isColor = productAttribute?.Name?.Contains("renk", StringComparison.OrdinalIgnoreCase) == true ||
                      productAttribute?.Name?.Contains("color", StringComparison.OrdinalIgnoreCase) == true;

        var pam = new ProductAttributeMapping
        {
            ProductId = model.ProductId,
            ProductAttributeId = model.ProductAttributeId,
            TextPrompt = string.Empty,
            IsRequired = true,
            AttributeControlTypeId = isColor ? (int)AttributeControlType.ColorSquares : (int)AttributeControlType.DropdownList,
            DisplayOrder = model.DisplayOrder
        };

        await _productAttributeService.InsertProductAttributeMappingAsync(pam);

        // Predefined values
        var predefinedValues = await _productAttributeService.GetPredefinedProductAttributeValuesAsync(model.ProductAttributeId);
        foreach (var predefinedValue in predefinedValues)
        {
            var pav = new ProductAttributeValue
            {
                ProductAttributeMappingId = pam.Id,
                AttributeValueType = AttributeValueType.Simple,
                Name = predefinedValue.Name,
                PriceAdjustment = predefinedValue.PriceAdjustment,
                PriceAdjustmentUsePercentage = predefinedValue.PriceAdjustmentUsePercentage,
                WeightAdjustment = predefinedValue.WeightAdjustment,
                Cost = predefinedValue.Cost,
                IsPreSelected = predefinedValue.IsPreSelected,
                DisplayOrder = predefinedValue.DisplayOrder
            };
            await _productAttributeService.InsertProductAttributeValueAsync(pav);

            var languages = await _languageService.GetAllLanguagesAsync(true);
            foreach (var lang in languages)
            {
                var name = await _localizationService.GetLocalizedAsync(predefinedValue, x => x.Name, lang.Id, false, false);
                if (!string.IsNullOrEmpty(name))
                    await _localizedEntityService.SaveLocalizedValueAsync(pav, x => x.Name, name, lang.Id);
            }
        }

        ViewBag.RefreshPage = true;
        return View("~/Plugins/Marketplace.VendorExtensions/Views/VendorProduct/ProductAttributeMappingCreatePopup.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> ProductAttributeMappingDelete(int id)
    {
        var vendor = await GetCurrentVendorOrDenyAsync();
        if (vendor == null)
            return Content("Access denied");

        var pam = await _productAttributeService.GetProductAttributeMappingByIdAsync(id);
        if (pam == null)
            return Content("Not found");

        var product = await GetVendorProductAsync(pam.ProductId, vendor.Id);
        if (product == null)
            return Content("This is not your product");

        await _productAttributeService.DeleteProductAttributeMappingAsync(pam);

        return new NullJsonResult();
    }

    [HttpPost]
    public async Task<IActionResult> ProductAttributeValueList(ProductAttributeValueSearchModel searchModel)
    {
        var vendor = await GetCurrentVendorOrDenyAsync();
        if (vendor == null)
            return Content("Access denied");

        var pam = await _productAttributeService.GetProductAttributeMappingByIdAsync(searchModel.ProductAttributeMappingId);
        if (pam == null)
            return Content("Not found");

        var product = await GetVendorProductAsync(pam.ProductId, vendor.Id);
        if (product == null)
            return Content("This is not your product");

        var model = await _productModelFactory.PrepareProductAttributeValueListModelAsync(searchModel, pam);
        return Json(model);
    }

    public async Task<IActionResult> ProductAttributeValuesPopup(int productAttributeMappingId, string btnId, string formId)
    {
        var vendor = await GetCurrentVendorOrDenyAsync();
        if (vendor == null)
            return AccessDeniedView();

        var pam = await _productAttributeService.GetProductAttributeMappingByIdAsync(productAttributeMappingId);
        if (pam == null)
            return RedirectToAction("List");

        var product = await GetVendorProductAsync(pam.ProductId, vendor.Id);
        if (product == null)
            return RedirectToAction("List");

        var model = await _productModelFactory.PrepareProductAttributeMappingModelAsync(null, product, pam);
        return View("~/Plugins/Marketplace.VendorExtensions/Views/VendorProduct/ProductAttributeValuesPopup.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> ProductAttributeValueCreateInline(int productAttributeMappingId, string name, string colorSquaresRgb, decimal priceAdjustment, int quantity, int displayOrder)
    {
        var vendor = await GetCurrentVendorOrDenyAsync();
        if (vendor == null)
            return Json(new { Result = false });

        var pam = await _productAttributeService.GetProductAttributeMappingByIdAsync(productAttributeMappingId);
        if (pam == null)
            return Json(new { Result = false });

        var product = await GetVendorProductAsync(pam.ProductId, vendor.Id);
        if (product == null)
            return Json(new { Result = false });

        var pav = new ProductAttributeValue
        {
            ProductAttributeMappingId = pam.Id,
            AttributeValueType = AttributeValueType.Simple,
            Name = name,
            ColorSquaresRgb = colorSquaresRgb,
            PriceAdjustment = priceAdjustment,
            PriceAdjustmentUsePercentage = false,
            WeightAdjustment = 0,
            Cost = 0,
            CustomerEntersQty = false,
            Quantity = quantity,
            IsPreSelected = false,
            DisplayOrder = displayOrder
        };

        await _productAttributeService.InsertProductAttributeValueAsync(pav);

        return Json(new { Result = true });
    }

    [HttpPost]
    public async Task<IActionResult> ProductAttributeValueUpdateInline(ProductAttributeValueModel model)
    {
        var vendor = await GetCurrentVendorOrDenyAsync();
        if (vendor == null)
            return Json(new { Result = false });

        var pav = await _productAttributeService.GetProductAttributeValueByIdAsync(model.Id);
        if (pav == null)
            return Json(new { Result = false });

        var pam = await _productAttributeService.GetProductAttributeMappingByIdAsync(pav.ProductAttributeMappingId);
        if (pam == null)
            return Json(new { Result = false });

        var product = await GetVendorProductAsync(pam.ProductId, vendor.Id);
        if (product == null)
            return Json(new { Result = false });

        pav.Name = model.Name;
        pav.ColorSquaresRgb = model.ColorSquaresRgb;
        pav.PriceAdjustment = model.PriceAdjustment;
        pav.Quantity = model.Quantity;
        pav.DisplayOrder = model.DisplayOrder;

        await _productAttributeService.UpdateProductAttributeValueAsync(pav);

        return new NullJsonResult();
    }

    public async Task<IActionResult> ProductAttributeValueCreate(int productAttributeMappingId)
    {
        var vendor = await GetCurrentVendorOrDenyAsync();
        if (vendor == null)
            return AccessDeniedView();

        var pam = await _productAttributeService.GetProductAttributeMappingByIdAsync(productAttributeMappingId);
        if (pam == null)
            return RedirectToAction("List");

        var product = await GetVendorProductAsync(pam.ProductId, vendor.Id);
        if (product == null)
            return RedirectToAction("List");

        var model = await _productModelFactory.PrepareProductAttributeValueModelAsync(new ProductAttributeValueModel(), pam, null);
        return View("~/Plugins/Marketplace.VendorExtensions/Views/VendorProduct/ProductAttributeValueCreate.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> ProductAttributeValueCreate(ProductAttributeValueModel model)
    {
        var vendor = await GetCurrentVendorOrDenyAsync();
        if (vendor == null)
            return AccessDeniedView();

        var pam = await _productAttributeService.GetProductAttributeMappingByIdAsync(model.ProductAttributeMappingId);
        if (pam == null)
            return RedirectToAction("List");

        var product = await GetVendorProductAsync(pam.ProductId, vendor.Id);
        if (product == null)
            return RedirectToAction("List");

        var pav = new ProductAttributeValue
        {
            ProductAttributeMappingId = pam.Id,
            AttributeValueType = AttributeValueType.Simple,
            Name = model.Name,
            ColorSquaresRgb = model.ColorSquaresRgb,
            PriceAdjustment = model.PriceAdjustment,
            PriceAdjustmentUsePercentage = model.PriceAdjustmentUsePercentage,
            WeightAdjustment = model.WeightAdjustment,
            Cost = model.Cost,
            CustomerEntersQty = model.CustomerEntersQty,
            Quantity = model.Quantity,
            IsPreSelected = model.IsPreSelected,
            DisplayOrder = model.DisplayOrder
        };

        await _productAttributeService.InsertProductAttributeValueAsync(pav);

        // Locales
        if (model.Locales != null)
        {
            foreach (var localized in model.Locales)
            {
                await _localizedEntityService.SaveLocalizedValueAsync(pav, x => x.Name, localized.Name, localized.LanguageId);
            }
        }

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.Attributes.Values.Added"));
        return RedirectToAction("Edit", new { id = product.Id });
    }

    public async Task<IActionResult> ProductAttributeValueEdit(int id)
    {
        var vendor = await GetCurrentVendorOrDenyAsync();
        if (vendor == null)
            return AccessDeniedView();

        var pav = await _productAttributeService.GetProductAttributeValueByIdAsync(id);
        if (pav == null)
            return RedirectToAction("List");

        var pam = await _productAttributeService.GetProductAttributeMappingByIdAsync(pav.ProductAttributeMappingId);
        if (pam == null)
            return RedirectToAction("List");

        var product = await GetVendorProductAsync(pam.ProductId, vendor.Id);
        if (product == null)
            return RedirectToAction("List");

        var model = await _productModelFactory.PrepareProductAttributeValueModelAsync(null, pam, pav);
        return View("~/Plugins/Marketplace.VendorExtensions/Views/VendorProduct/ProductAttributeValueEdit.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> ProductAttributeValueEdit(ProductAttributeValueModel model)
    {
        var vendor = await GetCurrentVendorOrDenyAsync();
        if (vendor == null)
            return AccessDeniedView();

        var pav = await _productAttributeService.GetProductAttributeValueByIdAsync(model.Id);
        if (pav == null)
            return RedirectToAction("List");

        var pam = await _productAttributeService.GetProductAttributeMappingByIdAsync(pav.ProductAttributeMappingId);
        if (pam == null)
            return RedirectToAction("List");

        var product = await GetVendorProductAsync(pam.ProductId, vendor.Id);
        if (product == null)
            return RedirectToAction("List");

        pav.Name = model.Name;
        pav.ColorSquaresRgb = model.ColorSquaresRgb;
        pav.PriceAdjustment = model.PriceAdjustment;
        pav.PriceAdjustmentUsePercentage = model.PriceAdjustmentUsePercentage;
        pav.WeightAdjustment = model.WeightAdjustment;
        pav.Cost = model.Cost;
        pav.CustomerEntersQty = model.CustomerEntersQty;
        pav.Quantity = model.Quantity;
        pav.IsPreSelected = model.IsPreSelected;
        pav.DisplayOrder = model.DisplayOrder;

        await _productAttributeService.UpdateProductAttributeValueAsync(pav);

        if (model.Locales != null)
        {
            foreach (var localized in model.Locales)
            {
                await _localizedEntityService.SaveLocalizedValueAsync(pav, x => x.Name, localized.Name, localized.LanguageId);
            }
        }

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.Attributes.Values.Updated"));
        return RedirectToAction("Edit", new { id = product.Id });
    }

    [HttpPost]
    public async Task<IActionResult> ProductAttributeValueDelete(int id)
    {
        var vendor = await GetCurrentVendorOrDenyAsync();
        if (vendor == null)
            return Content("Access denied");

        var pav = await _productAttributeService.GetProductAttributeValueByIdAsync(id);
        if (pav == null)
            return Content("Not found");

        var pam = await _productAttributeService.GetProductAttributeMappingByIdAsync(pav.ProductAttributeMappingId);
        if (pam == null)
            return Content("Not found");

        var product = await GetVendorProductAsync(pam.ProductId, vendor.Id);
        if (product == null)
            return Content("This is not your product");

        await _productAttributeService.DeleteProductAttributeValueAsync(pav);

        return new NullJsonResult();
    }

    #endregion

    #region Attribute Combinations

    [HttpPost]
    public async Task<IActionResult> ProductAttributeCombinationList(ProductAttributeCombinationSearchModel searchModel)
    {
        var vendor = await GetCurrentVendorOrDenyAsync();
        if (vendor == null)
            return Content("Access denied");

        var product = await GetVendorProductAsync(searchModel.ProductId, vendor.Id);
        if (product == null)
            return Content("This is not your product");

        var model = await _productModelFactory.PrepareProductAttributeCombinationListModelAsync(searchModel, product);
        return Json(model);
    }

    public async Task<IActionResult> ProductAttributeCombinationCreate(int productId)
    {
        var vendor = await GetCurrentVendorOrDenyAsync();
        if (vendor == null)
            return AccessDeniedView();

        var product = await GetVendorProductAsync(productId, vendor.Id);
        if (product == null)
            return RedirectToAction("List");

        var model = await _productModelFactory.PrepareProductAttributeCombinationModelAsync(new ProductAttributeCombinationModel(), product, null);
        return View("~/Plugins/Marketplace.VendorExtensions/Views/VendorProduct/ProductAttributeCombinationCreate.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> ProductAttributeCombinationCreate(int productId, ProductAttributeCombinationModel model, IFormCollection form)
    {
        var vendor = await GetCurrentVendorOrDenyAsync();
        if (vendor == null)
            return AccessDeniedView();

        var product = await GetVendorProductAsync(productId, vendor.Id);
        if (product == null)
            return RedirectToAction("List");

        var attributesXml = await GetAttributesXmlForCombinationAsync(form, product);

        var combination = new ProductAttributeCombination
        {
            ProductId = product.Id,
            AttributesXml = attributesXml,
            StockQuantity = model.StockQuantity,
            AllowOutOfStockOrders = false,
            Sku = model.Sku,
            ManufacturerPartNumber = model.ManufacturerPartNumber,
            Gtin = model.Gtin,
            OverriddenPrice = model.OverriddenPrice,
            NotifyAdminForQuantityBelow = 1,
            MinStockQuantity = model.MinStockQuantity
        };

        await _productAttributeService.InsertProductAttributeCombinationAsync(combination);

        // Stock history
        if (combination.StockQuantity > 0)
        {
            await _productService.AddStockQuantityHistoryEntryAsync(product, combination.StockQuantity, combination.StockQuantity,
                product.WarehouseId, await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.Edit"),
                combination.Id);
        }

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.AttributeCombinations.Added"));
        return RedirectToAction("Edit", new { id = product.Id });
    }

    public async Task<IActionResult> ProductAttributeCombinationCreatePopup(int productId, string btnId, string formId)
    {
        var vendor = await GetCurrentVendorOrDenyAsync();
        if (vendor == null)
            return AccessDeniedView();

        var product = await GetVendorProductAsync(productId, vendor.Id);
        if (product == null)
            return RedirectToAction("List");

        var model = await _productModelFactory.PrepareProductAttributeCombinationModelAsync(new ProductAttributeCombinationModel(), product, null);
        return View("~/Plugins/Marketplace.VendorExtensions/Views/VendorProduct/ProductAttributeCombinationCreatePopup.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> ProductAttributeCombinationCreatePopup(int productId, ProductAttributeCombinationModel model, IFormCollection form, string btnId, string formId)
    {
        var vendor = await GetCurrentVendorOrDenyAsync();
        if (vendor == null)
            return AccessDeniedView();

        var product = await GetVendorProductAsync(productId, vendor.Id);
        if (product == null)
            return RedirectToAction("List");

        var attributesXml = await GetAttributesXmlForCombinationAsync(form, product);

        var combination = new ProductAttributeCombination
        {
            ProductId = product.Id,
            AttributesXml = attributesXml,
            StockQuantity = model.StockQuantity,
            AllowOutOfStockOrders = false,
            Sku = model.Sku,
            ManufacturerPartNumber = model.ManufacturerPartNumber,
            Gtin = model.Gtin,
            OverriddenPrice = model.OverriddenPrice,
            NotifyAdminForQuantityBelow = 1,
            MinStockQuantity = model.MinStockQuantity
        };

        await _productAttributeService.InsertProductAttributeCombinationAsync(combination);

        // Save picture associations
        if (model.PictureIds != null && model.PictureIds.Any())
        {
            var productPictureIds = (await _pictureService.GetPicturesByProductIdAsync(product.Id)).Select(p => p.Id).ToList();
            foreach (var pictureId in model.PictureIds)
            {
                if (!productPictureIds.Contains(pictureId))
                    continue;

                await _productAttributeService.InsertProductAttributeCombinationPictureAsync(new ProductAttributeCombinationPicture
                {
                    ProductAttributeCombinationId = combination.Id,
                    PictureId = pictureId
                });
            }
        }

        // Stock history
        if (combination.StockQuantity > 0)
        {
            await _productService.AddStockQuantityHistoryEntryAsync(product, combination.StockQuantity, combination.StockQuantity,
                product.WarehouseId, await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.Edit"),
                combination.Id);
        }

        ViewBag.RefreshPage = true;
        return View("~/Plugins/Marketplace.VendorExtensions/Views/VendorProduct/ProductAttributeCombinationCreatePopup.cshtml", model);
    }

    private async Task<string> GetAttributesXmlForCombinationAsync(IFormCollection form, Product product)
    {
        var attributesXml = string.Empty;
        var productAttributes = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id);

        foreach (var attribute in productAttributes)
        {
            var controlId = $"product_attribute_{attribute.Id}";
            var ctrlAttributes = form[controlId];

            if (!Microsoft.Extensions.Primitives.StringValues.IsNullOrEmpty(ctrlAttributes))
            {
                foreach (var ctrlAttribute in ctrlAttributes.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    if (int.TryParse(ctrlAttribute, out var selectedAttributeId))
                    {
                        attributesXml = _productAttributeParser.AddProductAttribute(attributesXml, attribute, selectedAttributeId.ToString());
                    }
                }
            }
        }

        return attributesXml;
    }

    [HttpPost]
    public async Task<IActionResult> ProductAttributeCombinationDelete(int id)
    {
        var vendor = await GetCurrentVendorOrDenyAsync();
        if (vendor == null)
            return Content("Access denied");

        var combination = await _productAttributeService.GetProductAttributeCombinationByIdAsync(id);
        if (combination == null)
            return Content("Not found");

        var product = await GetVendorProductAsync(combination.ProductId, vendor.Id);
        if (product == null)
            return Content("This is not your product");

        await _productAttributeService.DeleteProductAttributeCombinationAsync(combination);

        return new NullJsonResult();
    }

    #endregion

    #region Specification Attributes

    [HttpPost]
    public async Task<IActionResult> ProductSpecAttrList(ProductSpecificationAttributeSearchModel searchModel)
    {
        var vendor = await GetCurrentVendorOrDenyAsync();
        if (vendor == null)
            return Content("Access denied");

        var product = await GetVendorProductAsync(searchModel.ProductId, vendor.Id);
        if (product == null)
            return Content("This is not your product");

        var model = await _productModelFactory.PrepareProductSpecificationAttributeListModelAsync(searchModel, product);
        return Json(model);
    }

    public async Task<IActionResult> ProductSpecAttrAddOrEdit(int productId, int? specificationId)
    {
        var vendor = await GetCurrentVendorOrDenyAsync();
        if (vendor == null)
            return AccessDeniedView();

        var product = await GetVendorProductAsync(productId, vendor.Id);
        if (product == null)
            return RedirectToAction("List");

        var model = await _productModelFactory.PrepareAddSpecificationAttributeModelAsync(productId, specificationId);
        return View("~/Plugins/Marketplace.VendorExtensions/Views/VendorProduct/ProductSpecAttrAddOrEdit.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> ProductSpecificationAttributeAdd(AddSpecificationAttributeModel model)
    {
        var vendor = await GetCurrentVendorOrDenyAsync();
        if (vendor == null)
            return AccessDeniedView();

        var product = await GetVendorProductAsync(model.ProductId, vendor.Id);
        if (product == null)
            return RedirectToAction("List");

        // Force defaults
        var psa = new ProductSpecificationAttribute
        {
            ProductId = model.ProductId,
            AttributeTypeId = (int)SpecificationAttributeType.Option,
            SpecificationAttributeOptionId = model.SpecificationAttributeOptionId,
            CustomValue = model.Value,
            AllowFiltering = true,
            ShowOnProductPage = true,
            DisplayOrder = model.DisplayOrder
        };

        await _specificationAttributeService.InsertProductSpecificationAttributeAsync(psa);

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.SpecificationAttributes.Added"));
        return RedirectToAction("Edit", new { id = product.Id });
    }

    [HttpPost]
    public async Task<IActionResult> ProductSpecAttrUpdate(AddSpecificationAttributeModel model)
    {
        var vendor = await GetCurrentVendorOrDenyAsync();
        if (vendor == null)
            return AccessDeniedView();

        var product = await GetVendorProductAsync(model.ProductId, vendor.Id);
        if (product == null)
            return RedirectToAction("List");

        var psa = await _specificationAttributeService.GetProductSpecificationAttributeByIdAsync(model.SpecificationId);
        if (psa == null || psa.ProductId != product.Id)
            return RedirectToAction("Edit", new { id = product.Id });

        psa.SpecificationAttributeOptionId = model.SpecificationAttributeOptionId;
        psa.CustomValue = model.Value;
        psa.DisplayOrder = model.DisplayOrder;
        // Keep forced defaults
        psa.AllowFiltering = true;
        psa.ShowOnProductPage = true;
        psa.AttributeTypeId = (int)SpecificationAttributeType.Option;

        await _specificationAttributeService.UpdateProductSpecificationAttributeAsync(psa);

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.SpecificationAttributes.Updated"));
        return RedirectToAction("Edit", new { id = product.Id });
    }

    public async Task<IActionResult> ProductSpecAttrAddOrEditPopup(int productId, int? specificationId, string btnId, string formId)
    {
        var vendor = await GetCurrentVendorOrDenyAsync();
        if (vendor == null)
            return AccessDeniedView();

        var product = await GetVendorProductAsync(productId, vendor.Id);
        if (product == null)
            return RedirectToAction("List");

        var model = await _productModelFactory.PrepareAddSpecificationAttributeModelAsync(productId, specificationId);
        return View("~/Plugins/Marketplace.VendorExtensions/Views/VendorProduct/ProductSpecAttrAddOrEditPopup.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> ProductSpecificationAttributeAddPopup(AddSpecificationAttributeModel model, string btnId, string formId)
    {
        var vendor = await GetCurrentVendorOrDenyAsync();
        if (vendor == null)
            return AccessDeniedView();

        var product = await GetVendorProductAsync(model.ProductId, vendor.Id);
        if (product == null)
            return RedirectToAction("List");

        var psa = new ProductSpecificationAttribute
        {
            ProductId = model.ProductId,
            AttributeTypeId = (int)SpecificationAttributeType.Option,
            SpecificationAttributeOptionId = model.SpecificationAttributeOptionId,
            CustomValue = model.Value,
            AllowFiltering = true,
            ShowOnProductPage = true,
            DisplayOrder = model.DisplayOrder
        };

        await _specificationAttributeService.InsertProductSpecificationAttributeAsync(psa);

        ViewBag.RefreshPage = true;
        return View("~/Plugins/Marketplace.VendorExtensions/Views/VendorProduct/ProductSpecAttrAddOrEditPopup.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> ProductSpecAttrUpdatePopup(AddSpecificationAttributeModel model, string btnId, string formId)
    {
        var vendor = await GetCurrentVendorOrDenyAsync();
        if (vendor == null)
            return AccessDeniedView();

        var product = await GetVendorProductAsync(model.ProductId, vendor.Id);
        if (product == null)
            return RedirectToAction("List");

        var psa = await _specificationAttributeService.GetProductSpecificationAttributeByIdAsync(model.SpecificationId);
        if (psa == null || psa.ProductId != product.Id)
        {
            ViewBag.RefreshPage = true;
            return View("~/Plugins/Marketplace.VendorExtensions/Views/VendorProduct/ProductSpecAttrAddOrEditPopup.cshtml", model);
        }

        psa.SpecificationAttributeOptionId = model.SpecificationAttributeOptionId;
        psa.CustomValue = model.Value;
        psa.DisplayOrder = model.DisplayOrder;
        psa.AllowFiltering = true;
        psa.ShowOnProductPage = true;
        psa.AttributeTypeId = (int)SpecificationAttributeType.Option;

        await _specificationAttributeService.UpdateProductSpecificationAttributeAsync(psa);

        ViewBag.RefreshPage = true;
        return View("~/Plugins/Marketplace.VendorExtensions/Views/VendorProduct/ProductSpecAttrAddOrEditPopup.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> ProductSpecAttrDelete(int id, int productId)
    {
        var vendor = await GetCurrentVendorOrDenyAsync();
        if (vendor == null)
            return Content("Access denied");

        var product = await GetVendorProductAsync(productId, vendor.Id);
        if (product == null)
            return Content("This is not your product");

        var psa = await _specificationAttributeService.GetProductSpecificationAttributeByIdAsync(id);
        if (psa == null || psa.ProductId != product.Id)
            return Content("Not found");

        await _specificationAttributeService.DeleteProductSpecificationAttributeAsync(psa);

        return new NullJsonResult();
    }

    #endregion

    #region Tier Prices

    [HttpPost]
    public async Task<IActionResult> TierPriceList(TierPriceSearchModel searchModel)
    {
        var vendor = await GetCurrentVendorOrDenyAsync();
        if (vendor == null)
            return Content("Access denied");

        var product = await GetVendorProductAsync(searchModel.ProductId, vendor.Id);
        if (product == null)
            return Content("This is not your product");

        var model = await _productModelFactory.PrepareTierPriceListModelAsync(searchModel, product);
        return Json(model);
    }

    [HttpPost]
    public async Task<IActionResult> TierPriceCreate(TierPriceModel model)
    {
        var vendor = await GetCurrentVendorOrDenyAsync();
        if (vendor == null)
            return Json(new { Result = false });

        var product = await GetVendorProductAsync(model.ProductId, vendor.Id);
        if (product == null)
            return Json(new { Result = false });

        var tierPrice = new TierPrice
        {
            ProductId = product.Id,
            StoreId = model.StoreId,
            CustomerRoleId = model.CustomerRoleId > 0 ? model.CustomerRoleId : (int?)null,
            Quantity = model.Quantity,
            Price = model.Price,
            StartDateTimeUtc = model.StartDateTimeUtc,
            EndDateTimeUtc = model.EndDateTimeUtc
        };

        await _productService.InsertTierPriceAsync(tierPrice);
        await _productService.UpdateProductAsync(product);

        return Json(new { Result = true });
    }

    [HttpPost]
    public async Task<IActionResult> TierPriceEdit(TierPriceModel model)
    {
        var vendor = await GetCurrentVendorOrDenyAsync();
        if (vendor == null)
            return Json(new { Result = false });

        var product = await GetVendorProductAsync(model.ProductId, vendor.Id);
        if (product == null)
            return Json(new { Result = false });

        var tierPrice = await _productService.GetTierPriceByIdAsync(model.Id);
        if (tierPrice == null || tierPrice.ProductId != product.Id)
            return Json(new { Result = false });

        tierPrice.StoreId = model.StoreId;
        tierPrice.CustomerRoleId = model.CustomerRoleId > 0 ? model.CustomerRoleId : (int?)null;
        tierPrice.Quantity = model.Quantity;
        tierPrice.Price = model.Price;
        tierPrice.StartDateTimeUtc = model.StartDateTimeUtc;
        tierPrice.EndDateTimeUtc = model.EndDateTimeUtc;

        await _productService.UpdateTierPriceAsync(tierPrice);
        await _productService.UpdateProductAsync(product);

        return Json(new { Result = true });
    }

    [HttpPost]
    public async Task<IActionResult> TierPriceDelete(int id)
    {
        var vendor = await GetCurrentVendorOrDenyAsync();
        if (vendor == null)
            return Content("Access denied");

        var tierPrice = await _productService.GetTierPriceByIdAsync(id);
        if (tierPrice == null)
            return Content("Not found");

        var product = await GetVendorProductAsync(tierPrice.ProductId, vendor.Id);
        if (product == null)
            return Content("This is not your product");

        await _productService.DeleteTierPriceAsync(tierPrice);
        await _productService.UpdateProductAsync(product);

        return new NullJsonResult();
    }

    #endregion
}
