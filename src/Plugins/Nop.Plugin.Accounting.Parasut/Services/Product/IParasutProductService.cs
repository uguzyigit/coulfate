namespace Nop.Plugin.Accounting.Parasut.Services.Product;

public interface IParasutProductService
{
    Task<ProductMappings> EnsureProductsExistAsync();
    Task<string> GetProductIdForChargeTypeAsync(int chargeType);
}

public record ProductMappings(string CommissionProductId, string ShippingProductId, string PenaltyProductId);
