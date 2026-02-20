using Marketplace.Abstractions.Domain;

namespace Nop.Plugin.Marketplace.ShippingManager.Services;

/// <summary>
/// Shipping provider service interface
/// </summary>
public interface IShippingProviderService
{
    /// <summary>
    /// Get shipping provider by ID
    /// </summary>
    Task<ShippingProvider> GetByIdAsync(int id);

    /// <summary>
    /// Get shipping provider by system name
    /// </summary>
    Task<ShippingProvider> GetBySystemNameAsync(string systemName);

    /// <summary>
    /// Get all shipping providers
    /// </summary>
    Task<IList<ShippingProvider>> GetAllAsync(bool activeOnly = true);

    /// <summary>
    /// Insert shipping provider
    /// </summary>
    Task InsertAsync(ShippingProvider provider);

    /// <summary>
    /// Update shipping provider
    /// </summary>
    Task UpdateAsync(ShippingProvider provider);

    /// <summary>
    /// Delete shipping provider
    /// </summary>
    Task DeleteAsync(ShippingProvider provider);
}
