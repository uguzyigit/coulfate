using Marketplace.Abstractions.Domain;
using Marketplace.Abstractions.Services;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Marketplace.VendorExtensions.Controllers;

[Area(AreaNames.ADMIN)]
[AuthorizeAdmin]
[AutoValidateAntiforgeryToken]
public class CategorySpecMappingController : BasePluginController
{
    private readonly ICategorySpecMappingService _categorySpecMappingService;

    public CategorySpecMappingController(ICategorySpecMappingService categorySpecMappingService)
    {
        _categorySpecMappingService = categorySpecMappingService;
    }

    [HttpPost]
    public async Task<IActionResult> Add(int categoryId, int specificationAttributeId, int displayOrder = 0)
    {
        if (categoryId <= 0 || specificationAttributeId <= 0)
            return Json(new { success = false, message = "Geçersiz parametreler." });

        // Check for duplicate
        var existing = await _categorySpecMappingService.GetByCategoryIdAsync(categoryId);
        if (existing.Any(m => m.SpecificationAttributeId == specificationAttributeId))
            return Json(new { success = false, message = "Bu belirtim niteliği zaten eşleştirilmiş." });

        var mapping = new CategorySpecificationAttribute
        {
            CategoryId = categoryId,
            SpecificationAttributeId = specificationAttributeId,
            DisplayOrder = displayOrder
        };

        await _categorySpecMappingService.InsertAsync(mapping);

        return Json(new { success = true });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var mapping = await _categorySpecMappingService.GetByIdAsync(id);
        if (mapping == null)
            return Json(new { success = false, message = "Eşleştirme bulunamadı." });

        await _categorySpecMappingService.DeleteAsync(mapping);

        return Json(new { success = true });
    }
}
