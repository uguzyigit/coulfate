using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Shipping.PttKargo.Api;
using Nop.Plugin.Shipping.PttKargo.Services;

namespace Nop.Plugin.Shipping.PttKargo.Infrastructure;

/// <summary>
/// PTT Kargo plugin startup configuration
/// </summary>
public class NopStartup : INopStartup
{
    public int Order => 3000;

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // HTTP Client for SOAP requests
        services.AddHttpClient<PttSoapClient>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(60);
            client.DefaultRequestHeaders.Add("Accept", "application/soap+xml");
        });

        // Services
        services.AddScoped<IPttApiService, PttApiService>();
    }

    public void Configure(IApplicationBuilder application)
    {
        // No additional configuration needed
    }
}
