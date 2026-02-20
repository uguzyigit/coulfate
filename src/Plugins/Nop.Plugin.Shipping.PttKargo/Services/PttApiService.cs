using Microsoft.Extensions.Logging;
using Nop.Plugin.Marketplace.ShippingManager.Services;
using Nop.Plugin.Shipping.PttKargo.Api;
using Nop.Plugin.Shipping.PttKargo.Api.Models;

namespace Nop.Plugin.Shipping.PttKargo.Services;

/// <summary>
/// PTT API Service Implementation
/// </summary>
public class PttApiService : IPttApiService
{
    private readonly PttSoapClient _soapClient;
    private readonly PttKargoSettings _settings;
    private readonly IVendorShippingService _vendorShippingService;
    private readonly IShippingProviderService _shippingProviderService;
    private readonly ILogger<PttApiService> _logger;

    // PTT Kargo provider system name
    private const string PTT_PROVIDER_SYSTEM_NAME = "Shipping.PttKargo";

    public PttApiService(
        PttSoapClient soapClient,
        PttKargoSettings settings,
        IVendorShippingService vendorShippingService,
        IShippingProviderService shippingProviderService,
        ILogger<PttApiService> logger)
    {
        _soapClient = soapClient;
        _settings = settings;
        _vendorShippingService = vendorShippingService;
        _shippingProviderService = shippingProviderService;
        _logger = logger;
    }

    /// <summary>
    /// Gönderi oluşturur
    /// </summary>
    public async Task<KabulEkle2Response> CreateShipmentAsync(
        KabulEkle2Request request,
        PttCredentials credentials = null)
    {
        credentials ??= await GetMarketplaceCredentialsAsync();

        if (credentials == null || string.IsNullOrEmpty(credentials.MusteriId))
        {
            return new KabulEkle2Response
            {
                Success = false,
                ErrorMessage = "PTT credential'ları yapılandırılmamış"
            };
        }

        // Dosya adı oluştur (benzersiz olmalı)
        request.DosyaAdi ??= $"MP_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}".Substring(0, 30);

        return await _soapClient.CreateShipmentAsync(request, credentials);
    }

    /// <summary>
    /// Barkod numarası ile gönderi sorgular
    /// </summary>
    public async Task<GonderiSorgu2Response> TrackByBarcodeAsync(
        string barcodeNo,
        PttCredentials credentials = null)
    {
        credentials ??= await GetMarketplaceCredentialsAsync();

        if (credentials == null || string.IsNullOrEmpty(credentials.MusteriId))
        {
            return new GonderiSorgu2Response
            {
                Success = false,
                ErrorMessage = "PTT credential'ları yapılandırılmamış"
            };
        }

        var request = new GonderiSorgu2Request
        {
            BarkodNo = barcodeNo
        };

        return await _soapClient.TrackShipmentAsync(request, credentials);
    }

    /// <summary>
    /// Müşteri referans numarası ile gönderi sorgular
    /// </summary>
    public async Task<GonderiSorgu2Response> TrackByReferenceAsync(
        string referenceNo,
        PttCredentials credentials = null)
    {
        credentials ??= await GetMarketplaceCredentialsAsync();

        if (credentials == null || string.IsNullOrEmpty(credentials.MusteriId))
        {
            return new GonderiSorgu2Response
            {
                Success = false,
                ErrorMessage = "PTT credential'ları yapılandırılmamış"
            };
        }

        var request = new GonderiSorgu2Request
        {
            MusteriReferansNo = referenceNo
        };

        return await _soapClient.TrackShipmentAsync(request, credentials);
    }

    /// <summary>
    /// Satıcının PTT credential'larını getirir
    /// </summary>
    public async Task<PttCredentials> GetVendorCredentialsAsync(int vendorId)
    {
        try
        {
            // PTT provider'ı getir
            var provider = await _shippingProviderService.GetBySystemNameAsync(PTT_PROVIDER_SYSTEM_NAME);
            if (provider == null)
            {
                _logger.LogWarning("PTT provider not found");
                return null;
            }

            // Satıcının kayıtlı credential'larını getir
            var musteriId = await _vendorShippingService.GetCredentialValueAsync(vendorId, provider.Id, "MusteriId");
            var sifre = await _vendorShippingService.GetCredentialValueAsync(vendorId, provider.Id, "Sifre");

            if (string.IsNullOrEmpty(musteriId) || string.IsNullOrEmpty(sifre))
            {
                _logger.LogWarning("Vendor {VendorId} does not have PTT credentials configured", vendorId);
                return null;
            }

            return new PttCredentials
            {
                MusteriId = musteriId,
                Sifre = sifre,
                Kullanici = "PttWs",
                UseSandbox = _settings.UseSandbox
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting vendor PTT credentials for vendor {VendorId}", vendorId);
            return null;
        }
    }

    /// <summary>
    /// Pazaryeri PTT credential'larını getirir
    /// </summary>
    public Task<PttCredentials> GetMarketplaceCredentialsAsync()
    {
        if (!_settings.MarketplaceContractEnabled ||
            string.IsNullOrEmpty(_settings.MarketplaceMusteriId) ||
            string.IsNullOrEmpty(_settings.MarketplaceSifre))
        {
            _logger.LogWarning("Marketplace PTT credentials are not configured");
            return Task.FromResult<PttCredentials>(null);
        }

        return Task.FromResult(new PttCredentials
        {
            MusteriId = _settings.MarketplaceMusteriId,
            Sifre = _settings.MarketplaceSifre,
            Kullanici = "PttWs",
            UseSandbox = _settings.UseSandbox
        });
    }
}
