using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Plugin.Marketplace.Commission.Domain;
using Nop.Plugin.Marketplace.Commission.Models;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Orders;
using Nop.Services.Vendors;

namespace Nop.Plugin.Marketplace.Commission.Services;

public class CommissionService : ICommissionService
{
    private readonly IRepository<CategoryCommission> _categoryCommissionRepository;
    private readonly IRepository<ProductCommission> _productCommissionRepository;
    private readonly IRepository<OrderCommission> _orderCommissionRepository;
    private readonly ICategoryService _categoryService;
    private readonly IProductService _productService;
    private readonly IOrderService _orderService;
    private readonly ISettingService _settingService;
    private readonly IVendorService _vendorService;

    public CommissionService(
        IRepository<CategoryCommission> categoryCommissionRepository,
        IRepository<ProductCommission> productCommissionRepository,
        IRepository<OrderCommission> orderCommissionRepository,
        ICategoryService categoryService,
        IProductService productService,
        IOrderService orderService,
        ISettingService settingService,
        IVendorService vendorService)
    {
        _categoryCommissionRepository = categoryCommissionRepository;
        _productCommissionRepository = productCommissionRepository;
        _orderCommissionRepository = orderCommissionRepository;
        _categoryService = categoryService;
        _productService = productService;
        _orderService = orderService;
        _settingService = settingService;
        _vendorService = vendorService;
    }

    public virtual async Task<IList<CategoryCommission>> GetAllCategoryCommissionsAsync()
    {
        var query = _categoryCommissionRepository.Table.Where(c => c.IsActive);
        return await query.ToListAsync();
    }

    public virtual async Task<decimal> GetCategoryCommissionRateAsync(int categoryId)
    {
        var commission = await _categoryCommissionRepository.Table
            .Where(c => c.CategoryId == categoryId && c.IsActive)
            .FirstOrDefaultAsync();
        
        if (commission != null) 
            return commission.Rate;
        
        var category = await _categoryService.GetCategoryByIdAsync(categoryId);
        if (category?.ParentCategoryId > 0) 
            return await GetCategoryCommissionRateAsync(category.ParentCategoryId);
        
        var settings = await _settingService.LoadSettingAsync<CommissionSettings>();
        return settings.DefaultCommissionRate;
    }

    public virtual async Task<decimal> GetProductCommissionRateAsync(int productId)
    {
        var commission = await _productCommissionRepository.Table
            .Where(c => c.ProductId == productId && c.IsActive)
            .FirstOrDefaultAsync();
        
        if (commission != null && (!commission.ExpiresOnUtc.HasValue || commission.ExpiresOnUtc.Value > DateTime.UtcNow)) 
            return commission.Rate;
        
        return 0;
    }

    public virtual async Task<CategoryCommission> SetCategoryCommissionRateAsync(int categoryId, decimal rate)
    {
        var existing = await _categoryCommissionRepository.Table
            .FirstOrDefaultAsync(c => c.CategoryId == categoryId);
        
        if (existing != null) 
        { 
            existing.Rate = rate; 
            existing.UpdatedOnUtc = DateTime.UtcNow; 
            await _categoryCommissionRepository.UpdateAsync(existing); 
            return existing; 
        }
        
        var commission = new CategoryCommission 
        { 
            CategoryId = categoryId, 
            Rate = rate, 
            IsActive = true, 
            CreatedOnUtc = DateTime.UtcNow 
        };
        
        await _categoryCommissionRepository.InsertAsync(commission);
        return commission;
    }

    public virtual async Task<ProductCommission> SetProductCommissionRateAsync(int productId, decimal rate)
    {
        var existing = await _productCommissionRepository.Table
            .FirstOrDefaultAsync(c => c.ProductId == productId);
        
        if (existing != null) 
        { 
            existing.Rate = rate; 
            existing.CreatedOnUtc = DateTime.UtcNow; 
            await _productCommissionRepository.UpdateAsync(existing); 
            return existing; 
        }
        
        var commission = new ProductCommission 
        { 
            ProductId = productId, 
            Rate = rate, 
            IsActive = true, 
            CreatedOnUtc = DateTime.UtcNow 
        };
        
        await _productCommissionRepository.InsertAsync(commission);
        return commission;
    }

    public virtual async Task<decimal> CalculateCommissionRateForProductAsync(int productId)
    {
        var productRate = await GetProductCommissionRateAsync(productId);
        if (productRate > 0) 
            return productRate;
        
        var product = await _productService.GetProductByIdAsync(productId);
        if (product == null) 
            return 0;
        
        var productCategories = await _categoryService.GetProductCategoriesByProductIdAsync(productId);
        var leafCategory = productCategories.OrderByDescending(pc => pc.DisplayOrder).FirstOrDefault();
        
        if (leafCategory != null) 
            return await GetCategoryCommissionRateAsync(leafCategory.CategoryId);
        
        var settings = await _settingService.LoadSettingAsync<CommissionSettings>();
        return settings.DefaultCommissionRate;
    }

