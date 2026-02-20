using Microsoft.Extensions.DependencyInjection;
using Nop.Data;
using Nop.Plugin.Marketplace.Performance.Data;

namespace Nop.Plugin.Marketplace.Performance.Infrastructure;

/// <summary>
/// Runs idempotent database migrations on application startup
/// </summary>
public static class MigrationRunner
{
    public static async Task RunAsync(IServiceProvider serviceProvider)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var dataProvider = scope.ServiceProvider.GetRequiredService<INopDataProvider>();

            // v1.1.0: Add ReviewCount column if missing
            var columnExists = await dataProvider.QueryAsync<int>(InstallationData.CheckReviewCountColumnScript);
            if (columnExists.FirstOrDefault() == 0)
                await dataProvider.ExecuteNonQueryAsync(InstallationData.AddReviewCountColumnScript);
        }
        catch
        {
            // Silently ignore — table may not exist yet (plugin not installed)
        }
    }
}
