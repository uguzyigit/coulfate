using Nop.Core;
using Nop.Core.Domain.Messages;
using Nop.Services.Localization;
using Nop.Services.Messages;

namespace Nop.Plugin.Marketplace.VendorApplication.Services;

/// <summary>
/// Vendor application message service implementation for email notifications
/// </summary>
public class VendorApplicationMessageService : IVendorApplicationMessageService
{
    #region Constants

    public const string APPLICATION_RECEIVED_TEMPLATE = "VendorApplication.Received";
    public const string APPLICATION_APPROVED_TEMPLATE = "VendorApplication.Approved";
    public const string APPLICATION_REJECTED_TEMPLATE = "VendorApplication.Rejected";
    public const string APPLICATION_NEW_ADMIN_TEMPLATE = "VendorApplication.NewApplication";

    #endregion

    #region Fields

    private readonly IMessageTemplateService _messageTemplateService;
    private readonly IEmailAccountService _emailAccountService;
    private readonly IQueuedEmailService _queuedEmailService;
    private readonly ITokenizer _tokenizer;
    private readonly IStoreContext _storeContext;
    private readonly ILanguageService _languageService;
    private readonly ILocalizationService _localizationService;
    private readonly VendorApplicationSettings _settings;
    private readonly IWebHelper _webHelper;
    private readonly EmailAccountSettings _emailAccountSettings;

    #endregion

    #region Ctor

    public VendorApplicationMessageService(
        IMessageTemplateService messageTemplateService,
        IEmailAccountService emailAccountService,
        IQueuedEmailService queuedEmailService,
        ITokenizer tokenizer,
        IStoreContext storeContext,
        ILanguageService languageService,
        ILocalizationService localizationService,
        VendorApplicationSettings settings,
        IWebHelper webHelper,
        EmailAccountSettings emailAccountSettings)
    {
        _messageTemplateService = messageTemplateService;
        _emailAccountService = emailAccountService;
        _queuedEmailService = queuedEmailService;
        _tokenizer = tokenizer;
        _storeContext = storeContext;
        _languageService = languageService;
        _localizationService = localizationService;
        _settings = settings;
        _webHelper = webHelper;
        _emailAccountSettings = emailAccountSettings;
    }

    #endregion

    #region Utilities

    protected virtual async Task<MessageTemplate> GetActiveMessageTemplateAsync(string templateName, int storeId)
    {
        var messageTemplates = await _messageTemplateService.GetMessageTemplatesByNameAsync(templateName, storeId);
        var messageTemplate = messageTemplates.FirstOrDefault();

        if (messageTemplate == null || !messageTemplate.IsActive)
            return null;

        return messageTemplate;
    }

    protected virtual async Task<EmailAccount> GetEmailAccountAsync(int emailAccountId)
    {
        var emailAccount = await _emailAccountService.GetEmailAccountByIdAsync(emailAccountId)
            ?? await _emailAccountService.GetEmailAccountByIdAsync(_emailAccountSettings.DefaultEmailAccountId);

        if (emailAccount == null)
            throw new NopException("Email account could not be loaded");

        return emailAccount;
    }

    protected virtual async Task<int> EnsureLanguageIsActiveAsync(int languageId, int storeId)
    {
        var language = await _languageService.GetLanguageByIdAsync(languageId);

        if (language == null || !language.Published)
        {
            language = await _languageService.GetAllLanguagesAsync(storeId: storeId).ContinueWith(t => t.Result.FirstOrDefault());
        }

        return language?.Id ?? languageId;
    }

    protected virtual IList<Token> GenerateTokens(Domain.VendorApplication application)
    {
        var store = _storeContext.GetCurrentStoreAsync().Result;
        var storeUrl = _webHelper.GetStoreLocation();

        var tokens = new List<Token>
        {
            new Token("VendorApplication.ApplicationNumber", application.ApplicationNumber),
            new Token("VendorApplication.CompanyName", application.CompanyName),
            new Token("VendorApplication.ContactPerson", application.ContactPerson),
            new Token("VendorApplication.Email", application.Email),
            new Token("VendorApplication.Phone", application.Phone ?? string.Empty),
            new Token("VendorApplication.TaxNumber", application.TaxNumber ?? string.Empty),
            new Token("VendorApplication.TradeRegistryNumber", application.TradeRegistryNumber ?? string.Empty),
            new Token("VendorApplication.Address", application.Address ?? string.Empty),
            new Token("VendorApplication.City", application.City ?? string.Empty),
            new Token("VendorApplication.District", application.District ?? string.Empty),
            new Token("VendorApplication.RejectionReason", application.RejectionReason ?? string.Empty),
            new Token("VendorApplication.StatusUrl", $"{storeUrl}VendorApplication/CheckStatus"),
            new Token("VendorApplication.AdminDetailUrl", $"{storeUrl}Admin/VendorApplicationAdmin/Detail/{application.Id}"),
            new Token("Store.Name", store.Name),
            new Token("Store.URL", storeUrl),
            new Token("Store.VendorPanelUrl", $"{storeUrl}Admin/Vendor")
        };

        return tokens;
    }

