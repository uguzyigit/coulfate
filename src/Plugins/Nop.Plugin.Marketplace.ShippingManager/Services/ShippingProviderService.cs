using Marketplace.Abstractions.Domain;
using Nop.Data;

namespace Nop.Plugin.Marketplace.ShippingManager.Services;

/// <summary>
/// Shipping provider service implementation
/// </summary>
public class ShippingProviderService : IShippingProviderService
{
    private readonly IRepository<ShippingProvider> _providerRepository;

    public ShippingProviderService(IRepository<ShippingProvider> providerRepository)
    {
        _providerRepository = providerRepository;
    }

    public async Task<ShippingProvider> GetByIdAsync(int id)
    {
        if (id <= 0)
            return null;

        return await _providerRepository.GetByIdAsync(id);
    }

    public async Task<ShippingProvider> GetBySystemNameAsync(string systemName)
    {
        if (string.IsNullOrWhiteSpace(systemName))
            return null;

        var query = from p in _providerRepository.Table
                    where p.SystemName == systemName
                    select p;

        return await query.FirstOrDefaultAsync();
    }

    public async Task<IList<ShippingProvider>> GetAllAsync(bool activeOnly = true)
    {
        var query = from p in _providerRepository.Table
                    where !activeOnly || p.IsActive
                    orderby p.DisplayOrder, p.Name
                    select p;

        return await query.ToListAsync();
    }

    public async Task InsertAsync(ShippingProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        await _providerRepository.InsertAsync(provider);
    }

    public async Task UpdateAsync(ShippingProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        await _providerRepository.UpdateAsync(provider);
    }

    public async Task DeleteAsync(ShippingProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        await _providerRepository.DeleteAsync(provider);
    }
}
