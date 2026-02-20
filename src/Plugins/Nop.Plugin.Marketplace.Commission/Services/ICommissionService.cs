using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Plugin.Marketplace.Commission.Domain;
using Nop.Plugin.Marketplace.Commission.Models;

namespace Nop.Plugin.Marketplace.Commission.Services;

public interface ICommissionService
{
    Task<IList<CategoryCommission>> GetAllCategoryCommissionsAsync();
    Task<decimal> GetCategoryCommissionRateAsync(int categoryId);
    Task<decimal> GetProductCommissionRateAsync(int productId);
    Task<CategoryCommission> SetCategoryCommissionRateAsync(int categoryId, decimal rate);
    Task<ProductCommission> SetProductCommissionRateAsync(int productId, decimal rate);
    Task<OrderCommission> CalculateAndCreateCommissionAsync(int orderItemId);
    Task<decimal> CalculateCommissionRateForProductAsync(int productId);

    #region Reports

    /// <summary>
    /// Gets commission report summary
    /// </summary>
    Task<CommissionReportSummary> GetReportSummaryAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        int? vendorId = null);

    /// <summary>
    /// Gets vendor commission breakdown
    /// </summary>
    Task<IList<VendorCommissionSummary>> GetVendorCommissionBreakdownAsync(
        DateTime? startDate = null,
        DateTime? endDate = null);

    /// <summary>
    /// Gets top products by commission
    /// </summary>
    Task<IList<ProductCommissionSummary>> GetTopProductsByCommissionAsync(
        int pageSize = 10,
        DateTime? startDate = null,
        DateTime? endDate = null);

    #endregion
}