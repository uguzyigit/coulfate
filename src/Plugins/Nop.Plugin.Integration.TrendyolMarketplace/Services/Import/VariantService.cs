using Nop.Core.Domain.Catalog;
using Nop.Plugin.Integration.TrendyolMarketplace.Services.Api.Models;
using Nop.Services.Catalog;
using Nop.Services.Logging;

namespace Nop.Plugin.Integration.TrendyolMarketplace.Services.Import;

/// <summary>
/// Variant service implementation for handling product attribute combinations
/// </summary>
public class VariantService : IVariantService
{
    private readonly IProductAttributeService _productAttributeService;
    private readonly IProductAttributeParser _productAttributeParser;
    private readonly IProductService _productService;
    private readonly ILogger _logger;

    public VariantService(
        IProductAttributeService productAttributeService,
        IProductAttributeParser productAttributeParser,
        IProductService productService,
        ILogger logger)
    {
        _productAttributeService = productAttributeService;
        _productAttributeParser = productAttributeParser;
        _productService = productService;
        _logger = logger;
    }

    /// <summary>
    /// Creates product attribute combinations from Trendyol variants
    /// </summary>
    public virtual async Task CreateVariantsAsync(int productId, List<TrendyolProductDto> variants)
    {
        if (productId <= 0 || variants == null || !variants.Any())
            return;

        // Analyze variants to find variant attributes (color, size, etc.)
        var variantAttributes = ExtractVariantAttributes(variants);

        if (!variantAttributes.Any())
        {
            // No variant attributes found, create simple combinations based on barcode
            foreach (var variant in variants)
            {
                await CreateSimpleCombinationAsync(productId, variant);
            }
            return;
        }

        // Create or get product attributes
        var attributeMappings = new Dictionary<string, ProductAttributeMapping>();

        foreach (var attrKvp in variantAttributes)
        {
            var attributeName = attrKvp.Key;
            var values = attrKvp.Value;

            // Get or create product attribute
            var productAttribute = await GetOrCreateProductAttributeAsync(attributeName);
            if (productAttribute == null)
                continue;

            // Create product attribute mapping
            var mapping = new ProductAttributeMapping
            {
                ProductId = productId,
                ProductAttributeId = productAttribute.Id,
                AttributeControlTypeId = (int)AttributeControlType.DropdownList,
                IsRequired = true,
                DisplayOrder = attributeMappings.Count
            };

            await _productAttributeService.InsertProductAttributeMappingAsync(mapping);
            attributeMappings[attributeName] = mapping;

            // Create attribute values
            var displayOrder = 0;
            foreach (var value in values.Distinct())
            {
                var attributeValue = new ProductAttributeValue
                {
                    ProductAttributeMappingId = mapping.Id,
                    Name = value,
                    AttributeValueTypeId = (int)AttributeValueType.Simple,
                    DisplayOrder = displayOrder++
                };

                await _productAttributeService.InsertProductAttributeValueAsync(attributeValue);
            }
        }

        // Create combinations
        foreach (var variant in variants)
        {
            await CreateCombinationAsync(productId, variant, attributeMappings);
        }
    }

    /// <summary>
    /// Updates existing product attribute combinations
    /// </summary>
    public virtual async Task UpdateVariantsAsync(int productId, List<TrendyolProductDto> variants)
    {
        // Get existing combinations
        var existingCombinations = await _productAttributeService.GetAllProductAttributeCombinationsAsync(productId);

        foreach (var variant in variants)
        {
            var existingCombination = existingCombinations.FirstOrDefault(c => c.Sku == variant.Barcode);

            if (existingCombination != null)
            {
                // Update existing combination
                existingCombination.StockQuantity = variant.Quantity;
                existingCombination.OverriddenPrice = variant.SalePrice;
                await _productAttributeService.UpdateProductAttributeCombinationAsync(existingCombination);
            }
            else
            {
                // This is a new variant, need to add it
                // For simplicity, we'll create a simple combination
                await CreateSimpleCombinationAsync(productId, variant);
            }
        }
    }

