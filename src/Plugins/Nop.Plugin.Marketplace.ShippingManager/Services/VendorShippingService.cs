using Marketplace.Abstractions.Domain;
using Nop.Data;

namespace Nop.Plugin.Marketplace.ShippingManager.Services;

/// <summary>
/// Vendor shipping service implementation
/// </summary>
public class VendorShippingService : IVendorShippingService
{
    private readonly IRepository<VendorShippingPreference> _preferenceRepository;
    private readonly IRepository<VendorShippingCredential> _credentialRepository;
    private readonly IRepository<MarketplaceShipment> _shipmentRepository;

    public VendorShippingService(
        IRepository<VendorShippingPreference> preferenceRepository,
        IRepository<VendorShippingCredential> credentialRepository,
        IRepository<MarketplaceShipment> shipmentRepository)
    {
        _preferenceRepository = preferenceRepository;
        _credentialRepository = credentialRepository;
        _shipmentRepository = shipmentRepository;
    }

    #region Vendor Shipping Preference

    public async Task<VendorShippingPreference> GetVendorPreferenceAsync(int vendorId)
    {
        if (vendorId <= 0)
            return null;

        var query = from p in _preferenceRepository.Table
                    where p.VendorId == vendorId
                    select p;

        return await query.FirstOrDefaultAsync();
    }

    public async Task InsertPreferenceAsync(VendorShippingPreference preference)
    {
        ArgumentNullException.ThrowIfNull(preference);
        preference.CreatedOnUtc = DateTime.UtcNow;
        await _preferenceRepository.InsertAsync(preference);
    }

    public async Task UpdatePreferenceAsync(VendorShippingPreference preference)
    {
        ArgumentNullException.ThrowIfNull(preference);
        preference.UpdatedOnUtc = DateTime.UtcNow;
        await _preferenceRepository.UpdateAsync(preference);
    }

    public async Task DeletePreferenceAsync(VendorShippingPreference preference)
    {
        ArgumentNullException.ThrowIfNull(preference);
        await _preferenceRepository.DeleteAsync(preference);
    }

    #endregion

    #region Vendor Shipping Credentials

    public async Task<IList<VendorShippingCredential>> GetVendorCredentialsAsync(int vendorId, int shippingProviderId)
    {
        var query = from c in _credentialRepository.Table
                    where c.VendorId == vendorId && c.ShippingProviderId == shippingProviderId
                    select c;

        return await query.ToListAsync();
    }

    public async Task<string> GetCredentialValueAsync(int vendorId, int shippingProviderId, string credentialKey)
    {
        if (string.IsNullOrWhiteSpace(credentialKey))
            return null;

        var query = from c in _credentialRepository.Table
                    where c.VendorId == vendorId
                          && c.ShippingProviderId == shippingProviderId
                          && c.CredentialKey == credentialKey
                    select c;

        var credential = await query.FirstOrDefaultAsync();
        return credential?.CredentialValue;
    }

    public async Task SetCredentialAsync(int vendorId, int shippingProviderId, string credentialKey, string credentialValue)
    {
        if (string.IsNullOrWhiteSpace(credentialKey))
            return;

        var query = from c in _credentialRepository.Table
                    where c.VendorId == vendorId
                          && c.ShippingProviderId == shippingProviderId
                          && c.CredentialKey == credentialKey
                    select c;

        var existing = await query.FirstOrDefaultAsync();

        if (existing != null)
        {
            existing.CredentialValue = credentialValue;
            await _credentialRepository.UpdateAsync(existing);
        }
        else
        {
            var credential = new VendorShippingCredential
            {
                VendorId = vendorId,
                ShippingProviderId = shippingProviderId,
                CredentialKey = credentialKey,
                CredentialValue = credentialValue,
                CreatedOnUtc = DateTime.UtcNow
            };
            await _credentialRepository.InsertAsync(credential);
        }
    }

    public async Task DeleteCredentialsAsync(int vendorId, int shippingProviderId)
    {
        var query = from c in _credentialRepository.Table
                    where c.VendorId == vendorId && c.ShippingProviderId == shippingProviderId
                    select c;

        var credentials = await query.ToListAsync();
        await _credentialRepository.DeleteAsync(credentials);
    }

    #endregion

    #region Marketplace Shipment

    public async Task<MarketplaceShipment> GetByNopShipmentIdAsync(int nopShipmentId)
    {
        if (nopShipmentId <= 0)
            return null;

        var query = from s in _shipmentRepository.Table
                    where s.NopShipmentId == nopShipmentId
                    select s;

        return await query.FirstOrDefaultAsync();
    }

    public async Task<MarketplaceShipment> GetByBarcodeAsync(string barcodeNumber)
    {
        if (string.IsNullOrWhiteSpace(barcodeNumber))
            return null;

        var query = from s in _shipmentRepository.Table
                    where s.BarcodeNumber == barcodeNumber
                    select s;

        return await query.FirstOrDefaultAsync();
    }

    public async Task<IList<MarketplaceShipment>> GetVendorShipmentsAsync(int vendorId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = from s in _shipmentRepository.Table
                    where s.VendorId == vendorId
                    select s;

        if (fromDate.HasValue)
            query = query.Where(s => s.CreatedOnUtc >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(s => s.CreatedOnUtc <= toDate.Value);

        query = query.OrderByDescending(s => s.CreatedOnUtc);

        return await query.ToListAsync();
    }

    public async Task InsertShipmentAsync(MarketplaceShipment shipment)
    {
        ArgumentNullException.ThrowIfNull(shipment);
        shipment.CreatedOnUtc = DateTime.UtcNow;
        await _shipmentRepository.InsertAsync(shipment);
    }

    public async Task UpdateShipmentAsync(MarketplaceShipment shipment)
    {
        ArgumentNullException.ThrowIfNull(shipment);
        await _shipmentRepository.UpdateAsync(shipment);
    }

    #endregion
}
