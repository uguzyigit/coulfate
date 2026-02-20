using Nop.Core.Domain.Catalog;
using Nop.Plugin.Integration.TrendyolMarketplace.Services.Api.Models;
using Nop.Services.Catalog;
using Nop.Services.Logging;

namespace Nop.Plugin.Integration.TrendyolMarketplace.Services.Import;

/// <summary>
/// Imports Trendyol product attributes as NopCommerce specification attributes
/// </summary>
public class SpecificationAttributeImportService : ISpecificationAttributeImportService
{
    private readonly ISpecificationAttributeService _specificationAttributeService;
    private readonly ILogger _logger;

    // Scoped cache: loaded once per DI scope (i.e. per import session)
    private Dictionary<string, SpecificationAttribute> _specAttributeCache;
    private Dictionary<int, List<SpecificationAttributeOption>> _specOptionCache;

    // Variant attribute names that should be skipped (handled by VariantService)
    private static readonly HashSet<string> _variantAttributeNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Renk", "Color", "Colour",
        "Beden", "Size", "Numara", "Boyut"
    };

    public SpecificationAttributeImportService(
        ISpecificationAttributeService specificationAttributeService,
        ILogger logger)
    {
        _specificationAttributeService = specificationAttributeService;
        _logger = logger;
    }

    public async Task<int> AssignSpecificationAttributesAsync(int productId, TrendyolProductDto productDto)
    {
        if (productId <= 0 || productDto.Attributes == null || !productDto.Attributes.Any())
            return 0;

        // Ensure caches are loaded
        await EnsureCacheLoadedAsync();

        var assignedCount = 0;

        // Get existing product specification attributes to avoid duplicates
        var existingProductSpecs = await _specificationAttributeService
            .GetProductSpecificationAttributesAsync(productId);

        var existingOptionIds = new HashSet<int>(
            existingProductSpecs.Select(psa => psa.SpecificationAttributeOptionId));

        foreach (var attr in productDto.Attributes)
        {
            // Skip empty attribute names or values
            if (string.IsNullOrWhiteSpace(attr.AttributeName) || string.IsNullOrWhiteSpace(attr.AttributeValue))
                continue;

            var attrName = attr.AttributeName.Trim();
            var attrValue = attr.AttributeValue.Trim();

            // Skip variant attributes (handled by VariantService)
            if (_variantAttributeNames.Contains(attrName))
                continue;

            try
            {
                // Find or create the specification attribute
                var specAttribute = await GetOrCreateSpecificationAttributeAsync(attrName);

                // Find or create the specification attribute option
                var specOption = await GetOrCreateSpecificationAttributeOptionAsync(specAttribute.Id, attrValue);

                // Skip if this product already has this option assigned
                if (existingOptionIds.Contains(specOption.Id))
                    continue;

                // Create product specification attribute mapping
                var productSpecAttribute = new ProductSpecificationAttribute
                {
                    ProductId = productId,
                    AttributeTypeId = (int)SpecificationAttributeType.Option,
                    SpecificationAttributeOptionId = specOption.Id,
                    AllowFiltering = true,
                    ShowOnProductPage = true,
                    DisplayOrder = assignedCount
                };

                await _specificationAttributeService.InsertProductSpecificationAttributeAsync(productSpecAttribute);

                existingOptionIds.Add(specOption.Id);
                assignedCount++;
            }
            catch (Exception ex)
            {
                await _logger.WarningAsync(
                    $"[TrendyolImport] Failed to assign specification attribute '{attrName}={attrValue}' to product {productId}: {ex.Message}");
            }
        }

        return assignedCount;
    }

    private async Task EnsureCacheLoadedAsync()
    {
        if (_specAttributeCache != null)
            return;

        _specAttributeCache = new Dictionary<string, SpecificationAttribute>(StringComparer.OrdinalIgnoreCase);
        _specOptionCache = new Dictionary<int, List<SpecificationAttributeOption>>();

        // Load all specification attributes
        var allAttributes = await _specificationAttributeService.GetAllSpecificationAttributesAsync(pageSize: int.MaxValue);
        foreach (var attr in allAttributes)
        {
            // Use first-wins for duplicate names
            _specAttributeCache.TryAdd(attr.Name, attr);
        }

        // Load all options for each attribute
        foreach (var attr in allAttributes)
        {
            var options = await _specificationAttributeService
                .GetSpecificationAttributeOptionsBySpecificationAttributeAsync(attr.Id);
            _specOptionCache[attr.Id] = options.ToList();
        }
    }

    private async Task<SpecificationAttribute> GetOrCreateSpecificationAttributeAsync(string name)
    {
        if (_specAttributeCache.TryGetValue(name, out var existing))
            return existing;

        // Create new specification attribute
        var specAttribute = new SpecificationAttribute
        {
            Name = name,
            DisplayOrder = _specAttributeCache.Count
        };

        await _specificationAttributeService.InsertSpecificationAttributeAsync(specAttribute);

        // Add to cache
        _specAttributeCache[name] = specAttribute;
        _specOptionCache[specAttribute.Id] = new List<SpecificationAttributeOption>();

        return specAttribute;
    }

    private async Task<SpecificationAttributeOption> GetOrCreateSpecificationAttributeOptionAsync(
        int specificationAttributeId, string value)
    {
        if (_specOptionCache.TryGetValue(specificationAttributeId, out var options))
        {
            var existing = options.FirstOrDefault(o =>
                string.Equals(o.Name, value, StringComparison.OrdinalIgnoreCase));

            if (existing != null)
                return existing;
        }

        // Create new option
        var option = new SpecificationAttributeOption
        {
            SpecificationAttributeId = specificationAttributeId,
            Name = value,
            DisplayOrder = options?.Count ?? 0
        };

        await _specificationAttributeService.InsertSpecificationAttributeOptionAsync(option);

        // Add to cache
        if (options != null)
            options.Add(option);
        else
            _specOptionCache[specificationAttributeId] = new List<SpecificationAttributeOption> { option };

        return option;
    }
}
