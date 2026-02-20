using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Marketplace.ShippingManager.Services;

namespace Nop.Plugin.Marketplace.ShippingManager.Infrastructure;

/// <summary>
/// NopStartup for dependency injection
/// Note: Entity table mappings are handled by Marketplace.Core's NameCompatibility
/// </summary>
public class NopStartup : INopStartup
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Register services
        services.AddScoped<IShippingProviderService, ShippingProviderService>();
        services.AddScoped<IVendorShippingService, VendorShippingService>();
    }

    public void Configure(IApplicationBuilder application)
    {
    }

    public int Order => 3000;
}
