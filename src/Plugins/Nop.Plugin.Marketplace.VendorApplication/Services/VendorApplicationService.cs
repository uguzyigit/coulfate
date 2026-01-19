using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Vendors;
using Nop.Data;
using Nop.Plugin.Marketplace.Core.Domain;
using Nop.Plugin.Marketplace.Core.Services;
using Nop.Plugin.Marketplace.VendorApplication.Domain;
using Nop.Services.Media;
using NopVendorService = Nop.Services.Vendors.IVendorService;

namespace Nop.Plugin.Marketplace.VendorApplication.Services;

/// <summary>
/// Vendor application service implementation
/// </summary>
public class VendorApplicationService : IVendorApplicationService
{
    #region Fields

    private readonly IRepository<Domain.VendorApplication> _applicationRepository;
    private readonly IRepository<VendorDocumentType> _documentTypeRepository;
    private readonly IRepository<VendorApplicationDocument> _documentRepository;
    private readonly IStaticCacheManager _staticCacheManager;
    private readonly NopVendorService _nopVendorService;
    private readonly IVendorExtensionService _vendorExtensionService;
    private readonly IDownloadService _downloadService;
    private readonly VendorApplicationSettings _settings;

    #endregion

    #region Ctor

    public VendorApplicationService(
        IRepository<Domain.VendorApplication> applicationRepository,
        IRepository<VendorDocumentType> documentTypeRepository,
        IRepository<VendorApplicationDocument> documentRepository,
        IStaticCacheManager staticCacheManager,
        NopVendorService nopVendorService,
        IVendorExtensionService vendorExtensionService,
        IDownloadService downloadService,
        VendorApplicationSettings settings)
    {
        _applicationRepository = applicationRepository;
        _documentTypeRepository = documentTypeRepository;
        _documentRepository = documentRepository;
        _staticCacheManager = staticCacheManager;
        _nopVendorService = nopVendorService;
        _vendorExtensionService = vendorExtensionService;
        _downloadService = downloadService;
        _settings = settings;
    }

    #endregion

    #region Applications

    public virtual async Task<Domain.VendorApplication> GetApplicationByIdAsync(int id)
    {
        return await _applicationRepository.GetByIdAsync(id, cache => default);
    }

    public virtual async Task<Domain.VendorApplication> GetApplicationByNumberAsync(string applicationNumber)
    {
        if (string.IsNullOrWhiteSpace(applicationNumber))
            return null;

        var query = from a in _applicationRepository.Table
                    where a.ApplicationNumber == applicationNumber
                    select a;

        return await query.FirstOrDefaultAsync();
    }

    public virtual async Task<Domain.VendorApplication> GetApplicationByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        var query = from a in _applicationRepository.Table
                    where a.Email == email
                    orderby a.CreatedOnUtc descending
                    select a;

