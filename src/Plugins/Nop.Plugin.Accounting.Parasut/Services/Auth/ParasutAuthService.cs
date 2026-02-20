using System.Net.Http.Json;
using Nop.Plugin.Accounting.Parasut.Models.Parasut;
using Nop.Services.Configuration;
using Nop.Services.Logging;

namespace Nop.Plugin.Accounting.Parasut.Services.Auth;

/// <summary>
/// Paraşüt authentication service implementation
/// </summary>
public class ParasutAuthService : IParasutAuthService
{
    private readonly HttpClient _httpClient;
    private readonly ISettingService _settingService;
    private readonly ILogger _logger;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    public ParasutAuthService(
        IHttpClientFactory httpClientFactory,
        ISettingService settingService,
        ILogger logger)
    {
        _httpClient = httpClientFactory.CreateClient("ParasutAuth");
        _settingService = settingService;
        _logger = logger;
    }

    /// <summary>
    /// Get a valid access token (refreshes if expired)
    /// </summary>
    public async Task<string> GetValidAccessTokenAsync()
    {
        var settings = await _settingService.LoadSettingAsync<ParasutSettings>();

        // Check if we have a token and it's not expired
        if (!string.IsNullOrEmpty(settings.AccessToken) &&
            settings.TokenExpiresAtUtc.HasValue &&
            settings.TokenExpiresAtUtc.Value > DateTime.UtcNow.AddMinutes(5)) // 5 min buffer
        {
            return settings.AccessToken;
        }

        // Token expired or missing - need to refresh or authenticate
        await _refreshLock.WaitAsync();
        try
        {
            // Double-check after acquiring lock
            settings = await _settingService.LoadSettingAsync<ParasutSettings>();
            if (!string.IsNullOrEmpty(settings.AccessToken) &&
                settings.TokenExpiresAtUtc.HasValue &&
                settings.TokenExpiresAtUtc.Value > DateTime.UtcNow.AddMinutes(5))
            {
                return settings.AccessToken;
            }

            // Try refresh token first
            if (!string.IsNullOrEmpty(settings.RefreshToken))
            {
                try
                {
                    return await RefreshAccessTokenAsync();
                }
                catch (Exception ex)
                {
                    await _logger.WarningAsync($"Token refresh failed, will try password auth: {ex.Message}");
                }
            }

            // Fall back to password authentication
            return await AuthenticateWithPasswordAsync();
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    /// <summary>
    /// Authenticate with password grant
    /// </summary>
    public async Task<string> AuthenticateWithPasswordAsync()
    {
        var settings = await _settingService.LoadSettingAsync<ParasutSettings>();

        if (!settings.UsePasswordGrant)
            throw new InvalidOperationException("Password grant is not enabled");

        if (string.IsNullOrEmpty(settings.Username) || string.IsNullOrEmpty(settings.Password))
            throw new InvalidOperationException("Username and password are required");

        var request = new ParasutAuthRequest
        {
            GrantType = "password",
            ClientId = settings.ClientId,
            ClientSecret = settings.ClientSecret,
            Username = settings.Username,
            Password = settings.Password,
            RedirectUri = settings.RedirectUri
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync("https://api.parasut.com/oauth/token", request);
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                await _logger.ErrorAsync($"Paraşüt authentication failed: {error}");
                throw new Exception($"Authentication failed: {response.StatusCode}");
            }

            var result = await response.Content.ReadFromJsonAsync<ParasutAuthResponse>();

            // Save tokens
            settings.AccessToken = result.AccessToken;
            settings.RefreshToken = result.RefreshToken;
            settings.TokenExpiresAtUtc = DateTime.UtcNow.AddSeconds(result.ExpiresIn);

            await _settingService.SaveSettingAsync(settings);

            await _logger.InformationAsync("Paraşüt authentication successful");

            return result.AccessToken;
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync($"Paraşüt authentication error: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// Refresh access token
    /// </summary>
    public async Task<string> RefreshAccessTokenAsync()
    {
        var settings = await _settingService.LoadSettingAsync<ParasutSettings>();

        if (string.IsNullOrEmpty(settings.RefreshToken))
            throw new InvalidOperationException("No refresh token available");

        var request = new ParasutAuthRequest
        {
            GrantType = "refresh_token",
            ClientId = settings.ClientId,
            ClientSecret = settings.ClientSecret,
            RefreshToken = settings.RefreshToken
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync("https://api.parasut.com/oauth/token", request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                await _logger.ErrorAsync($"Paraşüt token refresh failed: {error}");
                throw new Exception($"Token refresh failed: {response.StatusCode}");
            }

            var result = await response.Content.ReadFromJsonAsync<ParasutAuthResponse>();

            // Save new tokens
            settings.AccessToken = result.AccessToken;
            settings.RefreshToken = result.RefreshToken; // Refresh token rotates!
            settings.TokenExpiresAtUtc = DateTime.UtcNow.AddSeconds(result.ExpiresIn);

            await _settingService.SaveSettingAsync(settings);

            await _logger.InformationAsync("Paraşüt token refreshed successfully");

            return result.AccessToken;
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync($"Paraşüt token refresh error: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// Clear stored tokens
    /// </summary>
    public async Task ClearTokensAsync()
    {
        var settings = await _settingService.LoadSettingAsync<ParasutSettings>();
        settings.AccessToken = null;
        settings.RefreshToken = null;
        settings.TokenExpiresAtUtc = null;
        await _settingService.SaveSettingAsync(settings);
    }
}
