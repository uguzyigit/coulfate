using Nop.Plugin.Integration.TrendyolMarketplace.Domain;
using Nop.Plugin.Integration.TrendyolMarketplace.Services.Api.Models;

namespace Nop.Plugin.Integration.TrendyolMarketplace.Services.Import;

/// <summary>
/// Product import service interface
/// </summary>
public interface IProductImportService
{
    /// <summary>
    /// Gets a Trendyol product mapping by ID
    /// </summary>
    Task<TrendyolProduct> GetByIdAsync(int id);

    /// <summary>
    /// Gets a Trendyol product mapping by barcode and vendor ID
    /// </summary>
    Task<TrendyolProduct> GetByBarcodeAsync(string barcode, int vendorId);

    /// <summary>
    /// Gets all Trendyol products for a vendor (paged)
    /// </summary>
    Task<Nop.Core.IPagedList<TrendyolProduct>> GetAllAsync(
        int? vendorId = null,
        ImportStatus? status = null,
        string searchTerm = null,
        int pageIndex = 0,
        int pageSize = int.MaxValue);

    /// <summary>
    /// Gets product statistics for a vendor
    /// </summary>
    Task<(int Total, int Imported, int Pending, int Failed, int Skipped, int Deactivated)> GetStatisticsAsync(int vendorId);

    /// <summary>
    /// Imports products from Trendyol API for a vendor
    /// </summary>
    Task<(int Success, int Failed, int Skipped)> ImportProductsAsync(TrendyolVendorCredential credential, int syncLogId);

    /// <summary>
    /// Imports a single product from Trendyol DTO
    /// </summary>
    Task<(bool Success, string Message)> ImportSingleProductAsync(TrendyolProductDto productDto, int vendorId);

    /// <summary>
    /// Updates an existing Trendyol product mapping
    /// </summary>
    Task UpdateAsync(TrendyolProduct product);

    /// <summary>
    /// Deletes a Trendyol product mapping
    /// </summary>
    Task DeleteAsync(TrendyolProduct product);
}
