using Nop.Core.Configuration;

namespace Nop.Plugin.Marketplace.VendorApplication;

/// <summary>
/// Represents settings for Marketplace.VendorApplication plugin
/// </summary>
public class VendorApplicationSettings : ISettings
{
    /// <summary>
    /// Gets or sets a value indicating whether the vendor application feature is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to display CAPTCHA on the application form
    /// </summary>
    public bool DisplayCaptcha { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to require terms of service acceptance
    /// </summary>
    public bool RequireTermsOfService { get; set; } = true;

    /// <summary>
    /// Gets or sets the terms of service topic system name
    /// </summary>
    public string TermsOfServiceTopicSystemName { get; set; } = "VendorTermsOfService";

    /// <summary>
    /// Gets or sets a value indicating whether to send notification to admin on new application
    /// </summary>
    public bool NotifyAdminOnNewApplication { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to send notification to applicant when application is received
    /// </summary>
    public bool NotifyApplicantOnReceived { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to send notification to applicant when application is approved
    /// </summary>
    public bool NotifyApplicantOnApproved { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to send notification to applicant when application is rejected
    /// </summary>
    public bool NotifyApplicantOnRejected { get; set; } = true;

    /// <summary>
    /// Gets or sets the admin email for new application notifications (if empty, uses default store email)
    /// </summary>
    public string AdminNotificationEmail { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to automatically create a customer account if applicant is a guest
    /// </summary>
    public bool AutoCreateCustomerAccount { get; set; } = false;

    /// <summary>
    /// Gets or sets the default vendor status when application is approved
    /// </summary>
    public int DefaultVendorStatusOnApproval { get; set; } = 10; // VendorStatus.Approved from Marketplace.Core
}
