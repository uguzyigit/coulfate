using Marketplace.Abstractions.Domain;
using Nop.Core;

namespace Marketplace.Abstractions.Services;

public interface IProductPerformanceService
{
    Task<ProductPerformanceSnapshot> GetByProductIdAsync(int productId);
    Task<IPagedList<ProductPerformanceSnapshot>> GetAllAsync(int vendorId = 0, int pageIndex = 0, int pageSize = int.MaxValue);
    Task RecalculateAllAsync();
    Task CleanupOldInteractionsAsync(int keepDays = 60);
    Task<int> GetDiagnosticCountsAsync();
}
