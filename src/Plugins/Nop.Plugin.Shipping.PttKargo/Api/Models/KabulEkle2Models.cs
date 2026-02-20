namespace Nop.Plugin.Shipping.PttKargo.Api.Models;

/// <summary>
/// Gönderi oluşturma isteği
/// </summary>
public class KabulEkle2Request
{
    /// <summary>
    /// Gönderiler listesi
    /// </summary>
    public List<KabulEkle2Item> Gonderiler { get; set; } = new();

    /// <summary>
    /// Dosya adı (benzersiz bir isim)
    /// </summary>
    public string DosyaAdi { get; set; }

    /// <summary>
    /// Gönderi tipi: NORMAL, TAAHHUTLU, vb.
    /// </summary>
    public string GonderiTip { get; set; } = "NORMAL";

    /// <summary>
    /// Gönderi türü: KARGO, APG, GONDERI
    /// </summary>
    public string GonderiTur { get; set; } = "KARGO";
}

/// <summary>
/// Tek bir gönderi bilgisi
/// </summary>
public class KabulEkle2Item
{
    // Alıcı bilgileri
    public string AliciAdi { get; set; }
    public string Adres { get; set; }
    public string IlAdi { get; set; }
    public string IlceAdi { get; set; }
    public int IlKodu { get; set; }
    public int IlceKodu { get; set; }
    public string Telefon { get; set; }
    public string Email { get; set; }
    public string Sms { get; set; }

    // Gönderi bilgileri
    public string BarkodNo { get; set; }
    public string MusteriReferansNo { get; set; }
    public decimal Agirlik { get; set; } = 1;
    public decimal Desi { get; set; } = 1;
    public decimal En { get; set; } = 1;
    public decimal Boy { get; set; } = 1;
    public decimal Yukseklik { get; set; } = 1;
    public decimal Ucret { get; set; }
    public decimal DegerUcreti { get; set; }
    public decimal OdemeSartUcreti { get; set; }
    public string OdemeSekli { get; set; }
    public string TeslimTip { get; set; }
    public string EkHizmet { get; set; }

    // Gönderici bilgileri
    public GondericiBilgi GondericiBilgi { get; set; }

    // İade bilgileri
    public string IadeAliciAdi { get; set; }
    public string IadeAdres { get; set; }
    public string IadeIlAdi { get; set; }
    public string IadeIlceAdi { get; set; }
    public int? IadeIlKodu { get; set; }
    public int? IadeIlceKodu { get; set; }
    public string IadeTelefon { get; set; }
    public string IadeEmail { get; set; }
}

/// <summary>
/// Gönderici bilgileri
/// </summary>
public class GondericiBilgi
{
    public string GondericiAdi { get; set; }
    public string GondericiAdresi { get; set; }
    public string GondericiIlAdi { get; set; }
    public string GondericiIlceAdi { get; set; }
    public string GondericiTelefonu { get; set; }
    public string GondericiEmail { get; set; }
    public string GondericiSms { get; set; }
    public string GondericiPostaKodu { get; set; }
    public int? GondericiUlkeId { get; set; } = 1; // Türkiye
}

/// <summary>
/// Gönderi oluşturma yanıtı
/// </summary>
public class KabulEkle2Response
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
    /// Oluşturulan barkod numaraları
    /// </summary>
    public List<KabulEkle2ResultItem> Results { get; set; } = new();

    /// <summary>
    /// Ham API yanıtı
    /// </summary>
    public string RawResponse { get; set; }
}

/// <summary>
/// Gönderi sonucu
/// </summary>
public class KabulEkle2ResultItem
{
    public string MusteriReferansNo { get; set; }
    public string BarkodNo { get; set; }
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
}
