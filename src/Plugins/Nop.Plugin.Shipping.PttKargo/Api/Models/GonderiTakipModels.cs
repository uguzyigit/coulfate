namespace Nop.Plugin.Shipping.PttKargo.Api.Models;

/// <summary>
/// Gönderi takip sorgusu isteği
/// </summary>
public class GonderiSorgu2Request
{
    /// <summary>
    /// Barkod numarası ile sorgulama
    /// </summary>
    public string BarkodNo { get; set; }

    /// <summary>
    /// Müşteri referans numarası ile sorgulama
    /// </summary>
    public string MusteriReferansNo { get; set; }
}

/// <summary>
/// Gönderi takip sorgusu yanıtı
/// </summary>
public class GonderiSorgu2Response
{
    /// <summary>
    /// İşlem başarılı mı
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Hata mesajı
    /// </summary>
    public string ErrorMessage { get; set; }

    /// <summary>
    /// Gönderi bilgisi
    /// </summary>
    public GonderiBilgi GonderiBilgi { get; set; }

    /// <summary>
    /// Ham API yanıtı
    /// </summary>
    public string RawResponse { get; set; }
}

/// <summary>
/// Gönderi bilgisi
/// </summary>
public class GonderiBilgi
{
    /// <summary>
    /// Barkod numarası
    /// </summary>
    public string BarkodNo { get; set; }

    /// <summary>
    /// Alıcı adı
    /// </summary>
    public string AliciAdi { get; set; }

    /// <summary>
    /// Gönderici adı
    /// </summary>
    public string GondericiAdi { get; set; }

    /// <summary>
    /// Gönderi durumu
    /// </summary>
    public string Durum { get; set; }

    /// <summary>
    /// Kabul tarihi
    /// </summary>
    public DateTime? KabulTarihi { get; set; }

    /// <summary>
    /// Teslim tarihi
    /// </summary>
    public DateTime? TeslimTarihi { get; set; }

    /// <summary>
    /// Teslim alan kişi
    /// </summary>
    public string TeslimAlan { get; set; }

    /// <summary>
    /// Gönderi aşamaları (safahat)
    /// </summary>
    public List<GonderiSafahat> Safahatlar { get; set; } = new();
}

/// <summary>
/// Gönderi aşaması (safahat)
/// </summary>
public class GonderiSafahat
{
    /// <summary>
    /// İşlem tarihi
    /// </summary>
    public DateTime IslemTarihi { get; set; }

    /// <summary>
    /// İşlem açıklaması
    /// </summary>
    public string IslemAciklama { get; set; }

    /// <summary>
    /// Birim adı (işlemin yapıldığı yer)
    /// </summary>
    public string BirimAdi { get; set; }

    /// <summary>
    /// İşlem kodu
    /// </summary>
    public string IslemKodu { get; set; }
}

/// <summary>
/// İl bilgisi
/// </summary>
public class IlBilgi
{
    public int IlKodu { get; set; }
    public string IlAdi { get; set; }
}

/// <summary>
/// İlçe bilgisi
/// </summary>
public class IlceBilgi
{
    public int IlceKodu { get; set; }
    public string IlceAdi { get; set; }
    public int IlKodu { get; set; }
}
