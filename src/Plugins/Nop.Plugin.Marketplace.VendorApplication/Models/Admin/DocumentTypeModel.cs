using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Marketplace.VendorApplication.Models.Admin;

public record DocumentTypeListModel : BaseNopModel
{
    public DocumentTypeListModel()
    {
        DocumentTypes = new List<DocumentTypeModel>();
    }

    public IList<DocumentTypeModel> DocumentTypes { get; set; }
}

public record DocumentTypeModel : BaseNopEntityModel
{
    [NopResourceDisplayName("Plugins.Marketplace.VendorApplication.DocumentTypes.Name")]
    [Required]
    public string Name { get; set; }

    [NopResourceDisplayName("Plugins.Marketplace.VendorApplication.DocumentTypes.Description")]
    public string Description { get; set; }

    [NopResourceDisplayName("Plugins.Marketplace.VendorApplication.DocumentTypes.IsRequired")]
    public bool IsRequired { get; set; }

    [NopResourceDisplayName("Plugins.Marketplace.VendorApplication.DocumentTypes.DisplayOrder")]
    public int DisplayOrder { get; set; }

    [NopResourceDisplayName("Plugins.Marketplace.VendorApplication.DocumentTypes.IsActive")]
    public bool IsActive { get; set; }

    [NopResourceDisplayName("Plugins.Marketplace.VendorApplication.DocumentTypes.AllowedExtensions")]
    public string AllowedExtensions { get; set; }

    [NopResourceDisplayName("Plugins.Marketplace.VendorApplication.DocumentTypes.MaxFileSize")]
    public int MaxFileSizeKb { get; set; }
}
