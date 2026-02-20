using Nop.Data.Mapping;
using Nop.Plugin.Integration.TrendyolMarketplace.Domain;

namespace Nop.Plugin.Integration.TrendyolMarketplace.Data;

/// <summary>
/// Provides entity-to-table name mapping for the plugin
/// </summary>
public class TrendyolNameCompatibility : INameCompatibility
{
    /// <summary>
    /// Static constructor - registers this compatibility class
    /// </summary>
    static TrendyolNameCompatibility()
    {
        if (!NameCompatibilityManager.AdditionalNameCompatibilities.Contains(typeof(TrendyolNameCompatibility)))
            NameCompatibilityManager.AdditionalNameCompatibilities.Add(typeof(TrendyolNameCompatibility));
    }

    /// <summary>
    /// Gets the mapping between entity types and their table names
    /// </summary>
    public Dictionary<Type, string> TableNames => new()
    {
        { typeof(TrendyolVendorCredential), "TrendyolVendorCredential" },
        { typeof(TrendyolCategory), "TrendyolCategory" },
        { typeof(TrendyolBrand), "TrendyolBrand" },
        { typeof(TrendyolAttribute), "TrendyolAttribute" },
        { typeof(TrendyolAttributeValue), "TrendyolAttributeValue" },
        { typeof(TrendyolProduct), "TrendyolProduct" },
        { typeof(TrendyolSyncLog), "TrendyolSyncLog" }
    };

    /// <summary>
    /// Gets the mapping between (entity type, property name) and column names
    /// </summary>
    public Dictionary<(Type, string), string> ColumnName => new();
}
