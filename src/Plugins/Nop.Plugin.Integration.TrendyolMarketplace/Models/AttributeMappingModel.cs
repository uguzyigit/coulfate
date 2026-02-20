using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Integration.TrendyolMarketplace.Domain;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Integration.TrendyolMarketplace.Models;

/// <summary>
/// Represents the attribute mapping model
/// </summary>
public record AttributeMappingModel : BaseNopEntityModel
{
    public long TrendyolCategoryId { get; set; }

    public string TrendyolCategoryName { get; set; }

    public long TrendyolAttributeId { get; set; }

    [NopResourceDisplayName("Plugins.Integration.TrendyolMarketplace.AttributeMapping.TrendyolAttribute")]
    public string TrendyolAttributeName { get; set; }

    [NopResourceDisplayName("Plugins.Integration.TrendyolMarketplace.AttributeMapping.IsRequired")]
    public bool IsRequired { get; set; }

    [NopResourceDisplayName("Plugins.Integration.TrendyolMarketplace.AttributeMapping.IsVariant")]
    public bool IsVariantAttribute { get; set; }

    public bool AllowCustomValue { get; set; }

    [NopResourceDisplayName("Plugins.Integration.TrendyolMarketplace.AttributeMapping.MappingType")]
    public AttributeMappingType MappingType { get; set; }

    public string MappingTypeDisplay { get; set; }

    public int? NopSpecificationAttributeId { get; set; }

    public string NopSpecificationAttributeName { get; set; }

    public int? NopProductAttributeId { get; set; }

    public string NopProductAttributeName { get; set; }

    public IList<SelectListItem> AvailableMappingTypes { get; set; } = new List<SelectListItem>();

    public IList<SelectListItem> AvailableSpecificationAttributes { get; set; } = new List<SelectListItem>();

    public IList<SelectListItem> AvailableProductAttributes { get; set; } = new List<SelectListItem>();

    // Attribute values
    public IList<AttributeValueMappingModel> Values { get; set; } = new List<AttributeValueMappingModel>();
}

/// <summary>
/// Represents attribute value mapping model
/// </summary>
public record AttributeValueMappingModel : BaseNopEntityModel
{
    public int TrendyolAttributeId { get; set; }

    public long TrendyolValueId { get; set; }

    public string TrendyolValueName { get; set; }

    public int? NopSpecificationAttributeOptionId { get; set; }

    public string NopSpecificationAttributeOptionName { get; set; }

    public int? NopProductAttributeValueId { get; set; }

    public string NopProductAttributeValueName { get; set; }
}

/// <summary>
/// Represents attribute mapping list model
/// </summary>
public record AttributeMappingListModel : BasePagedListModel<AttributeMappingModel>
{
}

/// <summary>
/// Represents attribute mapping search model
/// </summary>
public record AttributeMappingSearchModel : BaseSearchModel
{
    public long? TrendyolCategoryId { get; set; }

    public string SearchTerm { get; set; }

    public bool? IsMapped { get; set; }

    public bool? IsVariant { get; set; }

    public IList<SelectListItem> AvailableCategories { get; set; } = new List<SelectListItem>();
}
