using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Marketplace.VendorApplication.Models.Public;

public record ApplyModel : BaseNopModel
{
    public ApplyModel()
    {
        DocumentTypes = new List<DocumentTypeUploadModel>();
    }

    #region Company Information

    [NopResourceDisplayName("Plugins.Marketplace.VendorApplication.Apply.CompanyName")]
    [Required(ErrorMessage = "Company name is required")]
    public string CompanyName { get; set; }

    [NopResourceDisplayName("Plugins.Marketplace.VendorApplication.Apply.ContactPerson")]
    [Required(ErrorMessage = "Contact person is required")]
    public string ContactPerson { get; set; }

    [NopResourceDisplayName("Plugins.Marketplace.VendorApplication.Apply.Email")]
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    public string Email { get; set; }

    [NopResourceDisplayName("Plugins.Marketplace.VendorApplication.Apply.Phone")]
    [Required(ErrorMessage = "Phone number is required")]
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

    #endregion

    #region Terms

    [NopResourceDisplayName("Plugins.Marketplace.VendorApplication.Apply.AcceptTerms")]
    public bool AcceptTerms { get; set; }

    #endregion

    #region Document Uploads

    public IList<DocumentTypeUploadModel> DocumentTypes { get; set; }

    #endregion

    #region Display Settings

    public bool DisplayCaptcha { get; set; }
    public bool RequireTermsOfService { get; set; }
    public string TermsOfServiceTopicSystemName { get; set; }

    #endregion
}

public record DocumentTypeUploadModel : BaseNopModel
{
    public int DocumentTypeId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsRequired { get; set; }
    public string AllowedExtensions { get; set; }
    public int MaxFileSizeKb { get; set; }

    // For form binding
    public IFormFile UploadedFile { get; set; }
}
