namespace Nop.Plugin.Integration.TrendyolMarketplace.Services.Import;

/// <summary>
/// Image import service interface
/// </summary>
public interface IImageImportService
{
    /// <summary>
    /// Imports images for a product from URLs
    /// </summary>
    Task<int> ImportImagesAsync(int productId, List<string> imageUrls);

    /// <summary>
    /// Downloads an image from URL
    /// </summary>
    Task<byte[]> DownloadImageAsync(string url);
}
