using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Marketplace.Core.Models;

/// <summary>
/// Represents a configuration model
/// </summary>
public record ConfigurationModel : BaseNopModel
{
    [NopResourceDisplayName("Plugins.Marketplace.Core.Fields.Enabled")]
    public bool Enabled { get; set; }

    [NopResourceDisplayName("Plugins.Marketplace.Core.Fields.RequireVendorApproval")]
    public bool RequireVendorApproval { get; set; }

    [NopResourceDisplayName("Plugins.Marketplace.Core.Fields.MinimumPayoutAmount")]
    public decimal MinimumPayoutAmount { get; set; }
}
