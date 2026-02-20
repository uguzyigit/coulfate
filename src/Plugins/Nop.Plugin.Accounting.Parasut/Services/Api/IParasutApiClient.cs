using Nop.Plugin.Accounting.Parasut.Models.Parasut;

namespace Nop.Plugin.Accounting.Parasut.Services.Api;

/// <summary>
/// Paraşüt API client interface
/// </summary>
public interface IParasutApiClient
{
    // Contact operations
    Task<string> CreateContactAsync(ParasutContactRequest request);
    Task<ParasutResourceData> GetContactAsync(string contactId);
    Task<List<ParasutResourceData>> SearchContactsAsync(string searchTerm = null, string email = null, string taxNumber = null);

    // Product operations
    Task<string> CreateProductAsync(ParasutProductRequest request);
    Task<ParasutResourceData> GetProductAsync(string productId);
    Task<List<ParasutResourceData>> SearchProductsAsync(string name = null, string code = null);

    // Sales invoice operations
    Task<string> CreateSalesInvoiceAsync(string contactId, List<InvoiceLine> lines, string description, DateTime issueDate, DateTime dueDate);
    Task<ParasutResourceData> GetSalesInvoiceAsync(string invoiceId, string include = null);

    // E-document operations
    Task<bool> CheckEInvoiceInboxAsync(string taxNumber);
    Task<string> CreateEInvoiceAsync(string invoiceId, string vkn);
    Task<string> CreateEArchiveAsync(string invoiceId, string url = null);

    // Trackable job operations
    Task<string> GetTrackableJobStatusAsync(string jobId);

    // PDF operations
    Task<byte[]> DownloadPdfAsync(string eDocumentType, string eDocumentId);

    // Test connection
    Task<bool> TestConnectionAsync();
}
