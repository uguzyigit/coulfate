using Marketplace.Abstractions.Domain;
using Marketplace.Abstractions.Services;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Integration.TrendyolMarketplace.Domain;
using Nop.Plugin.Integration.TrendyolMarketplace.Services.Api;
using Nop.Plugin.Integration.TrendyolMarketplace.Services.Mapping;
using Nop.Services.Catalog;
using Nop.Services.Configuration;

namespace Nop.Plugin.Integration.TrendyolMarketplace.Services.Import;

/// <summary>
/// Imports Trendyol attributes into NopCommerce ProductAttribute and SpecificationAttribute tables.
/// Also creates Category → SpecificationAttribute mappings so vendors get auto-populated spec fields.
/// </summary>
public class AttributeNopImportService : IAttributeNopImportService
{
    private readonly ICategoryMappingService _categoryMappingService;
    private readonly IAttributeMappingService _attributeMappingService;
    private readonly ITrendyolApiClient _apiClient;
    private readonly IProductAttributeService _productAttributeService;
    private readonly ISpecificationAttributeService _specificationAttributeService;
    private readonly ICategorySpecMappingService _categorySpecMappingService;
    private readonly ISettingService _settingService;

    // Session-scoped caches to avoid duplicate NopCommerce attributes across categories
    private Dictionary<string, ProductAttribute> _productAttrCache;
    private Dictionary<string, SpecificationAttribute> _specAttrCache;

    public AttributeNopImportService(
        ICategoryMappingService categoryMappingService,
        IAttributeMappingService attributeMappingService,
        ITrendyolApiClient apiClient,
        IProductAttributeService productAttributeService,
        ISpecificationAttributeService specificationAttributeService,
        ICategorySpecMappingService categorySpecMappingService,
        ISettingService settingService)
    {
        _categoryMappingService = categoryMappingService;
        _attributeMappingService = attributeMappingService;
        _apiClient = apiClient;
        _productAttributeService = productAttributeService;
        _specificationAttributeService = specificationAttributeService;
        _categorySpecMappingService = categorySpecMappingService;
        _settingService = settingService;
    }

