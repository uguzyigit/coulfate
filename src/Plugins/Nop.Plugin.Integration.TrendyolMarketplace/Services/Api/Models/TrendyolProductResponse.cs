using System.Text.Json.Serialization;

namespace Nop.Plugin.Integration.TrendyolMarketplace.Services.Api.Models;

/// <summary>
/// Trendyol products API response wrapper
/// </summary>
public class TrendyolProductsResponse
{
    [JsonPropertyName("page")]
    public int Page { get; set; }

    [JsonPropertyName("size")]
    public int Size { get; set; }

    [JsonPropertyName("totalPages")]
    public int TotalPages { get; set; }

    [JsonPropertyName("totalElements")]
    public int TotalElements { get; set; }

    [JsonPropertyName("content")]
    public List<TrendyolProductDto> Content { get; set; } = new();
}

/// <summary>
/// Trendyol product DTO from API
/// </summary>
public class TrendyolProductDto
{
    [JsonPropertyName("barcode")]
    public string Barcode { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("productMainId")]
    public string ProductMainId { get; set; }

    [JsonPropertyName("brandId")]
    public long BrandId { get; set; }

    [JsonPropertyName("brand")]
    public string BrandName { get; set; }

    [JsonPropertyName("categoryId")]
    public long CategoryId { get; set; }

    [JsonPropertyName("categoryName")]
    public string CategoryName { get; set; }

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("stockCode")]
    public string StockCode { get; set; }

    [JsonPropertyName("dimensionalWeight")]
    public decimal? DimensionalWeight { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("currencyType")]
    public string CurrencyType { get; set; }

    [JsonPropertyName("listPrice")]
    public decimal ListPrice { get; set; }

    [JsonPropertyName("salePrice")]
    public decimal SalePrice { get; set; }

    [JsonPropertyName("vatRate")]
    public int VatRate { get; set; }

    [JsonPropertyName("cargoCompanyId")]
    public int? CargoCompanyId { get; set; }

    [JsonPropertyName("images")]
    public List<TrendyolImageDto> Images { get; set; } = new();

    [JsonPropertyName("attributes")]
    public List<TrendyolProductAttributeDto> Attributes { get; set; } = new();

    [JsonPropertyName("productUrl")]
    public string ProductUrl { get; set; }

    [JsonPropertyName("approved")]
    public bool Approved { get; set; }

    [JsonPropertyName("archived")]
    public bool Archived { get; set; }

    [JsonPropertyName("locked")]
    public bool Locked { get; set; }

    [JsonPropertyName("onSale")]
    public bool OnSale { get; set; }

    [JsonPropertyName("blacklisted")]
    public bool Blacklisted { get; set; }

    [JsonPropertyName("hasActiveCampaign")]
    public bool HasActiveCampaign { get; set; }

    [JsonPropertyName("createDateTime")]
    public long? CreateDateTime { get; set; }

    [JsonPropertyName("lastUpdateDate")]
    public long? LastUpdateDate { get; set; }

    [JsonPropertyName("gender")]
    public string Gender { get; set; }

    [JsonPropertyName("color")]
    public string Color { get; set; }

    [JsonPropertyName("size")]
    public string Size { get; set; }

    [JsonPropertyName("ageGroup")]
    public string AgeGroup { get; set; }

    [JsonPropertyName("deliveryOption")]
    public TrendyolDeliveryOptionDto DeliveryOption { get; set; }

    [JsonPropertyName("stockUnitType")]
    public string StockUnitType { get; set; }
}

/// <summary>
/// Trendyol image DTO
/// </summary>
public class TrendyolImageDto
{
    [JsonPropertyName("url")]
    public string Url { get; set; }
}

/// <summary>
/// Trendyol product attribute DTO
/// </summary>
public class TrendyolProductAttributeDto
{
    [JsonPropertyName("attributeId")]
    public long AttributeId { get; set; }

    [JsonPropertyName("attributeName")]
    public string AttributeName { get; set; }

    [JsonPropertyName("attributeValueId")]
    public long? AttributeValueId { get; set; }

    [JsonPropertyName("attributeValue")]
    public string AttributeValue { get; set; }
}

/// <summary>
/// Trendyol delivery option DTO
/// </summary>
public class TrendyolDeliveryOptionDto
{
    [JsonPropertyName("deliveryDuration")]
    public int? DeliveryDuration { get; set; }

    [JsonPropertyName("fastDeliveryType")]
    public string FastDeliveryType { get; set; }
}
