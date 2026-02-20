using Nop.Plugin.Accounting.Parasut.Domain;

namespace Nop.Plugin.Accounting.Parasut.Services.Invoice;

public interface IParasutInvoiceService
{
    Task<ParasutInvoiceRecord> CreateVendorInvoiceAsync(int vendorId, int orderId, decimal commissionAmount, decimal shippingAmount, decimal penaltyAmount);
    Task ProcessPendingEDocumentsAsync();
}
