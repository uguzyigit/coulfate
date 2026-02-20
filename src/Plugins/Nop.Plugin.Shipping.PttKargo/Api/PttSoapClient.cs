using System.Text;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Nop.Plugin.Shipping.PttKargo.Api.Models;

namespace Nop.Plugin.Shipping.PttKargo.Api;

/// <summary>
/// PTT SOAP Web Service Client
/// </summary>
public class PttSoapClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PttSoapClient> _logger;

    public PttSoapClient(
        HttpClient httpClient,
        ILogger<PttSoapClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    #region Gönderi Oluşturma

    /// <summary>
    /// Gönderi oluşturur (kabulEkle2)
    /// </summary>
    public async Task<KabulEkle2Response> CreateShipmentAsync(
        KabulEkle2Request request,
        PttCredentials credentials)
    {
        var response = new KabulEkle2Response();

        try
        {
            var endpoint = credentials.UseSandbox
                ? PttEndpoints.VeriYuklemeTest
                : PttEndpoints.VeriYuklemeProduction;

            var soapRequest = BuildKabulEkle2Request(request, credentials);

            _logger.LogDebug("PTT kabulEkle2 Request: {Request}", soapRequest);

            var httpContent = new StringContent(soapRequest, Encoding.UTF8, "application/soap+xml");
            httpContent.Headers.Add("SOAPAction", PttEndpoints.KabulEkle2Action);

            var httpResponse = await _httpClient.PostAsync(endpoint, httpContent);
            var responseContent = await httpResponse.Content.ReadAsStringAsync();

            response.RawResponse = responseContent;

            _logger.LogDebug("PTT kabulEkle2 Response: {Response}", responseContent);

            if (!httpResponse.IsSuccessStatusCode)
            {
                response.Success = false;
                response.ErrorMessage = $"HTTP Error: {httpResponse.StatusCode}";
                return response;
            }

            // Parse response
            ParseKabulEkle2Response(responseContent, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PTT kabulEkle2 Error");
            response.Success = false;
            response.ErrorMessage = ex.Message;
        }

        return response;
    }

    private string BuildKabulEkle2Request(KabulEkle2Request request, PttCredentials credentials)
    {
        var sb = new StringBuilder();
        sb.AppendLine(@"<?xml version=""1.0"" encoding=""UTF-8""?>");
        sb.AppendLine(@"<soap:Envelope xmlns:soap=""http://www.w3.org/2003/05/soap-envelope"" xmlns:kab=""http://kabul.ptt.gov.tr"" xmlns:xsd=""http://kabul.ptt.gov.tr/xsd"">");
        sb.AppendLine(@"  <soap:Header/>");
        sb.AppendLine(@"  <soap:Body>");
        sb.AppendLine(@"    <kab:kabulEkle2>");
        sb.AppendLine(@"      <kab:input>");

        // Gönderiler
        foreach (var item in request.Gonderiler)
        {
            sb.AppendLine(@"        <xsd:dongu>");
            sb.AppendLine($@"          <xsd:aliciAdi>{EscapeXml(item.AliciAdi)}</xsd:aliciAdi>");
            sb.AppendLine($@"          <xsd:aAdres>{EscapeXml(item.Adres)}</xsd:aAdres>");
            sb.AppendLine($@"          <xsd:aliciIlAdi>{EscapeXml(item.IlAdi)}</xsd:aliciIlAdi>");
            sb.AppendLine($@"          <xsd:aliciIlceAdi>{EscapeXml(item.IlceAdi)}</xsd:aliciIlceAdi>");
            sb.AppendLine($@"          <xsd:aIlKodu>{item.IlKodu}</xsd:aIlKodu>");
            sb.AppendLine($@"          <xsd:aIlceKodu>{item.IlceKodu}</xsd:aIlceKodu>");

            if (!string.IsNullOrEmpty(item.Telefon))
                sb.AppendLine($@"          <xsd:aliciTel>{EscapeXml(item.Telefon)}</xsd:aliciTel>");
            if (!string.IsNullOrEmpty(item.Email))
                sb.AppendLine($@"          <xsd:aliciEmail>{EscapeXml(item.Email)}</xsd:aliciEmail>");
            if (!string.IsNullOrEmpty(item.Sms))
                sb.AppendLine($@"          <xsd:aliciSms>{EscapeXml(item.Sms)}</xsd:aliciSms>");

            if (!string.IsNullOrEmpty(item.BarkodNo))
                sb.AppendLine($@"          <xsd:barkodNo>{EscapeXml(item.BarkodNo)}</xsd:barkodNo>");

            sb.AppendLine($@"          <xsd:musteriReferansNo>{EscapeXml(item.MusteriReferansNo)}</xsd:musteriReferansNo>");
            sb.AppendLine($@"          <xsd:agirlik>{item.Agirlik.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture)}</xsd:agirlik>");
            sb.AppendLine($@"          <xsd:desi>{item.Desi.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture)}</xsd:desi>");
            sb.AppendLine($@"          <xsd:en>{item.En.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture)}</xsd:en>");
            sb.AppendLine($@"          <xsd:boy>{item.Boy.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture)}</xsd:boy>");
            sb.AppendLine($@"          <xsd:yukseklik>{item.Yukseklik.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture)}</xsd:yukseklik>");
            sb.AppendLine($@"          <xsd:ucret>{item.Ucret.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture)}</xsd:ucret>");
            sb.AppendLine($@"          <xsd:deger_ucreti>{item.DegerUcreti.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture)}</xsd:deger_ucreti>");
            sb.AppendLine($@"          <xsd:odeme_sart_ucreti>{item.OdemeSartUcreti.ToString("0", System.Globalization.CultureInfo.InvariantCulture)}</xsd:odeme_sart_ucreti>");

            if (!string.IsNullOrEmpty(item.OdemeSekli))
                sb.AppendLine($@"          <xsd:odemesekli>{EscapeXml(item.OdemeSekli)}</xsd:odemesekli>");
            if (!string.IsNullOrEmpty(item.TeslimTip))
                sb.AppendLine($@"          <xsd:teslim_tip>{EscapeXml(item.TeslimTip)}</xsd:teslim_tip>");
            if (!string.IsNullOrEmpty(item.EkHizmet))
                sb.AppendLine($@"          <xsd:ekhizmet>{EscapeXml(item.EkHizmet)}</xsd:ekhizmet>");

            // Gönderici bilgileri
            if (item.GondericiBilgi != null)
            {
                sb.AppendLine(@"          <xsd:gondericibilgi>");
                sb.AppendLine($@"            <xsd:gonderici_adi>{EscapeXml(item.GondericiBilgi.GondericiAdi)}</xsd:gonderici_adi>");
                sb.AppendLine($@"            <xsd:gonderici_adresi>{EscapeXml(item.GondericiBilgi.GondericiAdresi)}</xsd:gonderici_adresi>");
                sb.AppendLine($@"            <xsd:gonderici_il_ad>{EscapeXml(item.GondericiBilgi.GondericiIlAdi)}</xsd:gonderici_il_ad>");
                sb.AppendLine($@"            <xsd:gonderici_ilce_ad>{EscapeXml(item.GondericiBilgi.GondericiIlceAdi)}</xsd:gonderici_ilce_ad>");
                if (!string.IsNullOrEmpty(item.GondericiBilgi.GondericiTelefonu))
                    sb.AppendLine($@"            <xsd:gonderici_telefonu>{EscapeXml(item.GondericiBilgi.GondericiTelefonu)}</xsd:gonderici_telefonu>");
                if (!string.IsNullOrEmpty(item.GondericiBilgi.GondericiEmail))
                    sb.AppendLine($@"            <xsd:gonderici_email>{EscapeXml(item.GondericiBilgi.GondericiEmail)}</xsd:gonderici_email>");
                if (!string.IsNullOrEmpty(item.GondericiBilgi.GondericiSms))
                    sb.AppendLine($@"            <xsd:gonderici_sms>{EscapeXml(item.GondericiBilgi.GondericiSms)}</xsd:gonderici_sms>");
                if (!string.IsNullOrEmpty(item.GondericiBilgi.GondericiPostaKodu))
                    sb.AppendLine($@"            <xsd:gonderici_posta_kodu>{EscapeXml(item.GondericiBilgi.GondericiPostaKodu)}</xsd:gonderici_posta_kodu>");
                if (item.GondericiBilgi.GondericiUlkeId.HasValue)
                    sb.AppendLine($@"            <xsd:gonderici_ulke_id>{item.GondericiBilgi.GondericiUlkeId}</xsd:gonderici_ulke_id>");
                sb.AppendLine(@"          </xsd:gondericibilgi>");
            }

            // İade bilgileri
            if (!string.IsNullOrEmpty(item.IadeAliciAdi))
            {
                sb.AppendLine($@"          <xsd:iadeAliciAdi>{EscapeXml(item.IadeAliciAdi)}</xsd:iadeAliciAdi>");
                sb.AppendLine($@"          <xsd:iadeAAdres>{EscapeXml(item.IadeAdres)}</xsd:iadeAAdres>");
                sb.AppendLine($@"          <xsd:iadeAliciIlAdi>{EscapeXml(item.IadeIlAdi)}</xsd:iadeAliciIlAdi>");
                sb.AppendLine($@"          <xsd:iadeAliciIlceAdi>{EscapeXml(item.IadeIlceAdi)}</xsd:iadeAliciIlceAdi>");
                if (item.IadeIlKodu.HasValue)
                    sb.AppendLine($@"          <xsd:iadeAIlKodu>{item.IadeIlKodu}</xsd:iadeAIlKodu>");
                if (item.IadeIlceKodu.HasValue)
                    sb.AppendLine($@"          <xsd:iadeAIlceKodu>{item.IadeIlceKodu}</xsd:iadeAIlceKodu>");
                if (!string.IsNullOrEmpty(item.IadeTelefon))
                    sb.AppendLine($@"          <xsd:iadeAliciTel>{EscapeXml(item.IadeTelefon)}</xsd:iadeAliciTel>");
                if (!string.IsNullOrEmpty(item.IadeEmail))
                    sb.AppendLine($@"          <xsd:iadeAliciEmail>{EscapeXml(item.IadeEmail)}</xsd:iadeAliciEmail>");
            }

            sb.AppendLine(@"        </xsd:dongu>");
        }

        // Genel bilgiler
        sb.AppendLine($@"        <xsd:dosyaAdi>{EscapeXml(request.DosyaAdi)}</xsd:dosyaAdi>");
        sb.AppendLine($@"        <xsd:gonderiTip>{EscapeXml(request.GonderiTip)}</xsd:gonderiTip>");
        sb.AppendLine($@"        <xsd:gonderiTur>{EscapeXml(request.GonderiTur)}</xsd:gonderiTur>");
        sb.AppendLine($@"        <xsd:kullanici>{EscapeXml(credentials.Kullanici)}</xsd:kullanici>");
        sb.AppendLine($@"        <xsd:musteriId>{EscapeXml(credentials.MusteriId)}</xsd:musteriId>");
        sb.AppendLine($@"        <xsd:sifre>{EscapeXml(credentials.Sifre)}</xsd:sifre>");

        sb.AppendLine(@"      </kab:input>");
        sb.AppendLine(@"    </kab:kabulEkle2>");
        sb.AppendLine(@"  </soap:Body>");
        sb.AppendLine(@"</soap:Envelope>");

        return sb.ToString();
    }

    private void ParseKabulEkle2Response(string responseXml, KabulEkle2Response response)
    {
        try
        {
            var doc = XDocument.Parse(responseXml);
            var ns = XNamespace.Get(PttEndpoints.XsdNamespace);

            // Sonuç elementlerini bul
            var sonuclar = doc.Descendants(ns + "sonuc").ToList();

            if (sonuclar.Any())
            {
                response.Success = true;
                foreach (var sonuc in sonuclar)
                {
                    var resultItem = new KabulEkle2ResultItem
                    {
                        MusteriReferansNo = sonuc.Element(ns + "musteriReferansNo")?.Value,
                        BarkodNo = sonuc.Element(ns + "barkodNo")?.Value,
                        Success = sonuc.Element(ns + "durum")?.Value == "0",
                        ErrorMessage = sonuc.Element(ns + "aciklama")?.Value
                    };
                    response.Results.Add(resultItem);
                }
            }
            else
            {
                // Hata kontrolü
                var fault = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "Fault");
                if (fault != null)
                {
                    response.Success = false;
                    response.ErrorMessage = fault.Descendants()
                        .FirstOrDefault(e => e.Name.LocalName == "faultstring")?.Value
                        ?? "Unknown SOAP Fault";
                }
                else
                {
                    response.Success = false;
                    response.ErrorMessage = "Unexpected response format";
                }
            }
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.ErrorMessage = $"Parse Error: {ex.Message}";
        }
    }

    #endregion

    #region Gönderi Takip

    /// <summary>
    /// Gönderi sorgular (gonderiSorgu2)
    /// </summary>
    public async Task<GonderiSorgu2Response> TrackShipmentAsync(
        GonderiSorgu2Request request,
        PttCredentials credentials)
    {
        var response = new GonderiSorgu2Response();

        try
        {
            var endpoint = credentials.UseSandbox
                ? PttEndpoints.GonderiTakipTest
                : PttEndpoints.GonderiTakipProduction;

            var soapRequest = BuildGonderiSorgu2Request(request, credentials);

            _logger.LogDebug("PTT gonderiSorgu2 Request: {Request}", soapRequest);

            var httpContent = new StringContent(soapRequest, Encoding.UTF8, "application/soap+xml");
            httpContent.Headers.Add("SOAPAction", PttEndpoints.GonderiSorgu2Action);

            var httpResponse = await _httpClient.PostAsync(endpoint, httpContent);
            var responseContent = await httpResponse.Content.ReadAsStringAsync();

            response.RawResponse = responseContent;

            _logger.LogDebug("PTT gonderiSorgu2 Response: {Response}", responseContent);

            if (!httpResponse.IsSuccessStatusCode)
            {
                response.Success = false;
                response.ErrorMessage = $"HTTP Error: {httpResponse.StatusCode}";
                return response;
            }

            // Parse response
            ParseGonderiSorgu2Response(responseContent, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PTT gonderiSorgu2 Error");
            response.Success = false;
            response.ErrorMessage = ex.Message;
        }

        return response;
    }

    private string BuildGonderiSorgu2Request(GonderiSorgu2Request request, PttCredentials credentials)
    {
        var sb = new StringBuilder();
        sb.AppendLine(@"<?xml version=""1.0"" encoding=""UTF-8""?>");
        sb.AppendLine(@"<soap:Envelope xmlns:soap=""http://www.w3.org/2003/05/soap-envelope"" xmlns:ptt=""http://pttak.ptt.gov.tr"" xmlns:xsd=""http://pttak.ptt.gov.tr/xsd"">");
        sb.AppendLine(@"  <soap:Header/>");
        sb.AppendLine(@"  <soap:Body>");
        sb.AppendLine(@"    <ptt:gonderiSorgu2>");
        sb.AppendLine(@"      <ptt:input>");

        if (!string.IsNullOrEmpty(request.BarkodNo))
            sb.AppendLine($@"        <xsd:barkodNo>{EscapeXml(request.BarkodNo)}</xsd:barkodNo>");

        if (!string.IsNullOrEmpty(request.MusteriReferansNo))
            sb.AppendLine($@"        <xsd:musteriReferansNo>{EscapeXml(request.MusteriReferansNo)}</xsd:musteriReferansNo>");

        sb.AppendLine($@"        <xsd:kullanici>{EscapeXml(credentials.Kullanici)}</xsd:kullanici>");
        sb.AppendLine($@"        <xsd:musteriId>{EscapeXml(credentials.MusteriId)}</xsd:musteriId>");
        sb.AppendLine($@"        <xsd:sifre>{EscapeXml(credentials.Sifre)}</xsd:sifre>");

        sb.AppendLine(@"      </ptt:input>");
        sb.AppendLine(@"    </ptt:gonderiSorgu2>");
        sb.AppendLine(@"  </soap:Body>");
        sb.AppendLine(@"</soap:Envelope>");

        return sb.ToString();
    }

    private void ParseGonderiSorgu2Response(string responseXml, GonderiSorgu2Response response)
    {
        try
        {
            var doc = XDocument.Parse(responseXml);
            var ns = XNamespace.Get(PttEndpoints.GonderiTakipXsdNamespace);

            // Gönderi elementini bul
            var gonderi = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "gonderi");

            if (gonderi != null)
            {
                response.Success = true;
                response.GonderiBilgi = new GonderiBilgi
                {
                    BarkodNo = gonderi.Descendants().FirstOrDefault(e => e.Name.LocalName == "barkodNo")?.Value,
                    AliciAdi = gonderi.Descendants().FirstOrDefault(e => e.Name.LocalName == "aliciAdi")?.Value,
                    GondericiAdi = gonderi.Descendants().FirstOrDefault(e => e.Name.LocalName == "gondericiAdi")?.Value,
                    Durum = gonderi.Descendants().FirstOrDefault(e => e.Name.LocalName == "durum")?.Value,
                    TeslimAlan = gonderi.Descendants().FirstOrDefault(e => e.Name.LocalName == "teslimAlan")?.Value
                };

                // Tarihleri parse et
                var kabulTarihi = gonderi.Descendants().FirstOrDefault(e => e.Name.LocalName == "kabulTarihi")?.Value;
                if (!string.IsNullOrEmpty(kabulTarihi) && DateTime.TryParse(kabulTarihi, out var kTarih))
                    response.GonderiBilgi.KabulTarihi = kTarih;

                var teslimTarihi = gonderi.Descendants().FirstOrDefault(e => e.Name.LocalName == "teslimTarihi")?.Value;
                if (!string.IsNullOrEmpty(teslimTarihi) && DateTime.TryParse(teslimTarihi, out var tTarih))
                    response.GonderiBilgi.TeslimTarihi = tTarih;

                // Safahatları parse et
                var safahatlar = gonderi.Descendants().Where(e => e.Name.LocalName == "safahat").ToList();
                foreach (var safahat in safahatlar)
                {
                    var item = new GonderiSafahat
                    {
                        IslemAciklama = safahat.Descendants().FirstOrDefault(e => e.Name.LocalName == "islemAciklama")?.Value,
                        BirimAdi = safahat.Descendants().FirstOrDefault(e => e.Name.LocalName == "birimAdi")?.Value,
                        IslemKodu = safahat.Descendants().FirstOrDefault(e => e.Name.LocalName == "islemKodu")?.Value
                    };

                    var islemTarihi = safahat.Descendants().FirstOrDefault(e => e.Name.LocalName == "islemTarihi")?.Value;
                    if (!string.IsNullOrEmpty(islemTarihi) && DateTime.TryParse(islemTarihi, out var iTarih))
                        item.IslemTarihi = iTarih;

                    response.GonderiBilgi.Safahatlar.Add(item);
                }
            }
            else
            {
                // Hata kontrolü
                var fault = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "Fault");
                if (fault != null)
                {
                    response.Success = false;
                    response.ErrorMessage = fault.Descendants()
                        .FirstOrDefault(e => e.Name.LocalName == "faultstring")?.Value
                        ?? "Unknown SOAP Fault";
                }
                else
                {
                    response.Success = false;
                    response.ErrorMessage = "Gönderi bulunamadı";
                }
            }
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.ErrorMessage = $"Parse Error: {ex.Message}";
        }
    }

    #endregion

    #region Yardımcı Metodlar

    private static string EscapeXml(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return value
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;");
    }

    #endregion
}