        return await query.FirstOrDefaultAsync();
    }

    public virtual async Task<IPagedList<Domain.VendorApplication>> GetAllApplicationsAsync(
        VendorApplicationStatus? status = null,
        string searchTerm = null,
        DateTime? createdFrom = null,
        DateTime? createdTo = null,
        int pageIndex = 0,
        int pageSize = int.MaxValue)
    {
        var query = _applicationRepository.Table;

        if (status.HasValue)
            query = query.Where(a => a.StatusId == (int)status.Value);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(a =>
                a.CompanyName.Contains(searchTerm) ||
                a.ContactPerson.Contains(searchTerm) ||
                a.Email.Contains(searchTerm) ||
                a.ApplicationNumber.Contains(searchTerm));
        }

        if (createdFrom.HasValue)
            query = query.Where(a => a.CreatedOnUtc >= createdFrom.Value);

        if (createdTo.HasValue)
            query = query.Where(a => a.CreatedOnUtc <= createdTo.Value);

        query = query.OrderByDescending(a => a.CreatedOnUtc);

        return await query.ToPagedListAsync(pageIndex, pageSize);
    }

    public virtual async Task InsertApplicationAsync(Domain.VendorApplication application)
    {
        ArgumentNullException.ThrowIfNull(application);

        application.CreatedOnUtc = DateTime.UtcNow;
        application.UpdatedOnUtc = DateTime.UtcNow;

        if (string.IsNullOrEmpty(application.ApplicationNumber))
            application.ApplicationNumber = await GenerateApplicationNumberAsync();

        if (application.StatusId == 0)
            application.StatusId = (int)VendorApplicationStatus.Pending;

        await _applicationRepository.InsertAsync(application);
    }

    public virtual async Task UpdateApplicationAsync(Domain.VendorApplication application)
    {
        ArgumentNullException.ThrowIfNull(application);

        application.UpdatedOnUtc = DateTime.UtcNow;

        await _applicationRepository.UpdateAsync(application);
    }

    public virtual async Task DeleteApplicationAsync(Domain.VendorApplication application)
    {
        ArgumentNullException.ThrowIfNull(application);

        // Delete associated documents first
        await DeleteDocumentsByApplicationIdAsync(application.Id);

        await _applicationRepository.DeleteAsync(application);
    }

    public virtual async Task<(bool Success, int? VendorId, string ErrorMessage)> ApproveApplicationAsync(int applicationId, int reviewedByCustomerId)
    {
        var application = await GetApplicationByIdAsync(applicationId);
        if (application == null)
            return (false, null, "Application not found");

        if (application.StatusId == (int)VendorApplicationStatus.Approved)
            return (false, application.CreatedVendorId, "Application is already approved");

        try
        {
            // 1. Create NopCommerce native Vendor
            var vendor = new Vendor
            {
                Name = application.CompanyName,
                Email = application.Email ?? string.Empty,
                Description = application.Description ?? string.Empty,
                Active = true,
                Deleted = false,
                DisplayOrder = 0,
                PageSize = 6,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "6,3,9",
                PriceRangeFiltering = false,
                PriceFrom = 0,
                PriceTo = 10000,
                ManuallyPriceRange = false,
                AdminComment = $"Created from vendor application #{application.ApplicationNumber}"
            };

            await _nopVendorService.InsertVendorAsync(vendor);

            // 2. Create VendorExtension with marketplace-specific fields
            var vendorCode = await _vendorExtensionService.GenerateVendorCodeAsync(application.CompanyName);
            var vendorExtension = new VendorExtension
            {
                VendorId = vendor.Id,
                Code = vendorCode,
                MarketplaceStatus = MarketplaceVendorStatus.Active,
                Phone = application.Phone ?? string.Empty,
                BankAccountName = application.BankAccountHolder ?? string.Empty,
                BankIban = application.BankIban ?? string.Empty,
                TaxNumber = application.TaxNumber ?? string.Empty,
                TradeRegistryNumber = application.TradeRegistryNumber ?? string.Empty,
                RequiresProductApproval = true,
                MinimumPayoutAmount = 100
            };

            await _vendorExtensionService.InsertAsync(vendorExtension);

            // 3. Update application status
            application.StatusId = (int)VendorApplicationStatus.Approved;
            application.CreatedVendorId = vendor.Id;
            application.ReviewedOnUtc = DateTime.UtcNow;
            application.ReviewedByCustomerId = reviewedByCustomerId;

            await UpdateApplicationAsync(application);

            return (true, vendor.Id, null);
        }
        catch (Exception ex)
        {
            return (false, null, ex.Message);
        }
    }

    public virtual async Task<bool> RejectApplicationAsync(int applicationId, string reason, int reviewedByCustomerId)
    {
        var application = await GetApplicationByIdAsync(applicationId);
        if (application == null)
            return false;

        application.StatusId = (int)VendorApplicationStatus.Rejected;
        application.RejectionReason = reason;
        application.ReviewedOnUtc = DateTime.UtcNow;
        application.ReviewedByCustomerId = reviewedByCustomerId;

        await UpdateApplicationAsync(application);

        return true;
    }

    public virtual async Task ChangeStatusAsync(int applicationId, VendorApplicationStatus newStatus)
    {
        var application = await GetApplicationByIdAsync(applicationId);
        if (application == null)
            return;

        application.StatusId = (int)newStatus;
        await UpdateApplicationAsync(application);
    }

    public virtual async Task<string> GenerateApplicationNumberAsync()
    {
        var date = DateTime.UtcNow;
        var prefix = $"VA{date:yyyyMMdd}";

        // Get count of applications today
        var todayStart = date.Date;
        var todayEnd = todayStart.AddDays(1);

        var count = await _applicationRepository.Table
            .CountAsync(a => a.CreatedOnUtc >= todayStart && a.CreatedOnUtc < todayEnd);

        return $"{prefix}-{(count + 1):D4}";
    }

    #endregion

    #region Document Types

    public virtual async Task<VendorDocumentType> GetDocumentTypeByIdAsync(int id)
    {
        return await _documentTypeRepository.GetByIdAsync(id, cache => default);
    }

    public virtual async Task<IList<VendorDocumentType>> GetAllDocumentTypesAsync(bool activeOnly = true)
    {
        var query = _documentTypeRepository.Table;

        if (activeOnly)
            query = query.Where(dt => dt.IsActive);

        query = query.OrderBy(dt => dt.DisplayOrder).ThenBy(dt => dt.Name);

        return await query.ToListAsync();
    }

    public virtual async Task InsertDocumentTypeAsync(VendorDocumentType documentType)
    {
        ArgumentNullException.ThrowIfNull(documentType);

        documentType.CreatedOnUtc = DateTime.UtcNow;

        await _documentTypeRepository.InsertAsync(documentType);
    }

    public virtual async Task UpdateDocumentTypeAsync(VendorDocumentType documentType)
    {
        ArgumentNullException.ThrowIfNull(documentType);

        documentType.UpdatedOnUtc = DateTime.UtcNow;

        await _documentTypeRepository.UpdateAsync(documentType);
    }

    public virtual async Task DeleteDocumentTypeAsync(VendorDocumentType documentType)
    {
        ArgumentNullException.ThrowIfNull(documentType);

        await _documentTypeRepository.DeleteAsync(documentType);
    }

    #endregion

    #region Application Documents

    public virtual async Task<VendorApplicationDocument> GetDocumentByIdAsync(int id)
    {
        return await _documentRepository.GetByIdAsync(id, cache => default);
    }

    public virtual async Task<IList<VendorApplicationDocument>> GetDocumentsByApplicationIdAsync(int applicationId)
    {
        var query = from d in _documentRepository.Table
                    where d.VendorApplicationId == applicationId
                    orderby d.CreatedOnUtc
                    select d;

        return await query.ToListAsync();
    }

    public virtual async Task InsertDocumentAsync(VendorApplicationDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        document.CreatedOnUtc = DateTime.UtcNow;

        await _documentRepository.InsertAsync(document);
    }

    public virtual async Task DeleteDocumentAsync(VendorApplicationDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        // Delete the actual file from Download table
        if (document.DownloadId > 0)
        {
            var download = await _downloadService.GetDownloadByIdAsync(document.DownloadId);
            if (download != null)
                await _downloadService.DeleteDownloadAsync(download);
        }

        await _documentRepository.DeleteAsync(document);
    }

    public virtual async Task DeleteDocumentsByApplicationIdAsync(int applicationId)
    {
        var documents = await GetDocumentsByApplicationIdAsync(applicationId);

        foreach (var document in documents)
        {
            await DeleteDocumentAsync(document);
        }
    }

    #endregion
}
