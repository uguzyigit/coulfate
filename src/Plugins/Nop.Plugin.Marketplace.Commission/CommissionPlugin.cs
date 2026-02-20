using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;

namespace Nop.Plugin.Marketplace.Commission;

public class CommissionPlugin : BasePlugin
{
    private readonly IWebHelper _webHelper;
    private readonly ISettingService _settingService;
    private readonly ILocalizationService _localizationService;
    private readonly ILanguageService _languageService;

    public CommissionPlugin(
        IWebHelper webHelper,
        ISettingService settingService,
        ILocalizationService localizationService,
        ILanguageService languageService)
    {
        _webHelper = webHelper;
        _settingService = settingService;
        _localizationService = localizationService;
        _languageService = languageService;
    }

    public override string GetConfigurationPageUrl()
    {
        return $"{_webHelper.GetStoreLocation()}Admin/Commission/Configure";
    }

    public override async Task InstallAsync()
    {
        var settings = new Domain.CommissionSettings
        {
            DefaultCommissionRate = 10.0m,
            MarketplaceFeePerOrder = 5.0m,
            TaxWithholdingRate = 0.01m,
            CalculateOnDiscountedPrice = true,
            IncludeShippingInCommission = false
        };
        await _settingService.SaveSettingAsync(settings);

        // English Resources
        await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
        {
            // Plugin Info
            ["Plugins.Marketplace.Commission.FriendlyName"] = "Commission Management",
            ["Plugins.Marketplace.Commission.Description"] = "Manage commission rates for categories and products",
            
            // Menu Items
            ["Plugins.Marketplace.Commission.Menu.Marketplace"] = "Marketplace",
            ["Plugins.Marketplace.Commission.Menu.Commission"] = "Commission",
            ["Plugins.Marketplace.Commission.Menu.Settings"] = "Settings",
            ["Plugins.Marketplace.Commission.Menu.CategoryRates"] = "Category Rates",
            ["Plugins.Marketplace.Commission.Menu.ProductRates"] = "Product Rates",
            ["Plugins.Marketplace.Commission.Menu.Reports"] = "Reports",
            
            // Configure Page
            ["Plugins.Marketplace.Commission.Configure"] = "Commission Settings",
            ["Plugins.Marketplace.Commission.Fields.DefaultCommissionRate"] = "Default Commission Rate (%)",
            ["Plugins.Marketplace.Commission.Fields.DefaultCommissionRate.Hint"] = "Default commission rate when no category or product rate is set",
            ["Plugins.Marketplace.Commission.Fields.MarketplaceFeePerOrder"] = "Marketplace Fee Per Order",
            ["Plugins.Marketplace.Commission.Fields.MarketplaceFeePerOrder.Hint"] = "Fixed fee charged per order",
            ["Plugins.Marketplace.Commission.Fields.TaxWithholdingRate"] = "Tax Withholding Rate (%)",
            ["Plugins.Marketplace.Commission.Fields.TaxWithholdingRate.Hint"] = "Tax withholding percentage (stopaj)",
            ["Plugins.Marketplace.Commission.Fields.CalculateOnDiscountedPrice"] = "Calculate on Discounted Price",
            ["Plugins.Marketplace.Commission.Fields.CalculateOnDiscountedPrice.Hint"] = "Calculate commission after applying discounts",
            ["Plugins.Marketplace.Commission.Fields.IncludeShippingInCommission"] = "Include Shipping in Commission",
            ["Plugins.Marketplace.Commission.Fields.IncludeShippingInCommission.Hint"] = "Include shipping cost in commission calculation",
            
            // Category Commissions
            ["Plugins.Marketplace.Commission.CategoryCommissions"] = "Category Commission Rates",
            ["Plugins.Marketplace.Commission.CategoryCommissions.Title"] = "Category Commission Management",
            ["Plugins.Marketplace.Commission.CategoryCommissions.Info"] = "Set commission rates for each category. If not set, default rate will be used.",
            ["Plugins.Marketplace.Commission.CategoryCommissions.List"] = "Category List",
            ["Plugins.Marketplace.Commission.CategoryCommissions.Category"] = "Category",
            ["Plugins.Marketplace.Commission.CategoryCommissions.Rate"] = "Rate (%)",
            ["Plugins.Marketplace.Commission.CategoryCommissions.Actions"] = "Actions",
            ["Plugins.Marketplace.Commission.CategoryCommissions.Edit"] = "Edit",
            ["Plugins.Marketplace.Commission.CategoryCommissions.Save"] = "Save",
            ["Plugins.Marketplace.Commission.CategoryCommissions.Success"] = "Commission rate saved successfully",
            ["Plugins.Marketplace.Commission.CategoryCommissions.NoCategories"] = "No categories found",
            
            // Product Commissions
            ["Plugins.Marketplace.Commission.ProductCommissions"] = "Product Commission Rates",
            ["Plugins.Marketplace.Commission.ProductCommissions.Title"] = "Product Commission Management",
            ["Plugins.Marketplace.Commission.ProductCommissions.Info"] = "Set special commission rates for specific products. These override category rates and can have expiration dates for campaigns.",
            ["Plugins.Marketplace.Commission.ProductCommissions.AddNew"] = "Add New Product Commission",
            ["Plugins.Marketplace.Commission.ProductCommissions.List"] = "Product Commission List",
            ["Plugins.Marketplace.Commission.ProductCommissions.NoProducts"] = "No product commissions found",
            ["Plugins.Marketplace.Commission.ProductCommissions.ConfirmDelete"] = "Are you sure you want to delete this product commission?",
            
            ["Plugins.Marketplace.Commission.Fields.ProductId"] = "Product ID",
            ["Plugins.Marketplace.Commission.Fields.ProductName"] = "Product Name",
            ["Plugins.Marketplace.Commission.Fields.CategoryName"] = "Category",
            ["Plugins.Marketplace.Commission.Fields.CommissionRate"] = "Commission Rate (%)",
            ["Plugins.Marketplace.Commission.Fields.ExpiresOn"] = "Expires On",
            ["Plugins.Marketplace.Commission.Fields.Reason"] = "Reason",
            ["Plugins.Marketplace.Commission.Fields.CreatedOn"] = "Created On",
            ["Plugins.Marketplace.Commission.Fields.Actions"] = "Actions",
            
            // Reports
            ["Plugins.Marketplace.Commission.Reports"] = "Commission Reports",
            ["Plugins.Marketplace.Commission.Reports.Title"] = "Commission Reports",
            ["Plugins.Marketplace.Commission.Reports.Summary"] = "Summary",
            ["Plugins.Marketplace.Commission.Reports.Filters"] = "Filters",
            ["Plugins.Marketplace.Commission.Reports.StartDate"] = "Start Date",
            ["Plugins.Marketplace.Commission.Reports.EndDate"] = "End Date",
            ["Plugins.Marketplace.Commission.Reports.ApplyFilter"] = "Apply Filter",
            ["Plugins.Marketplace.Commission.Reports.ExportExcel"] = "Export Excel",
            ["Plugins.Marketplace.Commission.Reports.TotalOrders"] = "Total Orders",
            ["Plugins.Marketplace.Commission.Reports.Revenue"] = "Revenue",
            ["Plugins.Marketplace.Commission.Reports.Commission"] = "Commission",
            ["Plugins.Marketplace.Commission.Reports.MarketplaceFee"] = "Marketplace Fee",
            ["Plugins.Marketplace.Commission.Reports.TaxWithholding"] = "Tax Withholding",
            ["Plugins.Marketplace.Commission.Reports.VendorNet"] = "Vendor Net",
            ["Plugins.Marketplace.Commission.Reports.Details"] = "Commission Details (Last 100)",
            ["Plugins.Marketplace.Commission.Reports.Order"] = "Order #",
            ["Plugins.Marketplace.Commission.Reports.Vendor"] = "Vendor",
            ["Plugins.Marketplace.Commission.Reports.Product"] = "Product",
            ["Plugins.Marketplace.Commission.Reports.NetPrice"] = "Net Price",
            ["Plugins.Marketplace.Commission.Reports.Rate"] = "Rate",
            ["Plugins.Marketplace.Commission.Reports.Fee"] = "Fee",
            ["Plugins.Marketplace.Commission.Reports.Tax"] = "Tax",
            ["Plugins.Marketplace.Commission.Reports.Date"] = "Date",
            ["Plugins.Marketplace.Commission.Reports.Total"] = "TOTAL",
            ["Plugins.Marketplace.Commission.Reports.NoData"] = "No commission data found for selected period",
            
            // Common Buttons
            ["Plugins.Marketplace.Commission.Button.Save"] = "Save",
            ["Plugins.Marketplace.Commission.Button.Cancel"] = "Cancel",
            ["Plugins.Marketplace.Commission.Button.Delete"] = "Delete",
            ["Plugins.Marketplace.Commission.Button.Edit"] = "Edit",
            ["Plugins.Marketplace.Commission.Button.Settings"] = "Settings",
            
            // Messages
            ["Plugins.Marketplace.Commission.Success.Saved"] = "Commission rate saved successfully",
            ["Plugins.Marketplace.Commission.Success.Deleted"] = "Commission deleted successfully",
            ["Plugins.Marketplace.Commission.Reports.PlatformFee"] = "Platform Fee",
            ["Plugins.Marketplace.Commission.Reports.OrderNumber"] = "Order #",
            ["Plugins.Marketplace.Commission.Reports.ExportSoon"] = "Excel export feature will be implemented soon",
            ["Plugins.Marketplace.Commission.Error.Failed"] = "Failed to save commission rate",
            ["Plugins.Marketplace.Commission.Error.Loading"] = "Error loading data",
            ["Plugins.Marketplace.Commission.Loading"] = "Loading...",
            ["Plugins.Marketplace.Commission.Saving"] = "Saving...",
            
            // Calculation Example
            ["Plugins.Marketplace.Commission.CalculationExample"] = "Commission Calculation Example",
            ["Plugins.Marketplace.Commission.CalculationExample.ProductPrice"] = "Product Price (incl. VAT)",
            ["Plugins.Marketplace.Commission.CalculationExample.Discount"] = "Discount",
            ["Plugins.Marketplace.Commission.CalculationExample.NetPrice"] = "Net Price",
            ["Plugins.Marketplace.Commission.CalculationExample.Commission"] = "Commission",
            ["Plugins.Marketplace.Commission.CalculationExample.TaxWithholding"] = "Tax Withholding",
            ["Plugins.Marketplace.Commission.CalculationExample.MarketplaceFee"] = "Marketplace Fee",
            ["Plugins.Marketplace.Commission.CalculationExample.VendorNet"] = "Vendor Net Amount",
            
            ["Admin.Configuration.Menu.Commission"] = "Commission"
        });

