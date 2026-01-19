using Nop.Plugin.Marketplace.VendorApplication.Domain;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Marketplace.VendorApplication.Models.Admin;

public record ApplicationDetailModel : BaseNopEntityModel
{
    public ApplicationDetailModel()
    {
        Documents = new List<ApplicationDocumentModel>();
    }

    #region Application Info

    public string ApplicationNumber { get; set; }
    public VendorApplicationStatus Status { get; set; }
    public string StatusName { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public DateTime? ReviewedOnUtc { get; set; }
    public string IpAddress { get; set; }

    #endregion

    #region Company Information

    [NopResourceDisplayName("Plugins.Marketplace.VendorApplication.Apply.CompanyName")]
    public string CompanyName { get; set; }

    [NopResourceDisplayName("Plugins.Marketplace.VendorApplication.Apply.ContactPerson")]
    public string ContactPerson { get; set; }

    [NopResourceDisplayName("Plugins.Marketplace.VendorApplication.Apply.Email")]
    public string Email { get; set; }

    [NopResourceDisplayName("Plugins.Marketplace.VendorApplication.Apply.Phone")]
    public string Phone { get; set; }

    #endregion

    #region Legal Information

    [NopResourceDisplayName("Plugins.Marketplace.VendorApplication.Apply.TaxNumber")]
    public string TaxNumber { get; set; }

    [NopResourceDisplayName("Plugins.Marketplace.VendorApplication.Apply.TradeRegistryNumber")]
    public string TradeRegistryNumber { get; set; }

    #endregion

    #region Address Information

    [NopResourceDisplayName("Plugins.Marketplace.VendorApplication.Apply.Address")]
    public string Address { get; set; }

    [NopResourceDisplayName("Plugins.Marketplace.VendorApplication.Apply.City")]
    public string City { get; set; }

    [NopResourceDisplayName("Plugins.Marketplace.VendorApplication.Apply.District")]
    public string District { get; set; }

    [NopResourceDisplayName("Plugins.Marketplace.VendorApplication.Apply.PostalCode")]
    public string PostalCode { get; set; }

    #endregion

    #region Bank Information

    [NopResourceDisplayName("Plugins.Marketplace.VendorApplication.Apply.BankAccountHolder")]
    public string BankAccountHolder { get; set; }

    [NopResourceDisplayName("Plugins.Marketplace.VendorApplication.Apply.BankIban")]
    public string BankIban { get; set; }

    [NopResourceDisplayName("Plugins.Marketplace.VendorApplication.Apply.BankName")]
    public string BankName { get; set; }

    #endregion

    #region Additional Information

    [NopResourceDisplayName("Plugins.Marketplace.VendorApplication.Apply.Description")]
    public string Description { get; set; }

    [NopResourceDisplayName("Plugins.Marketplace.VendorApplication.Admin.Detail.AdminNotes")]
    public string AdminNotes { get; set; }

    [NopResourceDisplayName("Plugins.Marketplace.VendorApplication.Admin.Detail.RejectionReason")]
    public string RejectionReason { get; set; }

    #endregion

    #region Created Vendor

    public int? CreatedVendorId { get; set; }

    #endregion

    #region Documents

    public IList<ApplicationDocumentModel> Documents { get; set; }

    #endregion

    #region Actions

    public bool CanApprove => Status == VendorApplicationStatus.Pending || Status == VendorApplicationStatus.UnderReview;
    public bool CanReject => Status == VendorApplicationStatus.Pending || Status == VendorApplicationStatus.UnderReview;

    #endregion
}

public record ApplicationDocumentModel : BaseNopEntityModel
{
    public int DocumentTypeId { get; set; }
    public string DocumentTypeName { get; set; }
    public int DownloadId { get; set; }
    public string FileName { get; set; }
    public string ContentType { get; set; }
    public long FileSize { get; set; }
    public string FileSizeFormatted { get; set; }
    public DateTime CreatedOnUtc { get; set; }
}