    protected virtual async Task<IList<int>> SendNotificationAsync(
        MessageTemplate messageTemplate,
        EmailAccount emailAccount,
        int languageId,
        IList<Token> tokens,
        string toEmail,
        string toName,
        string replyToEmail = null,
        string replyToName = null)
    {
        var result = new List<int>();

        var bcc = await _localizationService.GetLocalizedAsync(messageTemplate, mt => mt.BccEmailAddresses, languageId);
        var subject = await _localizationService.GetLocalizedAsync(messageTemplate, mt => mt.Subject, languageId);
        var body = await _localizationService.GetLocalizedAsync(messageTemplate, mt => mt.Body, languageId);

        // Replace tokens
        var subjectReplaced = _tokenizer.Replace(subject, tokens, false);
        var bodyReplaced = _tokenizer.Replace(body, tokens, true);

        var email = new QueuedEmail
        {
            Priority = QueuedEmailPriority.High,
            From = emailAccount.Email,
            FromName = emailAccount.DisplayName,
            To = toEmail,
            ToName = toName,
            ReplyTo = replyToEmail,
            ReplyToName = replyToName,
            CC = string.Empty,
            Bcc = bcc,
            Subject = subjectReplaced,
            Body = bodyReplaced,
            AttachedDownloadId = 0,
            CreatedOnUtc = DateTime.UtcNow,
            EmailAccountId = emailAccount.Id,
            DontSendBeforeDateUtc = null
        };

        await _queuedEmailService.InsertQueuedEmailAsync(email);
        result.Add(email.Id);

        return result;
    }

    #endregion

    #region Methods

    public virtual async Task<IList<int>> SendApplicationReceivedNotificationAsync(Domain.VendorApplication application, int languageId)
    {
        ArgumentNullException.ThrowIfNull(application);

        if (!_settings.NotifyApplicantOnReceived)
            return new List<int>();

        var store = await _storeContext.GetCurrentStoreAsync();
        languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

        var messageTemplate = await GetActiveMessageTemplateAsync(APPLICATION_RECEIVED_TEMPLATE, store.Id);
        if (messageTemplate == null)
            return new List<int>();

        var emailAccount = await GetEmailAccountAsync(messageTemplate.EmailAccountId);
        var tokens = GenerateTokens(application);

        return await SendNotificationAsync(
            messageTemplate,
            emailAccount,
            languageId,
            tokens,
            application.Email,
            application.ContactPerson);
    }

    public virtual async Task<IList<int>> SendApplicationApprovedNotificationAsync(Domain.VendorApplication application, int languageId)
    {
        ArgumentNullException.ThrowIfNull(application);

        if (!_settings.NotifyApplicantOnApproved)
            return new List<int>();

        var store = await _storeContext.GetCurrentStoreAsync();
        languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

        var messageTemplate = await GetActiveMessageTemplateAsync(APPLICATION_APPROVED_TEMPLATE, store.Id);
        if (messageTemplate == null)
            return new List<int>();

        var emailAccount = await GetEmailAccountAsync(messageTemplate.EmailAccountId);
        var tokens = GenerateTokens(application);

        return await SendNotificationAsync(
            messageTemplate,
            emailAccount,
            languageId,
            tokens,
            application.Email,
            application.ContactPerson);
    }

    public virtual async Task<IList<int>> SendApplicationRejectedNotificationAsync(Domain.VendorApplication application, int languageId)
    {
        ArgumentNullException.ThrowIfNull(application);

        if (!_settings.NotifyApplicantOnRejected)
            return new List<int>();

        var store = await _storeContext.GetCurrentStoreAsync();
        languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

        var messageTemplate = await GetActiveMessageTemplateAsync(APPLICATION_REJECTED_TEMPLATE, store.Id);
        if (messageTemplate == null)
            return new List<int>();

        var emailAccount = await GetEmailAccountAsync(messageTemplate.EmailAccountId);
        var tokens = GenerateTokens(application);

        return await SendNotificationAsync(
            messageTemplate,
            emailAccount,
            languageId,
            tokens,
            application.Email,
            application.ContactPerson);
    }

    public virtual async Task<IList<int>> SendNewApplicationAdminNotificationAsync(Domain.VendorApplication application, int languageId)
    {
        ArgumentNullException.ThrowIfNull(application);

        if (!_settings.NotifyAdminOnNewApplication)
            return new List<int>();

        var store = await _storeContext.GetCurrentStoreAsync();
        languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

        var messageTemplate = await GetActiveMessageTemplateAsync(APPLICATION_NEW_ADMIN_TEMPLATE, store.Id);
        if (messageTemplate == null)
            return new List<int>();

        var emailAccount = await GetEmailAccountAsync(messageTemplate.EmailAccountId);
        var tokens = GenerateTokens(application);

        // Determine admin email
        var adminEmail = !string.IsNullOrEmpty(_settings.AdminNotificationEmail)
            ? _settings.AdminNotificationEmail
            : emailAccount.Email;

        return await SendNotificationAsync(
            messageTemplate,
            emailAccount,
            languageId,
            tokens,
            adminEmail,
            "Admin",
            application.Email,
            application.ContactPerson);
    }

    #endregion
}
