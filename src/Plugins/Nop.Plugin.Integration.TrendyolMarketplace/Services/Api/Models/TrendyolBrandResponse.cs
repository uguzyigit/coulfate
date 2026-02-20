using System.Text.Json.Serialization;

namespace Nop.Plugin.Integration.TrendyolMarketplace.Services.Api.Models;

/// <summary>
/// Trendyol brands API response wrapper
/// </summary>
public class TrendyolBrandsResponse
{
    [JsonPropertyName("brands")]
    public List<TrendyolBrandDto> Brands { get; set; } = new();

    [JsonPropertyName("page")]
    public int? Page { get; set; }

    [JsonPropertyName("size")]
    public int? Size { get; set; }

    [JsonPropertyName("totalPages")]
    public int? TotalPages { get; set; }

    [JsonPropertyName("totalElements")]
    public int? TotalElements { get; set; }
}

/// <summary>
/// Trendyol brand DTO from API
/// </summary>
public class TrendyolBrandDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }
}
