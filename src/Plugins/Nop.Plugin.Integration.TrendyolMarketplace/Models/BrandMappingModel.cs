using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Integration.TrendyolMarketplace.Models;

/// <summary>
/// Represents the brand mapping model
/// </summary>
public record BrandMappingModel : BaseNopEntityModel
{
    public long TrendyolBrandId { get; set; }

    [NopResourceDisplayName("Plugins.Integration.TrendyolMarketplace.BrandMapping.TrendyolBrand")]
    public string TrendyolBrandName { get; set; }

    [NopResourceDisplayName("Plugins.Integration.TrendyolMarketplace.BrandMapping.NopManufacturer")]
    public int? NopManufacturerId { get; set; }

    public string NopManufacturerName { get; set; }

    [NopResourceDisplayName("Plugins.Integration.TrendyolMarketplace.BrandMapping.IsAutoMapped")]
    public bool IsAutoMapped { get; set; }

    public bool AutoCreateIfNotExists { get; set; }

    public IList<SelectListItem> AvailableNopManufacturers { get; set; } = new List<SelectListItem>();
}

/// <summary>
/// Represents brand mapping list model
/// </summary>
public record BrandMappingListModel : BasePagedListModel<BrandMappingModel>
{
}

/// <summary>
/// Represents brand mapping search model
/// </summary>
public record BrandMappingSearchModel : BaseSearchModel
{
    [NopResourceDisplayName("Plugins.Integration.TrendyolMarketplace.BrandMapping.SearchTrendyolBrand")]
    public string SearchTrendyolBrand { get; set; }

    [NopResourceDisplayName("Plugins.Integration.TrendyolMarketplace.BrandMapping.UnmappedOnly")]
    public bool UnmappedOnly { get; set; }
}
