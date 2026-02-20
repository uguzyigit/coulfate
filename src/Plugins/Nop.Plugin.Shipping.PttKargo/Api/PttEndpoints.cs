namespace Nop.Plugin.Shipping.PttKargo.Api;

/// <summary>
/// PTT Web Service Endpoints
/// </summary>
public static class PttEndpoints
{
    // Veri Yükleme (Gönderi Oluşturma)
    public const string VeriYuklemeProduction = "https://pttws.ptt.gov.tr/PttVeriYukleme/services/Sorgu?wsdl";
    public const string VeriYuklemeTest = "https://pttws.ptt.gov.tr/PttVeriYuklemeTest/services/Sorgu?wsdl";

    // Gönderi Takip V2
    public const string GonderiTakipProduction = "https://pttws.ptt.gov.tr/GonderiTakipV2/services/Sorgu?wsdl";
    public const string GonderiTakipTest = "https://pttws.ptt.gov.tr/GonderiTakipV2Test/services/Sorgu?wsdl";

    // İl/İlçe Bilgi (Public)
    public const string PttBilgi = "https://pttws.ptt.gov.tr/PttBilgi/services/Sorgu?wsdl";
    public const string PttBilgiKullanici = "pttUser";
    public const string PttBilgiSifre = "PttBilgi*2015";

    // SOAP Action URLs
    public const string KabulEkle2Action = "urn:kabulEkle2";
    public const string GonderiSorgu2Action = "urn:gonderiSorgu2";
    public const string IlSorguAction = "urn:ilSorgu";
    public const string IlceSorguAction = "urn:ilceSorgu";

    // Namespaces
    public const string SoapNamespace = "http://www.w3.org/2003/05/soap-envelope";
    public const string KabulNamespace = "http://kabul.ptt.gov.tr";
    public const string XsdNamespace = "http://kabul.ptt.gov.tr/xsd";
    public const string GonderiTakipNamespace = "http://pttak.ptt.gov.tr";
    public const string GonderiTakipXsdNamespace = "http://pttak.ptt.gov.tr/xsd";
}

/// <summary>
/// PTT Credentials model
/// </summary>
public class PttCredentials
{
    /// <summary>
    /// Müşteri Numarası
    /// </summary>
    public string MusteriId { get; set; }

    /// <summary>
    /// Web Servis Şifresi
    /// </summary>
    public string Sifre { get; set; }

    /// <summary>
    /// Kullanıcı (Sabit: PttWs)
    /// </summary>
    public string Kullanici { get; set; } = "PttWs";

    /// <summary>
    /// Test ortamı mı
    /// </summary>
    public bool UseSandbox { get; set; }
}
