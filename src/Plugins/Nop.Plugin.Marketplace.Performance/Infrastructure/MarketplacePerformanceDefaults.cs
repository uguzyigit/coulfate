namespace Nop.Plugin.Marketplace.Performance.Infrastructure;

public static class MarketplacePerformanceDefaults
{
    public static string PerformanceScoreTaskName => "Marketplace: Performance Score Calculation";

    public static string PerformanceScoreTaskType =>
        "Nop.Plugin.Marketplace.Performance.Services.PerformanceScoreCalculationTask, Nop.Plugin.Marketplace.Performance";

    public const int PerformanceScoreTaskSeconds = 3600;
}
