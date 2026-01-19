using Nop.Core;
using Nop.Core.Domain.Messages;
using Nop.Data;
using Nop.Plugin.Marketplace.VendorApplication.Data;
using Nop.Plugin.Marketplace.VendorApplication.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Plugins;

namespace Nop.Plugin.Marketplace.VendorApplication;

public class VendorApplicationPlugin : BasePlugin
{
    private readonly IWebHelper _webHelper;
    private readonly ISettingService _settingService;
    private readonly ILocalizationService _localizationService;
    private readonly ILanguageService _languageService;
    private readonly INopDataProvider _dataProvider;
    private readonly IMessageTemplateService _messageTemplateService;
    private readonly IEmailAccountService _emailAccountService;
    private readonly EmailAccountSettings _emailAccountSettings;

    public VendorApplicationPlugin(
        IWebHelper webHelper,
        ISettingService settingService,
        ILocalizationService localizationService,
        ILanguageService languageService,
        INopDataProvider dataProvider,
        IMessageTemplateService messageTemplateService,
        IEmailAccountService emailAccountService,
        EmailAccountSettings emailAccountSettings)
    {
        _webHelper = webHelper;
        _settingService = settingService;
        _localizationService = localizationService;
        _languageService = languageService;
        _dataProvider = dataProvider;
        _messageTemplateService = messageTemplateService;
        _emailAccountService = emailAccountService;
        _emailAccountSettings = emailAccountSettings;
    }

    public override string GetConfigurationPageUrl()
    {
        return $"{_webHelper.GetStoreLocation()}Admin/VendorApplicationAdmin/Configure";
    }

    public override async Task InstallAsync()
    {
        // Create database tables
        await _dataProvider.ExecuteNonQueryAsync(InstallationData.CreateTablesScript);

        // Install default settings
        var settings = new VendorApplicationSettings
        {
            Enabled = true,
            DisplayCaptcha = true,
            RequireTermsOfService = false,
            NotifyAdminOnNewApplication = true,
            NotifyApplicantOnReceived = true,
            NotifyApplicantOnApproved = true,
            NotifyApplicantOnRejected = true
        };
        await _settingService.SaveSettingAsync(settings);

        // Install message templates
        await InstallMessageTemplatesAsync();

        // Install English localization resources
        await InstallEnglishResourcesAsync();

        // Install Turkish localization resources
        await InstallTurkishResourcesAsync();

        await base.InstallAsync();
    }

    public override async Task UninstallAsync()
    {
        // Delete settings
        await _settingService.DeleteSettingAsync<VendorApplicationSettings>();

        // Delete localization resources
        await _localizationService.DeleteLocaleResourcesAsync("Plugins.Marketplace.VendorApplication");

        // Delete message templates
        await DeleteMessageTemplatesAsync();

        // Drop tables
        await _dataProvider.ExecuteNonQueryAsync(InstallationData.DropTablesScript);

        await base.UninstallAsync();
    }

    #region Message Templates

