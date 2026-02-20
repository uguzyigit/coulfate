using System.Text.Json;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Seo;
using Nop.Data;
using Nop.Plugin.Integration.TrendyolMarketplace.Domain;
using Nop.Plugin.Integration.TrendyolMarketplace.Infrastructure;
using Nop.Plugin.Integration.TrendyolMarketplace.Services.Api;
using Nop.Plugin.Integration.TrendyolMarketplace.Services.Api.Models;
using Nop.Plugin.Integration.TrendyolMarketplace.Services.Mapping;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Services.Seo;

namespace Nop.Plugin.Integration.TrendyolMarketplace.Services.Import;

/// <summary>
/// Product import service implementation
/// </summary>
public class ProductImportService : IProductImportService
{
    private readonly IRepository<TrendyolProduct> _productRepository;
    private readonly ITrendyolApiClient _apiClient;
    private readonly ICategoryMappingService _categoryMappingService;
    private readonly IBrandMappingService _brandMappingService;
    private readonly IImageImportService _imageImportService;
    private readonly IVariantService _variantService;
    private readonly ISpecificationAttributeImportService _specificationAttributeImportService;
    private readonly ISyncLogService _syncLogService;
    private readonly IProductService _nopProductService;
    private readonly IProductAttributeService _productAttributeService;
    private readonly ICategoryService _categoryService;
    private readonly IManufacturerService _manufacturerService;
    private readonly IUrlRecordService _urlRecordService;
    private readonly ISettingService _settingService;
    private readonly ILogger _logger;

    public ProductImportService(
        IRepository<TrendyolProduct> productRepository,
        ITrendyolApiClient apiClient,
        ICategoryMappingService categoryMappingService,
        IBrandMappingService brandMappingService,
        IImageImportService imageImportService,
        IVariantService variantService,
        ISpecificationAttributeImportService specificationAttributeImportService,
        ISyncLogService syncLogService,
        IProductService nopProductService,
        IProductAttributeService productAttributeService,
        ICategoryService categoryService,
        IManufacturerService manufacturerService,
        IUrlRecordService urlRecordService,
        ISettingService settingService,
        ILogger logger)
    {
        _productRepository = productRepository;
        _apiClient = apiClient;
        _categoryMappingService = categoryMappingService;
        _brandMappingService = brandMappingService;
        _imageImportService = imageImportService;
        _variantService = variantService;
        _specificationAttributeImportService = specificationAttributeImportService;
        _syncLogService = syncLogService;
        _nopProductService = nopProductService;
        _productAttributeService = productAttributeService;
        _categoryService = categoryService;
        _manufacturerService = manufacturerService;
        _urlRecordService = urlRecordService;
        _settingService = settingService;
        _logger = logger;
    }

    /// <summary>
    /// Gets a Trendyol product mapping by ID
    /// </summary>
    public virtual async Task<TrendyolProduct> GetByIdAsync(int id)
    {
        if (id <= 0)
            return null;

        return await _productRepository.GetByIdAsync(id);
    }

    /// <summary>
    /// Gets a Trendyol product mapping by barcode and vendor ID
    /// </summary>
    public virtual async Task<TrendyolProduct> GetByBarcodeAsync(string barcode, int vendorId)
    {
        if (string.IsNullOrEmpty(barcode) || vendorId <= 0)
            return null;

        var query = from p in _productRepository.Table
                    where p.TrendyolBarcode == barcode && p.VendorId == vendorId
                    select p;

        return await query.FirstOrDefaultAsync();
    }

