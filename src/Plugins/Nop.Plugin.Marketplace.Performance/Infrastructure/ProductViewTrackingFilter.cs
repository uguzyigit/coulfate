using Marketplace.Abstractions.Domain;
using Marketplace.Abstractions.Services;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core;
using Nop.Services.Catalog;

namespace Nop.Plugin.Marketplace.Performance.Infrastructure;

public class ProductViewTrackingFilter : IAsyncActionFilter
{
    private readonly IProductInteractionService _interactionService;
    private readonly IProductService _productService;
    private readonly IWorkContext _workContext;

    public ProductViewTrackingFilter(
        IProductInteractionService interactionService,
        IProductService productService,
        IWorkContext workContext)
    {
        _interactionService = interactionService;
        _productService = productService;
        _workContext = workContext;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var result = await next();

        if (result.Exception != null)
            return;

        var routeData = context.RouteData;
        var controller = routeData.Values["controller"]?.ToString();
        var action = routeData.Values["action"]?.ToString();

        if (!string.Equals(controller, "Product", StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(action, "ProductDetails", StringComparison.OrdinalIgnoreCase))
            return;

        if (!context.ActionArguments.TryGetValue("productId", out var productIdObj) ||
            productIdObj is not int productId || productId <= 0)
            return;

        var product = await _productService.GetProductByIdAsync(productId);
        if (product == null || product.VendorId <= 0)
            return;

        var customer = await _workContext.GetCurrentCustomerAsync();
        await _interactionService.RecordInteractionAsync(
            productId, product.VendorId, customer?.Id, ProductInteractionType.View);
    }
}
