using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Plugin.Integration.TrendyolMarketplace.Domain;
using Nop.Plugin.Integration.TrendyolMarketplace.Infrastructure;
using Nop.Plugin.Integration.TrendyolMarketplace.Services.Api.Models;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Seo;

namespace Nop.Plugin.Integration.TrendyolMarketplace.Services.Mapping;

/// <summary>
/// Brand mapping service implementation
/// </summary>
public class BrandMappingService : IBrandMappingService
{
    private readonly IRepository<TrendyolBrand> _brandRepository;
    private readonly IStaticCacheManager _staticCacheManager;
    private readonly IManufacturerService _manufacturerService;
    private readonly IUrlRecordService _urlRecordService;
    private readonly ISettingService _settingService;

    public BrandMappingService(
        IRepository<TrendyolBrand> brandRepository,
        IStaticCacheManager staticCacheManager,
        IManufacturerService manufacturerService,
        IUrlRecordService urlRecordService,
        ISettingService settingService)
    {
        _brandRepository = brandRepository;
        _staticCacheManager = staticCacheManager;
        _manufacturerService = manufacturerService;
        _urlRecordService = urlRecordService;
        _settingService = settingService;
    }

    /// <summary>
    /// Gets a brand mapping by ID
    /// </summary>
    public virtual async Task<TrendyolBrand> GetByIdAsync(int id)
    {
        if (id <= 0)
            return null;

        return await _brandRepository.GetByIdAsync(id);
    }

    /// <summary>
    /// Gets a brand mapping by Trendyol brand ID
    /// </summary>
    public virtual async Task<TrendyolBrand> GetByTrendyolIdAsync(long trendyolBrandId)
    {
        var key = _staticCacheManager.PrepareKeyForDefaultCache(
            TrendyolDefaults.BrandByTrendyolIdCacheKey, trendyolBrandId);

        return await _staticCacheManager.GetAsync(key, async () =>
        {
            var query = from b in _brandRepository.Table
                        where b.TrendyolBrandId == trendyolBrandId
                        select b;

            return await query.FirstOrDefaultAsync();
        });
    }

    /// <summary>
    /// Gets all brand mappings (paged)
    /// </summary>
    public virtual async Task<IPagedList<TrendyolBrand>> GetAllAsync(
        string searchTerm = null,
        bool? isMapped = null,
        int pageIndex = 0,
        int pageSize = int.MaxValue)
    {
        var query = _brandRepository.Table;

        if (!string.IsNullOrEmpty(searchTerm))
        {
            searchTerm = searchTerm.ToLower();
            query = query.Where(b => b.TrendyolBrandName.ToLower().Contains(searchTerm));
        }

        if (isMapped.HasValue)
        {
            if (isMapped.Value)
                query = query.Where(b => b.NopManufacturerId.HasValue && b.NopManufacturerId.Value > 0);
            else
                query = query.Where(b => !b.NopManufacturerId.HasValue || b.NopManufacturerId.Value <= 0);
        }

        query = query.OrderBy(b => b.TrendyolBrandName);

        return await query.ToPagedListAsync(pageIndex, pageSize);
    }

    /// <summary>
    /// Gets all mapped brands
    /// </summary>
    public virtual async Task<IList<TrendyolBrand>> GetAllMappedAsync()
    {
        var query = from b in _brandRepository.Table
                    where b.NopManufacturerId.HasValue && b.NopManufacturerId.Value > 0
                    select b;

        return await query.ToListAsync();
    }

    /// <summary>
    /// Inserts a brand mapping
    /// </summary>
    public virtual async Task InsertAsync(TrendyolBrand brand)
    {
        ArgumentNullException.ThrowIfNull(brand);

        brand.CreatedOnUtc = DateTime.UtcNow;

        await _brandRepository.InsertAsync(brand);
        await _staticCacheManager.RemoveByPrefixAsync(TrendyolDefaults.BrandPrefix);
    }

    /// <summary>
    /// Updates a brand mapping
    /// </summary>
    public virtual async Task UpdateAsync(TrendyolBrand brand)
    {
        ArgumentNullException.ThrowIfNull(brand);

        await _brandRepository.UpdateAsync(brand);
        await _staticCacheManager.RemoveByPrefixAsync(TrendyolDefaults.BrandPrefix);
    }

    /// <summary>
    /// Deletes a brand mapping
    /// </summary>
    public virtual async Task DeleteAsync(TrendyolBrand brand)
    {
        ArgumentNullException.ThrowIfNull(brand);

        await _brandRepository.DeleteAsync(brand);
        await _staticCacheManager.RemoveByPrefixAsync(TrendyolDefaults.BrandPrefix);
    }