    /// <summary>
    /// Gets all Trendyol products for a vendor (paged)
    /// </summary>
    public virtual async Task<IPagedList<TrendyolProduct>> GetAllAsync(
        int? vendorId = null,
        ImportStatus? status = null,
        string searchTerm = null,
        int pageIndex = 0,
        int pageSize = int.MaxValue)
    {
        var query = _productRepository.Table;

        if (vendorId.HasValue)
            query = query.Where(p => p.VendorId == vendorId.Value);

        if (status.HasValue)
            query = query.Where(p => p.ImportStatus == status.Value);

        if (!string.IsNullOrEmpty(searchTerm))
        {
            searchTerm = searchTerm.ToLower();
            query = query.Where(p =>
                p.TrendyolBarcode.ToLower().Contains(searchTerm) ||
                p.TrendyolProductCode.ToLower().Contains(searchTerm) ||
                p.TrendyolTitle.ToLower().Contains(searchTerm) ||
                p.TrendyolStockCode.ToLower().Contains(searchTerm));
        }

        query = query.OrderByDescending(p => p.UpdatedOnUtc ?? p.CreatedOnUtc);

        return await query.ToPagedListAsync(pageIndex, pageSize);
    }

    /// <summary>
    /// Gets product statistics for a vendor
    /// </summary>
    public virtual async Task<(int Total, int Imported, int Pending, int Failed, int Skipped, int Deactivated)> GetStatisticsAsync(int vendorId)
    {
        var query = _productRepository.Table.Where(p => p.VendorId == vendorId);

        var total = await query.CountAsync();
        var imported = await query.CountAsync(p => p.ImportStatus == ImportStatus.Imported);
        var pending = await query.CountAsync(p => p.ImportStatus == ImportStatus.Pending);
        var failed = await query.CountAsync(p => p.ImportStatus == ImportStatus.Failed);
        var skipped = await query.CountAsync(p => p.ImportStatus == ImportStatus.Skipped);
        var deactivated = await query.CountAsync(p => p.ImportStatus == ImportStatus.Deactivated);

        return (total, imported, pending, failed, skipped, deactivated);
    }

    /// <summary>
    /// Imports products from Trendyol API for a vendor
    /// </summary>
    public virtual async Task<(int Success, int Failed, int Skipped)> ImportProductsAsync(TrendyolVendorCredential credential, int syncLogId)
    {
        var settings = await _settingService.LoadSettingAsync<TrendyolSettings>();
        var successCount = 0;
        var failedCount = 0;
        var skippedCount = 0;

        try
        {
            // Fetch only products that are on sale (approved=true&onSale=true)
            // This is much more efficient than fetching all 3000+ approved products
            var onSaleProducts = await _apiClient.GetAllProductsAsync(credential, approved: true, onSale: true);

            await _logger.InformationAsync($"[TrendyolImport] Vendor {credential.VendorId}: API returned {onSaleProducts.Count} onSale products");

            // Safety check: if API returns 0 products, skip deactivation to prevent mass-deactivation on API errors
            if (!onSaleProducts.Any())
            {
                await _logger.WarningAsync($"Trendyol API returned 0 onSale products for vendor {credential.VendorId}. Skipping import & deactivation.");
                return (0, 0, 0);
            }

            // Reactivate previously deactivated products that are back on sale
            var reactivatedCount = await ReactivateOnSaleProductsAsync(onSaleProducts, credential.VendorId, settings);
            successCount += reactivatedCount;

            // Import all onSale products (group by ProductMainId for variant handling)
            var productGroups = onSaleProducts
                .GroupBy(p => p.ProductMainId ?? p.Barcode)
                .ToList();

            var totalCount = productGroups.Count;
            await _logger.InformationAsync($"[TrendyolImport] Vendor {credential.VendorId}: {totalCount} product groups to process (reactivated: {reactivatedCount})");

            foreach (var group in productGroups)
            {
                try
                {
                    var result = await ImportProductGroupAsync(group.ToList(), credential.VendorId, settings);

                    switch (result)
                    {
                        case ImportStatus.Imported:
                            successCount++;
                            break;
                        case ImportStatus.Failed:
                            failedCount++;
                            break;
                        case ImportStatus.Skipped:
                            skippedCount++;
                            break;
                    }
                }
                catch (Exception ex)
                {
                    var innerMsg = ex.InnerException != null ? $" Inner: {ex.InnerException.Message}" : "";
                    await _logger.ErrorAsync($"[TrendyolImport] Error importing group '{group.Key}' ({group.Count()} products): {ex.Message}{innerMsg}", ex);
                    failedCount++;
                }

                // Update sync progress
                await _syncLogService.UpdateProgressAsync(
                    syncLogId, totalCount, successCount, failedCount, skippedCount);
            }

            // Deactivate previously imported products that are no longer on sale
            var deactivatedCount = await DeactivateOffSaleProductsAsync(onSaleProducts, credential.VendorId);
            if (deactivatedCount > 0)
                await _logger.InformationAsync($"Deactivated {deactivatedCount} products for vendor {credential.VendorId} (no longer in API)");
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync($"Error during product import for vendor {credential.VendorId}: {ex.Message}", ex);
            throw;
        }

        return (successCount, failedCount, skippedCount);
    }

