using Nop.Web.Framework.Models;

namespace Nop.Plugin.Marketplace.VendorApplication.Models.Public;

public record ApplySuccessModel : BaseNopModel
{
    public string ApplicationNumber { get; set; }
    public string Email { get; set; }
    public string CheckStatusUrl { get; set; }
}
