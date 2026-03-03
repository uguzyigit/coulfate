using Marketplace.Abstractions.Domain;
using Marketplace.Abstractions.Services;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Catalog;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Marketplace.VendorExtensions.Components;

public class CategorySpecificationMappingViewComponent : NopViewComponent
{
    private readonly ICategorySpecMappingService _categorySpecMappingService;
    private readonly ISpecificationAttributeService _specificationAttributeService;

    public CategorySpecificationMappingViewComponent(
        ICategorySpecMappingService categorySpecMappingService,
        ISpecificationAttributeService specificationAttributeService)
    {
        _categorySpecMappingService = categorySpecMappingService;
        _specificationAttributeService = specificationAttributeService;
    }

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        // additionalData is the CategoryModel from admin category edit page
        if (additionalData is not CategoryModel categoryModel || categoryModel.Id == 0)
            return Content(string.Empty);

        var categoryId = categoryModel.Id;

        // Load existing mappings (with error protection if table doesn't exist yet)
        IList<CategorySpecificationAttribute> mappings;
        try
        {
            mappings = await _categorySpecMappingService.GetByCategoryIdAsync(categoryId);
        }
        catch
        {
            // Table may not exist yet
            return Content(string.Empty);
        }
        var mappingItems = new List<CategorySpecMappingItemModel>();
        foreach (var m in mappings)
        {
            var specAttr = await _specificationAttributeService.GetSpecificationAttributeByIdAsync(m.SpecificationAttributeId);
            if (specAttr != null)
            {
                mappingItems.Add(new CategorySpecMappingItemModel
                {
                    Id = m.Id,
                    SpecificationAttributeId = m.SpecificationAttributeId,
                    SpecificationAttributeName = specAttr.Name,
                    DisplayOrder = m.DisplayOrder
                });
            }
        }

        // Load available specification attributes for dropdown
        var allSpecAttrs = await _specificationAttributeService.GetSpecificationAttributesWithOptionsAsync();
        var availableSpecAttrs = allSpecAttrs
            .Where(sa => !mappings.Any(m => m.SpecificationAttributeId == sa.Id))
            .Select(sa => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Text = sa.Name,
                Value = sa.Id.ToString()
            }).ToList();

        var model = new CategorySpecMappingModel
        {
            CategoryId = categoryId,
            Mappings = mappingItems,
            AvailableSpecificationAttributes = availableSpecAttrs
        };

        return View("~/Plugins/Marketplace.VendorExtensions/Views/Admin/CategorySpecificationMapping.cshtml", model);
    }
}

public class CategorySpecMappingModel
{
    public int CategoryId { get; set; }
    public List<CategorySpecMappingItemModel> Mappings { get; set; } = new();
    public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> AvailableSpecificationAttributes { get; set; } = new();
}

public class CategorySpecMappingItemModel
{
    public int Id { get; set; }
    public int SpecificationAttributeId { get; set; }
    public string SpecificationAttributeName { get; set; }
    public int DisplayOrder { get; set; }
}
