using Nop.Core;
using Nop.Core.Caching;
using Nop.Data;
using Nop.Plugin.Integration.TrendyolMarketplace.Domain;
using Nop.Plugin.Integration.TrendyolMarketplace.Infrastructure;
using Nop.Plugin.Integration.TrendyolMarketplace.Services.Api.Models;

namespace Nop.Plugin.Integration.TrendyolMarketplace.Services.Mapping;

/// <summary>
/// Attribute mapping service implementation
/// </summary>
public class AttributeMappingService : IAttributeMappingService
{
    private readonly IRepository<TrendyolAttribute> _attributeRepository;
    private readonly IRepository<TrendyolAttributeValue> _attributeValueRepository;
    private readonly IStaticCacheManager _staticCacheManager;

    public AttributeMappingService(
        IRepository<TrendyolAttribute> attributeRepository,
        IRepository<TrendyolAttributeValue> attributeValueRepository,
        IStaticCacheManager staticCacheManager)
    {
        _attributeRepository = attributeRepository;
        _attributeValueRepository = attributeValueRepository;
        _staticCacheManager = staticCacheManager;
    }

    /// <summary>
    /// Gets an attribute mapping by ID
    /// </summary>
    public virtual async Task<TrendyolAttribute> GetByIdAsync(int id)
    {
        if (id <= 0)
            return null;

        return await _attributeRepository.GetByIdAsync(id);
    }

    /// <summary>
    /// Gets an attribute mapping by Trendyol category ID and attribute ID
    /// </summary>
    public virtual async Task<TrendyolAttribute> GetByTrendyolIdsAsync(long trendyolCategoryId, long trendyolAttributeId)
    {
        var query = from a in _attributeRepository.Table
                    where a.TrendyolCategoryId == trendyolCategoryId &&
                          a.TrendyolAttributeId == trendyolAttributeId
                    select a;

        return await query.FirstOrDefaultAsync();
    }

    /// <summary>
    /// Gets all attribute mappings for a category (paged)
    /// </summary>
    public virtual async Task<IPagedList<TrendyolAttribute>> GetAllAsync(
        long? trendyolCategoryId = null,
        string searchTerm = null,
        bool? isMapped = null,
        bool? isVariant = null,
        int pageIndex = 0,
        int pageSize = int.MaxValue)
    {
        var query = _attributeRepository.Table;

        if (trendyolCategoryId.HasValue)
            query = query.Where(a => a.TrendyolCategoryId == trendyolCategoryId.Value);

        if (!string.IsNullOrEmpty(searchTerm))
        {
            searchTerm = searchTerm.ToLower();
            query = query.Where(a => a.TrendyolAttributeName.ToLower().Contains(searchTerm));
        }

        if (isMapped.HasValue)
        {
            if (isMapped.Value)
                query = query.Where(a => a.MappingType != AttributeMappingType.None);
            else
                query = query.Where(a => a.MappingType == AttributeMappingType.None);
        }

        if (isVariant.HasValue)
            query = query.Where(a => a.IsVariantAttribute == isVariant.Value);

        query = query.OrderBy(a => a.TrendyolCategoryId).ThenBy(a => a.TrendyolAttributeName);

        return await query.ToPagedListAsync(pageIndex, pageSize);
    }

    /// <summary>
    /// Gets all attribute mappings for a category (all types)
    /// </summary>
    public virtual async Task<IList<TrendyolAttribute>> GetByCategoryAsync(long trendyolCategoryId)
    {
        var query = from a in _attributeRepository.Table
                    where a.TrendyolCategoryId == trendyolCategoryId
                    orderby a.TrendyolAttributeName
                    select a;

        return await query.ToListAsync();
    }

    /// <summary>
    /// Gets all variant attributes for a category
    /// </summary>
    public virtual async Task<IList<TrendyolAttribute>> GetVariantAttributesAsync(long trendyolCategoryId)
    {
        var key = _staticCacheManager.PrepareKeyForDefaultCache(
            TrendyolDefaults.AttributesByCategoryCacheKey, trendyolCategoryId);

        return await _staticCacheManager.GetAsync(key, async () =>
        {
            var query = from a in _attributeRepository.Table
                        where a.TrendyolCategoryId == trendyolCategoryId && a.IsVariantAttribute
                        orderby a.TrendyolAttributeName
                        select a;

            return await query.ToListAsync();
        });
    }

    /// <summary>
    /// Inserts an attribute mapping
    /// </summary>
    public virtual async Task InsertAsync(TrendyolAttribute attribute)
    {
        ArgumentNullException.ThrowIfNull(attribute);

        attribute.CreatedOnUtc = DateTime.UtcNow;

        await _attributeRepository.InsertAsync(attribute);
        await _staticCacheManager.RemoveByPrefixAsync(TrendyolDefaults.AttributePrefix);
    }

    /// <summary>
    /// Updates an attribute mapping
    /// </summary>
    public virtual async Task UpdateAsync(TrendyolAttribute attribute)
    {
        ArgumentNullException.ThrowIfNull(attribute);

        await _attributeRepository.UpdateAsync(attribute);
        await _staticCacheManager.RemoveByPrefixAsync(TrendyolDefaults.AttributePrefix);
    }

    /// <summary>
    /// Deletes an attribute mapping
    /// </summary>
    public virtual async Task DeleteAsync(TrendyolAttribute attribute)
    {
        ArgumentNullException.ThrowIfNull(attribute);

        await _attributeRepository.DeleteAsync(attribute);
        await _staticCacheManager.RemoveByPrefixAsync(TrendyolDefaults.AttributePrefix);
    }

