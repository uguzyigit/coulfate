using Marketplace.Abstractions.Domain;
using Marketplace.Abstractions.Services;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Services.Catalog;

namespace Nop.Plugin.Marketplace.VendorExtensions.Services;

public class CategorySpecMappingService : ICategorySpecMappingService
{
    private readonly IRepository<CategorySpecificationAttribute> _repository;
    private readonly ISpecificationAttributeService _specificationAttributeService;

    public CategorySpecMappingService(
        IRepository<CategorySpecificationAttribute> repository,
        ISpecificationAttributeService specificationAttributeService)
    {
        _repository = repository;
        _specificationAttributeService = specificationAttributeService;
    }

    public async Task<IList<CategorySpecificationAttribute>> GetByCategoryIdAsync(int categoryId)
    {
        return await _repository.GetAllAsync(q =>
            q.Where(m => m.CategoryId == categoryId)
             .OrderBy(m => m.DisplayOrder));
    }

    public async Task InsertAsync(CategorySpecificationAttribute mapping)
    {
        await _repository.InsertAsync(mapping);
    }

    public async Task DeleteAsync(CategorySpecificationAttribute mapping)
    {
        await _repository.DeleteAsync(mapping);
    }

    public async Task<CategorySpecificationAttribute> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<IList<SpecificationAttribute>> GetSpecificationAttributesByCategoryIdAsync(int categoryId)
    {
        var mappings = await GetByCategoryIdAsync(categoryId);
        var result = new List<SpecificationAttribute>();

        foreach (var mapping in mappings)
        {
            var specAttr = await _specificationAttributeService.GetSpecificationAttributeByIdAsync(mapping.SpecificationAttributeId);
            if (specAttr != null)
                result.Add(specAttr);
        }

        return result;
    }

    public async Task AutoAddSpecAttributesToProductAsync(int productId, int categoryId)
    {
        if (categoryId <= 0)
            return;

        var mappings = await GetByCategoryIdAsync(categoryId);
        if (!mappings.Any())
            return;

        // Get existing product spec attributes to avoid duplicates
        var existingPsas = await _specificationAttributeService.GetProductSpecificationAttributesAsync(productId);

        foreach (var mapping in mappings)
        {
            // Get the specification attribute options
            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(mapping.SpecificationAttributeId);

            // Check if product already has this spec attribute (by checking if any option of this spec attr is already mapped)
            var specAttrOptionIds = options.Select(o => o.Id).ToHashSet();
            var alreadyExists = existingPsas.Any(psa =>
                specAttrOptionIds.Contains(psa.SpecificationAttributeOptionId));

            if (alreadyExists)
                continue;

            if (!options.Any())
                continue;

            // Find or create a "-- Seçiniz --" placeholder option so the value is not pre-filled
            var placeholderName = "-- Seçiniz --";
            var placeholderOption = options.FirstOrDefault(o => o.Name == placeholderName);
            if (placeholderOption == null)
            {
                placeholderOption = new SpecificationAttributeOption
                {
                    SpecificationAttributeId = mapping.SpecificationAttributeId,
                    Name = placeholderName,
                    DisplayOrder = -1
                };
                await _specificationAttributeService.InsertSpecificationAttributeOptionAsync(placeholderOption);
            }

            var psa = new ProductSpecificationAttribute
            {
                ProductId = productId,
                AttributeTypeId = (int)SpecificationAttributeType.Option,
                SpecificationAttributeOptionId = placeholderOption.Id,
                AllowFiltering = false,
                ShowOnProductPage = false,
                DisplayOrder = mapping.DisplayOrder
            };

            await _specificationAttributeService.InsertProductSpecificationAttributeAsync(psa);
        }
    }

    public async Task RemoveAutoAddedSpecAttributesAsync(int productId, int categoryId)
    {
        if (categoryId <= 0)
            return;

        var mappings = await GetByCategoryIdAsync(categoryId);
        if (!mappings.Any())
            return;

        var existingPsas = await _specificationAttributeService.GetProductSpecificationAttributesAsync(productId);

        foreach (var mapping in mappings)
        {
            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(mapping.SpecificationAttributeId);
            var specAttrOptionIds = options.Select(o => o.Id).ToHashSet();

            var matchingPsas = existingPsas.Where(psa =>
                psa.AttributeTypeId == (int)SpecificationAttributeType.Option &&
                specAttrOptionIds.Contains(psa.SpecificationAttributeOptionId)).ToList();

            foreach (var psa in matchingPsas)
            {
                await _specificationAttributeService.DeleteProductSpecificationAttributeAsync(psa);
            }
        }
    }
}
