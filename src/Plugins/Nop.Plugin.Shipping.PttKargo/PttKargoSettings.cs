using Nop.Core.Configuration;

namespace Nop.Plugin.Shipping.PttKargo;

/// <summary>
/// PTT Kargo plugin settings - Marketplace contract credentials
/// </summary>
public class PttKargoSettings : ISettings
{
    /// <summary>
    /// PTT Müşteri Numarası (Pazaryeri anlaşması)
    /// </summary>
    public string MarketplaceMusteriId { get; set; }

    /// <summary>
    /// PTT Web Servis Şifresi (Pazaryeri anlaşması)
    /// </summary>
    public string MarketplaceSifre { get; set; }

    /// <summary>
    /// Test ortamı kullanılsın mı
    /// </summary>
    public bool UseSandbox { get; set; }

    /// <summary>
    /// Pazaryeri anlaşması aktif mi
    /// </summary>
    public bool MarketplaceContractEnabled { get; set; }

    /// <summary>
    /// Satıcıların kendi anlaşmalarını kullanmasına izin ver
    /// </summary>
    public bool AllowVendorContracts { get; set; }

    /// <summary>
    /// Varsayılan gönderici adı
    /// </summary>
    public string DefaultSenderName { get; set; }

    /// <summary>
    /// Varsayılan gönderici adresi
    /// </summary>
    public string DefaultSenderAddress { get; set; }

    /// <summary>
    /// Varsayılan gönderici il kodu
    /// </summary>
    public int DefaultSenderCityCode { get; set; }

    /// <summary>
    /// Varsayılan gönderici ilçe kodu
    /// </summary>
    public int DefaultSenderDistrictCode { get; set; }

    /// <summary>
    /// Varsayılan gönderici telefon
    /// </summary>
    public string DefaultSenderPhone { get; set; }
}
