using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Plugin.Integration.TrendyolMarketplace.Domain;
using Nop.Plugin.Integration.TrendyolMarketplace.Services.Api;
using Nop.Plugin.Integration.TrendyolMarketplace.Services.Import;
using Nop.Services.Catalog;
using Nop.Services.Logging;

namespace Nop.Plugin.Integration.TrendyolMarketplace.Services.Sync;

/// <summary>
/// Stock synchronization service implementation
/// </summary>
public class StockSyncService : IStockSyncService
{
    private readonly IRepository<TrendyolProduct> _productRepository;
    private readonly ITrendyolApiClient _apiClient;
    private readonly IProductService _nopProductService;
    private readonly IProductAttributeService _productAttributeService;
    private readonly IVariantService _variantService;
    private readonly ISyncLogService _syncLogService;
    private readonly ILogger _logger;

    public StockSyncService(
        IRepository<TrendyolProduct> productRepository,
        ITrendyolApiClient apiClient,
        IProductService nopProductService,
        IProductAttributeService productAttributeService,
        IVariantService variantService,
        ISyncLogService syncLogService,
        ILogger logger)
    {
        _productRepository = productRepository;
        _apiClient = apiClient;
        _nopProductService = nopProductService;
        _productAttributeService = productAttributeService;
        _variantService = variantService;
        _syncLogService = syncLogService;
        _logger = logger;
    }

    /// <summary>
    /// Syncs stock for all products of a vendor from Trendyol
    /// </summary>
    public virtual async Task<(int Success, int Failed)> SyncStockAsync(TrendyolVendorCredential credential, int syncLogId)
    {
        var successCount = 0;
        var failedCount = 0;

        try
        {
            // Get all Trendyol products for this vendor that are imported
            var importedProducts = await GetImportedProductsAsync(credential.VendorId);

            if (!importedProducts.Any())
                return (0, 0);

            // Fetch current stock from Trendyol API (only onSale products)
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

                        // Use the actual stock from API (Trendyol handles onSale internally - quantity=0 when off sale)
                        var newStock = trendyolProduct.Quantity;

                        if (importedProduct.LastTrendyolStock != newStock)
                        {
                            var success = await SyncProductStockAsync(importedProduct, newStock);

                            if (success)
                            {
                                // Update Trendyol product record
                                importedProduct.LastTrendyolStock = newStock;
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
                            // Stock unchanged, just update sync time
                            importedProduct.LastSyncOnUtc = DateTime.UtcNow;
                            await _productRepository.UpdateAsync(importedProduct);
                            successCount++;
                        }
                    }
                    else
                    {
                        // Product not found in Trendyol API - zero stock as safety measure
                        await _logger.WarningAsync($"Product {importedProduct.TrendyolBarcode} not found in Trendyol API during stock sync - zeroing stock");
                        if (importedProduct.LastTrendyolStock != 0)
                        {
                            var success = await SyncProductStockAsync(importedProduct, 0);
                            if (success)
                            {
                                importedProduct.LastTrendyolStock = 0;
                                importedProduct.TrendyolOnSale = false;
                                importedProduct.LastSyncOnUtc = DateTime.UtcNow;
                                importedProduct.UpdatedOnUtc = DateTime.UtcNow;
                                await _productRepository.UpdateAsync(importedProduct);
                            }
                        }
                        failedCount++;
                    }
                }
                catch (Exception ex)
                {
                    await _logger.ErrorAsync($"Error syncing stock for product {importedProduct.TrendyolBarcode}: {ex.Message}", ex);
                    failedCount++;
                }

                // Update sync progress
                await _syncLogService.UpdateProgressAsync(
                    syncLogId, totalCount, successCount, failedCount, 0);
            }
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync($"Error during stock sync for vendor {credential.VendorId}: {ex.Message}", ex);
            throw;
        }

        return (successCount, failedCount);
    }

    /// <summary>
    /// Syncs stock for a single product
    /// </summary>
    public virtual async Task<bool> SyncProductStockAsync(TrendyolProduct trendyolProduct, int newStock)
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
                // Simple product - update stock directly
                nopProduct.StockQuantity = newStock;
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
                    combination.StockQuantity = newStock;
                    await _productAttributeService.UpdateProductAttributeCombinationAsync(combination);
                }
                else
                {
                    await _logger.WarningAsync($"Combination not found for SKU {trendyolProduct.TrendyolBarcode} in product {nopProduct.Id}");
                    return false;
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync($"Error updating stock for product {trendyolProduct.TrendyolBarcode}: {ex.Message}", ex);
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
