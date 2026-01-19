using Nop.Core;
using Nop.Core.Caching;
using Nop.Data;
using Nop.Plugin.Marketplace.Core.Domain;
using Nop.Plugin.Marketplace.Core.Infrastructure;

namespace Nop.Plugin.Marketplace.Core.Services;

/// <summary>
/// Vendor extension service implementation
/// </summary>
public class VendorExtensionService : IVendorExtensionService
{
    private readonly IRepository<VendorExtension> _vendorExtensionRepository;
    private readonly IStaticCacheManager _staticCacheManager;

    public VendorExtensionService(
        IRepository<VendorExtension> vendorExtensionRepository,
        IStaticCacheManager staticCacheManager)
    {
        _vendorExtensionRepository = vendorExtensionRepository;
        _staticCacheManager = staticCacheManager;
    }

    public virtual async Task<VendorExtension> GetByIdAsync(int id)
    {
        return await _vendorExtensionRepository.GetByIdAsync(id, cache => default);
    }

    public virtual async Task<VendorExtension> GetByVendorIdAsync(int vendorId)
    {
        if (vendorId <= 0)
            return null;

        var key = _staticCacheManager.PrepareKeyForDefaultCache(
            MarketplaceCoreDefaults.VendorExtensionByVendorIdCacheKey, vendorId);

        return await _staticCacheManager.GetAsync(key, async () =>
        {
            var query = from ve in _vendorExtensionRepository.Table
                        where ve.VendorId == vendorId
                        select ve;

            return await query.FirstOrDefaultAsync();
        });
    }

    public virtual async Task<VendorExtension> GetByCodeAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return null;

        var key = _staticCacheManager.PrepareKeyForDefaultCache(
            MarketplaceCoreDefaults.VendorExtensionByCodeCacheKey, code);

        return await _staticCacheManager.GetAsync(key, async () =>
        {
            var query = from ve in _vendorExtensionRepository.Table
                        where ve.Code == code
                        select ve;

            return await query.FirstOrDefaultAsync();
        });
    }

    public virtual async Task<IPagedList<VendorExtension>> GetAllAsync(
        MarketplaceVendorStatus? status = null,
        int pageIndex = 0,
        int pageSize = int.MaxValue)
    {
        var query = _vendorExtensionRepository.Table;

        if (status.HasValue)
            query = query.Where(ve => ve.MarketplaceStatus == status.Value);

        query = query.OrderByDescending(ve => ve.CreatedOnUtc);

        return await query.ToPagedListAsync(pageIndex, pageSize);
    }

    public virtual async Task InsertAsync(VendorExtension vendorExtension)
    {
        ArgumentNullException.ThrowIfNull(vendorExtension);

        vendorExtension.CreatedOnUtc = DateTime.UtcNow;
        vendorExtension.UpdatedOnUtc = DateTime.UtcNow;

        await _vendorExtensionRepository.InsertAsync(vendorExtension);
        await _staticCacheManager.RemoveByPrefixAsync(MarketplaceCoreDefaults.VendorExtensionPrefix);
    }

    public virtual async Task UpdateAsync(VendorExtension vendorExtension)
    {
        ArgumentNullException.ThrowIfNull(vendorExtension);

        vendorExtension.UpdatedOnUtc = DateTime.UtcNow;

        await _vendorExtensionRepository.UpdateAsync(vendorExtension);
        await _staticCacheManager.RemoveByPrefixAsync(MarketplaceCoreDefaults.VendorExtensionPrefix);
    }

    public virtual async Task DeleteAsync(VendorExtension vendorExtension)
    {
        await _vendorExtensionRepository.DeleteAsync(vendorExtension);
        await _staticCacheManager.RemoveByPrefixAsync(MarketplaceCoreDefaults.VendorExtensionPrefix);
    }

    public virtual async Task<IPagedList<VendorExtension>> SearchAsync(
        string searchTerm,
        int pageIndex = 0,
        int pageSize = int.MaxValue)
    {
        var query = _vendorExtensionRepository.Table;

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(ve => ve.Code.Contains(searchTerm));
        }

        query = query.OrderByDescending(ve => ve.CreatedOnUtc);

        return await query.ToPagedListAsync(pageIndex, pageSize);
    }

    public virtual async Task<bool> CodeExistsAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return false;

        var query = from ve in _vendorExtensionRepository.Table
                    where ve.Code == code
                    select ve.Id;

        return await query.AnyAsync();
    }

    public virtual async Task<string> GenerateVendorCodeAsync(string companyName)
    {
        // Generate a vendor code from company name
        var baseCode = new string(companyName
            .ToUpperInvariant()
            .Where(c => char.IsLetterOrDigit(c))
            .Take(6)
            .ToArray());

        if (string.IsNullOrEmpty(baseCode))
            baseCode = "VENDOR";

        var code = baseCode;
        var counter = 1;

        // Check if code exists
        while (await CodeExistsAsync(code))
        {
            code = $"{baseCode}{counter:D3}";
            counter++;
        }

        return code;
    }
}