        // Turkish Resources
        var languages = await _languageService.GetAllLanguagesAsync();
        var turkishLanguage = languages.FirstOrDefault(l => l.LanguageCulture == "tr-TR");
        
        if (turkishLanguage != null)
        {
            await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
            {
                // Plugin Bilgisi
                ["Plugins.Marketplace.Commission.FriendlyName"] = "Komisyon Yönetimi",
                ["Plugins.Marketplace.Commission.Description"] = "Kategori ve ürünler için komisyon oranlarını yönetin",
                
                // Menü Öğeleri
                ["Plugins.Marketplace.Commission.Menu.Marketplace"] = "Pazar Yeri",
                ["Plugins.Marketplace.Commission.Menu.Commission"] = "Komisyon",
                ["Plugins.Marketplace.Commission.Menu.Settings"] = "Ayarlar",
                ["Plugins.Marketplace.Commission.Menu.CategoryRates"] = "Kategori Oranları",
                ["Plugins.Marketplace.Commission.Menu.ProductRates"] = "Ürün Oranları",
                ["Plugins.Marketplace.Commission.Menu.Reports"] = "Raporlar",
                
                // Ayarlar Sayfası
                ["Plugins.Marketplace.Commission.Configure"] = "Komisyon Ayarları",
                ["Plugins.Marketplace.Commission.Fields.DefaultCommissionRate"] = "Varsayılan Komisyon Oranı (%)",
                ["Plugins.Marketplace.Commission.Fields.DefaultCommissionRate.Hint"] = "Kategori veya ürün için özel oran belirlenmemişse kullanılacak varsayılan komisyon oranı",
                ["Plugins.Marketplace.Commission.Fields.MarketplaceFeePerOrder"] = "Sipariş Başına Platform Ücreti",
                ["Plugins.Marketplace.Commission.Fields.MarketplaceFeePerOrder.Hint"] = "Her sipariş için alınacak sabit platform ücreti",
                ["Plugins.Marketplace.Commission.Fields.TaxWithholdingRate"] = "Stopaj Oranı (%)",
                ["Plugins.Marketplace.Commission.Fields.TaxWithholdingRate.Hint"] = "Satıcıdan kesilecek stopaj oranı",
                ["Plugins.Marketplace.Commission.Fields.CalculateOnDiscountedPrice"] = "İndirimli Fiyat Üzerinden Hesapla",
                ["Plugins.Marketplace.Commission.Fields.CalculateOnDiscountedPrice.Hint"] = "Komisyonu indirim uygulandıktan sonraki fiyat üzerinden hesapla",
                ["Plugins.Marketplace.Commission.Fields.IncludeShippingInCommission"] = "Kargo Ücretini Komisyona Dahil Et",
                ["Plugins.Marketplace.Commission.Fields.IncludeShippingInCommission.Hint"] = "Kargo ücretini komisyon hesaplamasına dahil et",
                
                // Kategori Komisyonları
                ["Plugins.Marketplace.Commission.CategoryCommissions"] = "Kategori Komisyon Oranları",
                ["Plugins.Marketplace.Commission.CategoryCommissions.Title"] = "Kategori Komisyon Yönetimi",
                ["Plugins.Marketplace.Commission.CategoryCommissions.Info"] = "Her kategori için komisyon oranı belirleyin. Belirlenmemişse varsayılan oran kullanılacaktır.",
                ["Plugins.Marketplace.Commission.CategoryCommissions.List"] = "Kategori Listesi",
                ["Plugins.Marketplace.Commission.CategoryCommissions.Category"] = "Kategori",
                ["Plugins.Marketplace.Commission.CategoryCommissions.Rate"] = "Oran (%)",
                ["Plugins.Marketplace.Commission.CategoryCommissions.Actions"] = "İşlemler",
                ["Plugins.Marketplace.Commission.CategoryCommissions.Edit"] = "Düzenle",
                ["Plugins.Marketplace.Commission.CategoryCommissions.Save"] = "Kaydet",
                ["Plugins.Marketplace.Commission.CategoryCommissions.Success"] = "Komisyon oranı başarıyla kaydedildi",
                ["Plugins.Marketplace.Commission.CategoryCommissions.NoCategories"] = "Kategori bulunamadı",
                
                // Ürün Komisyonları
                ["Plugins.Marketplace.Commission.ProductCommissions"] = "Ürün Komisyon Oranları",
                ["Plugins.Marketplace.Commission.ProductCommissions.Title"] = "Ürün Komisyon Yönetimi",
                ["Plugins.Marketplace.Commission.ProductCommissions.Info"] = "Belirli ürünler için özel komisyon oranları belirleyin. Bu oranlar kategori oranlarını geçersiz kılar ve kampanyalar için son kullanma tarihi içerebilir.",
                ["Plugins.Marketplace.Commission.ProductCommissions.AddNew"] = "Yeni Ürün Komisyonu Ekle",
                ["Plugins.Marketplace.Commission.ProductCommissions.List"] = "Ürün Komisyon Listesi",
                ["Plugins.Marketplace.Commission.ProductCommissions.NoProducts"] = "Ürün komisyonu bulunamadı",
                ["Plugins.Marketplace.Commission.ProductCommissions.ConfirmDelete"] = "Bu ürün komisyonunu silmek istediğinizden emin misiniz?",
                
                ["Plugins.Marketplace.Commission.Fields.ProductId"] = "Ürün ID",
                ["Plugins.Marketplace.Commission.Fields.ProductName"] = "Ürün Adı",
                ["Plugins.Marketplace.Commission.Fields.CategoryName"] = "Kategori",
                ["Plugins.Marketplace.Commission.Fields.CommissionRate"] = "Komisyon Oranı (%)",
                ["Plugins.Marketplace.Commission.Fields.ExpiresOn"] = "Bitiş Tarihi",
                ["Plugins.Marketplace.Commission.Fields.Reason"] = "Sebep",
                ["Plugins.Marketplace.Commission.Fields.CreatedOn"] = "Oluşturulma Tarihi",
                ["Plugins.Marketplace.Commission.Fields.Actions"] = "İşlemler",
                
                // Raporlar
                ["Plugins.Marketplace.Commission.Reports"] = "Komisyon Raporları",
                ["Plugins.Marketplace.Commission.Reports.Title"] = "Komisyon Raporları",
                ["Plugins.Marketplace.Commission.Reports.Summary"] = "Özet",
                ["Plugins.Marketplace.Commission.Reports.Filters"] = "Filtreler",
                ["Plugins.Marketplace.Commission.Reports.StartDate"] = "Başlangıç Tarihi",
                ["Plugins.Marketplace.Commission.Reports.EndDate"] = "Bitiş Tarihi",
                ["Plugins.Marketplace.Commission.Reports.ApplyFilter"] = "Filtre Uygula",
                ["Plugins.Marketplace.Commission.Reports.ExportExcel"] = "Excel'e Aktar",
                ["Plugins.Marketplace.Commission.Reports.TotalOrders"] = "Toplam Sipariş",
                ["Plugins.Marketplace.Commission.Reports.Revenue"] = "Gelir",
                ["Plugins.Marketplace.Commission.Reports.Commission"] = "Komisyon",
                ["Plugins.Marketplace.Commission.Reports.MarketplaceFee"] = "Platform Ücreti",
                ["Plugins.Marketplace.Commission.Reports.TaxWithholding"] = "Stopaj",
                ["Plugins.Marketplace.Commission.Reports.VendorNet"] = "Satıcı Net",
                ["Plugins.Marketplace.Commission.Reports.Details"] = "Komisyon Detayları (Son 100)",
                ["Plugins.Marketplace.Commission.Reports.Order"] = "Sipariş #",
                ["Plugins.Marketplace.Commission.Reports.Vendor"] = "Satıcı",
                ["Plugins.Marketplace.Commission.Reports.Product"] = "Ürün",
                ["Plugins.Marketplace.Commission.Reports.NetPrice"] = "Net Fiyat",
                ["Plugins.Marketplace.Commission.Reports.Rate"] = "Oran",
                ["Plugins.Marketplace.Commission.Reports.Fee"] = "Ücret",
                ["Plugins.Marketplace.Commission.Reports.Tax"] = "Stopaj",
                ["Plugins.Marketplace.Commission.Reports.Date"] = "Tarih",
                ["Plugins.Marketplace.Commission.Reports.Total"] = "TOPLAM",
                ["Plugins.Marketplace.Commission.Reports.NoData"] = "Seçilen dönem için komisyon verisi bulunamadı",
                
                // Ortak Butonlar
                ["Plugins.Marketplace.Commission.Button.Save"] = "Kaydet",
                ["Plugins.Marketplace.Commission.Button.Cancel"] = "İptal",
                ["Plugins.Marketplace.Commission.Button.Delete"] = "Sil",
                ["Plugins.Marketplace.Commission.Button.Edit"] = "Düzenle",
                ["Plugins.Marketplace.Commission.Button.Settings"] = "Ayarlar",
                
                // Mesajlar
                ["Plugins.Marketplace.Commission.Success.Saved"] = "Komisyon oranı başarıyla kaydedildi",
                ["Plugins.Marketplace.Commission.Success.Deleted"] = "Komisyon başarıyla silindi",
                ["Plugins.Marketplace.Commission.Error.Failed"] = "Komisyon oranı kaydedilemedi",
                ["Plugins.Marketplace.Commission.Error.Loading"] = "Veriler yüklenirken hata oluştu",
                ["Plugins.Marketplace.Commission.Loading"] = "Yükleniyor...",
                ["Plugins.Marketplace.Commission.Saving"] = "Kaydediliyor...",
                
                // Hesaplama Örneği
                ["Plugins.Marketplace.Commission.CalculationExample"] = "Komisyon Hesaplama Örneği",
                ["Plugins.Marketplace.Commission.CalculationExample.ProductPrice"] = "Ürün Fiyatı (KDV Dahil)",
                ["Plugins.Marketplace.Commission.CalculationExample.Discount"] = "İndirim",
                ["Plugins.Marketplace.Commission.CalculationExample.NetPrice"] = "Net Fiyat",
                ["Plugins.Marketplace.Commission.CalculationExample.Commission"] = "Komisyon",
                ["Plugins.Marketplace.Commission.CalculationExample.TaxWithholding"] = "Stopaj",
                ["Plugins.Marketplace.Commission.CalculationExample.MarketplaceFee"] = "Platform Ücreti",
                ["Plugins.Marketplace.Commission.CalculationExample.VendorNet"] = "Satıcıya Net Tutar",
                
                ["Admin.Configuration.Menu.Commission"] = "Komisyon"
            }, turkishLanguage.Id);
        }

        await base.InstallAsync();
    }

    public override async Task UninstallAsync()
    {
        await _settingService.DeleteSettingAsync<Domain.CommissionSettings>();
        await _localizationService.DeleteLocaleResourcesAsync("Plugins.Marketplace.Commission");
        await base.UninstallAsync();
    }
}