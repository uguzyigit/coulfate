using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Plugin.Marketplace.VendorApplication.Domain;
using Nop.Plugin.Marketplace.VendorApplication.Models.Admin;
using Nop.Plugin.Marketplace.VendorApplication.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Marketplace.VendorApplication.Controllers;

[Area(AreaNames.ADMIN)]
[AuthorizeAdmin]
[AutoValidateAntiforgeryToken]
public class VendorApplicationAdminController : BasePluginController
{
    private readonly IVendorApplicationService _applicationService;
    private readonly IVendorApplicationMessageService _messageService;
    private readonly VendorApplicationSettings _settings;
    private readonly ISettingService _settingService;
    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly IPermissionService _permissionService;
    private readonly IDownloadService _downloadService;
    private readonly IWorkContext _workContext;

    public VendorApplicationAdminController(
        IVendorApplicationService applicationService,
        IVendorApplicationMessageService messageService,
        VendorApplicationSettings settings,
        ISettingService settingService,
        ILocalizationService localizationService,
        INotificationService notificationService,
        IPermissionService permissionService,
        IDownloadService downloadService,
        IWorkContext workContext)
    {
        _applicationService = applicationService;
        _messageService = messageService;
        _settings = settings;
        _settingService = settingService;
        _localizationService = localizationService;
        _notificationService = notificationService;
        _permissionService = permissionService;
        _downloadService = downloadService;
        _workContext = workContext;
    }

    #region Configuration

    public async Task<IActionResult> Configure()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        var model = new ConfigurationModel
        {
            Enabled = _settings.Enabled,
            DisplayCaptcha = _settings.DisplayCaptcha,
            RequireTermsOfService = _settings.RequireTermsOfService,
            NotifyAdminOnNewApplication = _settings.NotifyAdminOnNewApplication,
            NotifyApplicantOnReceived = _settings.NotifyApplicantOnReceived,
            NotifyApplicantOnApproved = _settings.NotifyApplicantOnApproved,
            NotifyApplicantOnRejected = _settings.NotifyApplicantOnRejected,
            AdminNotificationEmail = _settings.AdminNotificationEmail
        };

        return View("~/Plugins/Marketplace.VendorApplication/Views/VendorApplicationAdmin/Configure.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        _settings.Enabled = model.Enabled;
        _settings.DisplayCaptcha = model.DisplayCaptcha;
        _settings.RequireTermsOfService = model.RequireTermsOfService;
        _settings.NotifyAdminOnNewApplication = model.NotifyAdminOnNewApplication;
        _settings.NotifyApplicantOnReceived = model.NotifyApplicantOnReceived;
        _settings.NotifyApplicantOnApproved = model.NotifyApplicantOnApproved;
        _settings.NotifyApplicantOnRejected = model.NotifyApplicantOnRejected;
        _settings.AdminNotificationEmail = model.AdminNotificationEmail;

        await _settingService.SaveSettingAsync(_settings);

        _notificationService.SuccessNotification(
            await _localizationService.GetResourceAsync("Plugins.Marketplace.VendorApplication.SettingsSaved"));

        return RedirectToAction("Configure");
    }

    #endregion

    #region Application List

    public async Task<IActionResult> List(int? statusId = null, string searchTerm = null, int page = 1)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        var pageSize = 20;
        var status = statusId.HasValue ? (VendorApplicationStatus?)statusId.Value : null;

        var applications = await _applicationService.GetAllApplicationsAsync(
            status: status,
            searchTerm: searchTerm,
            pageIndex: page - 1,
            pageSize: pageSize);

        var model = new ApplicationListModel
        {
            SearchStatusId = statusId,
            SearchTerm = searchTerm,
            PageIndex = page,
            PageSize = pageSize,
            TotalCount = applications.TotalCount,
            AvailableStatuses = await GetAvailableStatusesAsync(),
            Applications = applications.Select(a => new ApplicationItemModel
            {
                Id = a.Id,
                ApplicationNumber = a.ApplicationNumber,
                CompanyName = a.CompanyName,
                ContactPerson = a.ContactPerson,
                Email = a.Email,
                Status = (VendorApplicationStatus)a.StatusId,
                StatusName = GetStatusNameSync((VendorApplicationStatus)a.StatusId),
                CreatedOnUtc = a.CreatedOnUtc
            }).ToList()
        };

