using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using Nop.Plugin.Marketplace.VendorExtensions.Components;
using Nop.Plugin.Marketplace.VendorExtensions.Data;
using Nop.Core.Domain.Cms;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Marketplace.VendorExtensions;

public class VendorExtensionsPlugin : BasePlugin, IWidgetPlugin
{
    private readonly IWebHelper _webHelper;
    private readonly ISettingService _settingService;
    private readonly ILocalizationService _localizationService;
    private readonly ILanguageService _languageService;
    private readonly INopDataProvider _dataProvider;
    private readonly WidgetSettings _widgetSettings;

    public VendorExtensionsPlugin(
        IWebHelper webHelper,
        ISettingService settingService,
        ILocalizationService localizationService,
        ILanguageService languageService,
        INopDataProvider dataProvider,
        WidgetSettings widgetSettings)
    {
        _webHelper = webHelper;
        _settingService = settingService;
        _localizationService = localizationService;
        _languageService = languageService;
        _dataProvider = dataProvider;
        _widgetSettings = widgetSettings;
    }

    /// <summary>
    /// Gets a value indicating whether to hide this plugin on the widget list page in the admin area
    /// </summary>
    public bool HideInWidgetList => true;

    /// <summary>
    /// Gets widget zones where this widget should be rendered
    /// </summary>
    public Task<IList<string>> GetWidgetZonesAsync()
    {
        return Task.FromResult<IList<string>>(new List<string> { AdminWidgetZones.CategoryDetailsBlock });
    }

    /// <summary>
    /// Gets a type of a view component for displaying widget
    /// </summary>
    public Type GetWidgetViewComponent(string widgetZone)
    {
        return typeof(CategorySpecificationMappingViewComponent);
    }

    public override string GetConfigurationPageUrl()
    {
        return $"{_webHelper.GetStoreLocation()}Admin/VendorExtensions/Configure";
    }

