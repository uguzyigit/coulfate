using Nop.Core;
using Nop.Plugin.Integration.TrendyolMarketplace.Domain;
using Nop.Plugin.Integration.TrendyolMarketplace.Services.Api.Models;

namespace Nop.Plugin.Integration.TrendyolMarketplace.Services.Mapping;

/// <summary>
/// Attribute mapping service interface
/// </summary>
public interface IAttributeMappingService
{
    /// <summary>
    /// Gets an attribute mapping by ID
    /// </summary>
    Task<TrendyolAttribute> GetByIdAsync(int id);

    /// <summary>
    /// Gets an attribute mapping by Trendyol category ID and attribute ID
    /// </summary>
    Task<TrendyolAttribute> GetByTrendyolIdsAsync(long trendyolCategoryId, long trendyolAttributeId);

    /// <summary>
    /// Gets all attribute mappings for a category (paged)
    /// </summary>
    Task<IPagedList<TrendyolAttribute>> GetAllAsync(
        long? trendyolCategoryId = null,
        string searchTerm = null,
        bool? isMapped = null,
        bool? isVariant = null,
        int pageIndex = 0,
        int pageSize = int.MaxValue);

    /// <summary>
    /// Gets all variant attributes for a category
    /// </summary>
    Task<IList<TrendyolAttribute>> GetVariantAttributesAsync(long trendyolCategoryId);

    /// <summary>
    /// Inserts an attribute mapping
    /// </summary>
    Task InsertAsync(TrendyolAttribute attribute);

    /// <summary>
    /// Updates an attribute mapping
    /// </summary>
    Task UpdateAsync(TrendyolAttribute attribute);

    /// <summary>
    /// Deletes an attribute mapping
    /// </summary>
    Task DeleteAsync(TrendyolAttribute attribute);

    /// <summary>
    /// Syncs attributes from Trendyol API for a specific category
    /// </summary>
    Task<int> SyncFromApiAsync(long categoryId, TrendyolCategoryAttributesResponse response);

    /// <summary>
    /// Gets an attribute value mapping by ID
    /// </summary>
    Task<TrendyolAttributeValue> GetValueByIdAsync(int id);

    /// <summary>
    /// Gets an attribute value mapping by Trendyol IDs
    /// </summary>
    Task<TrendyolAttributeValue> GetValueByTrendyolIdsAsync(int trendyolAttributeId, long trendyolValueId);

    /// <summary>
    /// Gets all attribute values for an attribute
    /// </summary>
    Task<IList<TrendyolAttributeValue>> GetValuesByAttributeIdAsync(int trendyolAttributeId);

    /// <summary>
    /// Inserts an attribute value mapping
    /// </summary>
    Task InsertValueAsync(TrendyolAttributeValue value);

    /// <summary>
    /// Updates an attribute value mapping
    /// </summary>
    Task UpdateValueAsync(TrendyolAttributeValue value);
}
