using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Accounting.Parasut.Services;
using Nop.Plugin.Accounting.Parasut.Services.Auth;
using Nop.Plugin.Accounting.Parasut.Services.Api;
using Nop.Plugin.Accounting.Parasut.Services.Contact;
using Nop.Plugin.Accounting.Parasut.Services.Product;
using Nop.Plugin.Accounting.Parasut.Services.Invoice;
using Nop.Plugin.Accounting.Parasut.Tasks;

namespace Nop.Plugin.Accounting.Parasut.Infrastructure;

/// <summary>
/// Represents object for the configuring services on application startup
/// </summary>
public class NopStartup : INopStartup
{
    /// <summary>
    /// Add and configure services
    /// </summary>
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Register rate limiter as singleton
        services.AddSingleton<ParasutRateLimiter>();

        // Register HttpClient for Auth
        services.AddHttpClient("ParasutAuth");

        // Register HttpClient for API with custom handler
        services.AddHttpClient("ParasutApi")
            .ConfigureHttpClient(client =>
            {
                client.Timeout = TimeSpan.FromMinutes(5);
            });

        // Register services
        services.AddScoped<IParasutAuthService, ParasutAuthService>();
        services.AddScoped<IParasutApiClient, ParasutApiClient>();
        services.AddScoped<IParasutContactService, ParasutContactService>();
        services.AddScoped<IParasutProductService, ParasutProductService>();
        services.AddScoped<IParasutInvoiceService, ParasutInvoiceService>();

        // Register scheduled task
        services.AddScoped<ParasutEDocumentPollingTask>();
    }

    /// <summary>
    /// Configure the using of added middleware
    /// </summary>
    public void Configure(IApplicationBuilder application)
    {
        // No middleware needed
    }

    /// <summary>
    /// Gets order of this startup configuration implementation
    /// </summary>
    public int Order => 3000;
}