    private async Task InstallMessageTemplatesAsync()
    {
        var emailAccount = await _emailAccountService.GetEmailAccountByIdAsync(_emailAccountSettings.DefaultEmailAccountId)
            ?? (await _emailAccountService.GetAllEmailAccountsAsync()).FirstOrDefault();

        if (emailAccount == null)
            return;

        var messageTemplates = new List<MessageTemplate>
        {
            new MessageTemplate
            {
                Name = VendorApplicationMessageService.APPLICATION_RECEIVED_TEMPLATE,
                Subject = "Your Vendor Application Has Been Received - %Store.Name%",
                Body = @"<p>Dear %VendorApplication.ContactPerson%,</p>
<p>Thank you for your application to become a vendor at %Store.Name%.</p>
<p>Your application number is: <strong>%VendorApplication.ApplicationNumber%</strong></p>
<p>We have received your application and will review it shortly. You will receive an email notification once your application has been processed.</p>
<p>You can check your application status at:<br/>
<a href=""%VendorApplication.StatusUrl%"">%VendorApplication.StatusUrl%</a></p>
<p>Best regards,<br/>%Store.Name%</p>",
                IsActive = true,
                EmailAccountId = emailAccount.Id
            },
            new MessageTemplate
            {
                Name = VendorApplicationMessageService.APPLICATION_APPROVED_TEMPLATE,
                Subject = "Congratulations! Your Vendor Application Has Been Approved - %Store.Name%",
                Body = @"<p>Dear %VendorApplication.ContactPerson%,</p>
<p>Great news! Your vendor application for <strong>%VendorApplication.CompanyName%</strong> has been approved.</p>
<p>You can now log in to your vendor panel and start adding products.</p>
<p>Vendor Panel: <a href=""%Store.VendorPanelUrl%"">%Store.VendorPanelUrl%</a></p>
<p>If you have any questions, please don't hesitate to contact us.</p>
<p>Best regards,<br/>%Store.Name%</p>",
                IsActive = true,
                EmailAccountId = emailAccount.Id
            },
            new MessageTemplate
            {
                Name = VendorApplicationMessageService.APPLICATION_REJECTED_TEMPLATE,
                Subject = "Update on Your Vendor Application - %Store.Name%",
                Body = @"<p>Dear %VendorApplication.ContactPerson%,</p>
<p>We regret to inform you that your vendor application for <strong>%VendorApplication.CompanyName%</strong> has not been approved at this time.</p>
<p><strong>Reason:</strong> %VendorApplication.RejectionReason%</p>
<p>If you believe this decision was made in error or would like to provide additional information, please contact us.</p>
<p>Best regards,<br/>%Store.Name%</p>",
                IsActive = true,
                EmailAccountId = emailAccount.Id
            },
            new MessageTemplate
            {
                Name = VendorApplicationMessageService.APPLICATION_NEW_ADMIN_TEMPLATE,
                Subject = "New Vendor Application Received - %VendorApplication.CompanyName%",
                Body = @"<p>A new vendor application has been submitted:</p>
<table>
<tr><td><strong>Application Number:</strong></td><td>%VendorApplication.ApplicationNumber%</td></tr>
<tr><td><strong>Company Name:</strong></td><td>%VendorApplication.CompanyName%</td></tr>
<tr><td><strong>Contact Person:</strong></td><td>%VendorApplication.ContactPerson%</td></tr>
<tr><td><strong>Email:</strong></td><td>%VendorApplication.Email%</td></tr>
<tr><td><strong>Phone:</strong></td><td>%VendorApplication.Phone%</td></tr>
</table>
<p>Review the application:<br/>
<a href=""%VendorApplication.AdminDetailUrl%"">%VendorApplication.AdminDetailUrl%</a></p>",
                IsActive = true,
                EmailAccountId = emailAccount.Id
            }
        };

        foreach (var template in messageTemplates)
        {
            var existingTemplates = await _messageTemplateService.GetMessageTemplatesByNameAsync(template.Name, 0);
            if (!existingTemplates.Any())
                await _messageTemplateService.InsertMessageTemplateAsync(template);
        }
    }

    private async Task DeleteMessageTemplatesAsync()
    {
        var templateNames = new[]
        {
            VendorApplicationMessageService.APPLICATION_RECEIVED_TEMPLATE,
            VendorApplicationMessageService.APPLICATION_APPROVED_TEMPLATE,
            VendorApplicationMessageService.APPLICATION_REJECTED_TEMPLATE,
            VendorApplicationMessageService.APPLICATION_NEW_ADMIN_TEMPLATE
        };

        foreach (var name in templateNames)
        {
            var templates = await _messageTemplateService.GetMessageTemplatesByNameAsync(name, 0);
            foreach (var template in templates)
            {
                await _messageTemplateService.DeleteMessageTemplateAsync(template);
            }
        }
    }

    #endregion

    #region Localization

