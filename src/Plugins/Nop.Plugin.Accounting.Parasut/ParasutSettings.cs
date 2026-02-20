using Nop.Core.Configuration;
using Nop.Plugin.Accounting.Parasut.Domain;

namespace Nop.Plugin.Accounting.Parasut;

public class ParasutSettings : ISettings
{
    // ========== API Settings ==========
    public bool Enabled { get; set; }
    public string CompanyId { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public bool UsePasswordGrant { get; set; } = true;
    public string Username { get; set; }
    public string Password { get; set; }
    
    // ========== OAuth Token Cache ==========
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public DateTime? TokenExpiresAtUtc { get; set; }
    public string RedirectUri { get; set; }

    // ========== Invoice Settings ==========
    public decimal DefaultVatRate { get; set; } = 20m;
    public int PaymentTermDays { get; set; } = 15;
    
    // ========== Invoice Creation Strategy (YENİ) ==========
    
    /// <summary>
    /// Fatura kesme stratejisi
    /// Default: AfterReturnPeriod (En güvenli - İade süresi dolduktan sonra)
    /// </summary>
    public InvoiceCreationStrategy InvoiceStrategy { get; set; } = InvoiceCreationStrategy.AfterReturnPeriod;
    
    /// <summary>
    /// İade süresi (gün)
    /// Default: 14 gün (Tüketici Kanunu standart)
    /// </summary>
    public int ReturnPeriodDays { get; set; } = 14;
    
    /// <summary>
    /// İade süresi başlangıç tarihi
    /// Default: DeliveryDate (Teslim tarihinden itibaren)
    /// </summary>
    public ReturnPeriodStartFrom StartFrom { get; set; } = ReturnPeriodStartFrom.DeliveryDate;
    
    /// <summary>
    /// İade süresi dolan siparişleri otomatik faturala (Scheduled task ile)
    /// Default: true
    /// </summary>
    public bool AutoProcessExpiredReturnPeriods { get; set; } = true;
    
    // ========== Auto-Create Flags ==========
    public bool AutoCreateCommissionInvoice { get; set; } = true;
    public bool AutoCreateShippingInvoice { get; set; } = false;
    public bool AutoCreatePenaltyInvoice { get; set; } = true;

    // ========== Advanced Settings ==========
    public string PdfStoragePath { get; set; } = "vendorinvoices";
    public bool UseInternetSaleInfo { get; set; } = true;
    public bool SendVendorNotifications { get; set; } = true;
    public int MaxRetryAttempts { get; set; } = 3;
    public int RetryDelayMs { get; set; } = 1000;
    public string InvoiceDescriptionTemplate { get; set; } = "Marketplace Invoice - Order #{OrderId}";
    public string MarketplaceUrl { get; set; }
    public string PaymentPlatform { get; set; }
}
