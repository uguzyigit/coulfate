namespace Nop.Plugin.Integration.TrendyolMarketplace.Services.Import;

/// <summary>
/// Imports Trendyol attributes into NopCommerce ProductAttribute and SpecificationAttribute tables
/// </summary>
public interface IAttributeNopImportService
{
    /// <summary>
    /// Processes leaf Trendyol categories in batches.
    /// Categories already in DB are mapped immediately (no delay).
    /// Categories needing an API call are limited to <paramref name="maxApiCallsPerBatch"/> per invocation.
    /// Call repeatedly until remaining == 0.
    /// </summary>
    /// <param name="maxApiCallsPerBatch">Max Trendyol API calls per invocation (default 30)</param>
    /// <returns>(productAttrCount, specAttrCount, remaining) where remaining > 0 means more batches needed</returns>
    Task<(int productAttrCount, int specAttrCount, int remaining)> ImportAttributesAsync(int maxApiCallsPerBatch = 30);
}
