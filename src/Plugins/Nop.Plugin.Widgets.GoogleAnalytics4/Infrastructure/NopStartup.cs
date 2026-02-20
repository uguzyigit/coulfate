using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Widgets.GoogleAnalytics4.Services;

namespace Nop.Plugin.Widgets.GoogleAnalytics4.Infrastructure;

/// <summary>
/// Represents the plugin startup configuration
/// </summary>
public class NopStartup : INopStartup
{
    /// <summary>
    /// Add and configure any of the middleware
    /// </summary>
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Register services
        services.AddScoped<IGA4TrackingService, GA4TrackingService>();

        // Register HttpClient for Measurement Protocol
        services.AddHttpClient();
    }

    /// <summary>
    /// Configure the using of added middleware
    /// </summary>
    public void Configure(IApplicationBuilder application)
    {
    }

    /// <summary>
    /// Gets order of this startup configuration implementation
    /// </summary>
    public int Order => 3001;
}
