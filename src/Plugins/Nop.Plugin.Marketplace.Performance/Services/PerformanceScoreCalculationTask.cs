using Marketplace.Abstractions.Services;
using Nop.Services.ScheduleTasks;

namespace Nop.Plugin.Marketplace.Performance.Services;

public class PerformanceScoreCalculationTask : IScheduleTask
{
    private readonly IProductPerformanceService _performanceService;

    public PerformanceScoreCalculationTask(IProductPerformanceService performanceService)
    {
        _performanceService = performanceService;
    }

    public async Task ExecuteAsync()
    {
        await _performanceService.RecalculateAllAsync();
        await _performanceService.CleanupOldInteractionsAsync(60);
    }
}
