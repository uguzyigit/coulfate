using System.Reflection;
using System.Runtime.CompilerServices;
using Nop.Data.Mapping;

namespace Nop.Plugin.Marketplace.Core.Data;

/// <summary>
/// Module initializer that runs when the assembly is loaded
/// </summary>
internal static class ModuleInit
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        // Register name compatibility
        if (!NameCompatibilityManager.AdditionalNameCompatibilities.Contains(typeof(MarketplaceCoreNameCompatibility)))
            NameCompatibilityManager.AdditionalNameCompatibilities.Add(typeof(MarketplaceCoreNameCompatibility));

        // Reset NameCompatibilityManager to force re-initialization with our mappings
        try
        {
            var managerType = typeof(NameCompatibilityManager);

            // Reset _isInitialized flag
            var isInitializedField = managerType.GetField("_isInitialized", BindingFlags.NonPublic | BindingFlags.Static);
            isInitializedField?.SetValue(null, false);

            // Clear existing table names
            var tableNamesField = managerType.GetField("_tableNames", BindingFlags.NonPublic | BindingFlags.Static);
            if (tableNamesField?.GetValue(null) is System.Collections.IDictionary tableNames)
                tableNames.Clear();

            // Clear existing column names
            var columnNamesField = managerType.GetField("_columnName", BindingFlags.NonPublic | BindingFlags.Static);
            if (columnNamesField?.GetValue(null) is System.Collections.IDictionary columnNames)
                columnNames.Clear();

            // Clear loaded types
            var loadedForField = managerType.GetField("_loadedFor", BindingFlags.NonPublic | BindingFlags.Static);
            if (loadedForField?.GetValue(null) is System.Collections.IList loadedFor)
                loadedFor.Clear();
        }
        catch
        {
            // Ignore reflection errors
        }
    }
}