    public virtual async Task<OrderCommission> CalculateAndCreateCommissionAsync(int orderItemId)
    {
        var orderItem = await _orderService.GetOrderItemByIdAsync(orderItemId);
        if (orderItem == null) 
            return null;
        
        var product = await _productService.GetProductByIdAsync(orderItem.ProductId);
        if (product?.VendorId == 0) 
            return null;
        
        var order = await _orderService.GetOrderByIdAsync(orderItem.OrderId);
        var settings = await _settingService.LoadSettingAsync<CommissionSettings>();
        
        var productPrice = orderItem.PriceInclTax;
        var discountAmount = orderItem.DiscountAmountInclTax;
        var netPrice = productPrice - discountAmount;
        
        var commissionRate = await CalculateCommissionRateForProductAsync(product.Id);
        var commissionAmount = netPrice * (commissionRate / 100);
        var taxWithholding = netPrice * settings.TaxWithholdingRate;
        var vendorNetAmount = netPrice - commissionAmount - settings.MarketplaceFeePerOrder - taxWithholding;
        
        var productCategories = await _categoryService.GetProductCategoriesByProductIdAsync(product.Id);
        var leafCategoryId = productCategories.OrderByDescending(pc => pc.DisplayOrder).FirstOrDefault()?.CategoryId ?? 0;
        
        var orderCommission = new OrderCommission 
        { 
            OrderId = order.Id, 
            OrderItemId = orderItem.Id, 
            VendorId = product.VendorId, 
            ProductId = product.Id, 
            CategoryId = leafCategoryId, 
            Quantity = orderItem.Quantity, 
            ProductPrice = productPrice, 
            DiscountAmount = discountAmount, 
            NetPrice = netPrice, 
            CommissionRate = commissionRate, 
            CommissionAmount = commissionAmount, 
            MarketplaceFee = settings.MarketplaceFeePerOrder, 
            TaxWithholding = taxWithholding, 
            VendorNetAmount = vendorNetAmount, 
            CreatedOnUtc = DateTime.UtcNow 
        };
        
        await _orderCommissionRepository.InsertAsync(orderCommission);
        return orderCommission;
    }

    #region Reports

    public async Task<CommissionReportSummary> GetReportSummaryAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        int? vendorId = null)
    {
        var query = _orderCommissionRepository.Table;

        if (startDate.HasValue)
            query = query.Where(x => x.CreatedOnUtc >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(x => x.CreatedOnUtc <= endDate.Value);

        if (vendorId.HasValue)
            query = query.Where(x => x.VendorId == vendorId.Value);

        var commissions = await query.ToListAsync();

        return new CommissionReportSummary
        {
            TotalOrders = commissions.Count,
            TotalRevenue = commissions.Sum(x => x.NetPrice),
            TotalCommission = commissions.Sum(x => x.CommissionAmount),
            TotalMarketplaceFee = commissions.Sum(x => x.MarketplaceFee),
            TotalTaxWithholding = commissions.Sum(x => x.TaxWithholding),
            TotalVendorPayout = commissions.Sum(x => x.VendorNetAmount),
            AverageCommissionRate = commissions.Any() ? commissions.Average(x => x.CommissionRate) : 0
        };
    }

    public async Task<IList<VendorCommissionSummary>> GetVendorCommissionBreakdownAsync(
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var query = _orderCommissionRepository.Table;

        if (startDate.HasValue)
            query = query.Where(x => x.CreatedOnUtc >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(x => x.CreatedOnUtc <= endDate.Value);

        var grouped = await query
            .GroupBy(x => x.VendorId)
            .Select(g => new
            {
                VendorId = g.Key,
                OrderCount = g.Count(),
                TotalRevenue = g.Sum(x => x.NetPrice),
                TotalCommission = g.Sum(x => x.CommissionAmount),
                NetPayout = g.Sum(x => x.VendorNetAmount)
            })
            .ToListAsync();

        var result = new List<VendorCommissionSummary>();
        foreach (var item in grouped)
        {
            var vendor = await _vendorService.GetVendorByIdAsync(item.VendorId);
            result.Add(new VendorCommissionSummary
            {
                VendorId = item.VendorId,
                VendorName = vendor?.Name ?? "Unknown",
                OrderCount = item.OrderCount,
                TotalRevenue = item.TotalRevenue,
                TotalCommission = item.TotalCommission,
                NetPayout = item.NetPayout
            });
        }

        return result.OrderByDescending(x => x.TotalCommission).ToList();
    }

    public async Task<IList<ProductCommissionSummary>> GetTopProductsByCommissionAsync(
        int pageSize = 10,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var query = _orderCommissionRepository.Table;

        if (startDate.HasValue)
            query = query.Where(x => x.CreatedOnUtc >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(x => x.CreatedOnUtc <= endDate.Value);

        var grouped = await query
            .GroupBy(x => x.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                OrderCount = g.Count(),
                TotalRevenue = g.Sum(x => x.NetPrice),
                TotalCommission = g.Sum(x => x.CommissionAmount),
                AvgRate = g.Average(x => x.CommissionRate)
            })
            .OrderByDescending(x => x.TotalCommission)
            .Take(pageSize)
            .ToListAsync();

        var result = new List<ProductCommissionSummary>();
        foreach (var item in grouped)
        {
            var product = await _productService.GetProductByIdAsync(item.ProductId);
            result.Add(new ProductCommissionSummary
            {
                ProductId = item.ProductId,
                ProductName = product?.Name ?? "Unknown",
                OrderCount = item.OrderCount,
                TotalRevenue = item.TotalRevenue,
                TotalCommission = item.TotalCommission,
                CommissionRate = item.AvgRate
            });
        }

        return result;
    }

    #endregion
}