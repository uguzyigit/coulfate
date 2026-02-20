namespace Nop.Plugin.Accounting.Parasut.Services.Auth;

/// <summary>
/// Paraşüt authentication service
/// </summary>
public interface IParasutAuthService
{
    /// <summary>
    /// Get a valid access token (refreshes if expired)
    /// </summary>
    Task<string> GetValidAccessTokenAsync();

    /// <summary>
    /// Authenticate with password grant
    /// </summary>
    Task<string> AuthenticateWithPasswordAsync();

    /// <summary>
    /// Refresh access token
    /// </summary>
    Task<string> RefreshAccessTokenAsync();

    /// <summary>
    /// Clear stored tokens
    /// </summary>
    Task ClearTokensAsync();
}
