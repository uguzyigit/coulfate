namespace Nop.Plugin.Marketplace.VendorApplication.Services;

/// <summary>
/// Vendor application message service interface for email notifications
/// </summary>
public interface IVendorApplicationMessageService
{
    /// <summary>
    /// Sends notification to applicant when application is received
    /// </summary>
    Task<IList<int>> SendApplicationReceivedNotificationAsync(Domain.VendorApplication application, int languageId);

    /// <summary>
    /// Sends notification to applicant when application is approved
    /// </summary>
    Task<IList<int>> SendApplicationApprovedNotificationAsync(Domain.VendorApplication application, int languageId);

    /// <summary>
    /// Sends notification to applicant when application is rejected
    /// </summary>
    Task<IList<int>> SendApplicationRejectedNotificationAsync(Domain.VendorApplication application, int languageId);

    /// <summary>
    /// Sends notification to admin when a new application is submitted
    /// </summary>
    Task<IList<int>> SendNewApplicationAdminNotificationAsync(Domain.VendorApplication application, int languageId);
}
