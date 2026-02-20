using System.Collections.Generic;

namespace Nop.Plugin.Accounting.Parasut;

/// <summary>
/// Localization resources for Parasut plugin
/// </summary>
public static class LocalizationResources
{
    public static Dictionary<string, string> GetEnglishResources()
    {
        return new Dictionary<string, string>
        {
            // API Settings
            ["Plugins.Accounting.Parasut.Fields.Enabled"] = "Enabled",
            ["Plugins.Accounting.Parasut.Fields.CompanyId"] = "Company ID",
            ["Plugins.Accounting.Parasut.Fields.ClientId"] = "Client ID",
            ["Plugins.Accounting.Parasut.Fields.ClientSecret"] = "Client Secret",
            ["Plugins.Accounting.Parasut.Fields.Username"] = "Username",
            ["Plugins.Accounting.Parasut.Fields.Password"] = "Password",
            
            // Invoice Settings
            ["Plugins.Accounting.Parasut.Fields.DefaultVatRate"] = "Default VAT Rate (%)",
            ["Plugins.Accounting.Parasut.Fields.AutoCreateCommissionInvoice"] = "Auto Create Commission Invoice",
            ["Plugins.Accounting.Parasut.Fields.AutoCreateShippingInvoice"] = "Auto Create Shipping Invoice",
            ["Plugins.Accounting.Parasut.Fields.AutoCreatePenaltyInvoice"] = "Auto Create Penalty Invoice",
            
            // Invoice Strategy
            ["Plugins.Accounting.Parasut.Fields.InvoiceStrategy"] = "Invoice Creation Strategy",
            ["Plugins.Accounting.Parasut.Fields.ReturnPeriodDays"] = "Return Period (Days)",
            ["Plugins.Accounting.Parasut.Fields.StartFrom"] = "Return Period Starts From",
            ["Plugins.Accounting.Parasut.Fields.AutoProcessExpiredReturnPeriods"] = "Auto-Process Expired Return Periods",
            
            // Strategy Options
            ["Plugins.Accounting.Parasut.InvoiceStrategy.OnPayment"] = "On Payment (Risky - requires red invoice on return)",
            ["Plugins.Accounting.Parasut.InvoiceStrategy.OnDelivery"] = "On Delivery",
            ["Plugins.Accounting.Parasut.InvoiceStrategy.AfterReturnPeriod"] = "After Return Period ⭐ (Recommended)",
            ["Plugins.Accounting.Parasut.InvoiceStrategy.Manual"] = "Manual Approval",
            
            // Start From Options
            ["Plugins.Accounting.Parasut.StartFrom.OrderDate"] = "Order Date",
            ["Plugins.Accounting.Parasut.StartFrom.PaymentDate"] = "Payment Date",
            ["Plugins.Accounting.Parasut.StartFrom.ShippingDate"] = "Shipping Date",
            ["Plugins.Accounting.Parasut.StartFrom.DeliveryDate"] = "Delivery Date ⭐ (Recommended)",
            
            // Messages
            ["Plugins.Accounting.Parasut.InvoiceCreated"] = "Invoice created successfully",
            ["Plugins.Accounting.Parasut.InvoiceError"] = "Error creating invoice: {0}",
        };
    }
    
    public static Dictionary<string, string> GetTurkishResources()
    {
        return new Dictionary<string, string>
        {
            // API Ayarları
            ["Plugins.Accounting.Parasut.Fields.Enabled"] = "Etkin",
            ["Plugins.Accounting.Parasut.Fields.CompanyId"] = "Firma ID",
            ["Plugins.Accounting.Parasut.Fields.ClientId"] = "Client ID",
            ["Plugins.Accounting.Parasut.Fields.ClientSecret"] = "Client Secret",
            ["Plugins.Accounting.Parasut.Fields.Username"] = "Kullanıcı Adı",
            ["Plugins.Accounting.Parasut.Fields.Password"] = "Şifre",
            
            // Fatura Ayarları
            ["Plugins.Accounting.Parasut.Fields.DefaultVatRate"] = "Varsayılan KDV Oranı (%)",
            ["Plugins.Accounting.Parasut.Fields.AutoCreateCommissionInvoice"] = "Komisyon Faturası Otomatik Oluştur",
            ["Plugins.Accounting.Parasut.Fields.AutoCreateShippingInvoice"] = "Kargo Faturası Otomatik Oluştur",
            ["Plugins.Accounting.Parasut.Fields.AutoCreatePenaltyInvoice"] = "Ceza Faturası Otomatik Oluştur",
            
            // Faturalama Stratejisi
            ["Plugins.Accounting.Parasut.Fields.InvoiceStrategy"] = "Faturalama Stratejisi",
            ["Plugins.Accounting.Parasut.Fields.ReturnPeriodDays"] = "İade Süresi (Gün)",
            ["Plugins.Accounting.Parasut.Fields.StartFrom"] = "İade Süresi Başlangıcı",
            ["Plugins.Accounting.Parasut.Fields.AutoProcessExpiredReturnPeriods"] = "İade Süresi Dolan Siparişleri Otomatik Faturala",
            
            // Strateji Seçenekleri
            ["Plugins.Accounting.Parasut.InvoiceStrategy.OnPayment"] = "Ödeme Alındığında (Riskli - iade durumunda kırmızı fatura gerekir)",
            ["Plugins.Accounting.Parasut.InvoiceStrategy.OnDelivery"] = "Teslimat Anında",
            ["Plugins.Accounting.Parasut.InvoiceStrategy.AfterReturnPeriod"] = "İade Süresi Dolduktan Sonra ⭐ (Önerilen)",
            ["Plugins.Accounting.Parasut.InvoiceStrategy.Manual"] = "Manuel Onay",
            
            // Başlangıç Seçenekleri
            ["Plugins.Accounting.Parasut.StartFrom.OrderDate"] = "Sipariş Tarihinden",
            ["Plugins.Accounting.Parasut.StartFrom.PaymentDate"] = "Ödeme Tarihinden",
            ["Plugins.Accounting.Parasut.StartFrom.ShippingDate"] = "Kargoya Verilme Tarihinden",
            ["Plugins.Accounting.Parasut.StartFrom.DeliveryDate"] = "Teslim Tarihinden ⭐ (Önerilen)",
            
            // Mesajlar
            ["Plugins.Accounting.Parasut.InvoiceCreated"] = "Fatura başarıyla oluşturuldu",
            ["Plugins.Accounting.Parasut.InvoiceError"] = "Fatura oluşturulurken hata: {0}",
        };
    }

    public static Dictionary<string, string> GetMenuResources()
    {
        return new Dictionary<string, string>
        {
            // English
            ["Plugins.Accounting.Parasut.Menu.Finance"] = "Finance",
            ["Plugins.Accounting.Parasut.Menu.MyInvoices"] = "My Invoices",
            ["Plugins.Accounting.Parasut.Menu.AllVendorInvoices"] = "All Vendor Invoices",
        };
    }
    
    public static Dictionary<string, string> GetMenuResourcesTurkish()
    {
        return new Dictionary<string, string>
        {
            // Turkish
            ["Plugins.Accounting.Parasut.Menu.Finance"] = "Finans",
            ["Plugins.Accounting.Parasut.Menu.MyInvoices"] = "Faturalarım",
            ["Plugins.Accounting.Parasut.Menu.AllVendorInvoices"] = "Tüm Satıcı Faturaları",
        };
    }
}
