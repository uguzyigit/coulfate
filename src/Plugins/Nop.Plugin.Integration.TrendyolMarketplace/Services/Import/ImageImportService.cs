using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Logging;
using Nop.Services.Media;

namespace Nop.Plugin.Integration.TrendyolMarketplace.Services.Import;

/// <summary>
/// Image import service implementation
/// </summary>
public class ImageImportService : IImageImportService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IPictureService _pictureService;
    private readonly IProductService _productService;
    private readonly ILogger _logger;

    public ImageImportService(
        IHttpClientFactory httpClientFactory,
        IPictureService pictureService,
        IProductService productService,
        ILogger logger)
    {
        _httpClientFactory = httpClientFactory;
        _pictureService = pictureService;
        _productService = productService;
        _logger = logger;
    }

    /// <summary>
    /// Imports images for a product from URLs
    /// </summary>
    public virtual async Task<int> ImportImagesAsync(int productId, List<string> imageUrls)
    {
        if (productId <= 0 || imageUrls == null || !imageUrls.Any())
            return 0;

        var importedCount = 0;
        var displayOrder = 0;

        foreach (var url in imageUrls)
        {
            try
            {
                var imageBytes = await DownloadImageAsync(url);
                if (imageBytes == null || imageBytes.Length == 0)
                    continue;

                // Determine mime type from URL
                var mimeType = GetMimeTypeFromUrl(url);

                // Insert picture
                var picture = await _pictureService.InsertPictureAsync(
                    imageBytes,
                    mimeType,
                    seoFilename: null,
                    altAttribute: null,
                    titleAttribute: null,
                    isNew: true,
                    validateBinary: false);

                if (picture == null)
                    continue;

                // Add picture to product
                await _productService.InsertProductPictureAsync(new ProductPicture
                {
                    ProductId = productId,
                    PictureId = picture.Id,
                    DisplayOrder = displayOrder++
                });

                importedCount++;
            }
            catch (Exception ex)
            {
                await _logger.WarningAsync($"Failed to import image from {url}: {ex.Message}");
            }
        }

        return importedCount;
    }

    /// <summary>
    /// Downloads an image from URL
    /// </summary>
    public virtual async Task<byte[]> DownloadImageAsync(string url)
    {
        if (string.IsNullOrEmpty(url))
            return null;

        try
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(30);

            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsByteArrayAsync();
        }
        catch (Exception ex)
        {
            await _logger.WarningAsync($"Failed to download image from {url}: {ex.Message}");
            return null;
        }
    }

    private string GetMimeTypeFromUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
            return "image/jpeg";

        var extension = Path.GetExtension(url)?.ToLowerInvariant();

        return extension switch
        {
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".bmp" => "image/bmp",
            ".tiff" or ".tif" => "image/tiff",
            _ => "image/jpeg"
        };
    }
}