    /// <summary>
    /// Updates stock and price for a specific combination by SKU
    /// </summary>
    public virtual async Task UpdateCombinationAsync(int productId, string sku, int stock, decimal price)
    {
        var combinations = await _productAttributeService.GetAllProductAttributeCombinationsAsync(productId);
        var combination = combinations.FirstOrDefault(c => c.Sku == sku);

        if (combination != null)
        {
            combination.StockQuantity = stock;
            combination.OverriddenPrice = price;
            await _productAttributeService.UpdateProductAttributeCombinationAsync(combination);
        }
    }

    #region Private Methods

    // Exact attribute names we recognize as variant-defining attributes
    private static readonly HashSet<string> ColorAttributeExactNames = new(StringComparer.OrdinalIgnoreCase)
        { "Renk", "renk", "Color", "color" };
    private static readonly HashSet<string> SizeAttributeExactNames = new(StringComparer.OrdinalIgnoreCase)
        { "Beden", "beden", "Size", "size", "Numara", "numara", "Boyut", "boyut" };

    private Dictionary<string, List<string>> ExtractVariantAttributes(List<TrendyolProductDto> variants)
    {
        var result = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        // Step 1: Try direct Color/Size DTO fields first
        var colors = variants.Where(v => !string.IsNullOrEmpty(v.Color)).Select(v => v.Color).Distinct().ToList();
        var sizes = variants.Where(v => !string.IsNullOrEmpty(v.Size)).Select(v => v.Size).Distinct().ToList();

        if (colors.Any())
            result["Renk"] = colors;

        if (sizes.Any())
            result["Beden"] = sizes;

        // Step 2: If direct fields are empty, fall back to Attributes list (exact name match only)
        var needColorFromAttrs = !result.ContainsKey("Renk");
        var needSizeFromAttrs = !result.ContainsKey("Beden");

        if (needColorFromAttrs || needSizeFromAttrs)
        {
            foreach (var variant in variants)
            {
                if (variant.Attributes == null)
                    continue;

                foreach (var attr in variant.Attributes)
                {
                    if (string.IsNullOrEmpty(attr.AttributeName) || string.IsNullOrEmpty(attr.AttributeValue))
                        continue;

                    // Color from attributes (exact match only — excludes "Web Color" etc.)
                    if (needColorFromAttrs && ColorAttributeExactNames.Contains(attr.AttributeName))
                    {
                        if (!result.ContainsKey("Renk"))
                            result["Renk"] = new List<string>();

                        if (!result["Renk"].Contains(attr.AttributeValue))
                            result["Renk"].Add(attr.AttributeValue);
                    }

                    // Size from attributes (exact match only)
                    if (needSizeFromAttrs && SizeAttributeExactNames.Contains(attr.AttributeName))
                    {
                        if (!result.ContainsKey("Beden"))
                            result["Beden"] = new List<string>();

                        if (!result["Beden"].Contains(attr.AttributeValue))
                            result["Beden"].Add(attr.AttributeValue);
                    }
                }
            }
        }

        return result;
    }

    private async Task<ProductAttribute> GetOrCreateProductAttributeAsync(string attributeName)
    {
        // Search existing attributes
        var allAttributes = await _productAttributeService.GetAllProductAttributesAsync();
        var existing = allAttributes.FirstOrDefault(a =>
            a.Name.Equals(attributeName, StringComparison.OrdinalIgnoreCase));

        if (existing != null)
        {
            // Clean up legacy "Trendyol - ..." descriptions
            if (!string.IsNullOrEmpty(existing.Description) && existing.Description.StartsWith("Trendyol - "))
            {
                existing.Description = string.Empty;
                await _productAttributeService.UpdateProductAttributeAsync(existing);
            }
            return existing;
        }

        // Create new attribute
        var newAttribute = new ProductAttribute
        {
            Name = attributeName,
            Description = string.Empty
        };

        await _productAttributeService.InsertProductAttributeAsync(newAttribute);
        return newAttribute;
    }

