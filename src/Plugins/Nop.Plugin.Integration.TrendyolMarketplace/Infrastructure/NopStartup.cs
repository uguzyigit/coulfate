using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Data.Mapping;
using Nop.Plugin.Integration.TrendyolMarketplace.Data;
using Nop.Plugin.Integration.TrendyolMarketplace.Infrastructure.EventConsumers;
using Nop.Plugin.Integration.TrendyolMarketplace.Services;
using Nop.Plugin.Integration.TrendyolMarketplace.Services.Api;
using Nop.Plugin.Integration.TrendyolMarketplace.Services.Import;
using Nop.Plugin.Integration.TrendyolMarketplace.Services.Mapping;
using Nop.Plugin.Integration.TrendyolMarketplace.Services.Sync;
using Nop.Services.Events;
using Nop.Web.Framework.Events;

namespace Nop.Plugin.Integration.TrendyolMarketplace.Infrastructure;

/// <summary>
/// Represents plugin startup configuration
/// </summary>
public class NopStartup : INopStartup
{
    /// <summary>
    /// Configure services
    /// </summary>
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Register name compatibility for database mapping
        if (!NameCompatibilityManager.AdditionalNameCompatibilities.Contains(typeof(TrendyolNameCompatibility)))
            NameCompatibilityManager.AdditionalNameCompatibilities.Add(typeof(TrendyolNameCompatibility));

        // Register vendor credential service
        services.AddScoped<IVendorCredentialService, VendorCredentialService>();

        // Register API client
        services.AddScoped<ITrendyolApiClient, TrendyolApiClient>();

        // Register mapping services
        services.AddScoped<ICategoryMappingService, CategoryMappingService>();
        services.AddScoped<IBrandMappingService, BrandMappingService>();
        services.AddScoped<IAttributeMappingService, AttributeMappingService>();

        // Register import services
        services.AddScoped<IProductImportService, ProductImportService>();
        services.AddScoped<IImageImportService, ImageImportService>();
        services.AddScoped<IVariantService, VariantService>();
        services.AddScoped<ISpecificationAttributeImportService, SpecificationAttributeImportService>();

        // Register sync services
        services.AddScoped<IStockSyncService, StockSyncService>();
        services.AddScoped<IPriceSyncService, PriceSyncService>();

        // Register sync log service
        services.AddScoped<ISyncLogService, SyncLogService>();

        // Register event consumers
        services.AddScoped<IConsumer<AdminMenuCreatedEvent>, AdminMenuEventConsumer>();

        // Register HttpClient for Trendyol API
        services.AddHttpClient("TrendyolApi", client =>
        {
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("Accept-Language", "tr-TR,tr;q=0.9,en-US;q=0.8,en;q=0.7");
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (compatible; NopCommerce/4.90; +http://www.nopcommerce.com)");
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
        });
    }

    /// <summary>
    /// Configure the application
    /// </summary>
    public void Configure(IApplicationBuilder application)
    {
        // Auto-migrate old API base URL to new gateway
        using var scope = application.ApplicationServices.CreateScope();
        try
        {
            var settingService = scope.ServiceProvider.GetRequiredService<Nop.Services.Configuration.ISettingService>();
            var settings = settingService.LoadSettingAsync<TrendyolSettings>().GetAwaiter().GetResult();

            if (settings?.ApiBaseUrl != null && settings.ApiBaseUrl.Contains("api.trendyol.com/sapigw"))
            {
                settings.ApiBaseUrl = TrendyolDefaults.DefaultApiBaseUrl;
                settingService.SaveSettingAsync(settings).GetAwaiter().GetResult();
            }
        }
        catch
        {
            // Migration is best-effort; don't block startup
        }

        // Run database migration scripts (add new columns etc.)
        try
        {
            var dataProvider = scope.ServiceProvider.GetRequiredService<Nop.Data.INopDataProvider>();
            foreach (var script in InstallationData.MigrationScripts)
            {
                try
                {
                    dataProvider.ExecuteNonQueryAsync(script).GetAwaiter().GetResult();
                }
                catch
                {
                    // Column may already exist — safe to ignore
                }
            }
        }
        catch
        {
            // Migration is best-effort; don't block startup
        }
    }

    /// <summary>
    /// Order of startup configuration
    /// </summary>
    public int Order => 3100;
}