    private async Task InstallEnglishResourcesAsync()
    {
        await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
        {
            // Plugin Info
            ["Plugins.Marketplace.VendorApplication.FriendlyName"] = "Vendor Application",
            ["Plugins.Marketplace.VendorApplication.Description"] = "Allow anyone to apply to become a vendor",

            // Menu
            ["Plugins.Marketplace.VendorApplication.Menu.Applications"] = "Vendor Applications",
            ["Plugins.Marketplace.VendorApplication.Menu.DocumentTypes"] = "Document Types",
            ["Plugins.Marketplace.VendorApplication.Menu.Settings"] = "Settings",

            // Configure Page
            ["Plugins.Marketplace.VendorApplication.Configure"] = "Vendor Application Settings",
            ["Plugins.Marketplace.VendorApplication.Configure.Title"] = "Vendor Application Configuration",
            ["Plugins.Marketplace.VendorApplication.Fields.Enabled"] = "Enabled",
            ["Plugins.Marketplace.VendorApplication.Fields.DisplayCaptcha"] = "Display CAPTCHA",
            ["Plugins.Marketplace.VendorApplication.Fields.RequireTermsOfService"] = "Require Terms of Service",
            ["Plugins.Marketplace.VendorApplication.Fields.NotifyAdminOnNewApplication"] = "Notify Admin on New Application",
            ["Plugins.Marketplace.VendorApplication.Fields.NotifyApplicantOnReceived"] = "Notify Applicant When Received",
            ["Plugins.Marketplace.VendorApplication.Fields.NotifyApplicantOnApproved"] = "Notify Applicant When Approved",
            ["Plugins.Marketplace.VendorApplication.Fields.NotifyApplicantOnRejected"] = "Notify Applicant When Rejected",
            ["Plugins.Marketplace.VendorApplication.Fields.AdminNotificationEmail"] = "Admin Notification Email",

            // Public Form
            ["Plugins.Marketplace.VendorApplication.Apply.Title"] = "Become a Vendor",
            ["Plugins.Marketplace.VendorApplication.Apply.Subtitle"] = "Fill out the form below to apply to become a vendor",
            ["Plugins.Marketplace.VendorApplication.Apply.CompanyInfo"] = "Company Information",
            ["Plugins.Marketplace.VendorApplication.Apply.CompanyName"] = "Company Name",
            ["Plugins.Marketplace.VendorApplication.Apply.CompanyName.Hint"] = "Enter your company or business name",
            ["Plugins.Marketplace.VendorApplication.Apply.ContactPerson"] = "Contact Person",
            ["Plugins.Marketplace.VendorApplication.Apply.ContactPerson.Hint"] = "Full name of the primary contact",
            ["Plugins.Marketplace.VendorApplication.Apply.Email"] = "Email Address",
            ["Plugins.Marketplace.VendorApplication.Apply.Email.Hint"] = "We will contact you at this email",
            ["Plugins.Marketplace.VendorApplication.Apply.Phone"] = "Phone Number",
            ["Plugins.Marketplace.VendorApplication.Apply.Phone.Hint"] = "Include country code if applicable",

            ["Plugins.Marketplace.VendorApplication.Apply.LegalInfo"] = "Legal Information",
            ["Plugins.Marketplace.VendorApplication.Apply.TaxNumber"] = "Tax Number",
            ["Plugins.Marketplace.VendorApplication.Apply.TaxNumber.Hint"] = "Your tax identification number",
            ["Plugins.Marketplace.VendorApplication.Apply.TradeRegistryNumber"] = "Trade Registry Number",
            ["Plugins.Marketplace.VendorApplication.Apply.TradeRegistryNumber.Hint"] = "Your trade registry or business registration number",

            ["Plugins.Marketplace.VendorApplication.Apply.AddressInfo"] = "Address Information",
            ["Plugins.Marketplace.VendorApplication.Apply.Address"] = "Address",
            ["Plugins.Marketplace.VendorApplication.Apply.Address.Hint"] = "Street address",
            ["Plugins.Marketplace.VendorApplication.Apply.City"] = "City",
            ["Plugins.Marketplace.VendorApplication.Apply.District"] = "District",
            ["Plugins.Marketplace.VendorApplication.Apply.PostalCode"] = "Postal Code",

            ["Plugins.Marketplace.VendorApplication.Apply.BankInfo"] = "Bank Information",
            ["Plugins.Marketplace.VendorApplication.Apply.BankAccountHolder"] = "Account Holder Name",
            ["Plugins.Marketplace.VendorApplication.Apply.BankAccountHolder.Hint"] = "Name as it appears on bank account",
            ["Plugins.Marketplace.VendorApplication.Apply.BankIban"] = "IBAN",
            ["Plugins.Marketplace.VendorApplication.Apply.BankIban.Hint"] = "International Bank Account Number",
            ["Plugins.Marketplace.VendorApplication.Apply.BankName"] = "Bank Name",

            ["Plugins.Marketplace.VendorApplication.Apply.Documents"] = "Required Documents",
            ["Plugins.Marketplace.VendorApplication.Apply.Documents.Hint"] = "Please upload the required documents",
            ["Plugins.Marketplace.VendorApplication.Apply.AdditionalInfo"] = "Additional Information",
            ["Plugins.Marketplace.VendorApplication.Apply.Description"] = "Description",
            ["Plugins.Marketplace.VendorApplication.Apply.Description.Hint"] = "Tell us about your business and products",
            ["Plugins.Marketplace.VendorApplication.Apply.AcceptTerms"] = "I accept the terms of service",
            ["Plugins.Marketplace.VendorApplication.Apply.Submit"] = "Submit Application",

            // Success Page
            ["Plugins.Marketplace.VendorApplication.Apply.Success.Title"] = "Application Submitted",
            ["Plugins.Marketplace.VendorApplication.Apply.Success.Message"] = "Your application has been submitted successfully.",
            ["Plugins.Marketplace.VendorApplication.Apply.Success.ApplicationNumber"] = "Your application number is:",
            ["Plugins.Marketplace.VendorApplication.Apply.Success.NextSteps"] = "We will review your application and contact you shortly. You can check your application status anytime.",

            // Status Check
            ["Plugins.Marketplace.VendorApplication.CheckStatus.Title"] = "Check Application Status",
            ["Plugins.Marketplace.VendorApplication.CheckStatus.ApplicationNumber"] = "Application Number",
            ["Plugins.Marketplace.VendorApplication.CheckStatus.Email"] = "Email Address",
            ["Plugins.Marketplace.VendorApplication.CheckStatus.Submit"] = "Check Status",
            ["Plugins.Marketplace.VendorApplication.CheckStatus.NotFound"] = "Application not found. Please check your application number and email.",
            ["Plugins.Marketplace.VendorApplication.CheckStatus.Status"] = "Status",
            ["Plugins.Marketplace.VendorApplication.CheckStatus.SubmittedOn"] = "Submitted On",
            ["Plugins.Marketplace.VendorApplication.CheckStatus.RejectionReason"] = "Rejection Reason",

            // Navigation
            ["Plugins.Marketplace.VendorApplication.Nav.NewApplication"] = "New Store Application",
            ["Plugins.Marketplace.VendorApplication.Nav.CheckStatus"] = "Check Application Status",

            // Status Names
            ["Plugins.Marketplace.VendorApplication.Status.Pending"] = "Pending",
            ["Plugins.Marketplace.VendorApplication.Status.UnderReview"] = "Under Review",
            ["Plugins.Marketplace.VendorApplication.Status.DocumentsRequested"] = "Documents Requested",
            ["Plugins.Marketplace.VendorApplication.Status.Approved"] = "Approved",
            ["Plugins.Marketplace.VendorApplication.Status.Rejected"] = "Rejected",
            ["Plugins.Marketplace.VendorApplication.Status.Cancelled"] = "Cancelled",

            // Admin - Application List
            ["Plugins.Marketplace.VendorApplication.Admin.List.Title"] = "Vendor Applications",
            ["Plugins.Marketplace.VendorApplication.Admin.List.SearchStatus"] = "Status",
            ["Plugins.Marketplace.VendorApplication.Admin.List.SearchTerm"] = "Search",
            ["Plugins.Marketplace.VendorApplication.Admin.List.Search"] = "Search",
            ["Plugins.Marketplace.VendorApplication.Admin.List.ApplicationNumber"] = "Application #",
            ["Plugins.Marketplace.VendorApplication.Admin.List.CompanyName"] = "Company",
            ["Plugins.Marketplace.VendorApplication.Admin.List.ContactPerson"] = "Contact",
            ["Plugins.Marketplace.VendorApplication.Admin.List.Email"] = "Email",
            ["Plugins.Marketplace.VendorApplication.Admin.List.Status"] = "Status",
            ["Plugins.Marketplace.VendorApplication.Admin.List.CreatedOn"] = "Submitted",
            ["Plugins.Marketplace.VendorApplication.Admin.List.Actions"] = "Actions",
            ["Plugins.Marketplace.VendorApplication.Admin.List.View"] = "View",

            // Admin - Application Detail
            ["Plugins.Marketplace.VendorApplication.Admin.Detail.Title"] = "Application Details",
            ["Plugins.Marketplace.VendorApplication.Admin.Detail.ApplicationInfo"] = "Application Information",
            ["Plugins.Marketplace.VendorApplication.Admin.Detail.CompanyInfo"] = "Company Information",
            ["Plugins.Marketplace.VendorApplication.Admin.Detail.LegalInfo"] = "Legal Information",
            ["Plugins.Marketplace.VendorApplication.Admin.Detail.AddressInfo"] = "Address Information",
            ["Plugins.Marketplace.VendorApplication.Admin.Detail.BankInfo"] = "Bank Information",
            ["Plugins.Marketplace.VendorApplication.Admin.Detail.Documents"] = "Uploaded Documents",
            ["Plugins.Marketplace.VendorApplication.Admin.Detail.AdminNotes"] = "Admin Notes",
            ["Plugins.Marketplace.VendorApplication.Admin.Detail.Approve"] = "Approve",
            ["Plugins.Marketplace.VendorApplication.Admin.Detail.Reject"] = "Reject",
            ["Plugins.Marketplace.VendorApplication.Admin.Detail.RejectionReason"] = "Rejection Reason",
            ["Plugins.Marketplace.VendorApplication.Admin.Detail.RejectionReason.Hint"] = "Provide a reason for rejection (will be sent to applicant)",
            ["Plugins.Marketplace.VendorApplication.Admin.Detail.DownloadDocument"] = "Download",
            ["Plugins.Marketplace.VendorApplication.Admin.Detail.NoDocuments"] = "No documents uploaded",
            ["Plugins.Marketplace.VendorApplication.Admin.Detail.BackToList"] = "Back to list",
            ["Plugins.Marketplace.VendorApplication.Admin.Detail.SaveNotes"] = "Save Notes",

            // Admin - Document Types
            ["Plugins.Marketplace.VendorApplication.DocumentTypes.Title"] = "Document Types",
            ["Plugins.Marketplace.VendorApplication.DocumentTypes.AddNew"] = "Add New",
            ["Plugins.Marketplace.VendorApplication.DocumentTypes.Name"] = "Name",
            ["Plugins.Marketplace.VendorApplication.DocumentTypes.Description"] = "Description",
            ["Plugins.Marketplace.VendorApplication.DocumentTypes.IsRequired"] = "Required",
            ["Plugins.Marketplace.VendorApplication.DocumentTypes.DisplayOrder"] = "Display Order",
            ["Plugins.Marketplace.VendorApplication.DocumentTypes.IsActive"] = "Active",
            ["Plugins.Marketplace.VendorApplication.DocumentTypes.AllowedExtensions"] = "Allowed Extensions",
            ["Plugins.Marketplace.VendorApplication.DocumentTypes.AllowedExtensions.Hint"] = "Comma-separated list (e.g., .pdf,.jpg,.png)",
            ["Plugins.Marketplace.VendorApplication.DocumentTypes.MaxFileSize"] = "Max File Size (KB)",
            ["Plugins.Marketplace.VendorApplication.DocumentTypes.Edit"] = "Edit",
            ["Plugins.Marketplace.VendorApplication.DocumentTypes.Delete"] = "Delete",
            ["Plugins.Marketplace.VendorApplication.DocumentTypes.EditTitle"] = "Edit Document Type",
            ["Plugins.Marketplace.VendorApplication.DocumentTypes.CreateTitle"] = "Create Document Type",

            // Messages
            ["Plugins.Marketplace.VendorApplication.ApplicationApproved"] = "Application approved successfully. Vendor account created.",
            ["Plugins.Marketplace.VendorApplication.ApplicationRejected"] = "Application rejected.",
            ["Plugins.Marketplace.VendorApplication.DocumentTypeSaved"] = "Document type saved successfully.",
            ["Plugins.Marketplace.VendorApplication.DocumentTypeDeleted"] = "Document type deleted.",
            ["Plugins.Marketplace.VendorApplication.SettingsSaved"] = "Settings saved successfully.",
            ["Plugins.Marketplace.VendorApplication.NotesSaved"] = "Notes saved.",
            ["Plugins.Marketplace.VendorApplication.ApplicationSubmitted"] = "Your application has been submitted successfully.",

            // Validation
            ["Plugins.Marketplace.VendorApplication.Validation.CompanyNameRequired"] = "Company name is required",
            ["Plugins.Marketplace.VendorApplication.Validation.ContactPersonRequired"] = "Contact person is required",
            ["Plugins.Marketplace.VendorApplication.Validation.EmailRequired"] = "Email is required",
            ["Plugins.Marketplace.VendorApplication.Validation.EmailInvalid"] = "Please enter a valid email address",
            ["Plugins.Marketplace.VendorApplication.Validation.PhoneRequired"] = "Phone number is required",
            ["Plugins.Marketplace.VendorApplication.Validation.AcceptTerms"] = "You must accept the terms of service",
            ["Plugins.Marketplace.VendorApplication.Validation.RequiredDocument"] = "Please upload the required document: {0}",
            ["Plugins.Marketplace.VendorApplication.Validation.FileTypeNotAllowed"] = "File type not allowed for {0}. Allowed types: {1}",
            ["Plugins.Marketplace.VendorApplication.Validation.FileTooLarge"] = "File too large for {0}. Maximum size: {1} KB",

            // Common
            ["Plugins.Marketplace.VendorApplication.Save"] = "Save",
            ["Plugins.Marketplace.VendorApplication.Cancel"] = "Cancel",
            ["Plugins.Marketplace.VendorApplication.All"] = "All",
        });
    }

    private async Task InstallTurkishResourcesAsync()
    {
        var languages = await _languageService.GetAllLanguagesAsync();
        var turkishLanguage = languages.FirstOrDefault(l => l.LanguageCulture == "tr-TR");

        if (turkishLanguage == null)
            return;

        await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
        {
            // Plugin Bilgisi
            ["Plugins.Marketplace.VendorApplication.FriendlyName"] = "Satici Basvurusu",
            ["Plugins.Marketplace.VendorApplication.Description"] = "Herkesin satici basvurusu yapmasina izin verin",

            // Menu
            ["Plugins.Marketplace.VendorApplication.Menu.Applications"] = "Satici Basvurulari",
            ["Plugins.Marketplace.VendorApplication.Menu.DocumentTypes"] = "Belge Tipleri",
            ["Plugins.Marketplace.VendorApplication.Menu.Settings"] = "Ayarlar",

            // Ayarlar Sayfasi
            ["Plugins.Marketplace.VendorApplication.Configure"] = "Satici Basvuru Ayarlari",
            ["Plugins.Marketplace.VendorApplication.Configure.Title"] = "Satici Basvuru Yapilandirmasi",
            ["Plugins.Marketplace.VendorApplication.Fields.Enabled"] = "Etkin",
            ["Plugins.Marketplace.VendorApplication.Fields.DisplayCaptcha"] = "CAPTCHA Goster",
            ["Plugins.Marketplace.VendorApplication.Fields.RequireTermsOfService"] = "Kullanim Kosullari Zorunlu",
            ["Plugins.Marketplace.VendorApplication.Fields.NotifyAdminOnNewApplication"] = "Yeni Basvuruda Admin'e Bildir",
            ["Plugins.Marketplace.VendorApplication.Fields.NotifyApplicantOnReceived"] = "Basvuru Alindiginda Bildir",
            ["Plugins.Marketplace.VendorApplication.Fields.NotifyApplicantOnApproved"] = "Onaylandiginda Bildir",
            ["Plugins.Marketplace.VendorApplication.Fields.NotifyApplicantOnRejected"] = "Redddildiginde Bildir",
            ["Plugins.Marketplace.VendorApplication.Fields.AdminNotificationEmail"] = "Admin Bildirim E-postasi",

            // Herkese Acik Form
            ["Plugins.Marketplace.VendorApplication.Apply.Title"] = "Satici Olun",
            ["Plugins.Marketplace.VendorApplication.Apply.Subtitle"] = "Satici olmak icin asagidaki formu doldurun",
            ["Plugins.Marketplace.VendorApplication.Apply.CompanyInfo"] = "Sirket Bilgileri",
            ["Plugins.Marketplace.VendorApplication.Apply.CompanyName"] = "Sirket Adi",
            ["Plugins.Marketplace.VendorApplication.Apply.CompanyName.Hint"] = "Sirket veya isletme adinizi girin",
            ["Plugins.Marketplace.VendorApplication.Apply.ContactPerson"] = "Iletisim Kisisi",
            ["Plugins.Marketplace.VendorApplication.Apply.ContactPerson.Hint"] = "Yetkili kisinin tam adi",
            ["Plugins.Marketplace.VendorApplication.Apply.Email"] = "E-posta Adresi",
            ["Plugins.Marketplace.VendorApplication.Apply.Email.Hint"] = "Sizinle bu e-posta uzerinden iletisime gececegiz",
            ["Plugins.Marketplace.VendorApplication.Apply.Phone"] = "Telefon Numarasi",
            ["Plugins.Marketplace.VendorApplication.Apply.Phone.Hint"] = "Ulke kodu dahil",

            ["Plugins.Marketplace.VendorApplication.Apply.LegalInfo"] = "Yasal Bilgiler",
            ["Plugins.Marketplace.VendorApplication.Apply.TaxNumber"] = "Vergi Numarasi",
            ["Plugins.Marketplace.VendorApplication.Apply.TaxNumber.Hint"] = "Vergi kimlik numaraniz",
            ["Plugins.Marketplace.VendorApplication.Apply.TradeRegistryNumber"] = "Ticaret Sicil Numarasi",
            ["Plugins.Marketplace.VendorApplication.Apply.TradeRegistryNumber.Hint"] = "Ticaret sicil veya isletme tescil numaraniz",

            ["Plugins.Marketplace.VendorApplication.Apply.AddressInfo"] = "Adres Bilgileri",
            ["Plugins.Marketplace.VendorApplication.Apply.Address"] = "Adres",
            ["Plugins.Marketplace.VendorApplication.Apply.Address.Hint"] = "Acik adres",
            ["Plugins.Marketplace.VendorApplication.Apply.City"] = "Il",
            ["Plugins.Marketplace.VendorApplication.Apply.District"] = "Ilce",
            ["Plugins.Marketplace.VendorApplication.Apply.PostalCode"] = "Posta Kodu",

            ["Plugins.Marketplace.VendorApplication.Apply.BankInfo"] = "Banka Bilgileri",
            ["Plugins.Marketplace.VendorApplication.Apply.BankAccountHolder"] = "Hesap Sahibi",
            ["Plugins.Marketplace.VendorApplication.Apply.BankAccountHolder.Hint"] = "Banka hesabindaki isim",
            ["Plugins.Marketplace.VendorApplication.Apply.BankIban"] = "IBAN",
            ["Plugins.Marketplace.VendorApplication.Apply.BankIban.Hint"] = "Uluslararasi Banka Hesap Numarasi",
            ["Plugins.Marketplace.VendorApplication.Apply.BankName"] = "Banka Adi",

            ["Plugins.Marketplace.VendorApplication.Apply.Documents"] = "Gerekli Belgeler",
            ["Plugins.Marketplace.VendorApplication.Apply.Documents.Hint"] = "Lutfen gerekli belgeleri yukleyin",
            ["Plugins.Marketplace.VendorApplication.Apply.AdditionalInfo"] = "Ek Bilgiler",
            ["Plugins.Marketplace.VendorApplication.Apply.Description"] = "Aciklama",
            ["Plugins.Marketplace.VendorApplication.Apply.Description.Hint"] = "Isletmeniz ve urunleriniz hakkinda bilgi verin",
            ["Plugins.Marketplace.VendorApplication.Apply.AcceptTerms"] = "Kullanim kosullarini kabul ediyorum",
            ["Plugins.Marketplace.VendorApplication.Apply.Submit"] = "Basvuruyu Gonder",

            // Basari Sayfasi
            ["Plugins.Marketplace.VendorApplication.Apply.Success.Title"] = "Basvuru Gonderildi",
            ["Plugins.Marketplace.VendorApplication.Apply.Success.Message"] = "Basvurunuz basariyla gonderildi.",
            ["Plugins.Marketplace.VendorApplication.Apply.Success.ApplicationNumber"] = "Basvuru numaraniz:",
            ["Plugins.Marketplace.VendorApplication.Apply.Success.NextSteps"] = "Basvurunuzu inceleyip en kisa surede size donecegiz. Basvuru durumunuzu istediginiz zaman kontrol edebilirsiniz.",

            // Durum Kontrolu
            ["Plugins.Marketplace.VendorApplication.CheckStatus.Title"] = "Basvuru Durumunu Kontrol Et",
            ["Plugins.Marketplace.VendorApplication.CheckStatus.ApplicationNumber"] = "Basvuru Numarasi",
            ["Plugins.Marketplace.VendorApplication.CheckStatus.Email"] = "E-posta Adresi",
            ["Plugins.Marketplace.VendorApplication.CheckStatus.Submit"] = "Durumu Kontrol Et",
            ["Plugins.Marketplace.VendorApplication.CheckStatus.NotFound"] = "Basvuru bulunamadi. Lutfen basvuru numaranizi ve e-posta adresinizi kontrol edin.",
            ["Plugins.Marketplace.VendorApplication.CheckStatus.Status"] = "Durum",
            ["Plugins.Marketplace.VendorApplication.CheckStatus.SubmittedOn"] = "Basvuru Tarihi",
            ["Plugins.Marketplace.VendorApplication.CheckStatus.RejectionReason"] = "Red Sebebi",

            // Navigasyon
            ["Plugins.Marketplace.VendorApplication.Nav.NewApplication"] = "Yeni Magaza Basvurusu",
            ["Plugins.Marketplace.VendorApplication.Nav.CheckStatus"] = "Basvuru Durumunu Kontrol Et",

            // Durum Isimleri
            ["Plugins.Marketplace.VendorApplication.Status.Pending"] = "Beklemede",
            ["Plugins.Marketplace.VendorApplication.Status.UnderReview"] = "Inceleniyor",
            ["Plugins.Marketplace.VendorApplication.Status.DocumentsRequested"] = "Belge Istendi",
            ["Plugins.Marketplace.VendorApplication.Status.Approved"] = "Onaylandi",
            ["Plugins.Marketplace.VendorApplication.Status.Rejected"] = "Reddedildi",
            ["Plugins.Marketplace.VendorApplication.Status.Cancelled"] = "Iptal Edildi",

            // Admin - Basvuru Listesi
            ["Plugins.Marketplace.VendorApplication.Admin.List.Title"] = "Satici Basvurulari",
            ["Plugins.Marketplace.VendorApplication.Admin.List.SearchStatus"] = "Durum",
            ["Plugins.Marketplace.VendorApplication.Admin.List.SearchTerm"] = "Ara",
            ["Plugins.Marketplace.VendorApplication.Admin.List.Search"] = "Ara",
            ["Plugins.Marketplace.VendorApplication.Admin.List.ApplicationNumber"] = "Basvuru No",
            ["Plugins.Marketplace.VendorApplication.Admin.List.CompanyName"] = "Sirket",
            ["Plugins.Marketplace.VendorApplication.Admin.List.ContactPerson"] = "Iletisim",
            ["Plugins.Marketplace.VendorApplication.Admin.List.Email"] = "E-posta",
            ["Plugins.Marketplace.VendorApplication.Admin.List.Status"] = "Durum",
            ["Plugins.Marketplace.VendorApplication.Admin.List.CreatedOn"] = "Tarih",
            ["Plugins.Marketplace.VendorApplication.Admin.List.Actions"] = "Islemler",
            ["Plugins.Marketplace.VendorApplication.Admin.List.View"] = "Goruntule",

            // Admin - Basvuru Detayi
            ["Plugins.Marketplace.VendorApplication.Admin.Detail.Title"] = "Basvuru Detaylari",
            ["Plugins.Marketplace.VendorApplication.Admin.Detail.ApplicationInfo"] = "Basvuru Bilgileri",
            ["Plugins.Marketplace.VendorApplication.Admin.Detail.CompanyInfo"] = "Sirket Bilgileri",
            ["Plugins.Marketplace.VendorApplication.Admin.Detail.LegalInfo"] = "Yasal Bilgiler",
            ["Plugins.Marketplace.VendorApplication.Admin.Detail.AddressInfo"] = "Adres Bilgileri",
            ["Plugins.Marketplace.VendorApplication.Admin.Detail.BankInfo"] = "Banka Bilgileri",
            ["Plugins.Marketplace.VendorApplication.Admin.Detail.Documents"] = "Yuklenen Belgeler",
            ["Plugins.Marketplace.VendorApplication.Admin.Detail.AdminNotes"] = "Admin Notlari",
            ["Plugins.Marketplace.VendorApplication.Admin.Detail.Approve"] = "Onayla",
            ["Plugins.Marketplace.VendorApplication.Admin.Detail.Reject"] = "Reddet",
            ["Plugins.Marketplace.VendorApplication.Admin.Detail.RejectionReason"] = "Red Sebebi",
            ["Plugins.Marketplace.VendorApplication.Admin.Detail.RejectionReason.Hint"] = "Red sebebi belirtin (basvurana gonderilecek)",
            ["Plugins.Marketplace.VendorApplication.Admin.Detail.DownloadDocument"] = "Indir",
            ["Plugins.Marketplace.VendorApplication.Admin.Detail.NoDocuments"] = "Belge yuklenmemis",
            ["Plugins.Marketplace.VendorApplication.Admin.Detail.BackToList"] = "Listeye don",
            ["Plugins.Marketplace.VendorApplication.Admin.Detail.SaveNotes"] = "Notlari Kaydet",

            // Admin - Belge Tipleri
            ["Plugins.Marketplace.VendorApplication.DocumentTypes.Title"] = "Belge Tipleri",
            ["Plugins.Marketplace.VendorApplication.DocumentTypes.AddNew"] = "Yeni Ekle",
            ["Plugins.Marketplace.VendorApplication.DocumentTypes.Name"] = "Ad",
            ["Plugins.Marketplace.VendorApplication.DocumentTypes.Description"] = "Aciklama",
            ["Plugins.Marketplace.VendorApplication.DocumentTypes.IsRequired"] = "Zorunlu",
            ["Plugins.Marketplace.VendorApplication.DocumentTypes.DisplayOrder"] = "Siralama",
            ["Plugins.Marketplace.VendorApplication.DocumentTypes.IsActive"] = "Aktif",
            ["Plugins.Marketplace.VendorApplication.DocumentTypes.AllowedExtensions"] = "Izin Verilen Uzantilar",
            ["Plugins.Marketplace.VendorApplication.DocumentTypes.AllowedExtensions.Hint"] = "Virgulla ayrilmis (orn: .pdf,.jpg,.png)",
            ["Plugins.Marketplace.VendorApplication.DocumentTypes.MaxFileSize"] = "Max Dosya Boyutu (KB)",
            ["Plugins.Marketplace.VendorApplication.DocumentTypes.Edit"] = "Duzenle",
            ["Plugins.Marketplace.VendorApplication.DocumentTypes.Delete"] = "Sil",
            ["Plugins.Marketplace.VendorApplication.DocumentTypes.EditTitle"] = "Belge Tipini Duzenle",
            ["Plugins.Marketplace.VendorApplication.DocumentTypes.CreateTitle"] = "Belge Tipi Olustur",

            // Mesajlar
            ["Plugins.Marketplace.VendorApplication.ApplicationApproved"] = "Basvuru onaylandi. Satici hesabi olusturuldu.",
            ["Plugins.Marketplace.VendorApplication.ApplicationRejected"] = "Basvuru reddedildi.",
            ["Plugins.Marketplace.VendorApplication.DocumentTypeSaved"] = "Belge tipi kaydedildi.",
            ["Plugins.Marketplace.VendorApplication.DocumentTypeDeleted"] = "Belge tipi silindi.",
            ["Plugins.Marketplace.VendorApplication.SettingsSaved"] = "Ayarlar kaydedildi.",
            ["Plugins.Marketplace.VendorApplication.NotesSaved"] = "Notlar kaydedildi.",
            ["Plugins.Marketplace.VendorApplication.ApplicationSubmitted"] = "Basvurunuz basariyla gonderildi.",

            // Dogrulama
            ["Plugins.Marketplace.VendorApplication.Validation.CompanyNameRequired"] = "Sirket adi zorunludur",
            ["Plugins.Marketplace.VendorApplication.Validation.ContactPersonRequired"] = "Iletisim kisisi zorunludur",
            ["Plugins.Marketplace.VendorApplication.Validation.EmailRequired"] = "E-posta zorunludur",
            ["Plugins.Marketplace.VendorApplication.Validation.EmailInvalid"] = "Gecerli bir e-posta adresi girin",
            ["Plugins.Marketplace.VendorApplication.Validation.PhoneRequired"] = "Telefon numarasi zorunludur",
            ["Plugins.Marketplace.VendorApplication.Validation.AcceptTerms"] = "Kullanim kosullarini kabul etmelisiniz",
            ["Plugins.Marketplace.VendorApplication.Validation.RequiredDocument"] = "Lutfen gerekli belgeyi yukleyin: {0}",
            ["Plugins.Marketplace.VendorApplication.Validation.FileTypeNotAllowed"] = "{0} icin dosya tipi izin verilmiyor. Izin verilen tipler: {1}",
            ["Plugins.Marketplace.VendorApplication.Validation.FileTooLarge"] = "{0} icin dosya cok buyuk. Maksimum boyut: {1} KB",

            // Ortak
            ["Plugins.Marketplace.VendorApplication.Save"] = "Kaydet",
            ["Plugins.Marketplace.VendorApplication.Cancel"] = "Iptal",
            ["Plugins.Marketplace.VendorApplication.All"] = "Tumu",
        }, turkishLanguage.Id);
    }

    #endregion
}