    /// <summary>
    /// Syncs brands from Trendyol API
    /// </summary>
    public virtual async Task<int> SyncFromApiAsync(List<TrendyolBrandDto> brands)
    {
        var syncCount = 0;

        foreach (var brand in brands)
        {
            var existing = await GetByTrendyolIdAsync(brand.Id);
            if (existing == null)
            {
                // Insert new brand
                var newBrand = new TrendyolBrand
                {
                    TrendyolBrandId = brand.Id,
                    TrendyolBrandName = brand.Name,
                    IsAutoMapped = false,
                    AutoCreateIfNotExists = true,
                    CreatedOnUtc = DateTime.UtcNow
                };

                await _brandRepository.InsertAsync(newBrand);
                syncCount++;
            }
            else if (existing.TrendyolBrandName != brand.Name)
            {
                // Update brand name if changed
                existing.TrendyolBrandName = brand.Name;
                await _brandRepository.UpdateAsync(existing);
                syncCount++;
            }
        }

        await _staticCacheManager.RemoveByPrefixAsync(TrendyolDefaults.BrandPrefix);
        return syncCount;
    }

    /// <summary>
    /// Auto-maps brands by exact name match
    /// </summary>
    public virtual async Task<int> AutoMapBrandsAsync()
    {
        var mappedCount = 0;

        // Get all NopCommerce manufacturers
        var manufacturers = await _manufacturerService.GetAllManufacturersAsync(showHidden: true);
        var manufacturerDict = manufacturers.ToDictionary(m => m.Name.ToLower().Trim(), m => m.Id);

        // Get unmapped Trendyol brands
        var unmappedBrands = await GetAllAsync(isMapped: false);

        foreach (var trendyolBrand in unmappedBrands)
        {
            var searchName = trendyolBrand.TrendyolBrandName?.ToLower().Trim();
            if (string.IsNullOrEmpty(searchName))
                continue;

            if (manufacturerDict.TryGetValue(searchName, out var manufacturerId))
            {
                trendyolBrand.NopManufacturerId = manufacturerId;
                trendyolBrand.IsAutoMapped = true;
                await _brandRepository.UpdateAsync(trendyolBrand);
                mappedCount++;
            }
        }

        await _staticCacheManager.RemoveByPrefixAsync(TrendyolDefaults.BrandPrefix);
        return mappedCount;
    }

    /// <summary>
    /// Gets the NopCommerce manufacturer ID for a Trendyol brand
    /// Returns null if not mapped, creates manufacturer if auto-create is enabled
    /// </summary>
    public virtual async Task<int?> GetOrCreateNopManufacturerIdAsync(long trendyolBrandId, string brandName)
    {
        var brand = await GetByTrendyolIdAsync(trendyolBrandId);

        // If brand mapping exists and has a manufacturer ID, return it
        if (brand?.NopManufacturerId > 0)
            return brand.NopManufacturerId;

        // Check if we should auto-create
        var settings = await _settingService.LoadSettingAsync<TrendyolSettings>();
        if (!settings.AutoCreateManufacturers)
            return null;

        // Check if brand allows auto-create
        if (brand != null && !brand.AutoCreateIfNotExists)
            return null;

        // Resolve manufacturer name: use parameter, then existing mapping name, then fallback
        var manufacturerName = brandName ?? brand?.TrendyolBrandName ?? $"Brand-{trendyolBrandId}";

        // Create new manufacturer
        var manufacturer = new Manufacturer
        {
            Name = manufacturerName,
            Published = true,
            DisplayOrder = 0,
            CreatedOnUtc = DateTime.UtcNow,
            UpdatedOnUtc = DateTime.UtcNow
        };

        await _manufacturerService.InsertManufacturerAsync(manufacturer);

        // Generate URL slug
        var seName = await _urlRecordService.ValidateSeNameAsync(manufacturer, null, manufacturer.Name, true);
        await _urlRecordService.SaveSlugAsync(manufacturer, seName, 0);

        // Update brand mapping
        if (brand == null)
        {
            brand = new TrendyolBrand
            {
                TrendyolBrandId = trendyolBrandId,
                TrendyolBrandName = manufacturerName,
                NopManufacturerId = manufacturer.Id,
                IsAutoMapped = true,
                AutoCreateIfNotExists = true,
                CreatedOnUtc = DateTime.UtcNow
            };
            await _brandRepository.InsertAsync(brand);
        }
        else
        {
            brand.NopManufacturerId = manufacturer.Id;
            brand.IsAutoMapped = true;
            await _brandRepository.UpdateAsync(brand);
        }

        await _staticCacheManager.RemoveByPrefixAsync(TrendyolDefaults.BrandPrefix);
        return manufacturer.Id;
    }
}