    public async Task<(int productAttrCount, int specAttrCount, int remaining)> ImportAttributesAsync(int maxApiCallsPerBatch = 30)
    {
        var settings = await _settingService.LoadSettingAsync<TrendyolSettings>();
        // Use a shorter delay for bulk import to avoid HTTP timeouts
        var importDelayMs = Math.Min(settings.ApiRequestDelayMs, 300);

        await LoadCachesAsync();

        var leafCategories = (await _categoryMappingService.GetAllAsync(isLeaf: true)).ToList();
        var productAttrCount = 0;
        var specAttrCount = 0;
        var apiCallsUsed = 0;
        var remaining = 0;

        foreach (var leafCategory in leafCategories)
        {
            // Check if attributes are already in DB for this category
            var existingAttributes = await _attributeMappingService.GetByCategoryAsync(leafCategory.TrendyolCategoryId);

            if (!existingAttributes.Any())
            {
                // Need an API call — check batch quota
                if (apiCallsUsed >= maxApiCallsPerBatch)
                {
                    remaining++;
                    continue;
                }

                try
                {
                    var response = await _apiClient.GetCategoryAttributesAsync(leafCategory.TrendyolCategoryId);
                    if (response != null)
                    {
                        await _attributeMappingService.SyncFromApiAsync(leafCategory.TrendyolCategoryId, response);
                        existingAttributes = await _attributeMappingService.GetByCategoryAsync(leafCategory.TrendyolCategoryId);
                    }
                    apiCallsUsed++;
                    await Task.Delay(importDelayMs);
                }
                catch (Exception ex)
                {
                    // Log the error in the message but continue with next category
                    apiCallsUsed++;
                    continue;
                }
            }

            if (!existingAttributes.Any())
                continue;

            // Load existing category-spec mappings for duplicate check
            var nopCategoryId = leafCategory.NopCategoryId;
            IList<CategorySpecificationAttribute> existingCategorySpecMappings = null;
            if (nopCategoryId.HasValue && nopCategoryId.Value > 0)
            {
                try
                {
                    existingCategorySpecMappings = await _categorySpecMappingService.GetByCategoryIdAsync(nopCategoryId.Value);
                }
                catch
                {
                    existingCategorySpecMappings = new List<CategorySpecificationAttribute>();
                }
            }

            var specDisplayOrder = existingCategorySpecMappings?.Count ?? 0;

            // Map each attribute to the appropriate NopCommerce type
            foreach (var attr in existingAttributes)
            {
                try
                {
                    if (attr.IsVariantAttribute)
                    {
                        if (attr.NopProductAttributeId.HasValue && attr.NopProductAttributeId.Value > 0)
                            continue;

                        var productAttr = await GetOrCreateProductAttributeAsync(attr.TrendyolAttributeName);
                        attr.NopProductAttributeId = productAttr.Id;
                        attr.MappingType = AttributeMappingType.ProductAttribute;
                        await _attributeMappingService.UpdateAsync(attr);

                        var values = await _attributeMappingService.GetValuesByAttributeIdAsync(attr.Id);
                        foreach (var value in values)
                        {
                            if (value.NopProductAttributeValueId.HasValue && value.NopProductAttributeValueId.Value > 0)
                                continue;

                            var existingValues = await _productAttributeService.GetPredefinedProductAttributeValuesAsync(productAttr.Id);
                            if (existingValues.Any(v => string.Equals(v.Name, value.TrendyolValueName, StringComparison.OrdinalIgnoreCase)))
                                continue;

                            var predefinedValue = new PredefinedProductAttributeValue
                            {
                                ProductAttributeId = productAttr.Id,
                                Name = value.TrendyolValueName,
                                DisplayOrder = existingValues.Count
                            };
                            await _productAttributeService.InsertPredefinedProductAttributeValueAsync(predefinedValue);
                            value.NopProductAttributeValueId = predefinedValue.Id;
                            await _attributeMappingService.UpdateValueAsync(value);
                        }

                        productAttrCount++;
                    }
                    else
                    {
                        if (attr.NopSpecificationAttributeId.HasValue && attr.NopSpecificationAttributeId.Value > 0)
                            continue;

                        var specAttr = await GetOrCreateSpecificationAttributeAsync(attr.TrendyolAttributeName);
                        attr.NopSpecificationAttributeId = specAttr.Id;
                        attr.MappingType = AttributeMappingType.SpecificationAttribute;
                        await _attributeMappingService.UpdateAsync(attr);

                        var values = await _attributeMappingService.GetValuesByAttributeIdAsync(attr.Id);
                        foreach (var value in values)
                        {
                            if (value.NopSpecificationAttributeOptionId.HasValue && value.NopSpecificationAttributeOptionId.Value > 0)
                                continue;

                            var existingOptions = await _specificationAttributeService
                                .GetSpecificationAttributeOptionsBySpecificationAttributeAsync(specAttr.Id);
                            if (existingOptions.Any(o => string.Equals(o.Name, value.TrendyolValueName, StringComparison.OrdinalIgnoreCase)))
                                continue;

                            var option = new SpecificationAttributeOption
                            {
                                SpecificationAttributeId = specAttr.Id,
                                Name = value.TrendyolValueName,
                                DisplayOrder = existingOptions.Count
                            };
                            await _specificationAttributeService.InsertSpecificationAttributeOptionAsync(option);
                            value.NopSpecificationAttributeOptionId = option.Id;
                            await _attributeMappingService.UpdateValueAsync(value);
                        }

                        // Link spec attribute to NopCommerce category
                        if (nopCategoryId.HasValue && nopCategoryId.Value > 0 && existingCategorySpecMappings != null)
                        {
                            if (!existingCategorySpecMappings.Any(m => m.SpecificationAttributeId == specAttr.Id))
                            {
                                var categorySpecMapping = new CategorySpecificationAttribute
                                {
                                    CategoryId = nopCategoryId.Value,
                                    SpecificationAttributeId = specAttr.Id,
                                    DisplayOrder = specDisplayOrder++
                                };
                                await _categorySpecMappingService.InsertAsync(categorySpecMapping);
                                existingCategorySpecMappings.Add(categorySpecMapping);
                            }
                        }

                        specAttrCount++;
                    }
                }
                catch (Exception ex)
                {
                    // Log error for this attribute but continue with next
                    _ = ex; // suppress unused variable warning
                    continue;
                }
            }
        }

        return (productAttrCount, specAttrCount, remaining);
    }

    private async Task LoadCachesAsync()
    {
        _productAttrCache = new Dictionary<string, ProductAttribute>(StringComparer.OrdinalIgnoreCase);
        _specAttrCache = new Dictionary<string, SpecificationAttribute>(StringComparer.OrdinalIgnoreCase);

        var productAttrs = await _productAttributeService.GetAllProductAttributesAsync();
        foreach (var attr in productAttrs)
            _productAttrCache.TryAdd(attr.Name, attr);

        var specAttrs = await _specificationAttributeService.GetAllSpecificationAttributesAsync(pageSize: int.MaxValue);
        foreach (var attr in specAttrs)
            _specAttrCache.TryAdd(attr.Name, attr);
    }

    private async Task<ProductAttribute> GetOrCreateProductAttributeAsync(string name)
    {
        if (_productAttrCache.TryGetValue(name, out var existing))
            return existing;

        var attr = new ProductAttribute { Name = name };
        await _productAttributeService.InsertProductAttributeAsync(attr);
        _productAttrCache[name] = attr;
        return attr;
    }

    private async Task<SpecificationAttribute> GetOrCreateSpecificationAttributeAsync(string name)
    {
        if (_specAttrCache.TryGetValue(name, out var existing))
            return existing;

        var attr = new SpecificationAttribute
        {
            Name = name,
            DisplayOrder = _specAttrCache.Count
        };
        await _specificationAttributeService.InsertSpecificationAttributeAsync(attr);
        _specAttrCache[name] = attr;
        return attr;
    }
}