    /// <summary>
    /// Syncs attributes from Trendyol API for a specific category
    /// </summary>
    public virtual async Task<int> SyncFromApiAsync(long categoryId, TrendyolCategoryAttributesResponse response)
    {
        if (response?.CategoryAttributes == null)
            return 0;

        var syncCount = 0;

        foreach (var categoryAttribute in response.CategoryAttributes)
        {
            var attrInfo = categoryAttribute.Attribute;
            if (attrInfo == null)
                continue;

            var existing = await GetByTrendyolIdsAsync(categoryId, attrInfo.Id);

            if (existing == null)
            {
                // Insert new attribute
                var newAttribute = new TrendyolAttribute
                {
                    TrendyolCategoryId = categoryId,
                    TrendyolAttributeId = attrInfo.Id,
                    TrendyolAttributeName = attrInfo.Name,
                    IsRequired = categoryAttribute.Required,
                    IsVariantAttribute = categoryAttribute.Varianter,
                    AllowCustomValue = categoryAttribute.AllowCustom,
                    MappingType = AttributeMappingType.None,
                    CreatedOnUtc = DateTime.UtcNow
                };

                await _attributeRepository.InsertAsync(newAttribute);
                syncCount++;

                // Sync attribute values
                if (categoryAttribute.AttributeValues != null)
                {
                    foreach (var value in categoryAttribute.AttributeValues)
                    {
                        var newValue = new TrendyolAttributeValue
                        {
                            TrendyolAttributeId = newAttribute.Id,
                            TrendyolValueId = value.Id,
                            TrendyolValueName = value.Name,
                            CreatedOnUtc = DateTime.UtcNow
                        };
                        await _attributeValueRepository.InsertAsync(newValue);
                    }
                }
            }
            else
            {
                // Update existing attribute if needed
                var needsUpdate = false;

                if (existing.TrendyolAttributeName != attrInfo.Name)
                {
                    existing.TrendyolAttributeName = attrInfo.Name;
                    needsUpdate = true;
                }

                if (existing.IsRequired != categoryAttribute.Required)
                {
                    existing.IsRequired = categoryAttribute.Required;
                    needsUpdate = true;
                }

                if (existing.IsVariantAttribute != categoryAttribute.Varianter)
                {
                    existing.IsVariantAttribute = categoryAttribute.Varianter;
                    needsUpdate = true;
                }

                if (existing.AllowCustomValue != categoryAttribute.AllowCustom)
                {
                    existing.AllowCustomValue = categoryAttribute.AllowCustom;
                    needsUpdate = true;
                }

                if (needsUpdate)
                {
                    await _attributeRepository.UpdateAsync(existing);
                    syncCount++;
                }

                // Sync attribute values
                if (categoryAttribute.AttributeValues != null)
                {
                    foreach (var value in categoryAttribute.AttributeValues)
                    {
                        var existingValue = await GetValueByTrendyolIdsAsync(existing.Id, value.Id);
                        if (existingValue == null)
                        {
                            var newValue = new TrendyolAttributeValue
                            {
                                TrendyolAttributeId = existing.Id,
                                TrendyolValueId = value.Id,
                                TrendyolValueName = value.Name,
                                CreatedOnUtc = DateTime.UtcNow
                            };
                            await _attributeValueRepository.InsertAsync(newValue);
                        }
                        else if (existingValue.TrendyolValueName != value.Name)
                        {
                            existingValue.TrendyolValueName = value.Name;
                            await _attributeValueRepository.UpdateAsync(existingValue);
                        }
                    }
                }
            }
        }

        await _staticCacheManager.RemoveByPrefixAsync(TrendyolDefaults.AttributePrefix);
        return syncCount;
    }

    /// <summary>
    /// Gets an attribute value mapping by ID
    /// </summary>
    public virtual async Task<TrendyolAttributeValue> GetValueByIdAsync(int id)
    {
        if (id <= 0)
            return null;

        return await _attributeValueRepository.GetByIdAsync(id);
    }

    /// <summary>
    /// Gets an attribute value mapping by Trendyol IDs
    /// </summary>
    public virtual async Task<TrendyolAttributeValue> GetValueByTrendyolIdsAsync(int trendyolAttributeId, long trendyolValueId)
    {
        var query = from v in _attributeValueRepository.Table
                    where v.TrendyolAttributeId == trendyolAttributeId &&
                          v.TrendyolValueId == trendyolValueId
                    select v;

        return await query.FirstOrDefaultAsync();
    }

    /// <summary>
    /// Gets all attribute values for an attribute
    /// </summary>
    public virtual async Task<IList<TrendyolAttributeValue>> GetValuesByAttributeIdAsync(int trendyolAttributeId)
    {
        var query = from v in _attributeValueRepository.Table
                    where v.TrendyolAttributeId == trendyolAttributeId
                    orderby v.TrendyolValueName
                    select v;

        return await query.ToListAsync();
    }

    /// <summary>
    /// Inserts an attribute value mapping
    /// </summary>
    public virtual async Task InsertValueAsync(TrendyolAttributeValue value)
    {
        ArgumentNullException.ThrowIfNull(value);

        value.CreatedOnUtc = DateTime.UtcNow;
        await _attributeValueRepository.InsertAsync(value);
    }

    /// <summary>
    /// Updates an attribute value mapping
    /// </summary>
    public virtual async Task UpdateValueAsync(TrendyolAttributeValue value)
    {
        ArgumentNullException.ThrowIfNull(value);
        await _attributeValueRepository.UpdateAsync(value);
    }
}
