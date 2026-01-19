using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Data.Mapping;
using Nop.Plugin.Marketplace.Core.Data;
using Nop.Plugin.Marketplace.Core.Services;

namespace Nop.Plugin.Marketplace.Core.Infrastructure;

/// <summary>
/// Represents object for configuring services on application startup
/// </summary>
public class NopStartup : INopStartup
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Register name compatibility for table mapping
        if (!NameCompatibilityManager.AdditionalNameCompatibilities.Contains(typeof(MarketplaceCoreNameCompatibility)))
            NameCompatibilityManager.AdditionalNameCompatibilities.Add(typeof(MarketplaceCoreNameCompatibility));

        // Register services
        services.AddScoped<IVendorExtensionService, VendorExtensionService>();

        // TODO: Add more services as you implement them:
        // services.AddScoped<IVendorProductService, VendorProductService>();
        // services.AddScoped<IVendorOrderLineService, VendorOrderLineService>();
        // services.AddScoped<IVendorBalanceService, VendorBalanceService>();
    }

    public void Configure(IApplicationBuilder application)
    {
    }

    public int Order => 3000;
}
