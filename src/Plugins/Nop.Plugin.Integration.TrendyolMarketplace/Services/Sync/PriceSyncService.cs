using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Plugin.Integration.TrendyolMarketplace.Domain;
using Nop.Plugin.Integration.TrendyolMarketplace.Services.Api;
using Nop.Services.Catalog;
using Nop.Services.Logging;

namespace Nop.Plugin.Integration.TrendyolMarketplace.Services.Sync;

/// <summary>
/// Price synchronization service implementation
/// </summary>
public class PriceSyncService : IPriceSyncService
{
    private readonly IRepository<TrendyolProduct> _productRepository;
    private readonly ITrendyolApiClient _apiClient;
    private readonly IProductService _nopProductService;
    private readonly IProductAttributeService _productAttributeService;
    private readonly ISyncLogService _syncLogService;
    private readonly ILogger _logger;

    public PriceSyncService(
        IRepository<TrendyolProduct> productRepository,
        ITrendyolApiClient apiClient,
        IProductService nopProductService,
        IProductAttributeService productAttributeService,
        ISyncLogService syncLogService,
        ILogger logger)
    {
        _productRepository = productRepository;
        _apiClient = apiClient;
        _nopProductService = nopProductService;
        _productAttributeService = productAttributeService;
        _syncLogService = syncLogService;
        _logger = logger;
    }

    /// <summary>
    /// Syncs prices for all products of a vendor from Trendyol
    /// </summary>
    public virtual async Task<(int Success, int Failed)> SyncPricesAsync(TrendyolVendorCredential credential, int syncLogId)
    {
        var successCount = 0;
        var failedCount = 0;

        try
        {
            // Get all Trendyol products for this vendor that are imported
            var importedProducts = await GetImportedProductsAsync(credential.VendorId);

            if (!importedProducts.Any())
                return (0, 0);

            // Fetch current prices from Trendyol API (only onSale products)
            var trendyolProducts = await _apiClient.GetAllProductsAsync(credential, approved: true, onSale: true);
            var trendyolProductDict = trendyolProducts.ToDictionary(p => p.Barcode, p => p);

            var totalCount = importedProducts.Count;

            foreach (var importedProduct in importedProducts)
            {
                try
                {
                    if (trendyolProductDict.TryGetValue(importedProduct.TrendyolBarcode, out var trendyolProduct))
                    {
                        // Track onSale status from API
                        importedProduct.TrendyolOnSale = trendyolProduct.OnSale;

                        var newSalePrice = trendyolProduct.SalePrice;
                        var newListPrice = trendyolProduct.ListPrice;

                        if (importedProduct.LastTrendyolPrice != newSalePrice)
                        {
                            var success = await SyncProductPriceAsync(importedProduct, newSalePrice, newListPrice);

                            if (success)
                            {
                                // Update Trendyol product record
                                importedProduct.LastTrendyolPrice = newSalePrice;
                                importedProduct.LastSyncOnUtc = DateTime.UtcNow;
                                importedProduct.UpdatedOnUtc = DateTime.UtcNow;
                                await _productRepository.UpdateAsync(importedProduct);

                                successCount++;
                            }
                            else
                            {
                                failedCount++;
                            }
                        }
                        else
                        {
                            // Price unchanged, just update sync time
                            importedProduct.LastSyncOnUtc = DateTime.UtcNow;
                            await _productRepository.UpdateAsync(importedProduct);
                            successCount++;
                        }
                    }
                    else
                    {
                        // Product not found in Trendyol API
                        await _logger.WarningAsync($"Product {importedProduct.TrendyolBarcode} not found in Trendyol API during price sync");
                        if (importedProduct.TrendyolOnSale)
                        {
                            importedProduct.TrendyolOnSale = false;
                            importedProduct.UpdatedOnUtc = DateTime.UtcNow;
                            await _productRepository.UpdateAsync(importedProduct);
                        }
                        failedCount++;
                    }
                }
                catch (Exception ex)
                {
                    await _logger.ErrorAsync($"Error syncing price for product {importedProduct.TrendyolBarcode}: {ex.Message}", ex);
                    failedCount++;
                }

                // Update sync progress
                await _syncLogService.UpdateProgressAsync(
                    syncLogId, totalCount, successCount, failedCount, 0);
            }
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync($"Error during price sync for vendor {credential.VendorId}: {ex.Message}", ex);
            throw;
        }

        return (successCount, failedCount);
    }

    /// <summary>
    /// Syncs price for a single product
    /// </summary>
    public virtual async Task<bool> SyncProductPriceAsync(TrendyolProduct trendyolProduct, decimal salePrice, decimal listPrice)
    {
        try
        {
            if (!trendyolProduct.NopProductId.HasValue)
                return false;

            var nopProduct = await _nopProductService.GetProductByIdAsync(trendyolProduct.NopProductId.Value);
            if (nopProduct == null)
                return false;

            if (nopProduct.ManageInventoryMethod == ManageInventoryMethod.ManageStock)
            {
                // Simple product - update price directly
                nopProduct.Price = salePrice;
                nopProduct.OldPrice = listPrice > salePrice ? listPrice : 0;
                nopProduct.UpdatedOnUtc = DateTime.UtcNow;
                await _nopProductService.UpdateProductAsync(nopProduct);
            }
            else if (nopProduct.ManageInventoryMethod == ManageInventoryMethod.ManageStockByAttributes)
            {
                // Variant product - update the specific combination
                var combinations = await _productAttributeService.GetAllProductAttributeCombinationsAsync(nopProduct.Id);
                var combination = combinations.FirstOrDefault(c => c.Sku == trendyolProduct.TrendyolBarcode);

                if (combination != null)
                {
                    combination.OverriddenPrice = salePrice;
                    await _productAttributeService.UpdateProductAttributeCombinationAsync(combination);
                }
                else
                {
                    await _logger.WarningAsync($"Combination not found for SKU {trendyolProduct.TrendyolBarcode} in product {nopProduct.Id}");
                    return false;
                }

                // Also update the parent product's base price if needed
                var allCombinations = await _productAttributeService.GetAllProductAttributeCombinationsAsync(nopProduct.Id);
                var minPrice = allCombinations.Where(c => c.OverriddenPrice.HasValue).Min(c => c.OverriddenPrice);
                var maxListPrice = listPrice; // We could calculate max list price if needed

                if (minPrice.HasValue && nopProduct.Price != minPrice.Value)
                {
                    nopProduct.Price = minPrice.Value;
                    nopProduct.UpdatedOnUtc = DateTime.UtcNow;
                    await _nopProductService.UpdateProductAsync(nopProduct);
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync($"Error updating price for product {trendyolProduct.TrendyolBarcode}: {ex.Message}", ex);
            return false;
        }
    }

    private async Task<IList<TrendyolProduct>> GetImportedProductsAsync(int vendorId)
    {
        var query = from p in _productRepository.Table
                    where p.VendorId == vendorId &&
                          p.ImportStatus == ImportStatus.Imported &&
                          p.NopProductId.HasValue
                    select p;

        return await query.ToListAsync();
    }
}
