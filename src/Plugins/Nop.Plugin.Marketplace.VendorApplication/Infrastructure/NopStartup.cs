using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Data.Mapping;
using Nop.Plugin.Marketplace.VendorApplication.Data;
using Nop.Plugin.Marketplace.VendorApplication.Infrastructure.EventConsumers;
using Nop.Plugin.Marketplace.VendorApplication.Services;
using Nop.Services.Events;
using Nop.Web.Framework.Events;

namespace Nop.Plugin.Marketplace.VendorApplication.Infrastructure;

public class NopStartup : INopStartup
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Register name compatibility for table mapping - MUST be added to AdditionalNameCompatibilities
        // so it's discovered during NameCompatibilityManager initialization
        if (!NameCompatibilityManager.AdditionalNameCompatibilities.Contains(typeof(VendorApplicationNameCompatibility)))
            NameCompatibilityManager.AdditionalNameCompatibilities.Add(typeof(VendorApplicationNameCompatibility));

        // Register services
        services.AddScoped<IVendorApplicationService, VendorApplicationService>();
        services.AddScoped<IVendorApplicationMessageService, VendorApplicationMessageService>();

        // Register event consumers
        services.AddScoped<IConsumer<AdminMenuCreatedEvent>, AdminMenuEventConsumer>();
    }

    public void Configure(IApplicationBuilder application)
    {
    }

    public int Order => 3001; // After VendorExtensions
}
