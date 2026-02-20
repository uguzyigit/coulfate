using Marketplace.Abstractions.Domain;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Stores;
using Nop.Data;
using Nop.Services.Catalog;

namespace Nop.Plugin.Marketplace.Performance.Services;

public class PerformanceProductService : IProductService
{
    private readonly IProductService _inner;
    private readonly IRepository<ProductPerformanceSnapshot> _snapshotRepository;
    private readonly IRepository<Product> _productRepository;

    public PerformanceProductService(
        IProductService inner,
        IRepository<ProductPerformanceSnapshot> snapshotRepo,
        IRepository<Product> productRepo)
    {
        _inner = inner;
        _snapshotRepository = snapshotRepo;
        _productRepository = productRepo;
    }

    #region Products

    public Task DeleteProductAsync(Product product)
        => _inner.DeleteProductAsync(product);

    public Task DeleteProductsAsync(IList<Product> products)
        => _inner.DeleteProductsAsync(products);

    public Task<IList<Product>> GetAllProductsDisplayedOnHomepageAsync()
        => _inner.GetAllProductsDisplayedOnHomepageAsync();

    public Task<IList<Product>> GetCategoryFeaturedProductsAsync(int categoryId, int storeId = 0)
        => _inner.GetCategoryFeaturedProductsAsync(categoryId, storeId);

    public Task<IList<Product>> GetManufacturerFeaturedProductsAsync(int manufacturerId, int storeId = 0)
        => _inner.GetManufacturerFeaturedProductsAsync(manufacturerId, storeId);

    public Task<IPagedList<Product>> GetProductsMarkedAsNewAsync(int storeId = 0, int pageIndex = 0, int pageSize = int.MaxValue)
        => _inner.GetProductsMarkedAsNewAsync(storeId, pageIndex, pageSize);

    public Task<Product> GetProductByIdAsync(int productId)
        => _inner.GetProductByIdAsync(productId);

    public Task<IList<Product>> GetProductsByIdsAsync(int[] productIds)
        => _inner.GetProductsByIdsAsync(productIds);

    public Task InsertProductAsync(Product product)
        => _inner.InsertProductAsync(product);

    public Task InsertProductsAsync(IList<Product> products)
        => _inner.InsertProductsAsync(products);

    public Task UpdateProductAsync(Product product)
        => _inner.UpdateProductAsync(product);

    public Task UpdateProductsAsync(IList<Product> products)
        => _inner.UpdateProductsAsync(products);

    public Task<int> GetNumberOfProductsInCategoryAsync(IList<int> categoryIds = null, int storeId = 0)
        => _inner.GetNumberOfProductsInCategoryAsync(categoryIds, storeId);

    public async Task<IPagedList<Product>> SearchProductsAsync(
        int pageIndex = 0, int pageSize = int.MaxValue,
        IList<int> categoryIds = null, IList<int> manufacturerIds = null,
        int storeId = 0, int vendorId = 0, int warehouseId = 0,
        ProductType? productType = null,
        bool visibleIndividuallyOnly = false,
        bool excludeFeaturedProducts = false,
        decimal? priceMin = null, decimal? priceMax = null,
        int productTagId = 0, string keywords = null,
        bool searchDescriptions = false,
        bool searchManufacturerPartNumber = true,
        bool searchSku = true, bool searchProductTags = false,
        int languageId = 0,
        IList<SpecificationAttributeOption> filteredSpecOptions = null,
        ProductSortingEnum orderBy = ProductSortingEnum.Position,
        bool showHidden = false, bool? overridePublished = null)
    {
        if (orderBy == ProductSortingEnum.Position ||
            orderBy == ProductSortingEnum.BestSelling ||
            orderBy == ProductSortingEnum.MostWishlisted ||
            orderBy == ProductSortingEnum.MostReviewed)
        {
            var allProducts = await _inner.SearchProductsAsync(
                0, int.MaxValue, categoryIds, manufacturerIds, storeId, vendorId,
                warehouseId, productType, visibleIndividuallyOnly, excludeFeaturedProducts,
                priceMin, priceMax, productTagId, keywords, searchDescriptions,
                searchManufacturerPartNumber, searchSku, searchProductTags, languageId,
                filteredSpecOptions, ProductSortingEnum.Position, showHidden, overridePublished);

            var filtered = allProducts
                .Where(p => p.ManageInventoryMethodId == 0 || p.StockQuantity > 0)
                .ToList();

            var productIds = filtered.Select(p => p.Id).ToArray();
            var snapshots = await _snapshotRepository.Table
                .Where(s => productIds.Contains(s.ProductId))
                .ToDictionaryAsync(s => s.ProductId, s => s);

            var sorted = orderBy switch
            {
                ProductSortingEnum.BestSelling => filtered
                    .OrderByDescending(p => snapshots.TryGetValue(p.Id, out var s) ? s.GrossQty30d : 0),
                ProductSortingEnum.MostWishlisted => filtered
                    .OrderByDescending(p => snapshots.TryGetValue(p.Id, out var s) ? s.Wishlist30d : 0),
                ProductSortingEnum.MostReviewed => filtered
                    .OrderByDescending(p => snapshots.TryGetValue(p.Id, out var s) ? s.ReviewCount : 0),
                _ => filtered // Position — FinalScore sıralaması
                    .OrderByDescending(p => snapshots.TryGetValue(p.Id, out var s) ? s.FinalScore : decimal.MinValue),
            };

            var sortedList = sorted.ToList();
            var paged = sortedList.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            return new PagedList<Product>(paged, pageIndex, pageSize, sortedList.Count);
        }
        else
        {
            var allProducts = await _inner.SearchProductsAsync(
                0, int.MaxValue, categoryIds, manufacturerIds, storeId, vendorId,
                warehouseId, productType, visibleIndividuallyOnly, excludeFeaturedProducts,
                priceMin, priceMax, productTagId, keywords, searchDescriptions,
                searchManufacturerPartNumber, searchSku, searchProductTags, languageId,
                filteredSpecOptions, orderBy, showHidden, overridePublished);

            var filtered = allProducts
                .Where(p => p.ManageInventoryMethodId == 0 || p.StockQuantity > 0)
                .ToList();

            var paged = filtered.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            return new PagedList<Product>(paged, pageIndex, pageSize, filtered.Count);
        }
    }

