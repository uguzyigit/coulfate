using Marketplace.Abstractions.Domain;
using Marketplace.Abstractions.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Domain.Catalog;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Mapping;
using Nop.Plugin.Marketplace.Performance.Data;
using Nop.Plugin.Marketplace.Performance.Services;
using Nop.Services.Catalog;

namespace Nop.Plugin.Marketplace.Performance.Infrastructure;

public class NopStartup : INopStartup
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Register name compatibility for performance table mapping
        if (!NameCompatibilityManager.AdditionalNameCompatibilities.Contains(typeof(PerformanceNameCompatibility)))
            NameCompatibilityManager.AdditionalNameCompatibilities.Add(typeof(PerformanceNameCompatibility));

        // Performance tracking services
        services.AddScoped<IProductInteractionService, ProductInteractionService>();
        services.AddScoped<IProductPerformanceService, ProductPerformanceService>();

        // Scheduled task
        services.AddScoped<PerformanceScoreCalculationTask>();

        // Action filter for view tracking
        services.AddScoped<ProductViewTrackingFilter>();
        services.Configure<MvcOptions>(options =>
        {
            options.Filters.AddService<ProductViewTrackingFilter>();
        });

        // IProductService decorator — wrap the existing registration
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IProductService));
        if (descriptor?.ImplementationType != null)
        {
            services.Remove(descriptor);
            services.AddScoped(descriptor.ImplementationType);
            var innerType = descriptor.ImplementationType;
            services.AddScoped<IProductService>(sp => new PerformanceProductService(
                (IProductService)sp.GetRequiredService(innerType),
                sp.GetRequiredService<IRepository<ProductPerformanceSnapshot>>(),
                sp.GetRequiredService<IRepository<Product>>()
            ));
        }
    }

    public void Configure(IApplicationBuilder application)
    {
        // Run pending migrations on startup (idempotent — safe to run every time)
        MigrationRunner.RunAsync(application.ApplicationServices).GetAwaiter().GetResult();
    }

    public int Order => 5000;
}
