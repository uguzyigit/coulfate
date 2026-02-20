using Marketplace.Abstractions.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core;
using Nop.Core.Events;
using Nop.Core.Infrastructure;
using Nop.Data.Mapping;
using Nop.Plugin.Marketplace.VendorExtensions.Services;
using Nop.Plugin.Marketplace.VendorExtensions.Infrastructure.EventConsumers;
using Nop.Services.Events;
using Nop.Web.Framework.Events;

namespace Nop.Plugin.Marketplace.VendorExtensions.Infrastructure;

public class NopStartup : INopStartup
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Register services
        services.AddScoped<IVendorAccountService, VendorAccountService>();

        // Register table name compatibility (use AdditionalNameCompatibilities to avoid conflicts)
        if (!NameCompatibilityManager.AdditionalNameCompatibilities.Contains(typeof(Data.VendorExtensionsNameCompatibility)))
            NameCompatibilityManager.AdditionalNameCompatibilities.Add(typeof(Data.VendorExtensionsNameCompatibility));

        // Register event consumers
        services.AddScoped<IConsumer<EntityInsertedEvent<BaseEntity>>, OrderCommissionCreatedConsumer>();
        services.AddScoped<IConsumer<AdminMenuCreatedEvent>, AdminMenuEventConsumer>();
        services.AddScoped<IConsumer<AdminMenuCreatedEvent>, VendorMenuEventConsumer>();
    }

    public void Configure(IApplicationBuilder application)
    {
    }

    public int Order => 3000;
}
