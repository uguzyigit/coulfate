using Nop.Core;
using Nop.Data;
using Nop.Plugin.Integration.TrendyolMarketplace.Data;
using Nop.Plugin.Integration.TrendyolMarketplace.Infrastructure;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;

namespace Nop.Plugin.Integration.TrendyolMarketplace;

/// <summary>
/// Represents the Trendyol Marketplace integration plugin
/// </summary>
public class TrendyolMarketplacePlugin : BasePlugin
{
    private readonly IWebHelper _webHelper;
    private readonly ISettingService _settingService;
    private readonly ILocalizationService _localizationService;
    private readonly INopDataProvider _dataProvider;
    private readonly IEncryptionService _encryptionService;

    public TrendyolMarketplacePlugin(
        IWebHelper webHelper,
        ISettingService settingService,
        ILocalizationService localizationService,
        INopDataProvider dataProvider,
        IEncryptionService encryptionService)
    {
        _webHelper = webHelper;
        _settingService = settingService;
        _localizationService = localizationService;
        _dataProvider = dataProvider;
        _encryptionService = encryptionService;
    }

    /// <summary>
    /// Gets the configuration page URL
    /// </summary>
    public override string GetConfigurationPageUrl()
    {
        return $"{_webHelper.GetStoreLocation()}Admin/TrendyolAdmin/Configure";
    }

    /// <summary>
    /// Install the plugin
    /// </summary>
    public override async Task InstallAsync()
    {
        // Generate encryption key for API secrets
        var encryptionKey = Guid.NewGuid().ToString("N").Substring(0, 16);

        // Save default settings
        var settings = new TrendyolSettings
        {
            Enabled = true,
            ApiBaseUrl = TrendyolDefaults.DefaultApiBaseUrl,
            DefaultProductSyncIntervalMinutes = 60,
            DefaultStockPriceSyncIntervalMinutes = 15,
            CategoryBrandSyncIntervalHours = 24,
            ApiRequestDelayMs = 1000,
            MaxRetryCount = 3,
            ApiPageSize = 50,
            AutoCreateManufacturers = true,
            DownloadProductImages = true,
            DefaultStoreId = 0,
            PublishImportedProducts = false,
            SkipUnmappedCategories = true,
            ImportSpecificationAttributes = true,
            EncryptionKey = encryptionKey
        };
        await _settingService.SaveSettingAsync(settings);

        // Create database tables - execute each statement separately for better compatibility
        foreach (var statement in InstallationData.CreateTableScripts)
        {
            try
            {
                await _dataProvider.ExecuteNonQueryAsync(statement);
            }
            catch
            {
                // Table might already exist, continue with next statement
            }
        }

        // Run migration scripts (for existing installations)
        foreach (var statement in InstallationData.MigrationScripts)
        {
            try
            {
                await _dataProvider.ExecuteNonQueryAsync(statement);
            }
            catch
            {
                // Migration might fail if already applied, continue
            }
        }

        // Create scheduled tasks
        foreach (var statement in InstallationData.ScheduledTaskScripts)
        {
            try
            {
                await _dataProvider.ExecuteNonQueryAsync(statement);
            }
            catch
            {
                // Task might already exist, continue
            }
        }

        // Install localization resources
        await InstallLocalizationResourcesAsync();

        await base.InstallAsync();
    }

    /// <summary>
    /// Uninstall the plugin
    /// </summary>
    /// <remarks>
    /// GÜVENLI UNINSTALL: Veritabanı tabloları KORUNUR.
    /// Bu sayede plugin kaldırılıp yeniden kurulduğunda:
    /// - API credentials korunur
    /// - Kategori/marka eşlemeleri korunur
    /// - Ürün takip bilgileri korunur
    /// - Senkronizasyon logları korunur
    ///
    /// Verileri tamamen silmek için admin panelinden "Verileri Sil" seçeneğini kullanın
    /// veya veritabanından manuel olarak Trendyol* tablolarını silin.
    /// </remarks>
    public override async Task UninstallAsync()
    {
        // Delete settings (bu tekrar kurulumda varsayılanlarla oluşturulur)
        await _settingService.DeleteSettingAsync<TrendyolSettings>();

        // Delete scheduled tasks (plugin çalışmayacağı için gereksiz)
        await _dataProvider.ExecuteNonQueryAsync(InstallationData.DeleteScheduledTasksScript);

        // GÜVENLI: Veritabanı tabloları SİLİNMEZ - veriler korunur
        // Eski davranış (tehlikeli):
        // await _dataProvider.ExecuteNonQueryAsync(InstallationData.DropTablesScript);

        // Delete localization resources (tekrar kurulumda yeniden oluşturulur)
        await _localizationService.DeleteLocaleResourcesAsync(TrendyolDefaults.LocalizationPrefix);

        await base.UninstallAsync();
    }

    /// <summary>
    /// Tüm plugin verilerini kalıcı olarak siler (DİKKAT: GERİ ALINAMAZ!)
    /// </summary>
    /// <remarks>
    /// Bu metod sadece admin panelinden manuel çağrılmalıdır.
    /// Tüm Trendyol tablolarını ve verilerini kalıcı olarak siler.
    /// </remarks>
    public async Task PurgeAllDataAsync()
    {
        await _dataProvider.ExecuteNonQueryAsync(InstallationData.DropTablesScript);
    }

