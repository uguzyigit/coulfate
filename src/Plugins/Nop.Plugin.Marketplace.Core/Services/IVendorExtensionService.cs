using Nop.Core;
using Nop.Plugin.Marketplace.Core.Domain;

namespace Nop.Plugin.Marketplace.Core.Services;

/// <summary>
/// Vendor extension service interface
/// Works with VendorExtension entity which extends NopCommerce's native Vendor
/// </summary>
public interface IVendorExtensionService
{
    /// <summary>
    /// Gets a vendor extension by its ID
    /// </summary>
    Task<VendorExtension> GetByIdAsync(int id);

    /// <summary>
    /// Gets a vendor extension by the NopCommerce Vendor ID
    /// </summary>
    Task<VendorExtension> GetByVendorIdAsync(int vendorId);

    /// <summary>
    /// Gets a vendor extension by vendor code
    /// </summary>
    Task<VendorExtension> GetByCodeAsync(string code);

    /// <summary>
    /// Gets all vendor extensions
    /// </summary>
    Task<IPagedList<VendorExtension>> GetAllAsync(
        MarketplaceVendorStatus? status = null,
        int pageIndex = 0,
        int pageSize = int.MaxValue);

    /// <summary>
    /// Inserts a vendor extension
    /// </summary>
    Task InsertAsync(VendorExtension vendorExtension);

    /// <summary>
    /// Updates a vendor extension
    /// </summary>
    Task UpdateAsync(VendorExtension vendorExtension);

    /// <summary>
    /// Deletes a vendor extension
    /// </summary>
    Task DeleteAsync(VendorExtension vendorExtension);

    /// <summary>
    /// Searches vendor extensions
    /// </summary>
    Task<IPagedList<VendorExtension>> SearchAsync(
        string searchTerm,
        int pageIndex = 0,
        int pageSize = int.MaxValue);

    /// <summary>
    /// Checks if a vendor code already exists
    /// </summary>
    Task<bool> CodeExistsAsync(string code);

    /// <summary>
    /// Generates a unique vendor code from company name
    /// </summary>
    Task<string> GenerateVendorCodeAsync(string companyName);
}
