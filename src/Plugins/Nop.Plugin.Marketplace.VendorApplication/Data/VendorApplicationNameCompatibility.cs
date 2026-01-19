using Nop.Data.Mapping;
using Nop.Plugin.Marketplace.VendorApplication.Domain;

namespace Nop.Plugin.Marketplace.VendorApplication.Data;

/// <summary>
/// Maps entity types to database table names and column names
/// </summary>
public class VendorApplicationNameCompatibility : INameCompatibility
{
    public Dictionary<Type, string> TableNames => new()
    {
        { typeof(Domain.VendorApplication), "MarketplaceVendorApplication" },
        { typeof(VendorDocumentType), "MarketplaceVendorDocumentType" },
        { typeof(VendorApplicationDocument), "MarketplaceVendorApplicationDocument" }
    };

    public Dictionary<(Type, string), string> ColumnName => new()
    {
        // Map StatusId property to Status column in database
        { (typeof(Domain.VendorApplication), "StatusId"), "Status" }
    };
}
