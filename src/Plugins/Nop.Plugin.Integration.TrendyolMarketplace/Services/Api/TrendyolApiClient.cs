using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Nop.Plugin.Integration.TrendyolMarketplace.Domain;
using Nop.Plugin.Integration.TrendyolMarketplace.Infrastructure;
using Nop.Plugin.Integration.TrendyolMarketplace.Services.Api.Models;
using Nop.Services.Configuration;
using Nop.Services.Logging;

namespace Nop.Plugin.Integration.TrendyolMarketplace.Services.Api;

/// <summary>
/// Trendyol API client implementation
/// </summary>
public class TrendyolApiClient : ITrendyolApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ISettingService _settingService;
    private readonly IVendorCredentialService _vendorCredentialService;
    private readonly ILogger _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public TrendyolApiClient(
        IHttpClientFactory httpClientFactory,
        ISettingService settingService,
        IVendorCredentialService vendorCredentialService,
        ILogger logger)
    {
        _httpClientFactory = httpClientFactory;
        _settingService = settingService;
        _vendorCredentialService = vendorCredentialService;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    /// <summary>
    /// Tests the API connection with given credentials
    /// </summary>
    public virtual async Task<(bool Success, string Message)> TestConnectionAsync(TrendyolVendorCredential credential)
    {
        try
        {
            var settings = await _settingService.LoadSettingAsync<TrendyolSettings>();

            // First test: Try public endpoint (no auth needed) to verify connectivity
            var publicClient = CreateClient();
            var publicUrl = $"{settings.ApiBaseUrl}{TrendyolDefaults.BrandListEndpoint}?page=0&size=1";

            try
            {
                var publicResponse = await publicClient.GetAsync(publicUrl);
                if (!publicResponse.IsSuccessStatusCode)
                {
                    return (false, $"API baglanti hatasi: {publicResponse.StatusCode}. URL: {publicUrl}");
                }
            }
            catch (Exception pubEx)
            {
                return (false, $"Trendyol API'ye ulasilamiyor: {pubEx.Message}");
            }

            // Second test: Try authenticated endpoint with seller's products
            var authClient = CreateAuthenticatedClient(credential);
            var productEndpoint = string.Format(TrendyolDefaults.ProductListEndpoint, credential.TrendyolSupplierId);
            var authUrl = $"{settings.ApiBaseUrl}{productEndpoint}?page=0&size=1";
            var authResponse = await authClient.GetAsync(authUrl);

            if (authResponse.IsSuccessStatusCode)
            {
                var content = await authResponse.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<TrendyolProductsResponse>(content, _jsonOptions);
                return (true, $"Baglanti basarili! Toplam urun: {result?.TotalElements ?? 0}");
            }
            else
            {
                var errorContent = await authResponse.Content.ReadAsStringAsync();
                return (false, $"Kimlik dogrulama hatasi: {authResponse.StatusCode} - {errorContent.Substring(0, Math.Min(200, errorContent.Length))}");
            }
        }
        catch (Exception ex)
        {
            return (false, $"Hata: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets all categories from Trendyol (no auth required)
    /// </summary>
    public virtual async Task<List<TrendyolCategoryDto>> GetCategoriesAsync()
    {
        var settings = await _settingService.LoadSettingAsync<TrendyolSettings>();
        var url = $"{settings.ApiBaseUrl}{TrendyolDefaults.CategoryListEndpoint}";

        var client = CreateClient();
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<TrendyolCategoriesResponse>(content, _jsonOptions);

        // Flatten the category tree
        var allCategories = new List<TrendyolCategoryDto>();
        FlattenCategories(result?.Categories ?? new List<TrendyolCategoryDto>(), allCategories);

        return allCategories;
    }

    /// <summary>
    /// Gets category attributes from Trendyol (no auth required)
    /// </summary>
    public virtual async Task<TrendyolCategoryAttributesResponse> GetCategoryAttributesAsync(long categoryId)
    {
        var settings = await _settingService.LoadSettingAsync<TrendyolSettings>();
        var url = $"{settings.ApiBaseUrl}{string.Format(TrendyolDefaults.CategoryAttributesEndpoint, categoryId)}";

        var client = CreateClient();
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<TrendyolCategoryAttributesResponse>(content, _jsonOptions);
    }

    /// <summary>
    /// Gets brands from Trendyol (no auth required)
    /// </summary>
    public virtual async Task<List<TrendyolBrandDto>> GetBrandsAsync(int page = 0, int size = 1000)
    {
        var settings = await _settingService.LoadSettingAsync<TrendyolSettings>();
        var url = $"{settings.ApiBaseUrl}{TrendyolDefaults.BrandListEndpoint}?page={page}&size={size}";

        var client = CreateClient();
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<TrendyolBrandsResponse>(content, _jsonOptions);

        return result?.Brands ?? new List<TrendyolBrandDto>();
    }

    /// <summary>
    /// Gets all brands from Trendyol (paginated, no auth required)
    /// </summary>
    public virtual async Task<List<TrendyolBrandDto>> GetAllBrandsAsync()
    {
        var allBrands = new List<TrendyolBrandDto>();
        var settings = await _settingService.LoadSettingAsync<TrendyolSettings>();
        var page = 0;
        var size = 5000;
        var hasMore = true;

        while (hasMore)
        {
            var url = $"{settings.ApiBaseUrl}{TrendyolDefaults.BrandListEndpoint}?page={page}&size={size}";
            var client = CreateClient();

            try
            {
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<TrendyolBrandsResponse>(content, _jsonOptions);

                if (result?.Brands != null && result.Brands.Any())
                {
                    allBrands.AddRange(result.Brands);
                    page++;

                    // Check if we've reached the last page
                    if (result.TotalPages.HasValue && page >= result.TotalPages.Value)
                        hasMore = false;
                    else if (result.Brands.Count < size)
                        hasMore = false;
                }
                else
                {
                    hasMore = false;
                }

                // Rate limiting
                await Task.Delay(settings.ApiRequestDelayMs);
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"Error fetching brands page {page}: {ex.Message}", ex);
                hasMore = false;
            }
        }

        return allBrands;
    }

    /// <summary>
    /// Gets products for a vendor
    /// </summary>
    public virtual async Task<TrendyolProductsResponse> GetProductsAsync(TrendyolVendorCredential credential, int page = 0, int size = 50, bool? approved = null, string barcode = null, bool? onSale = null)
    {
        var settings = await _settingService.LoadSettingAsync<TrendyolSettings>();
        var endpoint = string.Format(TrendyolDefaults.ProductListEndpoint, credential.TrendyolSupplierId);
        var url = $"{settings.ApiBaseUrl}{endpoint}?page={page}&size={size}";

        if (approved.HasValue)
            url += $"&approved={approved.Value.ToString().ToLower()}";

        if (onSale.HasValue)
            url += $"&onSale={onSale.Value.ToString().ToLower()}";

        if (!string.IsNullOrEmpty(barcode))
            url += $"&barcode={barcode}";

        var client = CreateAuthenticatedClient(credential);
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<TrendyolProductsResponse>(content, _jsonOptions);

        return result;
    }

    /// <summary>
    /// Gets all products for a vendor (paginated)
    /// </summary>
    public virtual async Task<List<TrendyolProductDto>> GetAllProductsAsync(TrendyolVendorCredential credential, bool? approved = null, bool? onSale = null)
    {
        var allProducts = new List<TrendyolProductDto>();
        var settings = await _settingService.LoadSettingAsync<TrendyolSettings>();
        var page = 0;
        var size = settings.ApiPageSize;
        var hasMore = true;

        while (hasMore)
        {
            try
            {
                var response = await GetProductsAsync(credential, page, size, approved, onSale: onSale);

                if (response?.Content != null && response.Content.Any())
                {
                    allProducts.AddRange(response.Content);
                    page++;

                    // Check if we've reached the last page
                    if (page >= response.TotalPages)
                        hasMore = false;
                }
                else
                {
                    hasMore = false;
                }

                // Rate limiting
                await Task.Delay(settings.ApiRequestDelayMs);
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"Error fetching products page {page} for supplier {credential.TrendyolSupplierId}: {ex.Message}", ex);
                hasMore = false;
            }
        }

        return allProducts;
    }

    /// <summary>
    /// Gets a single product by barcode
    /// </summary>
    public virtual async Task<TrendyolProductDto> GetProductByBarcodeAsync(TrendyolVendorCredential credential, string barcode)
    {
        var response = await GetProductsAsync(credential, 0, 1, null, barcode);
        return response?.Content?.FirstOrDefault();
    }

    #region Private Methods

    private HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient("TrendyolApi");
        return client;
    }

    private HttpClient CreateAuthenticatedClient(TrendyolVendorCredential credential)
    {
        var client = _httpClientFactory.CreateClient("TrendyolApi");

        // Use API secret directly (no encryption for now)
        var apiSecret = credential.ApiSecret;

        string authBase64;

        // Check if apiSecret is already a Base64 token (contains ':' when decoded)
        // Trendyol provides a pre-encoded Token that can be used directly
        if (IsValidBase64Token(apiSecret))
        {
            authBase64 = apiSecret;
        }
        else
        {
            var authString = $"{credential.ApiKey}:{apiSecret}";
            authBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(authString));
        }

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authBase64);

        // User-Agent: Trendyol requires {SupplierId} - SelfIntegration format
        var userAgent = $"{credential.TrendyolSupplierId} - SelfIntegration";
        client.DefaultRequestHeaders.UserAgent.Clear();
        client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);

        // Recommended headers for the new API gateway
        client.DefaultRequestHeaders.TryAddWithoutValidation("x-agentname", "Coulfate");
        client.DefaultRequestHeaders.TryAddWithoutValidation("x-correlationid", Guid.NewGuid().ToString());

        return client;
    }

    /// <summary>
    /// Checks if the value is a valid Base64 encoded token (format: key:secret encoded)
    /// </summary>
    private bool IsValidBase64Token(string value)
    {
        if (string.IsNullOrEmpty(value) || value.Length < 20)
            return false;

        try
        {
            // Try to decode and check if it contains ':'
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(value));
            return decoded.Contains(':') && decoded.Split(':').Length == 2;
        }
        catch
        {
            return false;
        }
    }

    private void FlattenCategories(List<TrendyolCategoryDto> categories, List<TrendyolCategoryDto> result, string parentPath = "")
    {
        foreach (var category in categories)
        {
            var path = string.IsNullOrEmpty(parentPath) ? category.Name : $"{parentPath} > {category.Name}";

            // Add to flat list (but we'll store the path separately)
            result.Add(category);

            // Recursively process subcategories
            if (category.SubCategories != null && category.SubCategories.Any())
            {
                FlattenCategories(category.SubCategories, result, path);
            }
        }
    }

    #endregion
}
