using Marketplace.Abstractions.Domain;

namespace Nop.Plugin.Marketplace.ShippingManager.Services;

/// <summary>
/// Vendor shipping service interface
/// </summary>
public interface IVendorShippingService
{
    #region Vendor Shipping Preference

    /// <summary>
    /// Get vendor's shipping preference
    /// </summary>
    Task<VendorShippingPreference> GetVendorPreferenceAsync(int vendorId);

    /// <summary>
    /// Insert vendor shipping preference
    /// </summary>
    Task InsertPreferenceAsync(VendorShippingPreference preference);

    /// <summary>
    /// Update vendor shipping preference
    /// </summary>
    Task UpdatePreferenceAsync(VendorShippingPreference preference);

    /// <summary>
    /// Delete vendor shipping preference
    /// </summary>
    Task DeletePreferenceAsync(VendorShippingPreference preference);

    #endregion

    #region Vendor Shipping Credentials

    /// <summary>
    /// Get vendor's credentials for a shipping provider
    /// </summary>
    Task<IList<VendorShippingCredential>> GetVendorCredentialsAsync(int vendorId, int shippingProviderId);

    /// <summary>
    /// Get a specific credential value
    /// </summary>
    Task<string> GetCredentialValueAsync(int vendorId, int shippingProviderId, string credentialKey);

    /// <summary>
    /// Set a credential value
    /// </summary>
    Task SetCredentialAsync(int vendorId, int shippingProviderId, string credentialKey, string credentialValue);

    /// <summary>
    /// Delete all credentials for a vendor/provider combination
    /// </summary>
    Task DeleteCredentialsAsync(int vendorId, int shippingProviderId);

    #endregion

    #region Marketplace Shipment

    /// <summary>
    /// Get marketplace shipment by NopCommerce shipment ID
    /// </summary>
    Task<MarketplaceShipment> GetByNopShipmentIdAsync(int nopShipmentId);

    /// <summary>
    /// Get marketplace shipment by barcode number
    /// </summary>
    Task<MarketplaceShipment> GetByBarcodeAsync(string barcodeNumber);

    /// <summary>
    /// Get all shipments for a vendor
    /// </summary>
    Task<IList<MarketplaceShipment>> GetVendorShipmentsAsync(int vendorId, DateTime? fromDate = null, DateTime? toDate = null);

    /// <summary>
    /// Insert marketplace shipment
    /// </summary>
    Task InsertShipmentAsync(MarketplaceShipment shipment);

    /// <summary>
    /// Update marketplace shipment
    /// </summary>
    Task UpdateShipmentAsync(MarketplaceShipment shipment);

    #endregion
}
