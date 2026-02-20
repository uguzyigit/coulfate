using Marketplace.Abstractions.Domain;
using Marketplace.Abstractions.Services;
using Nop.Data;

namespace Nop.Plugin.Marketplace.Performance.Services;

public class ProductInteractionService : IProductInteractionService
{
    private readonly IRepository<ProductInteraction> _repository;

    public ProductInteractionService(IRepository<ProductInteraction> repository)
    {
        _repository = repository;
    }

    public virtual async Task RecordInteractionAsync(int productId, int vendorId, int? customerId, ProductInteractionType type)
    {
        // Spam protection: skip if same customer+product+type within last 5 minutes
        if (customerId.HasValue)
        {
            var fiveMinAgo = DateTime.UtcNow.AddMinutes(-5);
            var typeInt = (int)type;
            var exists = await _repository.Table
                .Where(x => x.ProductId == productId
                    && x.CustomerId == customerId.Value
                    && x.InteractionType == typeInt
                    && x.CreatedOnUtc >= fiveMinAgo)
                .AnyAsync();

            if (exists)
                return;
        }

        await _repository.InsertAsync(new ProductInteraction
        {
            ProductId = productId,
            VendorId = vendorId,
            CustomerId = customerId,
            InteractionType = (int)type,
            CreatedOnUtc = DateTime.UtcNow
        });
    }

    public virtual async Task<int> GetInteractionCountAsync(int productId, ProductInteractionType type, DateTime sinceUtc)
    {
        var typeInt = (int)type;
        return await _repository.Table
            .Where(x => x.ProductId == productId && x.InteractionType == typeInt && x.CreatedOnUtc >= sinceUtc)
            .CountAsync();
    }
}
