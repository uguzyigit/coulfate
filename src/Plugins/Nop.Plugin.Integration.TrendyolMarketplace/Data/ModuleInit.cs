using System.Runtime.CompilerServices;

namespace Nop.Plugin.Integration.TrendyolMarketplace.Data;

/// <summary>
/// Module initializer to register entity types at assembly load time
/// </summary>
public static class ModuleInit
{
    /// <summary>
    /// Initializes the module - called automatically when the assembly is loaded
    /// </summary>
    [ModuleInitializer]
    public static void Initialize()
    {
        // This method is called when the assembly is loaded
        // Entity type registration happens in NopStartup via NameCompatibilityManager
    }
}
