using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Payments.Iyzico.Services;

namespace Nop.Plugin.Payments.Iyzico.Infrastructure;

public class NopStartup : INopStartup
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IIyzicoApiService, IyzicoApiService>();
        services.AddScoped<IIyzicoPaymentService, IyzicoPaymentService>();
        services.AddScoped<IIyzicoTransactionService, IyzicoTransactionService>();
        // SubMerchant service temporarily disabled
    }

    public void Configure(IApplicationBuilder application)
    {
    }

    public int Order => 3000;
}
