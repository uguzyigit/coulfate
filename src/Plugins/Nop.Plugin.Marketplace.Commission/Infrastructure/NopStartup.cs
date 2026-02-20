using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Domain.Orders;
using Nop.Core.Events;
using Nop.Core.Infrastructure;
using Nop.Plugin.Marketplace.Commission.Services;
using Nop.Plugin.Marketplace.Commission.Infrastructure.EventConsumers;
using Nop.Services.Events;
using Nop.Data.Mapping;
using Nop.Web.Framework.Events;

namespace Nop.Plugin.Marketplace.Commission.Infrastructure;

public class NopStartup : INopStartup
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Register services
        services.AddScoped<ICommissionService, CommissionService>();

        // Register table name compatibility (use AdditionalNameCompatibilities to avoid conflicts)
        if (!NameCompatibilityManager.AdditionalNameCompatibilities.Contains(typeof(Data.CommissionNameCompatibility)))
            NameCompatibilityManager.AdditionalNameCompatibilities.Add(typeof(Data.CommissionNameCompatibility));

        // Register event consumers
        services.AddScoped<IConsumer<EntityUpdatedEvent<Order>>, OrderPaidEventConsumer>();
        services.AddScoped<IConsumer<EntityUpdatedEvent<Order>>, OrderUpdatedDebugConsumer>();
        services.AddScoped<IConsumer<AdminMenuCreatedEvent>, AdminMenuEventConsumer>();
    }

    public void Configure(IApplicationBuilder application)
    {
    }

    public int Order => 3000;
}