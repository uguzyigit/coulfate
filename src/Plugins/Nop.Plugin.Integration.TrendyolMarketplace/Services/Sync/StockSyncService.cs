using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Plugin.Integration.TrendyolMarketplace.Domain;
using Nop.Plugin.Integration.TrendyolMarketplace.Services.Api;
using Nop.Plugin.Integration.TrendyolMarketplace.Services.Import;
using Nop.Services.Catalog;
using Nop.Services.Logging;

namespace Nop.Plugin.Integration.TrendyolMarketplace.Services.Sync;

/// <summary>
/// Stock synchronization service
/// Strategy: onSale=true bulk fetch + NopCommerce actual stock comparison
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
            var importedProducts = await GetImportedProductsAsync(credential.VendorId);

            if (!importedProducts.Any())
                return (0, 0);

            // onSale=true ile satışta olanları çek (hızlı)
            var trendyolProducts = await _apiClient.GetAllProductsAsync(credential, approved: true, onSale: true);

            // Duplicate barcode'lara karşı güvenli dictionary oluştur
            var trendyolProductDict = new Dictionary<string, Api.Models.TrendyolProductDto>();
            foreach (var tp in trendyolProducts)
            {
                if (!string.IsNullOrEmpty(tp.Barcode))
                    trendyolProductDict[tp.Barcode] = tp;
            }

            var totalCount = importedProducts.Count;

            foreach (var importedProduct in importedProducts)
            {
                try
                {
                    // NopCommerce ürünü var mı kontrol et
                    if (!importedProduct.NopProductId.HasValue)
                    {
                        await _logger.WarningAsync($"[StockSync] {importedProduct.TrendyolBarcode}: NopProductId yok, atlanıyor");
                        successCount++;
                        await _syncLogService.UpdateProgressAsync(
                            syncLogId, totalCount, successCount, failedCount, 0);
                        continue;
                    }

                    var nopProduct = await _nopProductService.GetProductByIdAsync(importedProduct.NopProductId.Value);
                    if (nopProduct == null || nopProduct.Deleted)
                    {
                        await _logger.WarningAsync($"[StockSync] {importedProduct.TrendyolBarcode}: NopProduct {importedProduct.NopProductId} bulunamadı/silinmiş, atlanıyor");
                        successCount++;
                        await _syncLogService.UpdateProgressAsync(
                            syncLogId, totalCount, successCount, failedCount, 0);
                        continue;
                    }

                    // Trendyol'dan stok bilgisi
                    int newStock;
                    bool foundInTrendyol;
                    if (trendyolProductDict.TryGetValue(importedProduct.TrendyolBarcode, out var trendyolProduct))
                    {
                        importedProduct.TrendyolOnSale = true;
                        newStock = trendyolProduct.Quantity;
                        foundInTrendyol = true;
                    }
                    else
                    {
                        // onSale listesinde yok = stok 0
                        importedProduct.TrendyolOnSale = false;
                        newStock = 0;
                        foundInTrendyol = false;
                    }

                    // DEBUG: Her ürün için detaylı log
                    await _logger.InformationAsync(
                        $"[StockSync-Debug] {importedProduct.TrendyolBarcode}: " +
                        $"NopProductId={importedProduct.NopProductId}, " +
                        $"ManageInventory={nopProduct.ManageInventoryMethod} ({(int)nopProduct.ManageInventoryMethod}), " +
                        $"NopStockQty={nopProduct.StockQuantity}, " +
                        $"DisableBuyButton={nopProduct.DisableBuyButton}, " +
                        $"TrendyolFound={foundInTrendyol}, NewStock={newStock}");

                    // NopCommerce gerçek stoğunu oku
                    var nopStock = GetCurrentNopStock(nopProduct, importedProduct.TrendyolBarcode);
                    var expectedDisabled = newStock <= 0;

                    // Variant ürünlerde toplam stok ile DisableBuyButton kararı
                    if (nopProduct.ManageInventoryMethod == ManageInventoryMethod.ManageStockByAttributes)
                    {
                        var combinations = await _productAttributeService.GetAllProductAttributeCombinationsAsync(nopProduct.Id);
                        var totalStock = combinations.Sum(c =>
                            c.Sku == importedProduct.TrendyolBarcode ? newStock : c.StockQuantity);
                        expectedDisabled = totalStock <= 0;

                        var combination = combinations.FirstOrDefault(c => c.Sku == importedProduct.TrendyolBarcode);
                        nopStock = combination?.StockQuantity ?? -1;

                        await _logger.InformationAsync(
                            $"[StockSync-Debug] {importedProduct.TrendyolBarcode}: VARIANT — " +
                            $"CombinationFound={combination != null}, CombinationStock={nopStock}, " +
                            $"TotalStock={totalStock}, ExpectedDisabled={expectedDisabled}, " +
                            $"Combinations=[{string.Join(", ", combinations.Select(c => $"SKU={c.Sku}/Qty={c.StockQuantity}"))}]");

                        // Güncelleme gerekiyor mu?
                        if (nopStock == newStock && nopProduct.DisableBuyButton == expectedDisabled)
                        {
                            // Tutarlı — sadece sync zamanını güncelle
                            importedProduct.LastTrendyolStock = newStock;
                            importedProduct.LastSyncOnUtc = DateTime.UtcNow;
                            await _productRepository.UpdateAsync(importedProduct);
                            successCount++;
                            await _syncLogService.UpdateProgressAsync(
                                syncLogId, totalCount, successCount, failedCount, 0);
                            continue;
                        }

                        // Güncelle
                        if (combination != null)
                        {
                            combination.StockQuantity = newStock;
                            await _productAttributeService.UpdateProductAttributeCombinationAsync(combination);
                        }

                        // Published yönetimi:
                        // Stok=0 → gizle | Stok>0 VE daha önce sync kapatmışsa → tekrar aç
                        if (expectedDisabled)
                            nopProduct.Published = false;
                        else if (nopProduct.DisableBuyButton)
                            nopProduct.Published = true; // stok geri geldi, sync kapatmıştı → aç

                        nopProduct.DisableBuyButton = expectedDisabled;
                        nopProduct.DisableWishlistButton = expectedDisabled;
                        nopProduct.UpdatedOnUtc = DateTime.UtcNow;
                        await _nopProductService.UpdateProductAsync(nopProduct);
                    }
                    else if (nopProduct.ManageInventoryMethod == ManageInventoryMethod.ManageStock)
                    {
                        await _logger.InformationAsync(
                            $"[StockSync-Debug] {importedProduct.TrendyolBarcode}: SIMPLE — " +
                            $"NopStock={nopStock}, NewStock={newStock}, " +
                            $"NeedsUpdate={nopStock != newStock || nopProduct.DisableBuyButton != expectedDisabled}");

                        // Güncelleme gerekiyor mu?
                        if (nopStock == newStock && nopProduct.DisableBuyButton == expectedDisabled)
                        {
                            // Tutarlı — sadece sync zamanını güncelle
                            importedProduct.LastTrendyolStock = newStock;
                            importedProduct.LastSyncOnUtc = DateTime.UtcNow;
                            await _productRepository.UpdateAsync(importedProduct);
                            successCount++;
                            await _syncLogService.UpdateProgressAsync(
                                syncLogId, totalCount, successCount, failedCount, 0);
                            continue;
                        }

                        // Published yönetimi:
                        // Stok=0 → gizle | Stok>0 VE daha önce sync kapatmışsa → tekrar aç
                        if (expectedDisabled)
                            nopProduct.Published = false;
                        else if (nopProduct.DisableBuyButton)
                            nopProduct.Published = true;

                        // Güncelle
                        nopProduct.StockQuantity = newStock;
                        nopProduct.DisableBuyButton = expectedDisabled;
                        nopProduct.DisableWishlistButton = expectedDisabled;
                        nopProduct.UpdatedOnUtc = DateTime.UtcNow;
                        await _nopProductService.UpdateProductAsync(nopProduct);
                    }
                    else
                    {
                        // ManageInventory = DontManage — stok yönetimi kapalı, doğrudan güncelle
                        await _logger.WarningAsync(
                            $"[StockSync-Debug] {importedProduct.TrendyolBarcode}: DontManageStock! " +
                            $"NopProductId={nopProduct.Id}, StockQty={nopProduct.StockQuantity}. " +
                            $"ManageStock olarak değiştirip stoğu güncelliyorum.");

                        // ManageInventoryMethod'u ManageStock olarak değiştir
                        nopProduct.ManageInventoryMethodId = (int)ManageInventoryMethod.ManageStock;
                        nopProduct.StockQuantity = newStock;
                        // Published: sadece stok 0'a düşerse gizle
                        if (newStock <= 0)
                            nopProduct.Published = false;
                        else if (nopProduct.DisableBuyButton)
                            nopProduct.Published = true;
                        nopProduct.DisableBuyButton = (newStock <= 0);
                        nopProduct.DisableWishlistButton = (newStock <= 0);
                        nopProduct.UpdatedOnUtc = DateTime.UtcNow;
                        await _nopProductService.UpdateProductAsync(nopProduct);

                        importedProduct.LastTrendyolStock = newStock;
                        importedProduct.LastSyncOnUtc = DateTime.UtcNow;
                        importedProduct.UpdatedOnUtc = DateTime.UtcNow;
                        await _productRepository.UpdateAsync(importedProduct);
                        successCount++;
                        await _syncLogService.UpdateProgressAsync(
                            syncLogId, totalCount, successCount, failedCount, 0);
                        continue;
                    }

                    await _logger.InformationAsync(
                        $"[StockSync] {importedProduct.TrendyolBarcode}: stok {nopStock}→{newStock}, " +
                        $"buyButton→{(expectedDisabled ? "disabled" : "enabled")}");

                    importedProduct.LastTrendyolStock = newStock;
                    importedProduct.LastSyncOnUtc = DateTime.UtcNow;
                    importedProduct.UpdatedOnUtc = DateTime.UtcNow;
                    await _productRepository.UpdateAsync(importedProduct);
                    successCount++;
                }
                catch (Exception ex)
                {
                    await _logger.ErrorAsync($"Error syncing stock for product {importedProduct.TrendyolBarcode}: {ex.Message}", ex);
                    failedCount++;
                }

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
    /// Syncs stock for a single product (used by other services)
    /// </summary>
    public virtual async Task<bool> SyncProductStockAsync(TrendyolProduct trendyolProduct, int newStock)
    {
        try
        {
            if (!trendyolProduct.NopProductId.HasValue)
                return false;

            var nopProduct = await _nopProductService.GetProductByIdAsync(trendyolProduct.NopProductId.Value);
            if (nopProduct == null || nopProduct.Deleted)
                return false;

            if (nopProduct.ManageInventoryMethod == ManageInventoryMethod.ManageStock)
            {
                nopProduct.StockQuantity = newStock;
                nopProduct.DisableBuyButton = (newStock <= 0);
                nopProduct.DisableWishlistButton = (newStock <= 0);
                nopProduct.UpdatedOnUtc = DateTime.UtcNow;
                await _nopProductService.UpdateProductAsync(nopProduct);
            }
            else if (nopProduct.ManageInventoryMethod == ManageInventoryMethod.ManageStockByAttributes)
            {
                var combinations = await _productAttributeService.GetAllProductAttributeCombinationsAsync(nopProduct.Id);
                var combination = combinations.FirstOrDefault(c => c.Sku == trendyolProduct.TrendyolBarcode);

                if (combination != null)
                {
                    combination.StockQuantity = newStock;
                    await _productAttributeService.UpdateProductAttributeCombinationAsync(combination);

                    var totalStock = combinations.Sum(c => c.Sku == trendyolProduct.TrendyolBarcode ? newStock : c.StockQuantity);
                    nopProduct.DisableBuyButton = (totalStock <= 0);
                    nopProduct.DisableWishlistButton = (totalStock <= 0);
                    nopProduct.UpdatedOnUtc = DateTime.UtcNow;
                    await _nopProductService.UpdateProductAsync(nopProduct);
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync($"Error updating stock for product {trendyolProduct.TrendyolBarcode}: {ex.Message}", ex);
            return false;
        }
    }

    private int GetCurrentNopStock(Nop.Core.Domain.Catalog.Product nopProduct, string barcode)
    {
        if (nopProduct.ManageInventoryMethod == ManageInventoryMethod.ManageStock)
            return nopProduct.StockQuantity;

        return -1;
    }

    private async Task<IList<TrendyolProduct>> GetImportedProductsAsync(int vendorId)
    {
        // NopProductId olan tüm ürünleri dahil et (Imported, Deactivated, Skipped)
        // Skipped ürünler bile NopCommerce'de ürünü varsa stok güncellemesi almalı
        var query = from p in _productRepository.Table
                    where p.VendorId == vendorId &&
                          p.NopProductId.HasValue
                    select p;

        return await query.ToListAsync();
    }
}
