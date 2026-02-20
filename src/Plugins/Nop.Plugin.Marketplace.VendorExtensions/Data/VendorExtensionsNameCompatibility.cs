using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using Nop.Plugin.Marketplace.VendorExtensions.Domain;

namespace Nop.Plugin.Marketplace.VendorExtensions.Data;

public class VendorExtensionsNameCompatibility : INameCompatibility
{
    public Dictionary<Type, string> TableNames => new()
    {
        { typeof(VendorCurrentAccount), "MarketplaceVendorCurrentAccount" },
        { typeof(VendorTransaction), "MarketplaceVendorTransaction" },
        { typeof(VendorMarketplaceSettings), "MarketplaceVendorSettings" }
    };

    public Dictionary<(Type, string), string> ColumnName => new();
}