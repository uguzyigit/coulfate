using Marketplace.Abstractions.Domain;
using Nop.Data.Mapping;

namespace Nop.Plugin.Marketplace.Performance.Data;

public class PerformanceNameCompatibility : INameCompatibility
{
    static PerformanceNameCompatibility()
    {
        if (!NameCompatibilityManager.AdditionalNameCompatibilities.Contains(typeof(PerformanceNameCompatibility)))
            NameCompatibilityManager.AdditionalNameCompatibilities.Add(typeof(PerformanceNameCompatibility));
    }

    public Dictionary<Type, string> TableNames => new()
    {
        { typeof(ProductInteraction), "MarketplaceProductInteraction" },
        { typeof(ProductPerformanceSnapshot), "MarketplaceProductPerformanceSnapshot" }
    };

    public Dictionary<(Type, string), string> ColumnName => new();
}
