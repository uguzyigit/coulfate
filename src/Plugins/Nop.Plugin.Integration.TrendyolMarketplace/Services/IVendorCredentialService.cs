using Nop.Core;
using Nop.Plugin.Integration.TrendyolMarketplace.Domain;

namespace Nop.Plugin.Integration.TrendyolMarketplace.Services;

/// <summary>
/// Vendor credential service interface
/// </summary>
public interface IVendorCredentialService
{
    /// <summary>
    /// Gets a vendor credential by ID
    /// </summary>
    Task<TrendyolVendorCredential> GetByIdAsync(int id);

    /// <summary>
    /// Gets a vendor credential by vendor ID
    /// </summary>
    Task<TrendyolVendorCredential> GetByVendorIdAsync(int vendorId);

    /// <summary>
    /// Gets all active vendor credentials
    /// </summary>
    Task<IList<TrendyolVendorCredential>> GetAllActiveAsync();

    /// <summary>
    /// Gets all vendor credentials (paged)
    /// </summary>
    Task<IPagedList<TrendyolVendorCredential>> GetAllAsync(
        bool? isActive = null,
        int pageIndex = 0,
        int pageSize = int.MaxValue);

    /// <summary>
    /// Gets vendor credentials that need sync
    /// </summary>
    Task<IList<TrendyolVendorCredential>> GetCredentialsForSyncAsync();

    /// <summary>
    /// Inserts a vendor credential
    /// </summary>
    Task InsertAsync(TrendyolVendorCredential credential);

    /// <summary>
    /// Updates a vendor credential
    /// </summary>
    Task UpdateAsync(TrendyolVendorCredential credential);

    /// <summary>
    /// Deletes a vendor credential
    /// </summary>
    Task DeleteAsync(TrendyolVendorCredential credential);

    /// <summary>
    /// Updates the last sync time for a vendor
    /// </summary>
    Task UpdateLastSyncTimeAsync(int vendorId);

    /// <summary>
    /// Encrypts the API secret
    /// </summary>
    string EncryptApiSecret(string plainSecret);

    /// <summary>
    /// Decrypts the API secret
    /// </summary>
    string DecryptApiSecret(string encryptedSecret);

    /// <summary>
    /// Validates vendor credentials by testing API connection
    /// </summary>
    Task<(bool Success, string Message)> ValidateCredentialsAsync(TrendyolVendorCredential credential);
}
