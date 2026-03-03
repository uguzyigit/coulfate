using Nop.Core.Domain.Catalog;
using Nop.Plugin.Marketplace.VendorExtensions.Models;

namespace Nop.Plugin.Marketplace.VendorExtensions.Services;

public interface IVendorProductService
{
    Task<VendorProductDashboardModel> GetDashboardStatsAsync(int vendorId);
    Task ApplyCreateDefaultsAsync(Product product);
    Task ApplyEditDefaultsAsync(Product product, Product existing);
    Task HandlePriceChangeAsync(Product product, decimal previousPrice);
    Task PrepareModelAsync(VendorProductModel model, Product product);
    Task<(bool isValid, string errorMessage)> ValidateVideoAsync(string fileName, decimal? durationSeconds);
    Task<bool> IsGtinUniqueAsync(string gtin, int excludeProductId = 0);
}
