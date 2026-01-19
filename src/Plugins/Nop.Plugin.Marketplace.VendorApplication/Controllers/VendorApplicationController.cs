using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Media;
using Nop.Plugin.Marketplace.VendorApplication.Domain;
using Nop.Plugin.Marketplace.VendorApplication.Models.Public;
using Nop.Plugin.Marketplace.VendorApplication.Services;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Marketplace.VendorApplication.Controllers;

[AutoValidateAntiforgeryToken]
public class VendorApplicationController : BasePluginController
{
    private readonly IVendorApplicationService _applicationService;
    private readonly IVendorApplicationMessageService _messageService;
    private readonly VendorApplicationSettings _settings;
    private readonly ILocalizationService _localizationService;
    private readonly IDownloadService _downloadService;
    private readonly IWebHelper _webHelper;
    private readonly IWorkContext _workContext;

    public VendorApplicationController(
        IVendorApplicationService applicationService,
        IVendorApplicationMessageService messageService,
        VendorApplicationSettings settings,
        ILocalizationService localizationService,
        IDownloadService downloadService,
        IWebHelper webHelper,
        IWorkContext workContext)
    {
        _applicationService = applicationService;
        _messageService = messageService;
        _settings = settings;
        _localizationService = localizationService;
        _downloadService = downloadService;
        _webHelper = webHelper;
        _workContext = workContext;
    }

    public async Task<IActionResult> Apply()
    {
        if (!_settings.Enabled)
            return RedirectToRoute("Homepage");

        var model = await PrepareApplyModelAsync();
        return View("~/Plugins/Marketplace.VendorApplication/Views/VendorApplication/Apply.cshtml", model);
    }