    public Task<IPagedList<Product>> GetProductsByProductAttributeIdAsync(int productAttributeId,
        int pageIndex = 0, int pageSize = int.MaxValue)
        => _inner.GetProductsByProductAttributeIdAsync(productAttributeId, pageIndex, pageSize);

    public Task<IList<Product>> GetAssociatedProductsAsync(int parentGroupedProductId,
        int storeId = 0, int vendorId = 0, bool showHidden = false)
        => _inner.GetAssociatedProductsAsync(parentGroupedProductId, storeId, vendorId, showHidden);

    public Task<IPagedList<Product>> GetLowStockProductsAsync(int? vendorId = null, bool? loadPublishedOnly = true,
        int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false)
        => _inner.GetLowStockProductsAsync(vendorId, loadPublishedOnly, pageIndex, pageSize, getOnlyTotalCount);

    public Task<IPagedList<ProductAttributeCombination>> GetLowStockProductCombinationsAsync(int? vendorId = null, bool? loadPublishedOnly = true,
        int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false)
        => _inner.GetLowStockProductCombinationsAsync(vendorId, loadPublishedOnly, pageIndex, pageSize, getOnlyTotalCount);

    public Task<Product> GetProductBySkuAsync(string sku)
        => _inner.GetProductBySkuAsync(sku);

    public Task<IList<Product>> GetProductsBySkuAsync(string[] skuArray, int vendorId = 0)
        => _inner.GetProductsBySkuAsync(skuArray, vendorId);

    public Task<int> GetNumberOfProductsByVendorIdAsync(int vendorId)
        => _inner.GetNumberOfProductsByVendorIdAsync(vendorId);

    public int[] ParseRequiredProductIds(Product product)
        => _inner.ParseRequiredProductIds(product);

    public bool ProductIsAvailable(Product product, DateTime? dateTime = null)
        => _inner.ProductIsAvailable(product, dateTime);

    public int[] ParseAllowedQuantities(Product product)
        => _inner.ParseAllowedQuantities(product);

    public Task<int> GetTotalStockQuantityAsync(Product product, bool useReservedQuantity = true, int warehouseId = 0)
        => _inner.GetTotalStockQuantityAsync(product, useReservedQuantity, warehouseId);

    public int GetRentalPeriods(Product product, DateTime startDate, DateTime endDate)
        => _inner.GetRentalPeriods(product, startDate, endDate);

    public Task<string> FormatStockMessageAsync(Product product, string attributesXml)
        => _inner.FormatStockMessageAsync(product, attributesXml);

    public Task<string> FormatSkuAsync(Product product, string attributesXml = null)
        => _inner.FormatSkuAsync(product, attributesXml);

    public Task<string> FormatMpnAsync(Product product, string attributesXml = null)
        => _inner.FormatMpnAsync(product, attributesXml);

