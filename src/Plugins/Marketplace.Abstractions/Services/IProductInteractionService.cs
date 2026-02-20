using Marketplace.Abstractions.Domain;

namespace Marketplace.Abstractions.Services;

public interface IProductInteractionService
{
    Task RecordInteractionAsync(int productId, int vendorId, int? customerId, ProductInteractionType type);
    Task<int> GetInteractionCountAsync(int productId, ProductInteractionType type, DateTime sinceUtc);
}
