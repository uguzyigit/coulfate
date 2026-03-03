using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Marketplace.VendorExtensions.Models;

public record VendorProductListModel : BaseSearchModel
{
    public VendorProductListModel()
    {
        AvailableCategories = new List<SelectListItem>();
        AvailableManufacturers = new List<SelectListItem>();
        AvailablePublishedOptions = new List<SelectListItem>();
    }

    public string SearchProductName { get; set; }
    public int SearchCategoryId { get; set; }
    public int SearchManufacturerId { get; set; }
    public int SearchPublishedId { get; set; }

    public IList<SelectListItem> AvailableCategories { get; set; }
    public IList<SelectListItem> AvailableManufacturers { get; set; }
    public IList<SelectListItem> AvailablePublishedOptions { get; set; }
}