    public Task<string> FormatGtinAsync(Product product, string attributesXml = null)
        => _inner.FormatGtinAsync(product, attributesXml);

    public string FormatRentalDate(Product product, DateTime date)
        => _inner.FormatRentalDate(product, date);

    public Task<bool> HasAnyDownloadableProductAsync(int[] productIds)
        => _inner.HasAnyDownloadableProductAsync(productIds);

    public Task<bool> HasAnyGiftCardProductAsync(int[] productIds)
        => _inner.HasAnyGiftCardProductAsync(productIds);

    public Task<bool> HasAnyRecurringProductAsync(int[] productIds)
        => _inner.HasAnyRecurringProductAsync(productIds);

    public Task<string[]> GetNotExistingProductsAsync(string[] productSku)
        => _inner.GetNotExistingProductsAsync(productSku);

    #endregion

    #region Inventory management methods

    public Task AdjustInventoryAsync(Product product, int quantityToChange, string attributesXml = "", string message = "")
        => _inner.AdjustInventoryAsync(product, quantityToChange, attributesXml, message);

    public Task BookReservedInventoryAsync(Product product, int warehouseId, int quantity, string message = "")
        => _inner.BookReservedInventoryAsync(product, warehouseId, quantity, message);

    public Task<int> ReverseBookedInventoryAsync(Product product, ShipmentItem shipmentItem, string message = "")
        => _inner.ReverseBookedInventoryAsync(product, shipmentItem, message);

    #endregion

    #region Related products

    public Task DeleteRelatedProductAsync(RelatedProduct relatedProduct)
        => _inner.DeleteRelatedProductAsync(relatedProduct);

    public Task<IList<RelatedProduct>> GetRelatedProductsByProductId1Async(int productId1, bool showHidden = false)
        => _inner.GetRelatedProductsByProductId1Async(productId1, showHidden);

    public Task<RelatedProduct> GetRelatedProductByIdAsync(int relatedProductId)
        => _inner.GetRelatedProductByIdAsync(relatedProductId);

    public Task InsertRelatedProductAsync(RelatedProduct relatedProduct)
        => _inner.InsertRelatedProductAsync(relatedProduct);

    public Task UpdateRelatedProductAsync(RelatedProduct relatedProduct)
        => _inner.UpdateRelatedProductAsync(relatedProduct);

    public RelatedProduct FindRelatedProduct(IList<RelatedProduct> source, int productId1, int productId2)
        => _inner.FindRelatedProduct(source, productId1, productId2);

    #endregion

    #region Cross-sell products

    public Task DeleteCrossSellProductAsync(CrossSellProduct crossSellProduct)
        => _inner.DeleteCrossSellProductAsync(crossSellProduct);

    public Task<IList<CrossSellProduct>> GetCrossSellProductsByProductId1Async(int productId1, bool showHidden = false)
        => _inner.GetCrossSellProductsByProductId1Async(productId1, showHidden);

    public Task<CrossSellProduct> GetCrossSellProductByIdAsync(int crossSellProductId)
        => _inner.GetCrossSellProductByIdAsync(crossSellProductId);

    public Task InsertCrossSellProductAsync(CrossSellProduct crossSellProduct)
        => _inner.InsertCrossSellProductAsync(crossSellProduct);

    public Task<IList<Product>> GetCrossSellProductsByShoppingCartAsync(IList<ShoppingCartItem> cart, int numberOfProducts)
        => _inner.GetCrossSellProductsByShoppingCartAsync(cart, numberOfProducts);

    public CrossSellProduct FindCrossSellProduct(IList<CrossSellProduct> source, int productId1, int productId2)
        => _inner.FindCrossSellProduct(source, productId1, productId2);

    #endregion

    #region Tier prices

    public Task<IList<TierPrice>> GetTierPricesAsync(Product product, Customer customer, Store store)
        => _inner.GetTierPricesAsync(product, customer, store);

    public Task<IList<TierPrice>> GetTierPricesByProductAsync(int productId)
        => _inner.GetTierPricesByProductAsync(productId);

    public Task DeleteTierPriceAsync(TierPrice tierPrice)
        => _inner.DeleteTierPriceAsync(tierPrice);

    public Task<TierPrice> GetTierPriceByIdAsync(int tierPriceId)
        => _inner.GetTierPriceByIdAsync(tierPriceId);

    public Task InsertTierPriceAsync(TierPrice tierPrice)
        => _inner.InsertTierPriceAsync(tierPrice);

    public Task UpdateTierPriceAsync(TierPrice tierPrice)
        => _inner.UpdateTierPriceAsync(tierPrice);

