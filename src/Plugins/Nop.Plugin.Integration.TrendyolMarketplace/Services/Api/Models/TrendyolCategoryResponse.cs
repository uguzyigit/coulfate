using System.Text.Json.Serialization;

namespace Nop.Plugin.Integration.TrendyolMarketplace.Services.Api.Models;

/// <summary>
/// Trendyol categories API response wrapper
/// </summary>
public class TrendyolCategoriesResponse
{
    [JsonPropertyName("categories")]
    public List<TrendyolCategoryDto> Categories { get; set; } = new();
}

/// <summary>
/// Trendyol category DTO from API
/// </summary>
public class TrendyolCategoryDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("parentId")]
    public long? ParentId { get; set; }

    [JsonPropertyName("subCategories")]
    public List<TrendyolCategoryDto> SubCategories { get; set; } = new();
}

/// <summary>
/// Trendyol category attributes API response
/// </summary>
public class TrendyolCategoryAttributesResponse
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; }

    [JsonPropertyName("categoryAttributes")]
    public List<TrendyolCategoryAttributeDto> CategoryAttributes { get; set; } = new();
}

/// <summary>
/// Trendyol category attribute DTO
/// </summary>
public class TrendyolCategoryAttributeDto
{
    [JsonPropertyName("attribute")]
    public TrendyolAttributeInfoDto Attribute { get; set; }

    [JsonPropertyName("required")]
    public bool Required { get; set; }

    [JsonPropertyName("allowCustom")]
    public bool AllowCustom { get; set; }

    [JsonPropertyName("varianter")]
    public bool Varianter { get; set; }

    [JsonPropertyName("slicer")]
    public bool Slicer { get; set; }

    [JsonPropertyName("attributeValues")]
    public List<TrendyolAttributeValueDto> AttributeValues { get; set; } = new();
}

/// <summary>
/// Trendyol attribute info DTO
/// </summary>
public class TrendyolAttributeInfoDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }
}

/// <summary>
/// Trendyol attribute value DTO
/// </summary>
public class TrendyolAttributeValueDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }
}