    public override async Task InstallAsync()
    {
        await _dataProvider.ExecuteNonQueryAsync(InstallationData.CreateTablesScript);
        
        // English Resources
        await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
        {
            // Plugin Info
            ["Plugins.Marketplace.VendorExtensions.FriendlyName"] = "Vendor Account Management",
            ["Plugins.Marketplace.VendorExtensions.Description"] = "Manage vendor accounts, transactions, and current account balances",
            
            // Menu Items
            ["Plugins.Marketplace.VendorExtensions.Menu.VendorAccount"] = "Vendor Account",
            ["Plugins.Marketplace.VendorExtensions.Menu.Settings"] = "Settings",
            ["Plugins.Marketplace.VendorExtensions.Menu.Transactions"] = "Transaction History",
            
            // Configure Page
            ["Plugins.Marketplace.VendorExtensions.Configure"] = "Vendor Account Settings",
            ["Plugins.Marketplace.VendorExtensions.Configure.Title"] = "Vendor Account Configuration",
            
            // Transaction History
            ["Plugins.Marketplace.VendorExtensions.TransactionHistory"] = "Vendor Transaction History",
            ["Plugins.Marketplace.VendorExtensions.TransactionHistory.Title"] = "Transaction History",
            ["Plugins.Marketplace.VendorExtensions.TransactionHistory.Filters"] = "Filters",
            ["Plugins.Marketplace.VendorExtensions.TransactionHistory.Vendor"] = "Vendor",
            ["Plugins.Marketplace.VendorExtensions.TransactionHistory.SelectVendor"] = "-- Select Vendor --",
            ["Plugins.Marketplace.VendorExtensions.TransactionHistory.StartDate"] = "Start Date",
            ["Plugins.Marketplace.VendorExtensions.TransactionHistory.EndDate"] = "End Date",
            ["Plugins.Marketplace.VendorExtensions.TransactionHistory.TransactionType"] = "Transaction Type",
            ["Plugins.Marketplace.VendorExtensions.TransactionHistory.AllTypes"] = "-- All Types --",
            ["Plugins.Marketplace.VendorExtensions.TransactionHistory.Load"] = "Load",
            
            // Account Summary
            ["Plugins.Marketplace.VendorExtensions.AccountSummary"] = "Account Summary",
            ["Plugins.Marketplace.VendorExtensions.AccountSummary.CurrentBalance"] = "Current Balance",
            ["Plugins.Marketplace.VendorExtensions.AccountSummary.TotalCredit"] = "Total Credit (All Time)",
            ["Plugins.Marketplace.VendorExtensions.AccountSummary.TotalDebit"] = "Total Debit (All Time)",
            ["Plugins.Marketplace.VendorExtensions.AccountSummary.LastUpdated"] = "Last Updated",
            
            // Transaction List
            ["Plugins.Marketplace.VendorExtensions.Transactions"] = "Transactions (Last 100)",
            ["Plugins.Marketplace.VendorExtensions.Transactions.Date"] = "Date",
            ["Plugins.Marketplace.VendorExtensions.Transactions.Type"] = "Type",
            ["Plugins.Marketplace.VendorExtensions.Transactions.Description"] = "Description",
            ["Plugins.Marketplace.VendorExtensions.Transactions.Reference"] = "Reference",
            ["Plugins.Marketplace.VendorExtensions.Transactions.Order"] = "Order",
            ["Plugins.Marketplace.VendorExtensions.Transactions.Amount"] = "Amount",
            ["Plugins.Marketplace.VendorExtensions.Transactions.BalanceAfter"] = "Balance After",
            
            // Transaction Types
            ["Plugins.Marketplace.VendorExtensions.TransactionType.OrderRevenue"] = "Order Revenue",
            ["Plugins.Marketplace.VendorExtensions.TransactionType.Commission"] = "Commission",
            ["Plugins.Marketplace.VendorExtensions.TransactionType.MarketplaceFee"] = "Marketplace Fee",
            ["Plugins.Marketplace.VendorExtensions.TransactionType.TaxWithholding"] = "Tax Withholding",
            ["Plugins.Marketplace.VendorExtensions.TransactionType.ShippingCost"] = "Shipping Cost",
            ["Plugins.Marketplace.VendorExtensions.TransactionType.Refund"] = "Refund",
            ["Plugins.Marketplace.VendorExtensions.TransactionType.Payout"] = "Payout",
            ["Plugins.Marketplace.VendorExtensions.TransactionType.ManualAdjustment"] = "Manual Adjustment",
            ["Plugins.Marketplace.VendorExtensions.TransactionType.Penalty"] = "Penalty",
            ["Plugins.Marketplace.VendorExtensions.TransactionType.Bonus"] = "Bonus",
            ["Plugins.Marketplace.VendorExtensions.TransactionType.Chargeback"] = "Chargeback",
            ["Plugins.Marketplace.VendorExtensions.TransactionType.DebtCollection"] = "Debt Collection",
            ["Plugins.Marketplace.VendorExtensions.TransactionType.Interest"] = "Interest",
            ["Plugins.Marketplace.VendorExtensions.TransactionType.PaymentReceived"] = "Payment Received",

            ["Plugins.Marketplace.VendorExtensions.Transactions.NoData"] = "Please select a vendor and click Load",
            ["Plugins.Marketplace.VendorExtensions.Transactions.NotFound"] = "No transactions found for selected period",
            
            // Transaction Types
            ["Plugins.Marketplace.VendorExtensions.TransactionType.OrderRevenue"] = "Order Revenue",
            ["Plugins.Marketplace.VendorExtensions.TransactionType.Commission"] = "Commission",
            ["Plugins.Marketplace.VendorExtensions.TransactionType.MarketplaceFee"] = "Marketplace Fee",
            ["Plugins.Marketplace.VendorExtensions.TransactionType.TaxWithholding"] = "Tax Withholding",
            ["Plugins.Marketplace.VendorExtensions.TransactionType.ShippingCost"] = "Shipping Cost",
            ["Plugins.Marketplace.VendorExtensions.TransactionType.Refund"] = "Refund",
            ["Plugins.Marketplace.VendorExtensions.TransactionType.Payout"] = "Payout",
            ["Plugins.Marketplace.VendorExtensions.TransactionType.ManualAdjustment"] = "Manual Adjustment",
            ["Plugins.Marketplace.VendorExtensions.TransactionType.Penalty"] = "Penalty",
            ["Plugins.Marketplace.VendorExtensions.TransactionType.Bonus"] = "Bonus",
            ["Plugins.Marketplace.VendorExtensions.TransactionType.Chargeback"] = "Chargeback",
            ["Plugins.Marketplace.VendorExtensions.TransactionType.DebtCollection"] = "Debt Collection",
            ["Plugins.Marketplace.VendorExtensions.TransactionType.Interest"] = "Interest",
            
            // Messages
            ["Plugins.Marketplace.VendorExtensions.Success.Saved"] = "Settings saved successfully",
            ["Plugins.Marketplace.VendorExtensions.Error.SelectVendor"] = "Please select a vendor",
            ["Plugins.Marketplace.VendorExtensions.Error.Loading"] = "Error loading transactions",
            ["Plugins.Marketplace.VendorExtensions.Loading"] = "Loading...",
            ["Plugins.Marketplace.VendorExtensions.Installed"] = "Database tables created successfully!",
            
            // Common
            ["Plugins.Marketplace.VendorExtensions.Button.Save"] = "Save",
            ["Plugins.Marketplace.VendorExtensions.Button.Cancel"] = "Cancel",

            // Vendor Finance Menu 
["Plugins.Marketplace.VendorExtensions.Finance"] = "Finance",
["Plugins.Marketplace.VendorExtensions.MyCommissionReport"] = "My Commission Report",
["Plugins.Marketplace.VendorExtensions.MyCommissionReport.Title"] = "My Commission Report",
["Plugins.Marketplace.VendorExtensions.MyCommissionReport.Details"] = "My Commission Details (Last 100)",
["Plugins.Marketplace.VendorExtensions.MyTransactions"] = "My Transactions",
["Plugins.Marketplace.VendorExtensions.MyTransactions.Title"] = "My Transaction History",
        });