    public Task<TierPrice> GetPreferredTierPriceAsync(Product product, Customer customer, Store store, int quantity)
        => _inner.GetPreferredTierPriceAsync(product, customer, store, quantity);

    #endregion

    #region Product pictures

    public Task DeleteProductPictureAsync(ProductPicture productPicture)
        => _inner.DeleteProductPictureAsync(productPicture);

    public Task<IList<ProductPicture>> GetProductPicturesByProductIdAsync(int productId)
        => _inner.GetProductPicturesByProductIdAsync(productId);

    public Task<ProductPicture> GetProductPictureByIdAsync(int productPictureId)
        => _inner.GetProductPictureByIdAsync(productPictureId);

    public Task InsertProductPictureAsync(ProductPicture productPicture)
        => _inner.InsertProductPictureAsync(productPicture);

    public Task UpdateProductPictureAsync(ProductPicture productPicture)
        => _inner.UpdateProductPictureAsync(productPicture);

    public Task<IDictionary<int, int[]>> GetProductsImagesIdsAsync(int[] productsIds)
        => _inner.GetProductsImagesIdsAsync(productsIds);

    public Task<IPagedList<Product>> GetProductsWithAppliedDiscountAsync(int? discountId = null,
        bool showHidden = false, int pageIndex = 0, int pageSize = int.MaxValue)
        => _inner.GetProductsWithAppliedDiscountAsync(discountId, showHidden, pageIndex, pageSize);

    #endregion

    #region Product videos

    public Task DeleteProductVideoAsync(ProductVideo productVideo)
        => _inner.DeleteProductVideoAsync(productVideo);

    public Task<IList<ProductVideo>> GetProductVideosByProductIdAsync(int productId)
        => _inner.GetProductVideosByProductIdAsync(productId);

    public Task<ProductVideo> GetProductVideoByIdAsync(int productVideoId)
        => _inner.GetProductVideoByIdAsync(productVideoId);

    public Task InsertProductVideoAsync(ProductVideo productVideo)
        => _inner.InsertProductVideoAsync(productVideo);

    public Task UpdateProductVideoAsync(ProductVideo productVideo)
        => _inner.UpdateProductVideoAsync(productVideo);

    #endregion

    #region Product warehouses

    public Task<IList<ProductWarehouseInventory>> GetAllProductWarehouseInventoryRecordsAsync(int productId)
        => _inner.GetAllProductWarehouseInventoryRecordsAsync(productId);

    public Task DeleteProductWarehouseInventoryAsync(ProductWarehouseInventory pwi)
        => _inner.DeleteProductWarehouseInventoryAsync(pwi);

    public Task InsertProductWarehouseInventoryAsync(ProductWarehouseInventory pwi)
        => _inner.InsertProductWarehouseInventoryAsync(pwi);

    public Task UpdateProductWarehouseInventoryAsync(ProductWarehouseInventory pwi)
        => _inner.UpdateProductWarehouseInventoryAsync(pwi);

    #endregion

    #region Stock quantity history

    public Task AddStockQuantityHistoryEntryAsync(Product product, int quantityAdjustment, int stockQuantity,
        int warehouseId = 0, string message = "", int? combinationId = null)
        => _inner.AddStockQuantityHistoryEntryAsync(product, quantityAdjustment, stockQuantity, warehouseId, message, combinationId);

    public Task<IPagedList<StockQuantityHistory>> GetStockQuantityHistoryAsync(Product product, int warehouseId = 0, int combinationId = 0,
        int pageIndex = 0, int pageSize = int.MaxValue)
        => _inner.GetStockQuantityHistoryAsync(product, warehouseId, combinationId, pageIndex, pageSize);

    #endregion

    #region Product discounts

    public Task ClearDiscountProductMappingAsync(Discount discount)
        => _inner.ClearDiscountProductMappingAsync(discount);

    public Task<IList<DiscountProductMapping>> GetAllDiscountsAppliedToProductAsync(int productId)
        => _inner.GetAllDiscountsAppliedToProductAsync(productId);

    public Task<DiscountProductMapping> GetDiscountAppliedToProductAsync(int productId, int discountId)
        => _inner.GetDiscountAppliedToProductAsync(productId, discountId);

    public Task InsertDiscountProductMappingAsync(DiscountProductMapping discountProductMapping)
        => _inner.InsertDiscountProductMappingAsync(discountProductMapping);

    public Task DeleteDiscountProductMappingAsync(DiscountProductMapping discountProductMapping)
        => _inner.DeleteDiscountProductMappingAsync(discountProductMapping);

    #endregion
}
