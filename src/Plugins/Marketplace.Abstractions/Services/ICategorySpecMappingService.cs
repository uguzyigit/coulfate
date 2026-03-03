using Marketplace.Abstractions.Domain;
using Nop.Core.Domain.Catalog;

namespace Marketplace.Abstractions.Services;

/// <summary>
/// Service for managing category → specification attribute mappings.
/// Used to auto-add specification attributes to products when a vendor selects a category.
/// </summary>
public interface ICategorySpecMappingService
{
    Task<IList<CategorySpecificationAttribute>> GetByCategoryIdAsync(int categoryId);
    Task InsertAsync(CategorySpecificationAttribute mapping);
    Task DeleteAsync(CategorySpecificationAttribute mapping);
    Task<CategorySpecificationAttribute> GetByIdAsync(int id);
    Task<IList<SpecificationAttribute>> GetSpecificationAttributesByCategoryIdAsync(int categoryId);
    Task AutoAddSpecAttributesToProductAsync(int productId, int categoryId);
    Task RemoveAutoAddedSpecAttributesAsync(int productId, int categoryId);
}