    [HttpPost]
    [ValidateCaptcha]
    public async Task<IActionResult> Apply(ApplyModel model, IFormCollection form, bool captchaValid)
    {
        if (!_settings.Enabled)
            return RedirectToRoute("Homepage");

        // Validate CAPTCHA
        if (_settings.DisplayCaptcha && !captchaValid)
        {
            ModelState.AddModelError("", await _localizationService.GetResourceAsync("Common.WrongCaptchaMessage"));
        }

        // Validate terms of service
        if (_settings.RequireTermsOfService && !model.AcceptTerms)
        {
            ModelState.AddModelError("", await _localizationService.GetResourceAsync("Plugins.Marketplace.VendorApplication.Validation.AcceptTerms"));
        }

        // Get document types for validation
        var documentTypes = await _applicationService.GetAllDocumentTypesAsync(activeOnly: true);

        // Validate required documents
        foreach (var docType in documentTypes.Where(d => d.IsRequired))
        {
            var fileKey = $"document_{docType.Id}";
            var file = form.Files[fileKey];

            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", string.Format(
                    await _localizationService.GetResourceAsync("Plugins.Marketplace.VendorApplication.Validation.RequiredDocument"),
                    docType.Name));
            }
        }

        if (!ModelState.IsValid)
        {
            model = await PrepareApplyModelAsync(model);
            return View("~/Plugins/Marketplace.VendorApplication/Views/VendorApplication/Apply.cshtml", model);
        }

        // Create the application
        var application = new Domain.VendorApplication
        {
            CompanyName = model.CompanyName,
            ContactPerson = model.ContactPerson,
            Email = model.Email,
            Phone = model.Phone,
            TaxNumber = model.TaxNumber,
            TradeRegistryNumber = model.TradeRegistryNumber,
            Address = model.Address,
            City = model.City,
            District = model.District,
            PostalCode = model.PostalCode,
            BankAccountHolder = model.BankAccountHolder,
            BankIban = model.BankIban,
            BankName = model.BankName,
            Description = model.Description,
            IpAddress = _webHelper.GetCurrentIpAddress(),
            StatusId = (int)VendorApplicationStatus.Pending
        };

        await _applicationService.InsertApplicationAsync(application);

        // Save uploaded documents
        foreach (var docType in documentTypes)
        {
            var fileKey = $"document_{docType.Id}";
            var file = form.Files[fileKey];

            if (file != null && file.Length > 0)
            {
                // Validate file extension
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!string.IsNullOrEmpty(docType.AllowedExtensions))
                {
                    var allowedExtensions = docType.AllowedExtensions.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    if (!allowedExtensions.Any(e => e.Trim().Equals(extension, StringComparison.OrdinalIgnoreCase)))
                    {
                        continue; // Skip invalid file types
                    }
                }

                // Validate file size
                if (file.Length > docType.MaxFileSizeKb * 1024)
                {
                    continue; // Skip files that are too large
                }

                // Save file to Download table
                var fileBinary = await _downloadService.GetDownloadBitsAsync(file);
                var download = new Download
                {
                    DownloadGuid = Guid.NewGuid(),
                    UseDownloadUrl = false,
                    DownloadUrl = string.Empty,
                    DownloadBinary = fileBinary,
                    ContentType = file.ContentType,
                    Filename = Path.GetFileNameWithoutExtension(file.FileName),
                    Extension = extension,
                    IsNew = true
                };
                await _downloadService.InsertDownloadAsync(download);

                // Create application document record
                var document = new VendorApplicationDocument
                {
                    VendorApplicationId = application.Id,
                    DocumentTypeId = docType.Id,
                    DownloadId = download.Id,
                    FileName = file.FileName,
                    ContentType = file.ContentType,
                    FileSize = file.Length
                };
                await _applicationService.InsertDocumentAsync(document);
            }
        }

        // Send notifications
        var languageId = (await _workContext.GetWorkingLanguageAsync()).Id;
        await _messageService.SendApplicationReceivedNotificationAsync(application, languageId);
        await _messageService.SendNewApplicationAdminNotificationAsync(application, languageId);

        return RedirectToAction("ApplySuccess", new { applicationNumber = application.ApplicationNumber });
    }

    public IActionResult ApplySuccess(string applicationNumber)
    {
        var model = new ApplySuccessModel
        {
            ApplicationNumber = applicationNumber,
            CheckStatusUrl = Url.Action("CheckStatus", "VendorApplication")
        };

        return View("~/Plugins/Marketplace.VendorApplication/Views/VendorApplication/ApplySuccess.cshtml", model);
    }

    public IActionResult CheckStatus()
    {
        // Return empty ApplicationStatusModel - view will show only the form
        var model = new ApplicationStatusModel { Found = false };
        return View("~/Plugins/Marketplace.VendorApplication/Views/VendorApplication/CheckStatus.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> CheckStatus(CheckStatusModel model)
    {
        var application = await _applicationService.GetApplicationByNumberAsync(model.ApplicationNumber);

        var statusModel = new ApplicationStatusModel
        {
            Found = application != null &&
                    application.Email.Equals(model.Email, StringComparison.OrdinalIgnoreCase)
        };

        if (statusModel.Found)
        {
            statusModel.ApplicationNumber = application.ApplicationNumber;
            statusModel.CompanyName = application.CompanyName;
            statusModel.Status = (VendorApplicationStatus)application.StatusId;
            statusModel.StatusName = await GetStatusNameAsync((VendorApplicationStatus)application.StatusId);
            statusModel.SubmittedOn = application.CreatedOnUtc;
            statusModel.RejectionReason = application.RejectionReason;
        }

        ViewBag.CheckStatusModel = model;
        return View("~/Plugins/Marketplace.VendorApplication/Views/VendorApplication/CheckStatus.cshtml", statusModel);
    }

    #region Utilities

    private async Task<ApplyModel> PrepareApplyModelAsync(ApplyModel model = null)
    {
        model ??= new ApplyModel();

        model.DisplayCaptcha = _settings.DisplayCaptcha;
        model.RequireTermsOfService = _settings.RequireTermsOfService;
        model.TermsOfServiceTopicSystemName = _settings.TermsOfServiceTopicSystemName;

        // Load document types
        var documentTypes = await _applicationService.GetAllDocumentTypesAsync(activeOnly: true);
        model.DocumentTypes = documentTypes.Select(dt => new DocumentTypeUploadModel
        {
            DocumentTypeId = dt.Id,
            Name = dt.Name,
            Description = dt.Description,
            IsRequired = dt.IsRequired,
            AllowedExtensions = dt.AllowedExtensions,
            MaxFileSizeKb = dt.MaxFileSizeKb
        }).ToList();

        return model;
    }

    private async Task<string> GetStatusNameAsync(VendorApplicationStatus status)
    {
        var resourceKey = status switch
        {
            VendorApplicationStatus.Pending => "Plugins.Marketplace.VendorApplication.Status.Pending",
            VendorApplicationStatus.UnderReview => "Plugins.Marketplace.VendorApplication.Status.UnderReview",
            VendorApplicationStatus.DocumentsRequested => "Plugins.Marketplace.VendorApplication.Status.DocumentsRequested",
            VendorApplicationStatus.Approved => "Plugins.Marketplace.VendorApplication.Status.Approved",
            VendorApplicationStatus.Rejected => "Plugins.Marketplace.VendorApplication.Status.Rejected",
            VendorApplicationStatus.Cancelled => "Plugins.Marketplace.VendorApplication.Status.Cancelled",
            _ => "Plugins.Marketplace.VendorApplication.Status.Pending"
        };

        return await _localizationService.GetResourceAsync(resourceKey);
    }

    #endregion
}
