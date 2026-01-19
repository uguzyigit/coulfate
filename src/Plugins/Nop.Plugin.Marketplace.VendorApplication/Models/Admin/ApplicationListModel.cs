using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Marketplace.VendorApplication.Domain;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Marketplace.VendorApplication.Models.Admin;

public record ApplicationListModel : BaseNopModel
{
    public ApplicationListModel()
    {
        AvailableStatuses = new List<SelectListItem>();
        Applications = new List<ApplicationItemModel>();
    }

    [NopResourceDisplayName("Plugins.Marketplace.VendorApplication.Admin.List.SearchStatus")]
    public int? SearchStatusId { get; set; }

    [NopResourceDisplayName("Plugins.Marketplace.VendorApplication.Admin.List.SearchTerm")]
    public string SearchTerm { get; set; }

    public IList<SelectListItem> AvailableStatuses { get; set; }
    public IList<ApplicationItemModel> Applications { get; set; }

    // Paging
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

public record ApplicationItemModel : BaseNopEntityModel
{
    public string ApplicationNumber { get; set; }
    public string CompanyName { get; set; }
    public string ContactPerson { get; set; }
    public string Email { get; set; }
    public VendorApplicationStatus Status { get; set; }
    public string StatusName { get; set; }
    public DateTime CreatedOnUtc { get; set; }
}
