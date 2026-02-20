using Nop.Plugin.Shipping.PttKargo.Api;
using Nop.Plugin.Shipping.PttKargo.Api.Models;

namespace Nop.Plugin.Shipping.PttKargo.Services;

/// <summary>
/// PTT API Service Interface
/// </summary>
public interface IPttApiService
{
    /// <summary>
    /// Gönderi oluşturur
    /// </summary>
    Task<KabulEkle2Response> CreateShipmentAsync(KabulEkle2Request request, PttCredentials credentials = null);

    /// <summary>
    /// Barkod numarası ile gönderi sorgular
    /// </summary>
    Task<GonderiSorgu2Response> TrackByBarcodeAsync(string barcodeNo, PttCredentials credentials = null);

    /// <summary>
    /// Müşteri referans numarası ile gönderi sorgular
    /// </summary>
    Task<GonderiSorgu2Response> TrackByReferenceAsync(string referenceNo, PttCredentials credentials = null);

    /// <summary>
    /// Satıcının PTT credential'larını getirir
    /// </summary>
    Task<PttCredentials> GetVendorCredentialsAsync(int vendorId);

    /// <summary>
    /// Pazaryeri PTT credential'larını getirir
    /// </summary>
    Task<PttCredentials> GetMarketplaceCredentialsAsync();
}
