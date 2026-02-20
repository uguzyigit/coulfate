using Nop.Core.Domain.ScheduleTasks;
using Nop.Data;
using Nop.Plugin.Marketplace.Performance.Infrastructure;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.ScheduleTasks;

namespace Nop.Plugin.Marketplace.Performance;

public class MarketplacePerformancePlugin : BasePlugin
{
    private readonly ILocalizationService _localizationService;
    private readonly INopDataProvider _dataProvider;
    private readonly IScheduleTaskService _scheduleTaskService;

    public MarketplacePerformancePlugin(
        ILocalizationService localizationService,
        INopDataProvider dataProvider,
        IScheduleTaskService scheduleTaskService)
    {
        _localizationService = localizationService;
        _dataProvider = dataProvider;
        _scheduleTaskService = scheduleTaskService;
    }

    public override async Task InstallAsync()
    {
        // Create performance tables (includes ReviewCount for new installs)
        await _dataProvider.ExecuteNonQueryAsync(Data.InstallationData.CreateTablesScript);

        // Install localization resources
        await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
        {
            // Menu
            ["Plugins.Marketplace.Performance.Menu.Performance"] = "Performans",
            ["Plugins.Marketplace.Performance.Menu.ProductPerformance"] = "Urun Performansi",
            ["Plugins.Marketplace.Performance.Menu.SalesPerformance"] = "Satis Performansi",
            // Pages
            ["Plugins.Marketplace.Performance.ProductPerformance"] = "Urun Performansi",
            ["Plugins.Marketplace.Performance.SalesPerformance"] = "Satis Performansi",
            ["Plugins.Marketplace.Performance.Vendor"] = "Satici"
        });

        // Register scheduled task
        var task = await _scheduleTaskService.GetTaskByTypeAsync(MarketplacePerformanceDefaults.PerformanceScoreTaskType);
        if (task == null)
        {
            await _scheduleTaskService.InsertTaskAsync(new ScheduleTask
            {
                Name = MarketplacePerformanceDefaults.PerformanceScoreTaskName,
                Seconds = MarketplacePerformanceDefaults.PerformanceScoreTaskSeconds,
                Type = MarketplacePerformanceDefaults.PerformanceScoreTaskType,
                Enabled = true,
                StopOnError = false
            });
        }

        await base.InstallAsync();
    }

    public override async Task UpdateAsync(string currentVersion, string targetVersion)
    {
        // v1.0.0 -> v1.1.0: Add ReviewCount column to snapshot table
        if (string.Compare(currentVersion, "1.1.0", StringComparison.OrdinalIgnoreCase) < 0)
        {
            var columnExists = await _dataProvider.QueryAsync<int>(Data.InstallationData.CheckReviewCountColumnScript);
            if (columnExists.FirstOrDefault() == 0)
                await _dataProvider.ExecuteNonQueryAsync(Data.InstallationData.AddReviewCountColumnScript);
        }

        await base.UpdateAsync(currentVersion, targetVersion);
    }

    public override async Task UninstallAsync()
    {
        // Delete scheduled task
        var task = await _scheduleTaskService.GetTaskByTypeAsync(MarketplacePerformanceDefaults.PerformanceScoreTaskType);
        if (task != null)
            await _scheduleTaskService.DeleteTaskAsync(task);

        // Delete localization resources
        await _localizationService.DeleteLocaleResourcesAsync("Plugins.Marketplace.Performance");

        // Drop performance tables
        await _dataProvider.ExecuteNonQueryAsync(Data.InstallationData.DropTablesScript);

        await base.UninstallAsync();
    }
}
