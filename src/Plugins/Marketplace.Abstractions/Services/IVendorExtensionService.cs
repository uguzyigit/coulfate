using Marketplace.Abstractions.Domain;
using Nop.Core;

namespace Marketplace.Abstractions.Services;

/// <summary>
/// Vendor extension service interface
/// </summary>
public interface IVendorExtensionService
{
    Task<VendorExtension> GetByIdAsync(int id);
    Task<VendorExtension> GetByVendorIdAsync(int vendorId);
    Task<VendorExtension> GetByCodeAsync(string code);
    Task<IPagedList<VendorExtension>> GetAllAsync(
        MarketplaceVendorStatus? status = null,
        int pageIndex = 0,
        int pageSize = int.MaxValue);
    Task InsertAsync(VendorExtension vendorExtension);
    Task UpdateAsync(VendorExtension vendorExtension);
    Task DeleteAsync(VendorExtension vendorExtension);
    Task<IPagedList<VendorExtension>> SearchAsync(
        string searchTerm,
        int pageIndex = 0,
        int pageSize = int.MaxValue);
    Task<bool> CodeExistsAsync(string code);
    Task<string> GenerateVendorCodeAsync(string companyName);
}
