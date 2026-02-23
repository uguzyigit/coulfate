using Marketplace.Abstractions.Domain;
using Marketplace.Abstractions.Services;
using Nop.Core.Domain.Orders;
using Nop.Core.Events;
using Nop.Services.Catalog;
using Nop.Services.Events;

namespace Nop.Plugin.Marketplace.Performance.Infrastructure.EventConsumers;

public class AddToCartEventConsumer : IConsumer<EntityInsertedEvent<ShoppingCartItem>>
{
    private readonly IProductInteractionService _interactionService;
    private readonly IProductService _productService;

    public AddToCartEventConsumer(
        IProductInteractionService interactionService,
        IProductService productService)
    {
        _interactionService = interactionService;
        _productService = productService;
    }

    public async Task HandleEventAsync(EntityInsertedEvent<ShoppingCartItem> eventMessage)
    {
        var item = eventMessage.Entity;
        if (item == null)
            return;

        var product = await _productService.GetProductByIdAsync(item.ProductId);
        if (product == null)
            return;

        var type = item.ShoppingCartType switch
        {
            ShoppingCartType.ShoppingCart => ProductInteractionType.AddToCart,
            ShoppingCartType.Wishlist => ProductInteractionType.Wishlist,
            _ => (ProductInteractionType?)null
        };

        if (type == null)
            return;

        await _interactionService.RecordInteractionAsync(
            item.ProductId, product.VendorId, item.CustomerId, type.Value);
    }
}
