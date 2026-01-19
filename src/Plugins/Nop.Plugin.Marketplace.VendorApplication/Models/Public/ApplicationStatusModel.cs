using Nop.Plugin.Marketplace.VendorApplication.Domain;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Marketplace.VendorApplication.Models.Public;

public record CheckStatusModel : BaseNopModel
{
    [NopResourceDisplayName("Plugins.Marketplace.VendorApplication.CheckStatus.ApplicationNumber")]
    public string ApplicationNumber { get; set; }

    [NopResourceDisplayName("Plugins.Marketplace.VendorApplication.CheckStatus.Email")]
    public string Email { get; set; }
}

public record ApplicationStatusModel : BaseNopModel
{
    public string ApplicationNumber { get; set; }
    public string CompanyName { get; set; }
    public VendorApplicationStatus Status { get; set; }
    public string StatusName { get; set; }
    public DateTime SubmittedOn { get; set; }
    public string RejectionReason { get; set; }
    public bool Found { get; set; }
}
