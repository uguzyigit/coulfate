using Nop.Data;
using Nop.Plugin.Accounting.Parasut.Domain;
using Nop.Plugin.Accounting.Parasut.Models.Parasut;
using Nop.Plugin.Accounting.Parasut.Services.Api;
using Nop.Plugin.Accounting.Parasut.Services.Contact;
using Nop.Plugin.Accounting.Parasut.Services.Product;
using Nop.Services.Configuration;
using Nop.Services.Logging;

namespace Nop.Plugin.Accounting.Parasut.Services.Invoice;

/// <summary>
/// Main invoice orchestration service
/// </summary>
public class ParasutInvoiceService : IParasutInvoiceService
{
    private readonly IRepository<ParasutInvoiceRecord> _invoiceRepository;
    private readonly IParasutContactService _contactService;
    private readonly IParasutProductService _productService;
    private readonly IParasutApiClient _apiClient;
    private readonly ParasutSettings _settings;
    private readonly ILogger _logger;

    public ParasutInvoiceService(
        IRepository<ParasutInvoiceRecord> invoiceRepository,
        IParasutContactService contactService,
        IParasutProductService productService,
        IParasutApiClient apiClient,
        ISettingService settingService,
        ILogger logger)
    {
        _invoiceRepository = invoiceRepository;
        _contactService = contactService;
        _productService = productService;
        _apiClient = apiClient;
        _settings = settingService.LoadSettingAsync<ParasutSettings>().Result;
        _logger = logger;
    }

    public async Task<ParasutInvoiceRecord> CreateVendorInvoiceAsync(
        int vendorId,
        int orderId,
        decimal commissionAmount,
        decimal shippingAmount,
        decimal penaltyAmount)
    {
        try
        {
            // Check for existing invoice
            var existing = await _invoiceRepository.Table
                .FirstOrDefaultAsync(i => i.OrderId == orderId && i.VendorId == vendorId);

            if (existing != null)
            {
                await _logger.WarningAsync($"Invoice already exists for vendor {vendorId}, order {orderId}");
                return existing;
            }

            // 1. Ensure vendor contact exists
            var contactId = await _contactService.EnsureContactExistsAsync(vendorId);

            // 2. Ensure products exist
            var products = await _productService.EnsureProductsExistAsync();

            // 3. Build invoice lines
            var lines = new List<InvoiceLine>();

            if (commissionAmount > 0 && _settings.AutoCreateCommissionInvoice)
            {
                lines.Add(new InvoiceLine
                {
                    ProductId = products.CommissionProductId,
                    Quantity = 1,
                    UnitPrice = commissionAmount,
                    VatRate = _settings.DefaultVatRate,
                    Description = $"Commission for Order #{orderId}"
                });
            }

            if (shippingAmount > 0 && _settings.AutoCreateShippingInvoice)
            {
                lines.Add(new InvoiceLine
                {
                    ProductId = products.ShippingProductId,
                    Quantity = 1,
                    UnitPrice = shippingAmount,
                    VatRate = _settings.DefaultVatRate,
                    Description = $"Shipping for Order #{orderId}"
                });
            }

            if (penaltyAmount > 0 && _settings.AutoCreatePenaltyInvoice)
            {
                lines.Add(new InvoiceLine
                {
                    ProductId = products.PenaltyProductId,
                    Quantity = 1,
                    UnitPrice = penaltyAmount,
                    VatRate = _settings.DefaultVatRate,
                    Description = $"Penalty for Order #{orderId}"
                });
            }

            if (lines.Count == 0)
            {
                await _logger.WarningAsync($"No invoice lines for vendor {vendorId}, order {orderId}");
                return null;
            }

            // 4. Create sales invoice
            var description = _settings.InvoiceDescriptionTemplate.Replace("{OrderId}", orderId.ToString());
            var issueDate = DateTime.Now;
            var dueDate = issueDate.AddDays(_settings.PaymentTermDays);

            var salesInvoiceId = await _apiClient.CreateSalesInvoiceAsync(
                contactId, lines, description, issueDate, dueDate);

            // 5. Calculate totals
            var totalAmount = lines.Sum(l => l.UnitPrice);
            var vatAmount = lines.Sum(l => l.UnitPrice * l.VatRate / 100);
            var grossAmount = totalAmount + vatAmount;

            // 6. Create record
            var record = new ParasutInvoiceRecord
            {
                VendorId = vendorId,
                OrderId = orderId,
                InvoiceType = (int)InvoiceType.Combined,
                ParasutSalesInvoiceId = salesInvoiceId,
                TotalAmount = totalAmount,
                VatAmount = vatAmount,
                GrossAmount = grossAmount,
                Status = (int)InvoiceStatus.Created,
                StatusMessage = "Invoice created, waiting for e-document",
                CreatedOnUtc = DateTime.UtcNow
            };

            await _invoiceRepository.InsertAsync(record);

            await _logger.InformationAsync($"Created invoice {salesInvoiceId} for vendor {vendorId}, order {orderId}");

            // 7. Start e-document process (async)
            _ = Task.Run(async () =>
            {
                try
                {
                    await StartEDocumentProcessAsync(record.Id, contactId, salesInvoiceId);
                }
                catch (Exception ex)
                {
                    await _logger.ErrorAsync($"E-document process failed for record {record.Id}", ex);
                }
            });

            return record;
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync($"Failed to create invoice for vendor {vendorId}, order {orderId}", ex);
            throw;
        }
    }