        // Turkish Resources
        var languages = await _languageService.GetAllLanguagesAsync();
        var turkishLanguage = languages.FirstOrDefault(l => l.LanguageCulture == "tr-TR");
        
        if (turkishLanguage != null)
        {
            await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
            {
                // Plugin Bilgisi
                ["Plugins.Marketplace.VendorExtensions.FriendlyName"] = "Satıcı Hesap Yönetimi",
                ["Plugins.Marketplace.VendorExtensions.Description"] = "Satıcı hesaplarını, işlemlerini ve cari hesap bakiyelerini yönetin",
                
                // Menü Öğeleri
                ["Plugins.Marketplace.VendorExtensions.Menu.VendorAccount"] = "Satıcı Hesabı",
                ["Plugins.Marketplace.VendorExtensions.Menu.Settings"] = "Ayarlar",
                ["Plugins.Marketplace.VendorExtensions.Menu.Transactions"] = "İşlem Geçmişi",
                
                // Ayarlar Sayfası
                ["Plugins.Marketplace.VendorExtensions.Configure"] = "Satıcı Hesap Ayarları",
                ["Plugins.Marketplace.VendorExtensions.Configure.Title"] = "Satıcı Hesap Yapılandırması",
                
                // İşlem Geçmişi
                ["Plugins.Marketplace.VendorExtensions.TransactionHistory"] = "Satıcı İşlem Geçmişi",
                ["Plugins.Marketplace.VendorExtensions.TransactionHistory.Title"] = "İşlem Geçmişi",
                ["Plugins.Marketplace.VendorExtensions.TransactionHistory.Filters"] = "Filtreler",
                ["Plugins.Marketplace.VendorExtensions.TransactionHistory.Vendor"] = "Satıcı",
                ["Plugins.Marketplace.VendorExtensions.TransactionHistory.SelectVendor"] = "-- Satıcı Seçin --",
                ["Plugins.Marketplace.VendorExtensions.TransactionHistory.StartDate"] = "Başlangıç Tarihi",
                ["Plugins.Marketplace.VendorExtensions.TransactionHistory.EndDate"] = "Bitiş Tarihi",
                ["Plugins.Marketplace.VendorExtensions.TransactionHistory.TransactionType"] = "İşlem Tipi",
                ["Plugins.Marketplace.VendorExtensions.TransactionHistory.AllTypes"] = "-- Tüm Tipler --",
                ["Plugins.Marketplace.VendorExtensions.TransactionHistory.Load"] = "Yükle",
                
                // Hesap Özeti
                ["Plugins.Marketplace.VendorExtensions.AccountSummary"] = "Hesap Özeti",
                ["Plugins.Marketplace.VendorExtensions.AccountSummary.CurrentBalance"] = "Güncel Bakiye",
                ["Plugins.Marketplace.VendorExtensions.AccountSummary.TotalCredit"] = "Toplam Alacak (Tüm Zamanlar)",
                ["Plugins.Marketplace.VendorExtensions.AccountSummary.TotalDebit"] = "Toplam Borç (Tüm Zamanlar)",
                ["Plugins.Marketplace.VendorExtensions.AccountSummary.LastUpdated"] = "Son Güncelleme",
                
                // İşlem Listesi
                ["Plugins.Marketplace.VendorExtensions.Transactions"] = "İşlemler (Son 100)",
                ["Plugins.Marketplace.VendorExtensions.Transactions.Date"] = "Tarih",
                ["Plugins.Marketplace.VendorExtensions.Transactions.Type"] = "Tip",
                ["Plugins.Marketplace.VendorExtensions.Transactions.Description"] = "Açıklama",
                ["Plugins.Marketplace.VendorExtensions.Transactions.Reference"] = "Referans",
                ["Plugins.Marketplace.VendorExtensions.Transactions.Order"] = "Sipariş",
                ["Plugins.Marketplace.VendorExtensions.Transactions.Amount"] = "Tutar",
                ["Plugins.Marketplace.VendorExtensions.Transactions.BalanceAfter"] = "İşlem Sonrası Bakiye",
            
            // Transaction Types
            ["Plugins.Marketplace.VendorExtensions.TransactionType.OrderRevenue"] = "Order Revenue",
            ["Plugins.Marketplace.VendorExtensions.TransactionType.Commission"] = "Commission",
            ["Plugins.Marketplace.VendorExtensions.TransactionType.MarketplaceFee"] = "Marketplace Fee",
            ["Plugins.Marketplace.VendorExtensions.TransactionType.TaxWithholding"] = "Tax Withholding",
            ["Plugins.Marketplace.VendorExtensions.TransactionType.ShippingCost"] = "Shipping Cost",
            ["Plugins.Marketplace.VendorExtensions.TransactionType.Refund"] = "Refund",
            ["Plugins.Marketplace.VendorExtensions.TransactionType.Payout"] = "Payout",
            ["Plugins.Marketplace.VendorExtensions.TransactionType.ManualAdjustment"] = "Manual Adjustment",
            ["Plugins.Marketplace.VendorExtensions.TransactionType.Penalty"] = "Penalty",
            ["Plugins.Marketplace.VendorExtensions.TransactionType.Bonus"] = "Bonus",
            ["Plugins.Marketplace.VendorExtensions.TransactionType.Chargeback"] = "Chargeback",
            ["Plugins.Marketplace.VendorExtensions.TransactionType.DebtCollection"] = "Debt Collection",
            ["Plugins.Marketplace.VendorExtensions.TransactionType.Interest"] = "Interest",
            ["Plugins.Marketplace.VendorExtensions.TransactionType.PaymentReceived"] = "Payment Received",

                ["Plugins.Marketplace.VendorExtensions.Transactions.NoData"] = "Lütfen bir satıcı seçin ve Yükle'ye tıklayın",
                ["Plugins.Marketplace.VendorExtensions.Transactions.NotFound"] = "Seçilen dönem için işlem bulunamadı",
                
                // İşlem Tipleri
                ["Plugins.Marketplace.VendorExtensions.TransactionType.OrderRevenue"] = "Sipariş Geliri",
                ["Plugins.Marketplace.VendorExtensions.TransactionType.Commission"] = "Komisyon",
                ["Plugins.Marketplace.VendorExtensions.TransactionType.MarketplaceFee"] = "Platform Ücreti",
                ["Plugins.Marketplace.VendorExtensions.TransactionType.TaxWithholding"] = "Stopaj",
                ["Plugins.Marketplace.VendorExtensions.TransactionType.ShippingCost"] = "Kargo Ücreti",
                ["Plugins.Marketplace.VendorExtensions.TransactionType.Refund"] = "İade",
                ["Plugins.Marketplace.VendorExtensions.TransactionType.Payout"] = "Ödeme",
                ["Plugins.Marketplace.VendorExtensions.TransactionType.ManualAdjustment"] = "Manuel Düzeltme",
                ["Plugins.Marketplace.VendorExtensions.TransactionType.Penalty"] = "Ceza",
                ["Plugins.Marketplace.VendorExtensions.TransactionType.Bonus"] = "Bonus",
                ["Plugins.Marketplace.VendorExtensions.TransactionType.Chargeback"] = "Ters İbraz",
                ["Plugins.Marketplace.VendorExtensions.TransactionType.DebtCollection"] = "Borç Tahsilatı",
                ["Plugins.Marketplace.VendorExtensions.TransactionType.Interest"] = "Faiz",
                
                // Mesajlar
                ["Plugins.Marketplace.VendorExtensions.Success.Saved"] = "Ayarlar başarıyla kaydedildi",
                ["Plugins.Marketplace.VendorExtensions.Error.SelectVendor"] = "Lütfen bir satıcı seçin",
                ["Plugins.Marketplace.VendorExtensions.Error.Loading"] = "İşlemler yüklenirken hata oluştu",
                ["Plugins.Marketplace.VendorExtensions.Loading"] = "Yükleniyor...",
                ["Plugins.Marketplace.VendorExtensions.Installed"] = "Veritabanı tabloları başarıyla oluşturuldu!",
                
                // Ortak
                ["Plugins.Marketplace.VendorExtensions.Button.Save"] = "Kaydet",
                ["Plugins.Marketplace.VendorExtensions.Button.Cancel"] = "İptal",

                // Vendor Finance Menu 
["Plugins.Marketplace.VendorExtensions.Finance"] = "Finans",
["Plugins.Marketplace.VendorExtensions.MyCommissionReport"] = "Komisyon Raporlarım",
["Plugins.Marketplace.VendorExtensions.MyCommissionReport.Title"] = "Komisyon Raporlarım",
["Plugins.Marketplace.VendorExtensions.MyCommissionReport.Details"] = "Komisyon Detaylarım (Son 100)",
["Plugins.Marketplace.VendorExtensions.MyTransactions"] = "İşlemlerim",
["Plugins.Marketplace.VendorExtensions.MyTransactions.Title"] = "İşlem Geçmişim",
            }, turkishLanguage.Id);
        }

        // Activate widget
        if (!_widgetSettings.ActiveWidgetSystemNames.Contains(PluginDescriptor.SystemName))
        {
            _widgetSettings.ActiveWidgetSystemNames.Add(PluginDescriptor.SystemName);
            await _settingService.SaveSettingAsync(_widgetSettings);
        }

        await base.InstallAsync();
    }

    public override async Task UninstallAsync()
    {
        // Deactivate widget
        if (_widgetSettings.ActiveWidgetSystemNames.Contains(PluginDescriptor.SystemName))
        {
            _widgetSettings.ActiveWidgetSystemNames.Remove(PluginDescriptor.SystemName);
            await _settingService.SaveSettingAsync(_widgetSettings);
        }

        await _localizationService.DeleteLocaleResourcesAsync("Plugins.Marketplace.VendorExtensions");
        await base.UninstallAsync();
    }
}