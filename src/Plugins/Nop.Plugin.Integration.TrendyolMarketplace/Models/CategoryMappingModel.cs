using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Integration.TrendyolMarketplace.Models;

/// <summary>
/// Represents the category mapping model
/// </summary>
public record CategoryMappingModel : BaseNopEntityModel
{
    public long TrendyolCategoryId { get; set; }

    [NopResourceDisplayName("Plugins.Integration.TrendyolMarketplace.CategoryMapping.TrendyolCategory")]
    public string TrendyolCategoryName { get; set; }

    public string TrendyolCategoryPath { get; set; }

    [NopResourceDisplayName("Plugins.Integration.TrendyolMarketplace.CategoryMapping.NopCategory")]
    public int? NopCategoryId { get; set; }

    public string NopCategoryName { get; set; }

    [NopResourceDisplayName("Plugins.Integration.TrendyolMarketplace.CategoryMapping.IsAutoMapped")]
    public bool IsAutoMapped { get; set; }

    public bool IsLeaf { get; set; }

    public IList<SelectListItem> AvailableNopCategories { get; set; } = new List<SelectListItem>();
}

/// <summary>
/// Represents category mapping list model
/// </summary>
public record CategoryMappingListModel : BasePagedListModel<CategoryMappingModel>
{
}

/// <summary>
/// Represents category mapping search model
/// </summary>
public record CategoryMappingSearchModel : BaseSearchModel
{
    [NopResourceDisplayName("Plugins.Integration.TrendyolMarketplace.CategoryMapping.SearchTrendyolCategory")]
    public string SearchTrendyolCategory { get; set; }

    [NopResourceDisplayName("Plugins.Integration.TrendyolMarketplace.CategoryMapping.UnmappedOnly")]
    public bool UnmappedOnly { get; set; }

    public bool LeafOnly { get; set; }
}