    private async Task CreateSimpleCombinationAsync(int productId, TrendyolProductDto variant)
    {
        var combination = new ProductAttributeCombination
        {
            ProductId = productId,
            AttributesXml = string.Empty,
            StockQuantity = variant.Quantity,
            AllowOutOfStockOrders = false,
            Sku = variant.Barcode,
            Gtin = variant.Barcode,
            OverriddenPrice = variant.SalePrice
        };

        await _productAttributeService.InsertProductAttributeCombinationAsync(combination);
    }

    private async Task CreateCombinationAsync(
        int productId,
        TrendyolProductDto variant,
        Dictionary<string, ProductAttributeMapping> attributeMappings)
    {
        // Build attributes XML
        var attributesXml = string.Empty;

        // Resolve color: direct field first, then Attributes list fallback
        var resolvedColor = ResolveVariantColor(variant);
        if (!string.IsNullOrEmpty(resolvedColor) && attributeMappings.TryGetValue("Renk", out var colorMapping))
        {
            var colorValues = await _productAttributeService.GetProductAttributeValuesAsync(colorMapping.Id);
            var colorValue = colorValues.FirstOrDefault(v => v.Name == resolvedColor);

            if (colorValue != null)
            {
                attributesXml = _productAttributeParser.AddProductAttribute(
                    attributesXml, colorMapping, colorValue.Id.ToString());
            }
        }

        // Resolve size: direct field first, then Attributes list fallback
        var resolvedSize = ResolveVariantSize(variant);
        if (!string.IsNullOrEmpty(resolvedSize) && attributeMappings.TryGetValue("Beden", out var sizeMapping))
        {
            var sizeValues = await _productAttributeService.GetProductAttributeValuesAsync(sizeMapping.Id);
            var sizeValue = sizeValues.FirstOrDefault(v => v.Name == resolvedSize);

            if (sizeValue != null)
            {
                attributesXml = _productAttributeParser.AddProductAttribute(
                    attributesXml, sizeMapping, sizeValue.Id.ToString());
            }
        }

        // Create combination
        var combination = new ProductAttributeCombination
        {
            ProductId = productId,
            AttributesXml = attributesXml,
            StockQuantity = variant.Quantity,
            AllowOutOfStockOrders = false,
            Sku = variant.Barcode,
            Gtin = variant.Barcode,
            OverriddenPrice = variant.SalePrice
        };

        await _productAttributeService.InsertProductAttributeCombinationAsync(combination);
    }

    /// <summary>
    /// Resolves color value: direct Color field first, then Attributes list with exact name match
    /// </summary>
    private static string ResolveVariantColor(TrendyolProductDto variant)
    {
        if (!string.IsNullOrEmpty(variant.Color))
            return variant.Color;

        return variant.Attributes?
            .FirstOrDefault(a => !string.IsNullOrEmpty(a.AttributeName)
                              && !string.IsNullOrEmpty(a.AttributeValue)
                              && ColorAttributeExactNames.Contains(a.AttributeName))
            ?.AttributeValue;
    }

    /// <summary>
    /// Resolves size value: direct Size field first, then Attributes list with exact name match
    /// </summary>
    private static string ResolveVariantSize(TrendyolProductDto variant)
    {
        if (!string.IsNullOrEmpty(variant.Size))
            return variant.Size;

        return variant.Attributes?
            .FirstOrDefault(a => !string.IsNullOrEmpty(a.AttributeName)
                              && !string.IsNullOrEmpty(a.AttributeValue)
                              && SizeAttributeExactNames.Contains(a.AttributeName))
            ?.AttributeValue;
    }

    #endregion
}
