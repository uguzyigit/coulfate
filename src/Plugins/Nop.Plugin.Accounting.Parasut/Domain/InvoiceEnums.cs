namespace Nop.Plugin.Accounting.Parasut.Domain;

/// <summary>
/// Fatura kesme stratejisi
/// </summary>
public enum InvoiceCreationStrategy
{
    /// <summary>
    /// Ödeme alındığında hemen (Riskli - iade durumunda sorun olur)
    /// </summary>
    OnPayment = 1,
    
    /// <summary>
    /// Ürün teslim edildiğinde
    /// </summary>
    OnDelivery = 2,
    
    /// <summary>
    /// İade süresi dolduktan sonra (ÖNERİLEN - En güvenli yöntem)
    /// </summary>
    AfterReturnPeriod = 3,
    
    /// <summary>
    /// Manuel onay ile
    /// </summary>
    Manual = 4
}

/// <summary>
/// İade süresi başlangıç tarihi
/// </summary>
public enum ReturnPeriodStartFrom
{
    /// <summary>
    /// Sipariş tarihinden itibaren
    /// </summary>
    OrderDate = 1,
    
    /// <summary>
    /// Ödeme tarihinden itibaren
    /// </summary>
    PaymentDate = 2,
    
    /// <summary>
    /// Kargoya verilme tarihinden itibaren
    /// </summary>
    ShippingDate = 3,
    
    /// <summary>
    /// Teslim tarihinden itibaren (ÖNERİLEN - Tüketici kanununa uygun)
    /// </summary>
    DeliveryDate = 4
}
