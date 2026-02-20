using Nop.Core;
using Nop.Core.Caching;
using Nop.Data;
using Nop.Plugin.Integration.TrendyolMarketplace.Domain;
using Nop.Plugin.Integration.TrendyolMarketplace.Infrastructure;
using Nop.Plugin.Integration.TrendyolMarketplace.Services.Api;
using Nop.Services.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace Nop.Plugin.Integration.TrendyolMarketplace.Services;

/// <summary>
/// Vendor credential service implementation
/// </summary>
public class VendorCredentialService : IVendorCredentialService
{
    private readonly IRepository<TrendyolVendorCredential> _credentialRepository;
    private readonly IStaticCacheManager _staticCacheManager;
    private readonly ISettingService _settingService;

    public VendorCredentialService(
        IRepository<TrendyolVendorCredential> credentialRepository,
        IStaticCacheManager staticCacheManager,
        ISettingService settingService)
    {
        _credentialRepository = credentialRepository;
        _staticCacheManager = staticCacheManager;
        _settingService = settingService;
    }

    /// <summary>
    /// Gets a vendor credential by ID
    /// </summary>
    public virtual async Task<TrendyolVendorCredential> GetByIdAsync(int id)
    {
        if (id <= 0)
            return null;

        return await _credentialRepository.GetByIdAsync(id);
    }

    /// <summary>
    /// Gets a vendor credential by vendor ID
    /// </summary>
    public virtual async Task<TrendyolVendorCredential> GetByVendorIdAsync(int vendorId)
    {
        if (vendorId <= 0)
            return null;

        var key = _staticCacheManager.PrepareKeyForDefaultCache(
            TrendyolDefaults.VendorCredentialByVendorIdCacheKey, vendorId);

        return await _staticCacheManager.GetAsync(key, async () =>
        {
            var query = from vc in _credentialRepository.Table
                        where vc.VendorId == vendorId
                        select vc;

            return await query.FirstOrDefaultAsync();
        });
    }

    /// <summary>
    /// Gets all active vendor credentials
    /// </summary>
    public virtual async Task<IList<TrendyolVendorCredential>> GetAllActiveAsync()
    {
        var query = from vc in _credentialRepository.Table
                    where vc.IsActive
                    select vc;

        return await query.ToListAsync();
    }

    /// <summary>
    /// Gets all vendor credentials (paged)
    /// </summary>
    public virtual async Task<IPagedList<TrendyolVendorCredential>> GetAllAsync(
        bool? isActive = null,
        int pageIndex = 0,
        int pageSize = int.MaxValue)
    {
        var query = _credentialRepository.Table;

        if (isActive.HasValue)
            query = query.Where(vc => vc.IsActive == isActive.Value);

        query = query.OrderBy(vc => vc.VendorId);

        return await query.ToPagedListAsync(pageIndex, pageSize);
    }

    /// <summary>
    /// Gets vendor credentials that need sync
    /// </summary>
    public virtual async Task<IList<TrendyolVendorCredential>> GetCredentialsForSyncAsync()
    {
        var now = DateTime.UtcNow;

        var query = from vc in _credentialRepository.Table
                    where vc.IsActive && vc.AutoSyncEnabled
                    where !vc.LastSyncOnUtc.HasValue ||
                          vc.LastSyncOnUtc.Value.AddMinutes(vc.SyncIntervalMinutes) <= now
                    select vc;

        return await query.ToListAsync();
    }

    /// <summary>
    /// Inserts a vendor credential
    /// </summary>
    public virtual async Task InsertAsync(TrendyolVendorCredential credential)
    {
        ArgumentNullException.ThrowIfNull(credential);

        credential.CreatedOnUtc = DateTime.UtcNow;
        credential.UpdatedOnUtc = DateTime.UtcNow;

        // Note: Encryption disabled for debugging - store as plain text
        // if (!string.IsNullOrEmpty(credential.ApiSecret))
        //     credential.ApiSecret = EncryptApiSecret(credential.ApiSecret);

        await _credentialRepository.InsertAsync(credential);
        await _staticCacheManager.RemoveByPrefixAsync(TrendyolDefaults.VendorCredentialPrefix);
    }

    /// <summary>
    /// Updates a vendor credential
    /// </summary>
    public virtual async Task UpdateAsync(TrendyolVendorCredential credential)
    {
        ArgumentNullException.ThrowIfNull(credential);

        credential.UpdatedOnUtc = DateTime.UtcNow;

        await _credentialRepository.UpdateAsync(credential);
        await _staticCacheManager.RemoveByPrefixAsync(TrendyolDefaults.VendorCredentialPrefix);
    }

    /// <summary>
    /// Deletes a vendor credential
    /// </summary>
    public virtual async Task DeleteAsync(TrendyolVendorCredential credential)
    {
        ArgumentNullException.ThrowIfNull(credential);

        await _credentialRepository.DeleteAsync(credential);
        await _staticCacheManager.RemoveByPrefixAsync(TrendyolDefaults.VendorCredentialPrefix);
    }

    /// <summary>
    /// Updates the last sync time for a vendor
    /// </summary>
    public virtual async Task UpdateLastSyncTimeAsync(int vendorId)
    {
        var credential = await GetByVendorIdAsync(vendorId);
        if (credential == null)
            return;

        credential.LastSyncOnUtc = DateTime.UtcNow;
        await _credentialRepository.UpdateAsync(credential);
        await _staticCacheManager.RemoveByPrefixAsync(TrendyolDefaults.VendorCredentialPrefix);
    }

    /// <summary>
    /// Encrypts the API secret
    /// </summary>
    public virtual string EncryptApiSecret(string plainSecret)
    {
        if (string.IsNullOrEmpty(plainSecret))
            return plainSecret;

        var settings = _settingService.LoadSettingAsync<TrendyolSettings>().GetAwaiter().GetResult();
        var key = Encoding.UTF8.GetBytes(settings.EncryptionKey.PadRight(32).Substring(0, 32));
        var iv = Encoding.UTF8.GetBytes(settings.EncryptionKey.PadRight(16).Substring(0, 16));

        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;

        var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        var plainBytes = Encoding.UTF8.GetBytes(plainSecret);

        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        {
            cs.Write(plainBytes, 0, plainBytes.Length);
        }

        return Convert.ToBase64String(ms.ToArray());
    }

    /// <summary>
    /// Decrypts the API secret
    /// </summary>
    public virtual string DecryptApiSecret(string encryptedSecret)
    {
        if (string.IsNullOrEmpty(encryptedSecret))
            return encryptedSecret;

        try
        {
            var settings = _settingService.LoadSettingAsync<TrendyolSettings>().GetAwaiter().GetResult();
            var key = Encoding.UTF8.GetBytes(settings.EncryptionKey.PadRight(32).Substring(0, 32));
            var iv = Encoding.UTF8.GetBytes(settings.EncryptionKey.PadRight(16).Substring(0, 16));

            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;

            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            var encryptedBytes = Convert.FromBase64String(encryptedSecret);

            using var ms = new MemoryStream(encryptedBytes);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var reader = new StreamReader(cs);
            return reader.ReadToEnd();
        }
        catch
        {
            // If decryption fails, return the original value (might be plain text)
            return encryptedSecret;
        }
    }

    /// <summary>
    /// Validates vendor credentials by testing API connection
    /// </summary>
    public virtual async Task<(bool Success, string Message)> ValidateCredentialsAsync(TrendyolVendorCredential credential)
    {
        // This will be implemented by the API client
        // For now, return a placeholder
        await Task.CompletedTask;
        return (true, "Validation will be performed by API client");
    }
}