    public async Task ProcessPendingEDocumentsAsync()
    {
        var pendingRecords = await _invoiceRepository.Table
            .Where(r => r.Status == (int)InvoiceStatus.Formalizing && 
                       !string.IsNullOrEmpty(r.TrackableJobId))
            .ToListAsync();

        foreach (var record in pendingRecords)
        {
            try
            {
                await ProcessEDocumentAsync(record);
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"Failed to process e-document for record {record.Id}", ex);
            }
        }
    }

    private async Task StartEDocumentProcessAsync(int recordId, string contactId, string salesInvoiceId)
    {
        var record = await _invoiceRepository.GetByIdAsync(recordId);
        if (record == null) return;

        try
        {
            // Get contact to check VKN
            var contact = await _apiClient.GetContactAsync(contactId);
            var taxNumber = contact.Attributes.TryGetValue("tax_number", out var tn) ? tn?.ToString() : null;

            // Check e-invoice inbox
            var hasEInvoiceInbox = false;
            if (!string.IsNullOrEmpty(taxNumber) && taxNumber.Length == 10)
            {
                hasEInvoiceInbox = await _apiClient.CheckEInvoiceInboxAsync(taxNumber);
            }

            // Create appropriate e-document
            string jobId;
            string eDocType;

            if (hasEInvoiceInbox)
            {
                jobId = await _apiClient.CreateEInvoiceAsync(salesInvoiceId, taxNumber);
                eDocType = "e_invoice";
            }
            else
            {
                jobId = await _apiClient.CreateEArchiveAsync(salesInvoiceId, _settings.MarketplaceUrl);
                eDocType = "e_archive";
            }

            record.TrackableJobId = jobId;
            record.EDocumentType = eDocType;
            record.Status = (int)InvoiceStatus.Formalizing;
            record.StatusMessage = $"E-document creation started ({eDocType})";
            record.UpdatedOnUtc = DateTime.UtcNow;
            await _invoiceRepository.UpdateAsync(record);

            await _logger.InformationAsync($"Started {eDocType} for record {recordId}, job {jobId}");
        }
        catch (Exception ex)
        {
            record.Status = (int)InvoiceStatus.Failed;
            record.ErrorMessage = $"E-document start failed: {ex.Message}";
            record.UpdatedOnUtc = DateTime.UtcNow;
            await _invoiceRepository.UpdateAsync(record);
            throw;
        }
    }

    private async Task ProcessEDocumentAsync(ParasutInvoiceRecord record)
    {
        // Poll job status
        var status = await _apiClient.GetTrackableJobStatusAsync(record.TrackableJobId);

        if (status == "done")
        {
            // Get e-document ID
            var invoice = await _apiClient.GetSalesInvoiceAsync(
                record.ParasutSalesInvoiceId, 
                "active_e_document");

            // Extract e-document ID from included data
            string eDocId = null;
            if (invoice.Relationships != null &&
                invoice.Relationships.TryGetValue("active_e_document", out var edocRel))
            {
                var edocData = edocRel as Dictionary<string, object>;
                if (edocData != null && edocData.TryGetValue("data", out var data))
                {
                    var dataDict = data as Dictionary<string, object>;
                    eDocId = dataDict?["id"]?.ToString();
                }
            }

            if (string.IsNullOrEmpty(eDocId))
            {
                record.ErrorMessage = "Could not extract e-document ID";
                record.Status = (int)InvoiceStatus.Failed;
                await _invoiceRepository.UpdateAsync(record);
                return;
            }

            record.EDocumentId = eDocId;
            record.StatusMessage = "E-document ready, downloading PDF";
            await _invoiceRepository.UpdateAsync(record);

            // Download PDF (with polling)
            await DownloadPdfWithRetryAsync(record);
        }
        else if (status == "error")
        {
            record.Status = (int)InvoiceStatus.Failed;
            record.ErrorMessage = "E-document creation failed";
            record.UpdatedOnUtc = DateTime.UtcNow;
            await _invoiceRepository.UpdateAsync(record);
        }
        // else still pending/running - will check next time
    }

    private async Task DownloadPdfWithRetryAsync(ParasutInvoiceRecord record)
    {
        const int maxAttempts = 20;
        byte[] pdfData = null;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            if (attempt > 0)
                await Task.Delay(TimeSpan.FromSeconds(5));

            pdfData = await _apiClient.DownloadPdfAsync(
                record.EDocumentType, 
                record.EDocumentId);

            if (pdfData != null)
                break;
        }

        if (pdfData == null)
        {
            record.ErrorMessage = "PDF download timeout";
            record.UpdatedOnUtc = DateTime.UtcNow;
            await _invoiceRepository.UpdateAsync(record);
            return;
        }

        // Save PDF
        var fileName = $"invoice_{record.Id}_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
        var directory = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            _settings.PdfStoragePath,
            record.VendorId.ToString());

        Directory.CreateDirectory(directory);
        var filePath = Path.Combine(directory, fileName);

        await File.WriteAllBytesAsync(filePath, pdfData);

        record.PdfLocalPath = filePath;
        record.Status = (int)InvoiceStatus.PdfReady;
        record.StatusMessage = "PDF ready";
        record.PdfDownloadedOnUtc = DateTime.UtcNow;
        record.UpdatedOnUtc = DateTime.UtcNow;
        await _invoiceRepository.UpdateAsync(record);

        await _logger.InformationAsync($"PDF downloaded for record {record.Id}");
    }
}