    /// <summary>
    /// Install localization resources
    /// </summary>
    private async Task InstallLocalizationResourcesAsync()
    {
        var resources = new Dictionary<string, string>
        {
            // Plugin info
            [$"{TrendyolDefaults.LocalizationPrefix}.FriendlyName"] = "Trendyol Pazaryeri Entegrasyonu",
            [$"{TrendyolDefaults.LocalizationPrefix}.Description"] = "Trendyol API entegrasyonu - Urun aktarimi, stok ve fiyat senkronizasyonu",

            // Menu items
            [$"{TrendyolDefaults.LocalizationPrefix}.Menu.Marketplace"] = "Pazaryeri",
            [$"{TrendyolDefaults.LocalizationPrefix}.Menu.Trendyol"] = "Trendyol Entegrasyonu",
            [$"{TrendyolDefaults.LocalizationPrefix}.Menu.Admin.Dashboard"] = "Kontrol Paneli",
            [$"{TrendyolDefaults.LocalizationPrefix}.Menu.Admin.Configure"] = "Yapilandirma",
            [$"{TrendyolDefaults.LocalizationPrefix}.Menu.Admin.CategoryMapping"] = "Kategori Eslestirme",
            [$"{TrendyolDefaults.LocalizationPrefix}.Menu.Admin.BrandMapping"] = "Marka Eslestirme",
            [$"{TrendyolDefaults.LocalizationPrefix}.Menu.Admin.AttributeMapping"] = "Ozellik Eslestirme",
            [$"{TrendyolDefaults.LocalizationPrefix}.Menu.Admin.VendorList"] = "Satici Listesi",
            [$"{TrendyolDefaults.LocalizationPrefix}.Menu.VendorPanel"] = "Urun Entegrasyonu",
            [$"{TrendyolDefaults.LocalizationPrefix}.Menu.Vendor.Trendyol"] = "Trendyol",
            [$"{TrendyolDefaults.LocalizationPrefix}.Menu.Vendor.Dashboard"] = "Kontrol Paneli",
            [$"{TrendyolDefaults.LocalizationPrefix}.Menu.Vendor.Credentials"] = "API Bilgileri",
            [$"{TrendyolDefaults.LocalizationPrefix}.Menu.Vendor.Products"] = "Urunler",
            [$"{TrendyolDefaults.LocalizationPrefix}.Menu.Vendor.SyncLogs"] = "Senkronizasyon Kayitlari",
            [$"{TrendyolDefaults.LocalizationPrefix}.Menu.Vendor.ManualSync"] = "Manuel Senkronizasyon",

            // Configuration fields
            [$"{TrendyolDefaults.LocalizationPrefix}.Fields.Enabled"] = "Aktif",
            [$"{TrendyolDefaults.LocalizationPrefix}.Fields.Enabled.Hint"] = "Trendyol entegrasyonunu aktif veya pasif yapar",
            [$"{TrendyolDefaults.LocalizationPrefix}.Fields.ApiBaseUrl"] = "API Adresi",
            [$"{TrendyolDefaults.LocalizationPrefix}.Fields.ApiBaseUrl.Hint"] = "Trendyol API temel adresi (varsayilan: https://apigw.trendyol.com/integration)",
            [$"{TrendyolDefaults.LocalizationPrefix}.Fields.DefaultProductSyncIntervalMinutes"] = "Urun Senk. Araligi (dk)",
            [$"{TrendyolDefaults.LocalizationPrefix}.Fields.DefaultProductSyncIntervalMinutes.Hint"] = "Urun senkronizasyonu icin varsayilan aralik (dakika)",
            [$"{TrendyolDefaults.LocalizationPrefix}.Fields.DefaultStockPriceSyncIntervalMinutes"] = "Stok/Fiyat Senk. Araligi (dk)",
            [$"{TrendyolDefaults.LocalizationPrefix}.Fields.DefaultStockPriceSyncIntervalMinutes.Hint"] = "Stok ve fiyat senkronizasyonu icin varsayilan aralik (dakika)",
            [$"{TrendyolDefaults.LocalizationPrefix}.Fields.CategoryBrandSyncIntervalHours"] = "Kategori/Marka Senk. (saat)",
            [$"{TrendyolDefaults.LocalizationPrefix}.Fields.CategoryBrandSyncIntervalHours.Hint"] = "Kategori ve marka listesi senkronizasyon araligi (saat)",
            [$"{TrendyolDefaults.LocalizationPrefix}.Fields.ApiRequestDelayMs"] = "API Istek Gecikmesi (ms)",
            [$"{TrendyolDefaults.LocalizationPrefix}.Fields.ApiRequestDelayMs.Hint"] = "Hiz sinirlamasini onlemek icin API istekleri arasindaki bekleme suresi (milisaniye)",
            [$"{TrendyolDefaults.LocalizationPrefix}.Fields.MaxRetryCount"] = "Maks. Yeniden Deneme",
            [$"{TrendyolDefaults.LocalizationPrefix}.Fields.MaxRetryCount.Hint"] = "Basarisiz API istekleri icin maksimum yeniden deneme sayisi",
            [$"{TrendyolDefaults.LocalizationPrefix}.Fields.ApiPageSize"] = "API Sayfa Boyutu",
            [$"{TrendyolDefaults.LocalizationPrefix}.Fields.ApiPageSize.Hint"] = "Her API isteginde cekilecek kayit sayisi",
            [$"{TrendyolDefaults.LocalizationPrefix}.Fields.AutoCreateManufacturers"] = "Otomatik Uretici Olustur",
            [$"{TrendyolDefaults.LocalizationPrefix}.Fields.AutoCreateManufacturers.Hint"] = "Eslesmeyen Trendyol markalari icin otomatik uretici olusturur",
            [$"{TrendyolDefaults.LocalizationPrefix}.Fields.DownloadProductImages"] = "Urun Resimlerini Indir",
            [$"{TrendyolDefaults.LocalizationPrefix}.Fields.DownloadProductImages.Hint"] = "Urun resimlerini yerel olarak indirir ve saklar",
            [$"{TrendyolDefaults.LocalizationPrefix}.Fields.PublishImportedProducts"] = "Ithal Urunleri Yayinla",
            [$"{TrendyolDefaults.LocalizationPrefix}.Fields.PublishImportedProducts.Hint"] = "Aktarim sonrasi urunleri otomatik olarak yayinlar",
            [$"{TrendyolDefaults.LocalizationPrefix}.Fields.SkipUnmappedCategories"] = "Eslesmeyen Kategorileri Atla",
            [$"{TrendyolDefaults.LocalizationPrefix}.Fields.SkipUnmappedCategories.Hint"] = "Eslesmemis kategorilerdeki urunleri atlar",
            [$"{TrendyolDefaults.LocalizationPrefix}.Fields.ImportSpecificationAttributes"] = "Spesifikasyon Ozelliklerini Aktar",
            [$"{TrendyolDefaults.LocalizationPrefix}.Fields.ImportSpecificationAttributes.Hint"] = "Trendyol urun ozelliklerini (Kalip, Kumasi, Desen vb.) NopCommerce spesifikasyon ozelligi olarak aktarir. Urun sayfasinda ve kategori filtrelerinde goruntulenir.",

            // Vendor Credentials fields
            [$"{TrendyolDefaults.LocalizationPrefix}.Vendor.Fields.TrendyolSupplierId"] = "Trendyol Satici ID",
            [$"{TrendyolDefaults.LocalizationPrefix}.Vendor.Fields.TrendyolSupplierId.Hint"] = "Trendyol satici/tedarikci kimlik numaraniz",
            [$"{TrendyolDefaults.LocalizationPrefix}.Vendor.Fields.IntegrationReferenceCode"] = "Entegrasyon Referans Kodu",
            [$"{TrendyolDefaults.LocalizationPrefix}.Vendor.Fields.IntegrationReferenceCode.Hint"] = "Trendyol entegrasyon referans kodunuz",
            [$"{TrendyolDefaults.LocalizationPrefix}.Vendor.Fields.ApiKey"] = "API Kullanici Adi",
            [$"{TrendyolDefaults.LocalizationPrefix}.Vendor.Fields.ApiKey.Hint"] = "Trendyol API kullanici adiniz",
            [$"{TrendyolDefaults.LocalizationPrefix}.Vendor.Fields.ApiSecret"] = "API Sifresi (Token)",
            [$"{TrendyolDefaults.LocalizationPrefix}.Vendor.Fields.ApiSecret.Hint"] = "Trendyol API token/sifreniz",
            [$"{TrendyolDefaults.LocalizationPrefix}.Vendor.Fields.IsActive"] = "Aktif",
            [$"{TrendyolDefaults.LocalizationPrefix}.Vendor.Fields.IsActive.Hint"] = "Bu satici icin senkronizasyonu aktif veya pasif yapar",
            [$"{TrendyolDefaults.LocalizationPrefix}.Vendor.Fields.AutoSyncEnabled"] = "Otomatik Senkronizasyon",
            [$"{TrendyolDefaults.LocalizationPrefix}.Vendor.Fields.AutoSyncEnabled.Hint"] = "Otomatik senkronizasyonu aktif eder",
            [$"{TrendyolDefaults.LocalizationPrefix}.Vendor.Fields.SyncIntervalMinutes"] = "Senk. Araligi (dakika)",
            [$"{TrendyolDefaults.LocalizationPrefix}.Vendor.Fields.SyncIntervalMinutes.Hint"] = "Otomatik senkronizasyon araligi (dakika)",

            // Category Mapping
            [$"{TrendyolDefaults.LocalizationPrefix}.CategoryMapping.Title"] = "Kategori Eslestirme",
            [$"{TrendyolDefaults.LocalizationPrefix}.CategoryMapping.TrendyolCategory"] = "Trendyol Kategorisi",
            [$"{TrendyolDefaults.LocalizationPrefix}.CategoryMapping.NopCategory"] = "NopCommerce Kategorisi",
            [$"{TrendyolDefaults.LocalizationPrefix}.CategoryMapping.IsAutoMapped"] = "Otomatik Eslesti",
            [$"{TrendyolDefaults.LocalizationPrefix}.CategoryMapping.SearchTrendyolCategory"] = "Trendyol Kategori Ara",
            [$"{TrendyolDefaults.LocalizationPrefix}.CategoryMapping.SearchNopCategory"] = "NopCommerce Kategori Ara",
            [$"{TrendyolDefaults.LocalizationPrefix}.CategoryMapping.UnmappedOnly"] = "Sadece Eslesmemisler",

            // Brand Mapping
            [$"{TrendyolDefaults.LocalizationPrefix}.BrandMapping.Title"] = "Marka Eslestirme",
            [$"{TrendyolDefaults.LocalizationPrefix}.BrandMapping.TrendyolBrand"] = "Trendyol Markasi",
            [$"{TrendyolDefaults.LocalizationPrefix}.BrandMapping.NopManufacturer"] = "NopCommerce Ureticisi",
            [$"{TrendyolDefaults.LocalizationPrefix}.BrandMapping.IsAutoMapped"] = "Otomatik Eslesti",
            [$"{TrendyolDefaults.LocalizationPrefix}.BrandMapping.SearchTrendyolBrand"] = "Trendyol Marka Ara",
            [$"{TrendyolDefaults.LocalizationPrefix}.BrandMapping.UnmappedOnly"] = "Sadece Eslesmemisler",
            [$"{TrendyolDefaults.LocalizationPrefix}.BrandMapping.TrendyolId"] = "Trendyol ID",
            [$"{TrendyolDefaults.LocalizationPrefix}.BrandMapping.EnterManufacturerId"] = "NopCommerce Uretici ID girin (0 temizler)",

            // Category Mapping Extra
            [$"{TrendyolDefaults.LocalizationPrefix}.CategoryMapping.LeafOnly"] = "Sadece Alt Kategoriler",
            [$"{TrendyolDefaults.LocalizationPrefix}.CategoryMapping.IsLeaf"] = "Alt Kategori",
            [$"{TrendyolDefaults.LocalizationPrefix}.CategoryMapping.EnterCategoryId"] = "NopCommerce Kategori ID girin (0 temizler)",

            // Common
            [$"{TrendyolDefaults.LocalizationPrefix}.Common.All"] = "Tumu",
            [$"{TrendyolDefaults.LocalizationPrefix}.Common.Search"] = "Ara",
            [$"{TrendyolDefaults.LocalizationPrefix}.Common.Action"] = "Islem",
            [$"{TrendyolDefaults.LocalizationPrefix}.Common.MappingUpdated"] = "Esleme basariyla guncellendi",
            [$"{TrendyolDefaults.LocalizationPrefix}.Common.AutoMapping"] = "Otomatik Esleme",

            // Attribute Mapping
            [$"{TrendyolDefaults.LocalizationPrefix}.AttributeMapping.Title"] = "Ozellik Eslestirme",
            [$"{TrendyolDefaults.LocalizationPrefix}.AttributeMapping.TrendyolAttribute"] = "Trendyol Ozelligi",
            [$"{TrendyolDefaults.LocalizationPrefix}.AttributeMapping.NopAttribute"] = "NopCommerce Ozelligi",
            [$"{TrendyolDefaults.LocalizationPrefix}.AttributeMapping.MappingType"] = "Eslestirme Tipi",
            [$"{TrendyolDefaults.LocalizationPrefix}.AttributeMapping.MappingType.None"] = "Eslenmedi",
            [$"{TrendyolDefaults.LocalizationPrefix}.AttributeMapping.MappingType.SpecificationAttribute"] = "Spesifikasyon Ozelligi",
            [$"{TrendyolDefaults.LocalizationPrefix}.AttributeMapping.MappingType.ProductAttribute"] = "Urun Ozelligi (Varyant)",
            [$"{TrendyolDefaults.LocalizationPrefix}.AttributeMapping.IsVariant"] = "Varyant Ozelligi mi",
            [$"{TrendyolDefaults.LocalizationPrefix}.AttributeMapping.IsRequired"] = "Zorunlu",

            // Product List
            [$"{TrendyolDefaults.LocalizationPrefix}.Products.Title"] = "Trendyol Urunleri",
            [$"{TrendyolDefaults.LocalizationPrefix}.Products.Barcode"] = "Barkod",
            [$"{TrendyolDefaults.LocalizationPrefix}.Products.ProductCode"] = "Urun Kodu",
            [$"{TrendyolDefaults.LocalizationPrefix}.Products.Title.Column"] = "Baslik",
            [$"{TrendyolDefaults.LocalizationPrefix}.Products.TrendyolPrice"] = "Trendyol Fiyati",
            [$"{TrendyolDefaults.LocalizationPrefix}.Products.TrendyolStock"] = "Trendyol Stok",
            [$"{TrendyolDefaults.LocalizationPrefix}.Products.ImportStatus"] = "Aktarim Durumu",
            [$"{TrendyolDefaults.LocalizationPrefix}.Products.ImportStatus.Pending"] = "Bekliyor",
            [$"{TrendyolDefaults.LocalizationPrefix}.Products.ImportStatus.Imported"] = "Aktarildi",
            [$"{TrendyolDefaults.LocalizationPrefix}.Products.ImportStatus.Failed"] = "Basarisiz",
            [$"{TrendyolDefaults.LocalizationPrefix}.Products.ImportStatus.Skipped"] = "Atlandi",
            [$"{TrendyolDefaults.LocalizationPrefix}.Products.ImportStatus.Deactivated"] = "Devre Disi",
            [$"{TrendyolDefaults.LocalizationPrefix}.Products.TrendyolOnSale"] = "Trendyol Satista",
            [$"{TrendyolDefaults.LocalizationPrefix}.Products.LastSyncOnUtc"] = "Son Senkronizasyon",
            [$"{TrendyolDefaults.LocalizationPrefix}.Products.NopProductId"] = "NopCommerce Urunu",

            // Sync Logs
            [$"{TrendyolDefaults.LocalizationPrefix}.SyncLogs.Title"] = "Senkronizasyon Kayitlari",
            [$"{TrendyolDefaults.LocalizationPrefix}.SyncLogs.SyncType"] = "Senk. Tipi",
            [$"{TrendyolDefaults.LocalizationPrefix}.SyncLogs.SyncType.Product"] = "Urun",
            [$"{TrendyolDefaults.LocalizationPrefix}.SyncLogs.SyncType.Stock"] = "Stok",
            [$"{TrendyolDefaults.LocalizationPrefix}.SyncLogs.SyncType.Price"] = "Fiyat",
            [$"{TrendyolDefaults.LocalizationPrefix}.SyncLogs.SyncType.Category"] = "Kategori",
            [$"{TrendyolDefaults.LocalizationPrefix}.SyncLogs.SyncType.Brand"] = "Marka",
            [$"{TrendyolDefaults.LocalizationPrefix}.SyncLogs.SyncType.Full"] = "Tam Senkronizasyon",
            [$"{TrendyolDefaults.LocalizationPrefix}.SyncLogs.StartedOnUtc"] = "Baslangic",
            [$"{TrendyolDefaults.LocalizationPrefix}.SyncLogs.CompletedOnUtc"] = "Bitis",
            [$"{TrendyolDefaults.LocalizationPrefix}.SyncLogs.TotalItems"] = "Toplam",
            [$"{TrendyolDefaults.LocalizationPrefix}.SyncLogs.SuccessCount"] = "Basarili",
            [$"{TrendyolDefaults.LocalizationPrefix}.SyncLogs.FailedCount"] = "Basarisiz",
            [$"{TrendyolDefaults.LocalizationPrefix}.SyncLogs.SkippedCount"] = "Atlanan",
            [$"{TrendyolDefaults.LocalizationPrefix}.SyncLogs.Status"] = "Durum",
            [$"{TrendyolDefaults.LocalizationPrefix}.SyncLogs.Status.Running"] = "Calisiyor",
            [$"{TrendyolDefaults.LocalizationPrefix}.SyncLogs.Status.Completed"] = "Tamamlandi",
            [$"{TrendyolDefaults.LocalizationPrefix}.SyncLogs.Status.CompletedWithErrors"] = "Hatalarla Tamamlandi",
            [$"{TrendyolDefaults.LocalizationPrefix}.SyncLogs.Status.Failed"] = "Basarisiz",
            [$"{TrendyolDefaults.LocalizationPrefix}.SyncLogs.Status.Cancelled"] = "Iptal Edildi",

            // Dashboard
            [$"{TrendyolDefaults.LocalizationPrefix}.Dashboard.Title"] = "Trendyol Kontrol Paneli",
            [$"{TrendyolDefaults.LocalizationPrefix}.Dashboard.TotalProducts"] = "Toplam Urun",
            [$"{TrendyolDefaults.LocalizationPrefix}.Dashboard.ImportedProducts"] = "Aktarilan Urun",
            [$"{TrendyolDefaults.LocalizationPrefix}.Dashboard.PendingProducts"] = "Bekleyen Urun",
            [$"{TrendyolDefaults.LocalizationPrefix}.Dashboard.FailedProducts"] = "Basarisiz Urun",
            [$"{TrendyolDefaults.LocalizationPrefix}.Dashboard.DeactivatedProducts"] = "Devre Disi Urun",
            [$"{TrendyolDefaults.LocalizationPrefix}.Dashboard.LastSync"] = "Son Senkronizasyon",
            [$"{TrendyolDefaults.LocalizationPrefix}.Dashboard.ConnectionStatus"] = "Baglanti Durumu",
            [$"{TrendyolDefaults.LocalizationPrefix}.Dashboard.Connected"] = "Bagli",
            [$"{TrendyolDefaults.LocalizationPrefix}.Dashboard.NotConnected"] = "Bagli Degil",
            [$"{TrendyolDefaults.LocalizationPrefix}.Dashboard.NotConfigured"] = "Yapilandirilmadi",
            [$"{TrendyolDefaults.LocalizationPrefix}.Dashboard.TotalVendors"] = "Toplam Satici",
            [$"{TrendyolDefaults.LocalizationPrefix}.Dashboard.Active"] = "Aktif",
            [$"{TrendyolDefaults.LocalizationPrefix}.Dashboard.Inactive"] = "Pasif",
            [$"{TrendyolDefaults.LocalizationPrefix}.Dashboard.TrendyolCategories"] = "Trendyol Kategorileri",
            [$"{TrendyolDefaults.LocalizationPrefix}.Dashboard.TrendyolBrands"] = "Trendyol Markalari",
            [$"{TrendyolDefaults.LocalizationPrefix}.Dashboard.Mapped"] = "Eslenmis",
            [$"{TrendyolDefaults.LocalizationPrefix}.Dashboard.Unmapped"] = "Eslenmemis",
            [$"{TrendyolDefaults.LocalizationPrefix}.Dashboard.Imported"] = "Aktarildi",
            [$"{TrendyolDefaults.LocalizationPrefix}.Dashboard.QuickActions"] = "Hizli Islemler",
            [$"{TrendyolDefaults.LocalizationPrefix}.Dashboard.LastCategorySync"] = "Son Kategori Senk.",
            [$"{TrendyolDefaults.LocalizationPrefix}.Dashboard.LastBrandSync"] = "Son Marka Senk.",
            [$"{TrendyolDefaults.LocalizationPrefix}.Dashboard.Never"] = "Hic",
            [$"{TrendyolDefaults.LocalizationPrefix}.Dashboard.VendorSummary"] = "Satici Ozeti",
            [$"{TrendyolDefaults.LocalizationPrefix}.Dashboard.Vendor"] = "Satici",
            [$"{TrendyolDefaults.LocalizationPrefix}.Dashboard.Status"] = "Durum",
            [$"{TrendyolDefaults.LocalizationPrefix}.Dashboard.Products"] = "Urunler",
            [$"{TrendyolDefaults.LocalizationPrefix}.Dashboard.NoVendorsConfigured"] = "Henuz satici yapilandirilmadi",
            [$"{TrendyolDefaults.LocalizationPrefix}.Dashboard.RecentSyncLogs"] = "Son Senkronizasyon Kayitlari",
            [$"{TrendyolDefaults.LocalizationPrefix}.Dashboard.Duration"] = "Sure",
            [$"{TrendyolDefaults.LocalizationPrefix}.Dashboard.NoSyncLogs"] = "Henuz senkronizasyon kaydi yok",
            [$"{TrendyolDefaults.LocalizationPrefix}.Dashboard.Syncing"] = "Senkronize Ediliyor",
            [$"{TrendyolDefaults.LocalizationPrefix}.Dashboard.ErrorOccurred"] = "Bir hata olustu",

            // Messages
            [$"{TrendyolDefaults.LocalizationPrefix}.Message.Saved"] = "Ayarlar basariyla kaydedildi",
            [$"{TrendyolDefaults.LocalizationPrefix}.Message.ConnectionSuccess"] = "Baglanti basarili!",
            [$"{TrendyolDefaults.LocalizationPrefix}.Message.ConnectionFailed"] = "Baglanti basarisiz: {0}",
            [$"{TrendyolDefaults.LocalizationPrefix}.Message.SyncStarted"] = "Senkronizasyon basladi",
            [$"{TrendyolDefaults.LocalizationPrefix}.Message.SyncCompleted"] = "Senkronizasyon tamamlandi",
            [$"{TrendyolDefaults.LocalizationPrefix}.Message.SyncFailed"] = "Senkronizasyon basarisiz: {0}",
            [$"{TrendyolDefaults.LocalizationPrefix}.Message.CategorySyncCompleted"] = "Kategori senkronizasyonu tamamlandi. {0} kategori senkronize edildi.",
            [$"{TrendyolDefaults.LocalizationPrefix}.Message.BrandSyncCompleted"] = "Marka senkronizasyonu tamamlandi. {0} marka senkronize edildi.",
            [$"{TrendyolDefaults.LocalizationPrefix}.Message.NoCredentials"] = "Lutfen once Trendyol API bilgilerinizi yapilandirin",
            [$"{TrendyolDefaults.LocalizationPrefix}.Message.VendorNotFound"] = "Satici bulunamadi",
            [$"{TrendyolDefaults.LocalizationPrefix}.Message.AlreadySyncing"] = "Bir senkronizasyon islemi zaten devam ediyor",

            // Buttons
            [$"{TrendyolDefaults.LocalizationPrefix}.Button.Save"] = "Kaydet",
            [$"{TrendyolDefaults.LocalizationPrefix}.Button.TestConnection"] = "Baglantiyi Test Et",
            [$"{TrendyolDefaults.LocalizationPrefix}.Button.SyncCategories"] = "Kategorileri Senkronize Et",
            [$"{TrendyolDefaults.LocalizationPrefix}.Button.SyncBrands"] = "Markalari Senkronize Et",
            [$"{TrendyolDefaults.LocalizationPrefix}.Button.StartProductSync"] = "Urun Senkronizasyonu Baslat",
            [$"{TrendyolDefaults.LocalizationPrefix}.Button.StartStockPriceSync"] = "Stok/Fiyat Senkronizasyonu Baslat",
            [$"{TrendyolDefaults.LocalizationPrefix}.Button.AutoMap"] = "Otomatik Esle",

            // Validation
            [$"{TrendyolDefaults.LocalizationPrefix}.Validation.SupplierIdRequired"] = "Satici ID zorunludur",
            [$"{TrendyolDefaults.LocalizationPrefix}.Validation.ApiKeyRequired"] = "API Anahtari zorunludur",
            [$"{TrendyolDefaults.LocalizationPrefix}.Validation.ApiSecretRequired"] = "API Sifresi zorunludur",

            // ManualSync Page
            [$"{TrendyolDefaults.LocalizationPrefix}.ManualSync.ProductSync"] = "Urun Senkronizasyonu",
            [$"{TrendyolDefaults.LocalizationPrefix}.ManualSync.ProductSync.Description"] = "Trendyol'dan yeni urunleri aktar ve mevcut urun bilgilerini guncelle.",
            [$"{TrendyolDefaults.LocalizationPrefix}.ManualSync.ProductSync.Running"] = "Urun senkronizasyonu devam ediyor...",
            [$"{TrendyolDefaults.LocalizationPrefix}.ManualSync.LastProductSync"] = "Son Urun Senkronizasyonu",
            [$"{TrendyolDefaults.LocalizationPrefix}.ManualSync.Started"] = "Baslama",
            [$"{TrendyolDefaults.LocalizationPrefix}.ManualSync.Status"] = "Durum",
            [$"{TrendyolDefaults.LocalizationPrefix}.ManualSync.Success"] = "Basarili",
            [$"{TrendyolDefaults.LocalizationPrefix}.ManualSync.Failed"] = "Basarisiz",
            [$"{TrendyolDefaults.LocalizationPrefix}.ManualSync.Skipped"] = "Atlanan",
            [$"{TrendyolDefaults.LocalizationPrefix}.ManualSync.StockPriceSync"] = "Stok & Fiyat Senkronizasyonu",
            [$"{TrendyolDefaults.LocalizationPrefix}.ManualSync.StockPriceSync.Description"] = "Mevcut urunlerin stok miktarlarini ve fiyatlarini Trendyol'dan guncelle.",
            [$"{TrendyolDefaults.LocalizationPrefix}.ManualSync.StockPriceSync.Running"] = "Stok/Fiyat senkronizasyonu devam ediyor...",
            [$"{TrendyolDefaults.LocalizationPrefix}.ManualSync.LastStockSync"] = "Son Stok Senkronizasyonu",
            [$"{TrendyolDefaults.LocalizationPrefix}.ManualSync.LastPriceSync"] = "Son Fiyat Senkronizasyonu",
            [$"{TrendyolDefaults.LocalizationPrefix}.ManualSync.ApiDebug"] = "API Hata Ayiklama",
            [$"{TrendyolDefaults.LocalizationPrefix}.ManualSync.ApiDebug.Description"] = "Hesabiniz icin Trendyol API'sinin ne dondurdugunu test edin. Bu, aktarim sorunlarini teshis etmeye yardimci olur.",
            [$"{TrendyolDefaults.LocalizationPrefix}.ManualSync.ApiDebug.TestButton"] = "API Yanitini Test Et",
            [$"{TrendyolDefaults.LocalizationPrefix}.ManualSync.SyncInfo"] = "Senkronizasyon Bilgisi",
            [$"{TrendyolDefaults.LocalizationPrefix}.ManualSync.SyncInfo.ProductSync"] = "Trendyol'dan tum urunleri ceker ve yeni urunleri aktarir. Ayrica mevcut urun detaylarini gunceller.",
            [$"{TrendyolDefaults.LocalizationPrefix}.ManualSync.SyncInfo.StockPriceSync"] = "Onceden aktarilmis urunlerin stok miktarlarini ve fiyatlarini gunceller. Tam urun senkronizasyonundan daha hizlidir.",
            [$"{TrendyolDefaults.LocalizationPrefix}.ManualSync.SyncInfo.AutoSync"] = "Otomatik senkronizasyon, yapilandirdiginiz araliga gore arka planda calisir.",
            [$"{TrendyolDefaults.LocalizationPrefix}.ManualSync.SyncInfo.ManualSync"] = "Manuel senkronizasyon, baska bir senkronizasyon calismiyor iken istediginiz zaman baslatilabilir.",
            [$"{TrendyolDefaults.LocalizationPrefix}.ManualSync.StartingSync"] = "Baslatiliyor...",
            [$"{TrendyolDefaults.LocalizationPrefix}.ManualSync.QueryingApi"] = "API sorgulanıyor...",

            // Common Extended
            [$"{TrendyolDefaults.LocalizationPrefix}.Common.Error"] = "Bir hata olustu",
            [$"{TrendyolDefaults.LocalizationPrefix}.Common.Loading"] = "Yukleniyor...",

            // Vendor Dashboard Extended
            [$"{TrendyolDefaults.LocalizationPrefix}.VendorDashboard.ConfigureNow"] = "Simdi yapilandir",
            [$"{TrendyolDefaults.LocalizationPrefix}.VendorDashboard.ConnectionStatus"] = "Baglanti Durumu",
            [$"{TrendyolDefaults.LocalizationPrefix}.VendorDashboard.RecentSyncLogs"] = "Son Senkronizasyon Kayitlari",
            [$"{TrendyolDefaults.LocalizationPrefix}.VendorDashboard.NoSyncLogs"] = "Senkronizasyon kaydi bulunamadi",
            [$"{TrendyolDefaults.LocalizationPrefix}.VendorDashboard.ViewAllLogs"] = "Tum kayitlari gor",
            [$"{TrendyolDefaults.LocalizationPrefix}.VendorDashboard.Syncing"] = "Senkronize ediliyor...",

            // Products Extended
            [$"{TrendyolDefaults.LocalizationPrefix}.Products.SearchTerm"] = "Ara (Barkod, Urun Kodu, Baslik)",

            // Import Status
            [$"{TrendyolDefaults.LocalizationPrefix}.ImportStatus.Pending"] = "Bekliyor",
            [$"{TrendyolDefaults.LocalizationPrefix}.ImportStatus.Imported"] = "Aktarildi",
            [$"{TrendyolDefaults.LocalizationPrefix}.ImportStatus.Failed"] = "Basarisiz",
            [$"{TrendyolDefaults.LocalizationPrefix}.ImportStatus.Skipped"] = "Atlandi",

            // Sync Type
            [$"{TrendyolDefaults.LocalizationPrefix}.SyncType.Product"] = "Urun",
            [$"{TrendyolDefaults.LocalizationPrefix}.SyncType.Stock"] = "Stok",
            [$"{TrendyolDefaults.LocalizationPrefix}.SyncType.Price"] = "Fiyat",

            // Sync Status
            [$"{TrendyolDefaults.LocalizationPrefix}.SyncStatus.Running"] = "Calisiyor",
            [$"{TrendyolDefaults.LocalizationPrefix}.SyncStatus.Completed"] = "Tamamlandi",
            [$"{TrendyolDefaults.LocalizationPrefix}.SyncStatus.CompletedWithErrors"] = "Hatalarla Tamamlandi",
            [$"{TrendyolDefaults.LocalizationPrefix}.SyncStatus.Failed"] = "Basarisiz",
            [$"{TrendyolDefaults.LocalizationPrefix}.SyncStatus.WithErrors"] = "Hatali",

            // Sync Logs Extended
            [$"{TrendyolDefaults.LocalizationPrefix}.SyncLogs.Duration"] = "Sure",

            // Data Purge (Danger Zone)
            [$"{TrendyolDefaults.LocalizationPrefix}.PurgeData.DangerZone"] = "Tehlikeli Bolge",
            [$"{TrendyolDefaults.LocalizationPrefix}.PurgeData.Warning"] = "Dikkat! Bu islem geri alinamaz!",
            [$"{TrendyolDefaults.LocalizationPrefix}.PurgeData.Description"] = "Asagidaki buton tum Trendyol entegrasyon verilerini kalici olarak siler:",
            [$"{TrendyolDefaults.LocalizationPrefix}.PurgeData.Item.Credentials"] = "Tum satici API bilgileri (SupplierId, ApiKey, Token)",
            [$"{TrendyolDefaults.LocalizationPrefix}.PurgeData.Item.CategoryMappings"] = "Tum kategori eslemeleri",
            [$"{TrendyolDefaults.LocalizationPrefix}.PurgeData.Item.BrandMappings"] = "Tum marka eslemeleri",
            [$"{TrendyolDefaults.LocalizationPrefix}.PurgeData.Item.ProductTracking"] = "Tum urun takip bilgileri (hangi urun aktarildi)",
            [$"{TrendyolDefaults.LocalizationPrefix}.PurgeData.Item.SyncLogs"] = "Tum senkronizasyon kayitlari",
            [$"{TrendyolDefaults.LocalizationPrefix}.PurgeData.NopProductsNote"] = "Not: NopCommerce'e aktarilmis urunler SILINMEZ, sadece Trendyol ile baglantilari kesilir.",
            [$"{TrendyolDefaults.LocalizationPrefix}.PurgeData.ConfirmLabel"] = "Onay icin 'SIL' yazin",
            [$"{TrendyolDefaults.LocalizationPrefix}.PurgeData.ConfirmHint"] = "Silme islemini onaylamak icin yukaridaki alana buyuk harflerle 'SIL' yazin",
            [$"{TrendyolDefaults.LocalizationPrefix}.PurgeData.Button"] = "Tum Verileri Sil",
            [$"{TrendyolDefaults.LocalizationPrefix}.PurgeData.ConfirmationRequired"] = "Onay icin 'SIL' yazmaniz gerekiyor",
            [$"{TrendyolDefaults.LocalizationPrefix}.PurgeData.FinalConfirm"] = "EMIN MISINIZ? Tum Trendyol verileri kalici olarak silinecek. Bu islem geri alinamaz!",
            [$"{TrendyolDefaults.LocalizationPrefix}.PurgeData.Success"] = "Tum Trendyol verileri basariyla silindi. Tablolar bos olarak yeniden olusturuldu.",
            [$"{TrendyolDefaults.LocalizationPrefix}.PurgeData.Failed"] = "Veri silme islemi basarisiz: {0}"
        };

        await _localizationService.AddOrUpdateLocaleResourceAsync(resources);
    }
}
