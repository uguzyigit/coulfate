using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Nop.Core.Domain.Common;
using Nop.Data;
using Nop.Plugin.Accounting.Parasut.Domain;
using Nop.Plugin.Accounting.Parasut.Models.Parasut;
using Nop.Plugin.Accounting.Parasut.Services.Auth;
using Nop.Services.Configuration;
using Nop.Services.Logging;

namespace Nop.Plugin.Accounting.Parasut.Services.Api;

/// <summary>
/// Paraşüt API client implementation
/// </summary>
public class ParasutApiClient : IParasutApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IParasutAuthService _authService;
    private readonly ISettingService _settingService;
    private readonly ILogger _logger;
    private readonly IRepository<ParasutApiLog> _logRepository;
    private readonly ParasutRateLimiter _rateLimiter;

    public ParasutApiClient(
        IHttpClientFactory httpClientFactory,
        IParasutAuthService authService,
        ISettingService settingService,
        ILogger logger,
        IRepository<ParasutApiLog> logRepository,
        ParasutRateLimiter rateLimiter)
    {
        _httpClient = httpClientFactory.CreateClient("ParasutApi");
        _authService = authService;
        _settingService = settingService;
        _logger = logger;
        _logRepository = logRepository;
        _rateLimiter = rateLimiter;
    }

    #region Contact Operations

    public async Task<string> CreateContactAsync(ParasutContactRequest request)
    {
        var settings = await _settingService.LoadSettingAsync<ParasutSettings>();
        var endpoint = $"https://api.parasut.com/v4/{settings.CompanyId}/contacts";

        var response = await ExecuteRequestAsync<ParasutResponse<ParasutResourceData>>(
            "POST", endpoint, request, "CreateContact");

        return response?.Data?.Id;
    }

    public async Task<ParasutResourceData> GetContactAsync(string contactId)
    {
        var settings = await _settingService.LoadSettingAsync<ParasutSettings>();
        var endpoint = $"https://api.parasut.com/v4/{settings.CompanyId}/contacts/{contactId}";

        var response = await ExecuteRequestAsync<ParasutResponse<ParasutResourceData>>(
            "GET", endpoint, null, "GetContact");

        return response?.Data;
    }

    public async Task<List<ParasutResourceData>> SearchContactsAsync(
        string searchTerm = null, 
        string email = null, 
        string taxNumber = null)
    {
        var settings = await _settingService.LoadSettingAsync<ParasutSettings>();
        var queryParams = new List<string>();

        if (!string.IsNullOrEmpty(searchTerm))
            queryParams.Add($"filter[name]={Uri.EscapeDataString(searchTerm)}");
        if (!string.IsNullOrEmpty(email))
            queryParams.Add($"filter[email]={Uri.EscapeDataString(email)}");
        if (!string.IsNullOrEmpty(taxNumber))
            queryParams.Add($"filter[tax_number]={Uri.EscapeDataString(taxNumber)}");

        var query = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
        var endpoint = $"https://api.parasut.com/v4/{settings.CompanyId}/contacts{query}";

        var response = await ExecuteRequestAsync<ParasutResponse<List<ParasutResourceData>>>(
            "GET", endpoint, null, "SearchContacts");

        return response?.Data ?? new List<ParasutResourceData>();
    }

    #endregion

    #region Product Operations

    public async Task<string> CreateProductAsync(ParasutProductRequest request)
    {
        var settings = await _settingService.LoadSettingAsync<ParasutSettings>();
        var endpoint = $"https://api.parasut.com/v4/{settings.CompanyId}/products";

        var response = await ExecuteRequestAsync<ParasutResponse<ParasutResourceData>>(
            "POST", endpoint, request, "CreateProduct");

        return response?.Data?.Id;
    }

    public async Task<ParasutResourceData> GetProductAsync(string productId)
    {
        var settings = await _settingService.LoadSettingAsync<ParasutSettings>();
        var endpoint = $"https://api.parasut.com/v4/{settings.CompanyId}/products/{productId}";

        var response = await ExecuteRequestAsync<ParasutResponse<ParasutResourceData>>(
            "GET", endpoint, null, "GetProduct");

        return response?.Data;
    }

    public async Task<List<ParasutResourceData>> SearchProductsAsync(string name = null, string code = null)
    {
        var settings = await _settingService.LoadSettingAsync<ParasutSettings>();
        var queryParams = new List<string>();

        if (!string.IsNullOrEmpty(name))
            queryParams.Add($"filter[name]={Uri.EscapeDataString(name)}");
        if (!string.IsNullOrEmpty(code))
            queryParams.Add($"filter[code]={Uri.EscapeDataString(code)}");

        var query = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
        var endpoint = $"https://api.parasut.com/v4/{settings.CompanyId}/products{query}";

        var response = await ExecuteRequestAsync<ParasutResponse<List<ParasutResourceData>>>(
            "GET", endpoint, null, "SearchProducts");

        return response?.Data ?? new List<ParasutResourceData>();
    }

    #endregion

    #region Sales Invoice Operations

    public async Task<string> CreateSalesInvoiceAsync(
        string contactId,
        List<InvoiceLine> lines,
        string description,
        DateTime issueDate,
        DateTime dueDate)
    {
        var settings = await _settingService.LoadSettingAsync<ParasutSettings>();
        var endpoint = $"https://api.parasut.com/v4/{settings.CompanyId}/sales_invoices";

        // Build JSON:API payload
        var payload = new
        {
            data = new
            {
                type = "sales_invoices",
                attributes = new
                {
                    item_type = "invoice",
                    description = description,
                    issue_date = issueDate.ToString("yyyy-MM-dd"),
                    due_date = dueDate.ToString("yyyy-MM-dd"),
                    currency = "TRL",
                    exchange_rate = 1
                },
                relationships = new
                {
                    contact = new
                    {
                        data = new
                        {
                            type = "contacts",
                            id = contactId
                        }
                    },
                    details = new
                    {
                        data = lines.Select(line => new
                        {
                            type = "sales_invoice_details",
                            attributes = new
                            {
                                quantity = line.Quantity,
                                unit_price = line.UnitPrice,
                                vat_rate = line.VatRate,
                                description = line.Description
                            },
                            relationships = new
                            {
                                product = new
                                {
                                    data = new
                                    {
                                        type = "products",
                                        id = line.ProductId
                                    }
                                }
                            }
                        }).ToList()
                    }
                }
            }
        };

        var response = await ExecuteRequestAsync<ParasutResponse<ParasutResourceData>>(
            "POST", endpoint, payload, "CreateSalesInvoice");

        return response?.Data?.Id;
    }

    public async Task<ParasutResourceData> GetSalesInvoiceAsync(string invoiceId, string include = null)
    {
        var settings = await _settingService.LoadSettingAsync<ParasutSettings>();
        var query = !string.IsNullOrEmpty(include) ? $"?include={include}" : "";
        var endpoint = $"https://api.parasut.com/v4/{settings.CompanyId}/sales_invoices/{invoiceId}{query}";

        var response = await ExecuteRequestAsync<ParasutResponse<ParasutResourceData>>(
            "GET", endpoint, null, "GetSalesInvoice");

        return response?.Data;
    }

    #endregion

    #region E-Document Operations

    public async Task<bool> CheckEInvoiceInboxAsync(string taxNumber)
    {
        var settings = await _settingService.LoadSettingAsync<ParasutSettings>();
        var endpoint = $"https://api.parasut.com/v4/{settings.CompanyId}/e_invoice_inboxes?filter[vkn]={taxNumber}";

        try
        {
            var response = await ExecuteRequestAsync<ParasutResponse<List<ParasutResourceData>>>(
                "GET", endpoint, null, "CheckEInvoiceInbox");

            return response?.Data?.Any() ?? false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> CreateEInvoiceAsync(string invoiceId, string vkn)
    {
        var settings = await _settingService.LoadSettingAsync<ParasutSettings>();
        var endpoint = $"https://api.parasut.com/v4/{settings.CompanyId}/e_invoices";

        var payload = new
        {
            data = new
            {
                type = "e_invoices",
                attributes = new
                {
                    vkn = vkn,
                    invoice_id = invoiceId,
                    scenario = "basic"
                }
            }
        };

        var response = await ExecuteRequestAsync<dynamic>(
            "POST", endpoint, payload, "CreateEInvoice");

        // Extract trackable job ID from response
        var jobId = response?.data?.relationships?.trackable_job?.data?.id?.ToString();
        return jobId;
    }

    public async Task<string> CreateEArchiveAsync(string invoiceId, string url = null)
    {
        var settings = await _settingService.LoadSettingAsync<ParasutSettings>();
        var endpoint = $"https://api.parasut.com/v4/{settings.CompanyId}/e_archives";

        var payload = new
        {
            data = new
            {
                type = "e_archives",
                attributes = new
                {
                    invoice_id = invoiceId,
                    internet_sale = settings.UseInternetSaleInfo ? new
                    {
                        url = url ?? settings.MarketplaceUrl,
                        payment_type = "KREDIKARTI/BANKAKARTI",
                        payment_platform = settings.PaymentPlatform,
                        payment_date = DateTime.Now.ToString("yyyy-MM-dd")
                    } : null
                }
            }
        };

        var response = await ExecuteRequestAsync<dynamic>(
            "POST", endpoint, payload, "CreateEArchive");

        // Extract trackable job ID from response
        var jobId = response?.data?.relationships?.trackable_job?.data?.id?.ToString();
        return jobId;
    }

    #endregion

    #region Trackable Job Operations

    public async Task<string> GetTrackableJobStatusAsync(string jobId)
    {
        var settings = await _settingService.LoadSettingAsync<ParasutSettings>();
        var endpoint = $"https://api.parasut.com/v4/{settings.CompanyId}/trackable_jobs/{jobId}";

        var response = await ExecuteRequestAsync<ParasutResponse<ParasutResourceData>>(
            "GET", endpoint, null, "GetTrackableJobStatus");

        if (response?.Data?.Attributes != null && 
            response.Data.Attributes.TryGetValue("status", out var status))
        {
            return status?.ToString();
        }

        return null;
    }

    #endregion

    #region PDF Operations

    public async Task<byte[]> DownloadPdfAsync(string eDocumentType, string eDocumentId)
    {
        var settings = await _settingService.LoadSettingAsync<ParasutSettings>();
        var endpoint = $"https://api.parasut.com/v4/{settings.CompanyId}/{eDocumentType}s/{eDocumentId}/pdf";

        // This might return 204 (No Content) if PDF not ready yet
        var response = await ExecuteRequestAsync<dynamic>(
            "GET", endpoint, null, $"DownloadPdf_{eDocumentType}");

        if (response == null)
            return null; // 204 - not ready

        // Extract PDF URL
        var pdfUrl = response?.url?.ToString();
        if (string.IsNullOrEmpty(pdfUrl))
            return null;

        // Download PDF from temporary URL
        try
        {
            var pdfData = await _httpClient.GetByteArrayAsync(pdfUrl);
            return pdfData;
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync($"Failed to download PDF from {pdfUrl}: {ex.Message}");
            return null;
        }
    }

    #endregion

    #region Test Connection

    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            var token = await _authService.GetValidAccessTokenAsync();
            
            var settings = await _settingService.LoadSettingAsync<ParasutSettings>();
            var endpoint = "https://api.parasut.com/v4/me";

            var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            await _rateLimiter.WaitIfNeededAsync();
            var response = await _httpClient.SendAsync(request);

            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    #endregion

    #region Helper Methods

    private async Task<T> ExecuteRequestAsync<T>(
        string method,
        string endpoint,
        object payload,
        string requestType)
    {
        var stopwatch = Stopwatch.StartNew();
        var log = new ParasutApiLog
        {
            RequestType = requestType,
            Endpoint = endpoint,
            Method = method,
            CreatedOnUtc = DateTime.UtcNow
        };

        try
        {
            // Get valid access token
            var token = await _authService.GetValidAccessTokenAsync();

            // Create request
            var request = new HttpRequestMessage(new HttpMethod(method), endpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (payload != null)
            {
                var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions 
                { 
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                });
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                log.RequestPayload = json;
            }

            // Rate limiting
            await _rateLimiter.WaitIfNeededAsync();

            // Execute request
            var response = await _httpClient.SendAsync(request);
            stopwatch.Stop();

            log.StatusCode = (int)response.StatusCode;
            log.DurationMs = (int)stopwatch.ElapsedMilliseconds;

            var responseContent = await response.Content.ReadAsStringAsync();
            log.ResponsePayload = responseContent;

            // Handle 204 No Content (PDF not ready)
            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                log.IsSuccess = true;
                await _logRepository.InsertAsync(log);
                return default(T);
            }

            if (!response.IsSuccessStatusCode)
            {
                log.IsSuccess = false;
                log.ErrorMessage = $"HTTP {response.StatusCode}: {responseContent}";
                await _logRepository.InsertAsync(log);

                throw new Exception($"Paraşüt API error: {response.StatusCode} - {responseContent}");
            }

            var result = JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            log.IsSuccess = true;
            await _logRepository.InsertAsync(log);

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            log.IsSuccess = false;
            log.ErrorMessage = ex.Message;
            log.DurationMs = (int)stopwatch.ElapsedMilliseconds;
            await _logRepository.InsertAsync(log);

            await _logger.ErrorAsync($"Paraşüt API request failed: {requestType} - {ex.Message}", ex);
            throw;
        }
    }

    #endregion
}