        return View("~/Plugins/Marketplace.VendorApplication/Views/VendorApplicationAdmin/List.cshtml", model);
    }

    #endregion

    #region Application Detail

    public async Task<IActionResult> Detail(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        var application = await _applicationService.GetApplicationByIdAsync(id);
        if (application == null)
            return RedirectToAction("List");

        var documents = await _applicationService.GetDocumentsByApplicationIdAsync(id);
        var documentTypes = await _applicationService.GetAllDocumentTypesAsync(activeOnly: false);

        var model = new ApplicationDetailModel
        {
            Id = application.Id,
            ApplicationNumber = application.ApplicationNumber,
            Status = (VendorApplicationStatus)application.StatusId,
            StatusName = GetStatusNameSync((VendorApplicationStatus)application.StatusId),
            CreatedOnUtc = application.CreatedOnUtc,
            ReviewedOnUtc = application.ReviewedOnUtc,
            IpAddress = application.IpAddress,
            CompanyName = application.CompanyName,
            ContactPerson = application.ContactPerson,
            Email = application.Email,
            Phone = application.Phone,
            TaxNumber = application.TaxNumber,
            TradeRegistryNumber = application.TradeRegistryNumber,
            Address = application.Address,
            City = application.City,
            District = application.District,
            PostalCode = application.PostalCode,
            BankAccountHolder = application.BankAccountHolder,
            BankIban = application.BankIban,
            BankName = application.BankName,
            Description = application.Description,
            AdminNotes = application.AdminNotes,
            RejectionReason = application.RejectionReason,
            CreatedVendorId = application.CreatedVendorId,
            Documents = documents.Select(d => new ApplicationDocumentModel
            {
                Id = d.Id,
                DocumentTypeId = d.DocumentTypeId,
                DocumentTypeName = documentTypes.FirstOrDefault(dt => dt.Id == d.DocumentTypeId)?.Name ?? "Unknown",
                DownloadId = d.DownloadId,
                FileName = d.FileName,
                ContentType = d.ContentType,
                FileSize = d.FileSize,
                FileSizeFormatted = FormatFileSize(d.FileSize),
                CreatedOnUtc = d.CreatedOnUtc
            }).ToList()
        };

        return View("~/Plugins/Marketplace.VendorApplication/Views/VendorApplicationAdmin/Detail.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> SaveNotes(int id, string adminNotes)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        var application = await _applicationService.GetApplicationByIdAsync(id);
        if (application == null)
            return RedirectToAction("List");

        application.AdminNotes = adminNotes;
        await _applicationService.UpdateApplicationAsync(application);

        _notificationService.SuccessNotification(
            await _localizationService.GetResourceAsync("Plugins.Marketplace.VendorApplication.NotesSaved"));

        return RedirectToAction("Detail", new { id });
    }

    [HttpPost]
    public async Task<IActionResult> Approve(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        var customer = await _workContext.GetCurrentCustomerAsync();
        var result = await _applicationService.ApproveApplicationAsync(id, customer.Id);

        if (result.Success)
        {
            // Send notification
            var application = await _applicationService.GetApplicationByIdAsync(id);
            var languageId = (await _workContext.GetWorkingLanguageAsync()).Id;
            await _messageService.SendApplicationApprovedNotificationAsync(application, languageId);

            _notificationService.SuccessNotification(
                await _localizationService.GetResourceAsync("Plugins.Marketplace.VendorApplication.ApplicationApproved"));
        }
        else
        {
            _notificationService.ErrorNotification(result.ErrorMessage);
        }

        return RedirectToAction("Detail", new { id });
    }

    [HttpPost]
    public async Task<IActionResult> Reject(int id, string rejectionReason)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        var customer = await _workContext.GetCurrentCustomerAsync();
        var success = await _applicationService.RejectApplicationAsync(id, rejectionReason, customer.Id);

        if (success)
        {
            // Send notification
            var application = await _applicationService.GetApplicationByIdAsync(id);
            var languageId = (await _workContext.GetWorkingLanguageAsync()).Id;
            await _messageService.SendApplicationRejectedNotificationAsync(application, languageId);

            _notificationService.SuccessNotification(
                await _localizationService.GetResourceAsync("Plugins.Marketplace.VendorApplication.ApplicationRejected"));
        }

        return RedirectToAction("Detail", new { id });
    }

    public async Task<IActionResult> DownloadDocument(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        var document = await _applicationService.GetDocumentByIdAsync(id);
        if (document == null)
            return NotFound();

        var download = await _downloadService.GetDownloadByIdAsync(document.DownloadId);
        if (download == null)
            return NotFound();

        return File(download.DownloadBinary, download.ContentType, document.FileName);
    }

    #endregion

    #region Document Types

    public async Task<IActionResult> DocumentTypes()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        var documentTypes = await _applicationService.GetAllDocumentTypesAsync(activeOnly: false);

        var model = new DocumentTypeListModel
        {
            DocumentTypes = documentTypes.Select(dt => new DocumentTypeModel
            {
                Id = dt.Id,
                Name = dt.Name,
                Description = dt.Description,
                IsRequired = dt.IsRequired,
                DisplayOrder = dt.DisplayOrder,
                IsActive = dt.IsActive,
                AllowedExtensions = dt.AllowedExtensions,
                MaxFileSizeKb = dt.MaxFileSizeKb
            }).ToList()
        };

        return View("~/Plugins/Marketplace.VendorApplication/Views/VendorApplicationAdmin/DocumentTypes.cshtml", model);
    }

    public async Task<IActionResult> DocumentTypeCreate()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        var model = new DocumentTypeModel
        {
            IsActive = true,
            IsRequired = false,
            MaxFileSizeKb = 5120,
            AllowedExtensions = ".pdf,.jpg,.jpeg,.png"
        };

        return View("~/Plugins/Marketplace.VendorApplication/Views/VendorApplicationAdmin/DocumentTypeEdit.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> DocumentTypeCreate(DocumentTypeModel model)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        if (!ModelState.IsValid)
            return View("~/Plugins/Marketplace.VendorApplication/Views/VendorApplicationAdmin/DocumentTypeEdit.cshtml", model);

        var documentType = new VendorDocumentType
        {
            Name = model.Name,
            Description = model.Description,
            IsRequired = model.IsRequired,
            DisplayOrder = model.DisplayOrder,
            IsActive = model.IsActive,
            AllowedExtensions = model.AllowedExtensions,
            MaxFileSizeKb = model.MaxFileSizeKb
        };

        await _applicationService.InsertDocumentTypeAsync(documentType);

        _notificationService.SuccessNotification(
            await _localizationService.GetResourceAsync("Plugins.Marketplace.VendorApplication.DocumentTypeSaved"));

        return RedirectToAction("DocumentTypes");
    }

    public async Task<IActionResult> DocumentTypeEdit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        var documentType = await _applicationService.GetDocumentTypeByIdAsync(id);
        if (documentType == null)
            return RedirectToAction("DocumentTypes");

        var model = new DocumentTypeModel
        {
            Id = documentType.Id,
            Name = documentType.Name,
            Description = documentType.Description,
            IsRequired = documentType.IsRequired,
            DisplayOrder = documentType.DisplayOrder,
            IsActive = documentType.IsActive,
            AllowedExtensions = documentType.AllowedExtensions,
            MaxFileSizeKb = documentType.MaxFileSizeKb
        };

        return View("~/Plugins/Marketplace.VendorApplication/Views/VendorApplicationAdmin/DocumentTypeEdit.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> DocumentTypeEdit(DocumentTypeModel model)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        if (!ModelState.IsValid)
            return View("~/Plugins/Marketplace.VendorApplication/Views/VendorApplicationAdmin/DocumentTypeEdit.cshtml", model);

        var documentType = await _applicationService.GetDocumentTypeByIdAsync(model.Id);
        if (documentType == null)
            return RedirectToAction("DocumentTypes");

        documentType.Name = model.Name;
        documentType.Description = model.Description;
        documentType.IsRequired = model.IsRequired;
        documentType.DisplayOrder = model.DisplayOrder;
        documentType.IsActive = model.IsActive;
        documentType.AllowedExtensions = model.AllowedExtensions;
        documentType.MaxFileSizeKb = model.MaxFileSizeKb;

        await _applicationService.UpdateDocumentTypeAsync(documentType);

        _notificationService.SuccessNotification(
            await _localizationService.GetResourceAsync("Plugins.Marketplace.VendorApplication.DocumentTypeSaved"));

        return RedirectToAction("DocumentTypes");
    }

    [HttpPost]
    public async Task<IActionResult> DocumentTypeDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        var documentType = await _applicationService.GetDocumentTypeByIdAsync(id);
        if (documentType != null)
        {
            await _applicationService.DeleteDocumentTypeAsync(documentType);

            _notificationService.SuccessNotification(
                await _localizationService.GetResourceAsync("Plugins.Marketplace.VendorApplication.DocumentTypeDeleted"));
        }

        return RedirectToAction("DocumentTypes");
    }

    #endregion

    #region Utilities

    private async Task<IList<SelectListItem>> GetAvailableStatusesAsync()
    {
        var statuses = new List<SelectListItem>
        {
            new SelectListItem
            {
                Text = await _localizationService.GetResourceAsync("Plugins.Marketplace.VendorApplication.All"),
                Value = ""
            }
        };

        foreach (VendorApplicationStatus status in Enum.GetValues(typeof(VendorApplicationStatus)))
        {
            statuses.Add(new SelectListItem
            {
                Text = GetStatusNameSync(status),
                Value = ((int)status).ToString()
            });
        }

        return statuses;
    }

    private string GetStatusNameSync(VendorApplicationStatus status)
    {
        return status switch
        {
            VendorApplicationStatus.Pending => "Pending",
            VendorApplicationStatus.UnderReview => "Under Review",
            VendorApplicationStatus.DocumentsRequested => "Documents Requested",
            VendorApplicationStatus.Approved => "Approved",
            VendorApplicationStatus.Rejected => "Rejected",
            VendorApplicationStatus.Cancelled => "Cancelled",
            _ => "Unknown"
        };
    }

    private string FormatFileSize(long bytes)
    {
        if (bytes < 1024)
            return $"{bytes} B";
        if (bytes < 1024 * 1024)
            return $"{bytes / 1024.0:F1} KB";
        return $"{bytes / (1024.0 * 1024.0):F1} MB";
    }

    #endregion
}
