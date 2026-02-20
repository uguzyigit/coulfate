using Marketplace.Abstractions.Domain;
using Marketplace.Abstractions.Services;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Data;

namespace Nop.Plugin.Marketplace.Performance.Services;

public class ProductPerformanceService : IProductPerformanceService
{
    private readonly IRepository<ProductPerformanceSnapshot> _snapshotRepository;
    private readonly IRepository<ProductInteraction> _interactionRepository;
    private readonly IRepository<OrderItem> _orderItemRepository;
    private readonly IRepository<Order> _orderRepository;
    private readonly IRepository<ReturnRequest> _returnRequestRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<ProductReview> _productReviewRepository;

    public ProductPerformanceService(
        IRepository<ProductPerformanceSnapshot> snapshotRepository,
        IRepository<ProductInteraction> interactionRepository,
        IRepository<OrderItem> orderItemRepository,
        IRepository<Order> orderRepository,
        IRepository<ReturnRequest> returnRequestRepository,
        IRepository<Product> productRepository,
        IRepository<ProductReview> productReviewRepository)
    {
        _snapshotRepository = snapshotRepository;
        _interactionRepository = interactionRepository;
        _orderItemRepository = orderItemRepository;
        _orderRepository = orderRepository;
        _returnRequestRepository = returnRequestRepository;
        _productRepository = productRepository;
        _productReviewRepository = productReviewRepository;
    }

    public virtual async Task<ProductPerformanceSnapshot> GetByProductIdAsync(int productId)
    {
        return await _snapshotRepository.Table
            .Where(x => x.ProductId == productId)
            .FirstOrDefaultAsync();
    }

    public virtual async Task<IPagedList<ProductPerformanceSnapshot>> GetAllAsync(
        int vendorId = 0, int pageIndex = 0, int pageSize = int.MaxValue)
    {
        var query = _snapshotRepository.Table;

        if (vendorId > 0)
            query = query.Where(x => x.VendorId == vendorId);

        query = query.OrderByDescending(x => x.FinalScore);

        return await query.ToPagedListAsync(pageIndex, pageSize);
    }