    /// <summary>
    /// Imports a single product from Trendyol DTO
    /// </summary>
    public virtual async Task<(bool Success, string Message)> ImportSingleProductAsync(TrendyolProductDto productDto, int vendorId)
    {
        var settings = await _settingService.LoadSettingAsync<TrendyolSettings>();

        try
        {
            var result = await ImportProductGroupAsync(new List<TrendyolProductDto> { productDto }, vendorId, settings);
            return result == ImportStatus.Imported
                ? (true, "Product imported successfully")
                : (false, $"Product {(result == ImportStatus.Skipped ? "skipped" : "failed")}");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    /// <summary>
    /// Updates an existing Trendyol product mapping
    /// </summary>
    public virtual async Task UpdateAsync(TrendyolProduct product)
    {
        ArgumentNullException.ThrowIfNull(product);

        product.UpdatedOnUtc = DateTime.UtcNow;
        await _productRepository.UpdateAsync(product);
    }

    /// <summary>
    /// Deletes a Trendyol product mapping
    /// </summary>
    public virtual async Task DeleteAsync(TrendyolProduct product)
    {
        ArgumentNullException.ThrowIfNull(product);
        await _productRepository.DeleteAsync(product);
    }

    #region Private Methods

    private async Task<ImportStatus> ImportProductGroupAsync(List<TrendyolProductDto> products, int vendorId, TrendyolSettings settings)
    {
        if (!products.Any())
            return ImportStatus.Skipped;

        var mainProduct = products.First();
        var isVariantGroup = products.Count > 1;

        // Log product data for debugging null fields
        if (string.IsNullOrEmpty(mainProduct.Title))
            await _logger.WarningAsync($"[TrendyolImport] Product '{mainProduct.Barcode}' has null/empty Title. StockCode={mainProduct.StockCode}, BrandName={mainProduct.BrandName}, BrandId={mainProduct.BrandId}");

        // Check category mapping
        var nopCategoryId = await _categoryMappingService.GetNopCategoryIdAsync(mainProduct.CategoryId);
        if (!nopCategoryId.HasValue && settings.SkipUnmappedCategories)
        {
            var skipMessage = $"Category not mapped: Trendyol CategoryId={mainProduct.CategoryId}. Map this category or set SkipUnmappedCategories=false in plugin settings.";
            await _logger.WarningAsync($"[TrendyolImport] Skipping product group '{mainProduct.Title}' (barcode: {mainProduct.Barcode}): {skipMessage}");

            // Save/update products as skipped (preserve onSale from API)
            foreach (var product in products)
            {
                await SaveOrUpdateTrendyolProductAsync(product, vendorId, ImportStatus.Skipped,
                    skipMessage, null, null, trendyolOnSale: product.OnSale);
            }
            return ImportStatus.Skipped;
        }

        // Get or create manufacturer
        var nopManufacturerId = await _brandMappingService.GetOrCreateNopManufacturerIdAsync(
            mainProduct.BrandId, mainProduct.BrandName);

        try
        {
            int nopProductId;

            if (isVariantGroup)
            {
                // Import as variant product
                nopProductId = await ImportVariantProductAsync(products, vendorId, nopCategoryId, nopManufacturerId, settings);
            }
            else
            {
                // Import as simple product
                nopProductId = await ImportSimpleProductAsync(mainProduct, vendorId, nopCategoryId, nopManufacturerId, settings);
            }

            // Save/update Trendyol product mappings (preserve onSale from API)
            foreach (var product in products)
            {
                var isVariant = isVariantGroup && product != mainProduct;
                await SaveOrUpdateTrendyolProductAsync(product, vendorId, ImportStatus.Imported,
                    null, nopProductId, isVariantGroup ? nopProductId : null, trendyolOnSale: product.OnSale);
            }

            return ImportStatus.Imported;
        }
        catch (Exception ex)
        {
            var errorDetail = ex.InnerException != null ? $"{ex.Message} -> {ex.InnerException.Message}" : ex.Message;
            // Save/update products as failed (preserve onSale from API)
            foreach (var product in products)
            {
                await SaveOrUpdateTrendyolProductAsync(product, vendorId, ImportStatus.Failed,
                    errorDetail, null, null, trendyolOnSale: product.OnSale);
            }
            throw;
        }
    }

    private async Task<int> ImportSimpleProductAsync(
        TrendyolProductDto productDto,
        int vendorId,
        int? nopCategoryId,
        int? nopManufacturerId,
        TrendyolSettings settings)
    {
        // Check if product already exists
        var existingMapping = await GetByBarcodeAsync(productDto.Barcode, vendorId);
        if (existingMapping?.NopProductId > 0)
        {
            var existingProduct = await _nopProductService.GetProductByIdAsync(existingMapping.NopProductId.Value);
            if (existingProduct != null)
            {
                // Update existing product
                await UpdateNopProductAsync(existingProduct, productDto, settings);

                // Assign specification attributes (idempotent - safe for updates)
                if (settings.ImportSpecificationAttributes && productDto.Attributes?.Any() == true)
                    await _specificationAttributeImportService.AssignSpecificationAttributesAsync(existingProduct.Id, productDto);

                return existingProduct.Id;
            }
        }

        // Create new NopCommerce product
        var productName = productDto.Title ?? productDto.StockCode ?? productDto.Barcode ?? "Unnamed Product";
        var product = new Product
        {
            Name = productName,
            ShortDescription = productName,
            FullDescription = productDto.Description ?? productName,
            Sku = productDto.Barcode,
            Gtin = productDto.Barcode,
            ManufacturerPartNumber = productDto.StockCode,
            VendorId = vendorId,
            ProductTypeId = (int)ProductType.SimpleProduct,
            Price = productDto.SalePrice,
            OldPrice = productDto.ListPrice > productDto.SalePrice ? productDto.ListPrice : 0,
            StockQuantity = productDto.Quantity,
            ManageInventoryMethodId = (int)ManageInventoryMethod.ManageStock,
            OrderMinimumQuantity = 1,
            OrderMaximumQuantity = 10000,
            Published = settings.PublishImportedProducts,
            VisibleIndividually = true,
            AllowCustomerReviews = true,
            CreatedOnUtc = DateTime.UtcNow,
            UpdatedOnUtc = DateTime.UtcNow
        };

        // Set tax rate if available
        if (productDto.VatRate > 0)
            product.TaxCategoryId = await GetOrCreateTaxCategoryIdAsync(productDto.VatRate);

        // Set weight if available
        if (productDto.DimensionalWeight.HasValue)
            product.Weight = productDto.DimensionalWeight.Value;

        await _nopProductService.InsertProductAsync(product);

        // Create URL slug
        var seName = await _urlRecordService.ValidateSeNameAsync(product, null, product.Name, true);
        await _urlRecordService.SaveSlugAsync(product, seName, 0);

        // Add to category
        if (nopCategoryId.HasValue)
        {
            await _categoryService.InsertProductCategoryAsync(new ProductCategory
            {
                ProductId = product.Id,
                CategoryId = nopCategoryId.Value,
                DisplayOrder = 0
            });
        }

        // Add manufacturer
        if (nopManufacturerId.HasValue)
        {
            await _manufacturerService.InsertProductManufacturerAsync(new ProductManufacturer
            {
                ProductId = product.Id,
                ManufacturerId = nopManufacturerId.Value,
                DisplayOrder = 0
            });
        }

        // Import images
        if (settings.DownloadProductImages && productDto.Images?.Any() == true)
        {
            await _imageImportService.ImportImagesAsync(product.Id, productDto.Images.Select(i => i.Url).ToList());
        }

        // Assign specification attributes
        if (settings.ImportSpecificationAttributes && productDto.Attributes?.Any() == true)
            await _specificationAttributeImportService.AssignSpecificationAttributesAsync(product.Id, productDto);

        return product.Id;
    }

    private async Task<int> ImportVariantProductAsync(
        List<TrendyolProductDto> products,
        int vendorId,
        int? nopCategoryId,
        int? nopManufacturerId,
        TrendyolSettings settings)
    {
        var mainProduct = products.First();

        // Check if parent product already exists
        var existingMapping = await GetByBarcodeAsync(mainProduct.Barcode, vendorId);
        Product parentProduct;

        if (existingMapping?.NopProductId > 0)
        {
            parentProduct = await _nopProductService.GetProductByIdAsync(existingMapping.NopProductId.Value);
            if (parentProduct != null)
            {
                // Update existing product and variants
                await UpdateNopProductAsync(parentProduct, mainProduct, settings);
                await _variantService.UpdateVariantsAsync(parentProduct.Id, products);

                // Assign specification attributes (idempotent - safe for updates)
                if (settings.ImportSpecificationAttributes && mainProduct.Attributes?.Any() == true)
                    await _specificationAttributeImportService.AssignSpecificationAttributesAsync(parentProduct.Id, mainProduct);

                return parentProduct.Id;
            }
        }

        // Create parent product (with the first variant's info)
        var parentName = mainProduct.Title ?? mainProduct.StockCode ?? mainProduct.Barcode ?? "Unnamed Product";
        parentProduct = new Product
        {
            Name = parentName,
            ShortDescription = parentName,
            FullDescription = mainProduct.Description ?? parentName,
            Sku = mainProduct.ProductMainId ?? mainProduct.Barcode,
            ManufacturerPartNumber = mainProduct.StockCode,
            VendorId = vendorId,
            ProductTypeId = (int)ProductType.SimpleProduct,
            Price = products.Min(p => p.SalePrice),
            OldPrice = products.Max(p => p.ListPrice),
            StockQuantity = 0, // Managed by combinations
            ManageInventoryMethodId = (int)ManageInventoryMethod.ManageStockByAttributes,
            OrderMinimumQuantity = 1,
            OrderMaximumQuantity = 10000,
            Published = settings.PublishImportedProducts,
            VisibleIndividually = true,
            AllowCustomerReviews = true,
            CreatedOnUtc = DateTime.UtcNow,
            UpdatedOnUtc = DateTime.UtcNow
        };

        if (mainProduct.VatRate > 0)
            parentProduct.TaxCategoryId = await GetOrCreateTaxCategoryIdAsync(mainProduct.VatRate);

        if (mainProduct.DimensionalWeight.HasValue)
            parentProduct.Weight = mainProduct.DimensionalWeight.Value;

        await _nopProductService.InsertProductAsync(parentProduct);

        // Create URL slug
        var seName = await _urlRecordService.ValidateSeNameAsync(parentProduct, null, parentProduct.Name, true);
        await _urlRecordService.SaveSlugAsync(parentProduct, seName, 0);

        // Add to category
        if (nopCategoryId.HasValue)
        {
            await _categoryService.InsertProductCategoryAsync(new ProductCategory
            {
                ProductId = parentProduct.Id,
                CategoryId = nopCategoryId.Value,
                DisplayOrder = 0
            });
        }

        // Add manufacturer
        if (nopManufacturerId.HasValue)
        {
            await _manufacturerService.InsertProductManufacturerAsync(new ProductManufacturer
            {
                ProductId = parentProduct.Id,
                ManufacturerId = nopManufacturerId.Value,
                DisplayOrder = 0
            });
        }

        // Import images from all variants
        if (settings.DownloadProductImages)
        {
            var allImageUrls = products
                .Where(p => p.Images != null)
                .SelectMany(p => p.Images.Select(i => i.Url))
                .Distinct()
                .ToList();

            if (allImageUrls.Any())
            {
                await _imageImportService.ImportImagesAsync(parentProduct.Id, allImageUrls);
            }
        }

        // Create product attribute combinations
        await _variantService.CreateVariantsAsync(parentProduct.Id, products);

        // Assign specification attributes (from mainProduct, shared across all variants)
        if (settings.ImportSpecificationAttributes && mainProduct.Attributes?.Any() == true)
            await _specificationAttributeImportService.AssignSpecificationAttributesAsync(parentProduct.Id, mainProduct);

        return parentProduct.Id;
    }

    private async Task UpdateNopProductAsync(Product product, TrendyolProductDto productDto, TrendyolSettings settings)
    {
        // Update basic fields
        product.Price = productDto.SalePrice;
        product.OldPrice = productDto.ListPrice > productDto.SalePrice ? productDto.ListPrice : 0;
        product.StockQuantity = productDto.Quantity;
        product.UpdatedOnUtc = DateTime.UtcNow;

        await _nopProductService.UpdateProductAsync(product);
    }

    private async Task SaveOrUpdateTrendyolProductAsync(
        TrendyolProductDto productDto,
        int vendorId,
        ImportStatus status,
        string message,
        int? nopProductId,
        int? nopParentProductId,
        bool trendyolOnSale = true)
    {
        var existing = await GetByBarcodeAsync(productDto.Barcode, vendorId);

        if (existing == null)
        {
            var newProduct = new TrendyolProduct
            {
                TrendyolBarcode = productDto.Barcode,
                TrendyolProductCode = productDto.ProductMainId,
                TrendyolStockCode = productDto.StockCode,
                TrendyolTitle = productDto.Title ?? productDto.StockCode ?? productDto.Barcode,
                TrendyolCategoryId = productDto.CategoryId,
                TrendyolBrandId = productDto.BrandId,
                NopProductId = nopProductId,
                NopParentProductId = nopParentProductId,
                VendorId = vendorId,
                IsVariant = nopParentProductId.HasValue,
                LastTrendyolPrice = productDto.SalePrice,
                LastTrendyolStock = productDto.Quantity,
                LastSyncOnUtc = DateTime.UtcNow,
                ImportStatus = status,
                ImportMessage = message,
                TrendyolJsonData = JsonSerializer.Serialize(productDto),
                TrendyolOnSale = trendyolOnSale,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };

            await _productRepository.InsertAsync(newProduct);
        }
        else
        {
            existing.TrendyolProductCode = productDto.ProductMainId;
            existing.TrendyolStockCode = productDto.StockCode;
            existing.TrendyolTitle = productDto.Title ?? productDto.StockCode ?? productDto.Barcode;
            existing.TrendyolCategoryId = productDto.CategoryId;
            existing.TrendyolBrandId = productDto.BrandId;
            existing.NopProductId = nopProductId ?? existing.NopProductId;
            existing.NopParentProductId = nopParentProductId ?? existing.NopParentProductId;
            existing.IsVariant = nopParentProductId.HasValue || existing.IsVariant;
            existing.LastTrendyolPrice = productDto.SalePrice;
            existing.LastTrendyolStock = productDto.Quantity;
            existing.LastSyncOnUtc = DateTime.UtcNow;
            existing.ImportStatus = status;
            existing.ImportMessage = message;
            existing.TrendyolJsonData = JsonSerializer.Serialize(productDto);
            existing.TrendyolOnSale = trendyolOnSale;
            existing.UpdatedOnUtc = DateTime.UtcNow;

            await _productRepository.UpdateAsync(existing);
        }
    }

    /// <summary>
    /// Deactivates products that were previously imported but are no longer on sale.
    /// Compares imported products against the current onSale=true API list.
    /// Any imported product not in the onSale list gets deactivated (unpublished + stock zeroed).
    /// </summary>
    private async Task<int> DeactivateOffSaleProductsAsync(List<TrendyolProductDto> allApiProducts, int vendorId)
    {
        var deactivatedCount = 0;

        // Build a set of all barcodes currently in the API response
        var apiBarcodes = new HashSet<string>(allApiProducts.Select(p => p.Barcode));

        // Get all imported products for this vendor
        var importedProducts = await (from p in _productRepository.Table
                                      where p.VendorId == vendorId &&
                                            p.ImportStatus == ImportStatus.Imported &&
                                            p.NopProductId.HasValue
                                      select p).ToListAsync();

        foreach (var product in importedProducts)
        {
            // Only deactivate if product completely disappeared from API
            if (apiBarcodes.Contains(product.TrendyolBarcode))
                continue;

            product.ImportMessage = "Product no longer found in Trendyol API";

            // For variant products: only deactivate if ALL variants in the group disappeared
            if (product.NopParentProductId.HasValue || product.IsVariant)
            {
                var groupCode = product.TrendyolProductCode;
                if (!string.IsNullOrEmpty(groupCode))
                {
                    var siblingVariants = await (from p in _productRepository.Table
                                                 where p.VendorId == vendorId &&
                                                       p.TrendyolProductCode == groupCode &&
                                                       p.ImportStatus == ImportStatus.Imported
                                                 select p.TrendyolBarcode).ToListAsync();

                    var anyStillInApi = siblingVariants.Any(b => apiBarcodes.Contains(b));
                    if (anyStillInApi)
                    {
                        // Some variants are still in API - just zero the stock of this variant, keep parent published
                        await ZeroVariantStockAsync(product);
                        product.TrendyolOnSale = false;
                        product.LastTrendyolStock = 0;
                        product.UpdatedOnUtc = DateTime.UtcNow;
                        await _productRepository.UpdateAsync(product);
                        continue;
                    }
                }
            }

            // Deactivate in NopCommerce: unpublish + zero stock
            await DeactivateNopProductAsync(product);

            product.ImportStatus = ImportStatus.Deactivated;
            product.TrendyolOnSale = false;
            product.LastTrendyolStock = 0;
            product.UpdatedOnUtc = DateTime.UtcNow;
            await _productRepository.UpdateAsync(product);

            deactivatedCount++;
        }

        return deactivatedCount;
    }

    /// <summary>
    /// Reactivates previously deactivated products that are back on sale
    /// </summary>
    private async Task<int> ReactivateOnSaleProductsAsync(List<TrendyolProductDto> onSaleProducts, int vendorId, TrendyolSettings settings)
    {
        var reactivatedCount = 0;

        // Get deactivated products for this vendor
        var deactivatedProducts = await (from p in _productRepository.Table
                                          where p.VendorId == vendorId &&
                                                p.ImportStatus == ImportStatus.Deactivated &&
                                                p.NopProductId.HasValue
                                          select p).ToListAsync();

        if (!deactivatedProducts.Any())
            return 0;

        var onSaleBarcodeDict = onSaleProducts.ToDictionary(p => p.Barcode, p => p);

        foreach (var product in deactivatedProducts)
        {
            if (!onSaleBarcodeDict.TryGetValue(product.TrendyolBarcode, out var apiProduct))
                continue;

            // Product is back on sale - reactivate in NopCommerce
            await ReactivateNopProductAsync(product, apiProduct, settings);

            product.ImportStatus = ImportStatus.Imported;
            product.TrendyolOnSale = true;
            product.LastTrendyolPrice = apiProduct.SalePrice;
            product.LastTrendyolStock = apiProduct.Quantity;
            product.ImportMessage = "Reactivated - back on sale in Trendyol";
            product.LastSyncOnUtc = DateTime.UtcNow;
            product.UpdatedOnUtc = DateTime.UtcNow;
            await _productRepository.UpdateAsync(product);

            reactivatedCount++;
            await _logger.InformationAsync($"Reactivated product {product.TrendyolBarcode} (NopProduct: {product.NopProductId}) - back on sale");
        }

        return reactivatedCount;
    }

    /// <summary>
    /// Deactivates a NopCommerce product (unpublish + zero stock)
    /// </summary>
    private async Task DeactivateNopProductAsync(TrendyolProduct trendyolProduct)
    {
        if (!trendyolProduct.NopProductId.HasValue)
            return;

        var nopProduct = await _nopProductService.GetProductByIdAsync(trendyolProduct.NopProductId.Value);
        if (nopProduct == null)
            return;

        nopProduct.Published = false;
        nopProduct.StockQuantity = 0;
        nopProduct.UpdatedOnUtc = DateTime.UtcNow;
        await _nopProductService.UpdateProductAsync(nopProduct);

        await _logger.InformationAsync($"Deactivated NopCommerce product {nopProduct.Id} ({nopProduct.Name}) - Trendyol barcode: {trendyolProduct.TrendyolBarcode}");
    }

    /// <summary>
    /// Reactivates a NopCommerce product (publish + restore stock/price)
    /// </summary>
    private async Task ReactivateNopProductAsync(TrendyolProduct trendyolProduct, TrendyolProductDto apiProduct, TrendyolSettings settings)
    {
        if (!trendyolProduct.NopProductId.HasValue)
            return;

        var nopProduct = await _nopProductService.GetProductByIdAsync(trendyolProduct.NopProductId.Value);
        if (nopProduct == null)
            return;

        nopProduct.Published = settings.PublishImportedProducts;
        nopProduct.Price = apiProduct.SalePrice;
        nopProduct.OldPrice = apiProduct.ListPrice > apiProduct.SalePrice ? apiProduct.ListPrice : 0;
        nopProduct.UpdatedOnUtc = DateTime.UtcNow;

        if (nopProduct.ManageInventoryMethod == ManageInventoryMethod.ManageStock)
        {
            nopProduct.StockQuantity = apiProduct.Quantity;
        }

        await _nopProductService.UpdateProductAsync(nopProduct);
    }

    /// <summary>
    /// Zeros the stock of a specific variant combination without unpublishing the parent
    /// </summary>
    private async Task ZeroVariantStockAsync(TrendyolProduct trendyolProduct)
    {
        if (!trendyolProduct.NopProductId.HasValue)
            return;

        var nopProduct = await _nopProductService.GetProductByIdAsync(trendyolProduct.NopProductId.Value);
        if (nopProduct == null)
            return;

        if (nopProduct.ManageInventoryMethod == ManageInventoryMethod.ManageStockByAttributes)
        {
            var combinations = await _productAttributeService.GetAllProductAttributeCombinationsAsync(nopProduct.Id);
            var combination = combinations.FirstOrDefault(c => c.Sku == trendyolProduct.TrendyolBarcode);
            if (combination != null)
            {
                combination.StockQuantity = 0;
                await _productAttributeService.UpdateProductAttributeCombinationAsync(combination);
            }
        }
        else if (nopProduct.ManageInventoryMethod == ManageInventoryMethod.ManageStock)
        {
            nopProduct.StockQuantity = 0;
            nopProduct.UpdatedOnUtc = DateTime.UtcNow;
            await _nopProductService.UpdateProductAsync(nopProduct);
        }
    }

    private async Task<int> GetOrCreateTaxCategoryIdAsync(int vatRate)
    {
        // This would need to be implemented based on your tax configuration
        // For now, return 0 (no specific tax category)
        await Task.CompletedTask;
        return 0;
    }

    #endregion
}
