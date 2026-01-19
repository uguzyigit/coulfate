using System.Runtime.CompilerServices;
using Nop.Data.Mapping;

namespace Nop.Plugin.Marketplace.VendorApplication.Data;

/// <summary>
/// Module initializer that runs when the assembly is loaded
/// </summary>
internal static class ModuleInit
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        // Register name compatibility at assembly load time - before NameCompatibilityManager initializes
        if (!NameCompatibilityManager.AdditionalNameCompatibilities.Contains(typeof(VendorApplicationNameCompatibility)))
            NameCompatibilityManager.AdditionalNameCompatibilities.Add(typeof(VendorApplicationNameCompatibility));
    }
}
