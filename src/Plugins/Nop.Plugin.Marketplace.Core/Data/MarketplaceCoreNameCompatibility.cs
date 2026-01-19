using Nop.Data.Mapping;
using Nop.Plugin.Marketplace.Core.Domain;

namespace Nop.Plugin.Marketplace.Core.Data;

/// <summary>
/// Maps entity types to database table names
/// </summary>
public class MarketplaceCoreNameCompatibility : INameCompatibility
{
    /// <summary>
    /// Static constructor - runs before any instance is created
    /// </summary>
    static MarketplaceCoreNameCompatibility()
    {
        if (!NameCompatibilityManager.AdditionalNameCompatibilities.Contains(typeof(MarketplaceCoreNameCompatibility)))
            NameCompatibilityManager.AdditionalNameCompatibilities.Add(typeof(MarketplaceCoreNameCompatibility));
    }

    public Dictionary<Type, string> TableNames => new()
    {
        { typeof(VendorExtension), "MarketplaceVendorExtension" },
        { typeof(VendorProduct), "MarketplaceVendorProduct" },
        { typeof(VendorOrderLine), "MarketplaceVendorOrderLine" },
        { typeof(VendorBalance), "MarketplaceVendorBalance" },
        { typeof(VendorShippingAccount), "MarketplaceVendorShippingAccount" },
        { typeof(VendorPayout), "MarketplaceVendorPayout" },
        { typeof(CategoryCommission), "MarketplaceCategoryCommission" },
        { typeof(ProductCommission), "MarketplaceProductCommission" },
        { typeof(OrderCommission), "MarketplaceOrderCommission" },
        { typeof(VendorCurrentAccount), "MarketplaceVendorCurrentAccount" },
        { typeof(VendorTransaction), "MarketplaceVendorTransaction" }
    };

    public Dictionary<(Type, string), string> ColumnName => new();
}
