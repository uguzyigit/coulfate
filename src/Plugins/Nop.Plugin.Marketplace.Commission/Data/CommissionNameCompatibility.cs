using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using Nop.Plugin.Marketplace.Commission.Domain;

namespace Nop.Plugin.Marketplace.Commission.Data;

public class CommissionNameCompatibility : INameCompatibility
{
    public Dictionary<Type, string> TableNames => new()
    {
        { typeof(OrderCommission), "MarketplaceOrderCommission" },
        { typeof(CategoryCommission), "MarketplaceCategoryCommission" },
        { typeof(ProductCommission), "MarketplaceProductCommission" }
    };

    public Dictionary<(Type, string), string> ColumnName => new();
}