    public virtual async Task RecalculateAllAsync()
    {
        var now = DateTime.UtcNow;
        var since30d = now.AddDays(-30);
        var since7d = now.AddDays(-7);

        // 1. Get eligible vendor products (published, not deleted)
        var products = await _productRepository.Table
            .Where(p => p.VendorId > 0 && !p.Deleted && p.Published)
            .Select(p => new { p.Id, p.VendorId, p.ManageInventoryMethodId, p.StockQuantity, p.CreatedOnUtc })
            .ToListAsync();

        // Filter out out-of-stock products (when inventory is managed)
        var eligibleProducts = products
            .Where(p => p.ManageInventoryMethodId == 0 || p.StockQuantity > 0)
            .ToList();

        var eligibleProductIds = eligibleProducts.Select(p => p.Id).ToHashSet();

        // 2. Aggregate interactions (30d) grouped by product
        var interactionViewType = (int)ProductInteractionType.View;
        var interactionCartType = (int)ProductInteractionType.AddToCart;
        var interactionWishlistType = (int)ProductInteractionType.Wishlist;

        var interactions = await _interactionRepository.Table
            .Where(i => i.CreatedOnUtc >= since30d)
            .GroupBy(i => new { i.ProductId, i.InteractionType })
            .Select(g => new { g.Key.ProductId, g.Key.InteractionType, Count = g.Count() })
            .ToListAsync();

        var interactionLookup = interactions.ToLookup(x => x.ProductId);

        // 3. Aggregate order data (paid orders, 30d)
        // PaymentStatusId == 30 means Paid
        var paidOrderIds30d = _orderRepository.Table
            .Where(o => o.PaymentStatusId == 30 && o.CreatedOnUtc >= since30d);

        var orderItems30d = await (
            from oi in _orderItemRepository.Table
            join o in paidOrderIds30d on oi.OrderId equals o.Id
            group new { oi, o } by oi.ProductId into g
            select new
            {
                ProductId = g.Key,
                GrossOrders = g.Select(x => x.oi.OrderId).Distinct().Count(),
                GrossQty = g.Sum(x => x.oi.Quantity),
                GrossRevenue = g.Sum(x => x.oi.PriceExclTax),
                DiscountAmount = g.Sum(x => x.oi.DiscountAmountExclTax)
            }).ToListAsync();

        var orderLookup30d = orderItems30d.ToDictionary(x => x.ProductId);

        // 4. Aggregate 7d order data for trend
        var paidOrderIds7d = _orderRepository.Table
            .Where(o => o.PaymentStatusId == 30 && o.CreatedOnUtc >= since7d);

        var orderItems7d = await (
            from oi in _orderItemRepository.Table
            join o in paidOrderIds7d on oi.OrderId equals o.Id
            group new { oi, o } by oi.ProductId into g
            select new
            {
                ProductId = g.Key,
                Orders = g.Select(x => x.oi.OrderId).Distinct().Count(),
                Revenue = g.Sum(x => x.oi.PriceExclTax)
            }).ToListAsync();

        var orderLookup7d = orderItems7d.ToDictionary(x => x.ProductId);

        // 5. Aggregate return requests (30d, statuses: Pending=10, Received=20, ReturnAuthorized=40)
        var returnData = await (
            from rr in _returnRequestRepository.Table
            where rr.CreatedOnUtc >= since30d
                && (rr.ReturnRequestStatusId == 10 || rr.ReturnRequestStatusId == 20 || rr.ReturnRequestStatusId == 40)
            join oi in _orderItemRepository.Table on rr.OrderItemId equals oi.Id
            group new { rr, oi } by oi.ProductId into g
            select new
            {
                ProductId = g.Key,
                ReturnQty = g.Sum(x => x.rr.Quantity),
                ReturnRevenue = g.Sum(x => x.rr.Quantity * x.oi.UnitPriceExclTax)
            }).ToListAsync();

        var returnLookup = returnData.ToDictionary(x => x.ProductId);

        // 6. Aggregate cancelled orders (30d, OrderStatusId == 40 means Cancelled)
        var cancelData = await (
            from oi in _orderItemRepository.Table
            join o in _orderRepository.Table on oi.OrderId equals o.Id
            where o.OrderStatusId == 40 && o.CreatedOnUtc >= since30d
            group new { oi, o } by oi.ProductId into g
            select new
            {
                ProductId = g.Key,
                CancelQty = g.Sum(x => x.oi.Quantity),
                CancelRevenue = g.Sum(x => x.oi.PriceExclTax)
            }).ToListAsync();

        var cancelLookup = cancelData.ToDictionary(x => x.ProductId);

        // 6b. Aggregate approved review counts per product
        var reviewCounts = await _productReviewRepository.Table
            .Where(r => r.IsApproved)
            .GroupBy(r => r.ProductId)
            .Select(g => new { ProductId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ProductId, x => x.Count);

        // 7. Calculate global avg conversion for Bayesian smoothing
        var totalViews = interactions.Where(x => x.InteractionType == interactionViewType).Sum(x => x.Count);
        var totalOrders30d = orderItems30d.Sum(x => x.GrossOrders);
        var avgConv = totalViews > 0 ? (double)totalOrders30d / totalViews : 0.01;

        // 8. Load existing snapshots
        var existingSnapshots = await _snapshotRepository.Table.ToListAsync();
        var snapshotDict = existingSnapshots.ToDictionary(x => x.ProductId);

        var toInsert = new List<ProductPerformanceSnapshot>();
        var toUpdate = new List<ProductPerformanceSnapshot>();
        var toDelete = new List<ProductPerformanceSnapshot>();

        foreach (var product in eligibleProducts)
        {
            var productInteractions = interactionLookup[product.Id].ToList();
            var views = productInteractions.FirstOrDefault(x => x.InteractionType == interactionViewType)?.Count ?? 0;
            var addToCart = productInteractions.FirstOrDefault(x => x.InteractionType == interactionCartType)?.Count ?? 0;
            var wishlist = productInteractions.FirstOrDefault(x => x.InteractionType == interactionWishlistType)?.Count ?? 0;

            orderLookup30d.TryGetValue(product.Id, out var sales30d);
            orderLookup7d.TryGetValue(product.Id, out var sales7d);
            returnLookup.TryGetValue(product.Id, out var returns);
            cancelLookup.TryGetValue(product.Id, out var cancels);

            var grossOrders = sales30d?.GrossOrders ?? 0;
            var grossQty = sales30d?.GrossQty ?? 0;
            var grossRevenue = sales30d?.GrossRevenue ?? 0m;
            var discountAmount = sales30d?.DiscountAmount ?? 0m;
            var revenue7d = sales7d?.Revenue ?? 0m;
            var orders7d = sales7d?.Orders ?? 0;
            var cancelQty = cancels?.CancelQty ?? 0;
            var returnQty = returns?.ReturnQty ?? 0;
            var cancelRevenue = cancels?.CancelRevenue ?? 0m;
            var returnRevenue = returns?.ReturnRevenue ?? 0m;
            var netQty = grossQty - cancelQty - returnQty;
            var netRevenue = grossRevenue - cancelRevenue - returnRevenue;

            // Score calculation
            var salesScore = (decimal)(Math.Log(1 + (double)grossRevenue) + 0.6 * Math.Log(1 + grossQty) + 0.3 * Math.Log(1 + grossOrders));
            var trendScore = (decimal)((double)revenue7d / 7.0 / ((double)grossRevenue / 30.0 + 0.001));
            var conversionScore = (decimal)((grossOrders + 200.0 * avgConv) / (views + 200.0));
            var maxGrossQty = Math.Max(1, grossQty);
            var qualityPenalty = (decimal)(2.0 * returnQty / maxGrossQty + 1.0 * cancelQty / maxGrossQty);
            var daysSinceCreated = (now - product.CreatedOnUtc).TotalDays;
            var newProductBoost = (decimal)(Math.Exp(-daysSinceCreated / 14.0) * 0.15);
            var finalScore = 0.75m * salesScore + 0.20m * trendScore + 0.05m * conversionScore - qualityPenalty + newProductBoost;

            if (snapshotDict.TryGetValue(product.Id, out var existing))
            {
                existing.VendorId = product.VendorId;
                existing.Views30d = views;
                existing.AddToCart30d = addToCart;
                existing.Wishlist30d = wishlist;
                existing.GrossOrders30d = grossOrders;
                existing.GrossQty30d = grossQty;
                existing.GrossRevenue30d = grossRevenue;
                existing.DiscountAmount30d = discountAmount;
                existing.Revenue7d = revenue7d;
                existing.Orders7d = orders7d;
                existing.CancelQty30d = cancelQty;
                existing.ReturnQty30d = returnQty;
                existing.CancelRevenue30d = cancelRevenue;
                existing.ReturnRevenue30d = returnRevenue;
                existing.NetQty30d = netQty;
                existing.NetRevenue30d = netRevenue;
                existing.SalesScore = salesScore;
                existing.TrendScore = trendScore;
                existing.ConversionScore = conversionScore;
                existing.QualityPenalty = qualityPenalty;
                existing.NewProductBoost = newProductBoost;
                existing.FinalScore = finalScore;
                existing.ReviewCount = reviewCounts.GetValueOrDefault(product.Id);
                existing.CalculatedOnUtc = now;
                toUpdate.Add(existing);
                snapshotDict.Remove(product.Id);
            }
            else
            {
                toInsert.Add(new ProductPerformanceSnapshot
                {
                    ProductId = product.Id,
                    VendorId = product.VendorId,
                    Views30d = views,
                    AddToCart30d = addToCart,
                    Wishlist30d = wishlist,
                    GrossOrders30d = grossOrders,
                    GrossQty30d = grossQty,
                    GrossRevenue30d = grossRevenue,
                    DiscountAmount30d = discountAmount,
                    Revenue7d = revenue7d,
                    Orders7d = orders7d,
                    CancelQty30d = cancelQty,
                    ReturnQty30d = returnQty,
                    CancelRevenue30d = cancelRevenue,
                    ReturnRevenue30d = returnRevenue,
                    NetQty30d = netQty,
                    NetRevenue30d = netRevenue,
                    SalesScore = salesScore,
                    TrendScore = trendScore,
                    ConversionScore = conversionScore,
                    QualityPenalty = qualityPenalty,
                    NewProductBoost = newProductBoost,
                    FinalScore = finalScore,
                    ReviewCount = reviewCounts.GetValueOrDefault(product.Id),
                    CalculatedOnUtc = now
                });
            }
        }

        // Remaining snapshots in dict are for products that are no longer eligible — delete them
        toDelete.AddRange(snapshotDict.Values);

        if (toInsert.Count > 0)
            await _snapshotRepository.InsertAsync(toInsert);

        if (toUpdate.Count > 0)
            await _snapshotRepository.UpdateAsync(toUpdate);

        if (toDelete.Count > 0)
            await _snapshotRepository.DeleteAsync(toDelete);
    }

    public virtual async Task<int> GetDiagnosticCountsAsync()
    {
        return await _interactionRepository.Table.CountAsync();
    }

    public virtual async Task CleanupOldInteractionsAsync(int keepDays = 60)
    {
        var cutoff = DateTime.UtcNow.AddDays(-keepDays);
        var old = await _interactionRepository.Table
            .Where(x => x.CreatedOnUtc < cutoff)
            .ToListAsync();

        if (old.Count > 0)
            await _interactionRepository.DeleteAsync(old);
    }
}
