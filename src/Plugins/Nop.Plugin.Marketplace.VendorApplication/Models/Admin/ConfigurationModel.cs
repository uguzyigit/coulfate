using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Marketplace.VendorApplication.Models.Admin;

public record ConfigurationModel : BaseNopModel
{
    [NopResourceDisplayName("Plugins.Marketplace.VendorApplication.Fields.Enabled")]
    public bool Enabled { get; set; }

    [NopResourceDisplayName("Plugins.Marketplace.VendorApplication.Fields.DisplayCaptcha")]
    public bool DisplayCaptcha { get; set; }

    [NopResourceDisplayName("Plugins.Marketplace.VendorApplication.Fields.RequireTermsOfService")]
    public bool RequireTermsOfService { get; set; }

    [NopResourceDisplayName("Plugins.Marketplace.VendorApplication.Fields.NotifyAdminOnNewApplication")]
    public bool NotifyAdminOnNewApplication { get; set; }

    [NopResourceDisplayName("Plugins.Marketplace.VendorApplication.Fields.NotifyApplicantOnReceived")]
    public bool NotifyApplicantOnReceived { get; set; }

    [NopResourceDisplayName("Plugins.Marketplace.VendorApplication.Fields.NotifyApplicantOnApproved")]
    public bool NotifyApplicantOnApproved { get; set; }

    [NopResourceDisplayName("Plugins.Marketplace.VendorApplication.Fields.NotifyApplicantOnRejected")]
    public bool NotifyApplicantOnRejected { get; set; }

    [NopResourceDisplayName("Plugins.Marketplace.VendorApplication.Fields.AdminNotificationEmail")]
    public string AdminNotificationEmail { get; set; }
}
