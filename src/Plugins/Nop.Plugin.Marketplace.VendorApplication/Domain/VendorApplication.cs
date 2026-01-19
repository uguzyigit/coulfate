using Nop.Core;

namespace Nop.Plugin.Marketplace.VendorApplication.Domain;

/// <summary>
/// Represents a vendor application
/// </summary>
public partial class VendorApplication : BaseEntity
{
    /// <summary>
    /// Gets or sets the unique application number (e.g., VA20260116-0001)
    /// </summary>
    public string ApplicationNumber { get; set; }

    /// <summary>
    /// Gets or sets the application status (stored as int in DB "Status" column)
    /// Use VendorApplicationStatus enum values cast to int
    /// </summary>
    public int StatusId { get; set; }

    #region Company Information

    /// <summary>
    /// Gets or sets the company name
    /// </summary>
    public string CompanyName { get; set; }

    /// <summary>
    /// Gets or sets the contact person name
    /// </summary>
    public string ContactPerson { get; set; }

    /// <summary>
    /// Gets or sets the email address
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// Gets or sets the phone number
    /// </summary>
    public string Phone { get; set; }

    #endregion

    #region Legal Information

    /// <summary>
    /// Gets or sets the tax number (Vergi Numarasi)
    /// </summary>
    public string TaxNumber { get; set; }

    /// <summary>
    /// Gets or sets the trade registry number (Ticaret Sicil Numarasi)
    /// </summary>
    public string TradeRegistryNumber { get; set; }

    #endregion

    #region Address Information

    /// <summary>
    /// Gets or sets the address
    /// </summary>
    public string Address { get; set; }

    /// <summary>
    /// Gets or sets the city (Il)
    /// </summary>
    public string City { get; set; }

    /// <summary>
    /// Gets or sets the district (Ilce)
    /// </summary>
    public string District { get; set; }

    /// <summary>
    /// Gets or sets the postal code
    /// </summary>
    public string PostalCode { get; set; }

    /// <summary>
    /// Gets or sets the country identifier
    /// </summary>
    public int? CountryId { get; set; }

    /// <summary>
    /// Gets or sets the state/province identifier
    /// </summary>
    public int? StateProvinceId { get; set; }

    #endregion

    #region Bank Information

    /// <summary>
    /// Gets or sets the bank account holder name
    /// </summary>
    public string BankAccountHolder { get; set; }

    /// <summary>
    /// Gets or sets the IBAN
    /// </summary>
    public string BankIban { get; set; }

    /// <summary>
    /// Gets or sets the bank name
    /// </summary>
    public string BankName { get; set; }

    #endregion

    #region Additional Information

    /// <summary>
    /// Gets or sets the description/additional information from applicant
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the admin notes (internal)
    /// </summary>
    public string AdminNotes { get; set; }

    /// <summary>
    /// Gets or sets the rejection reason
    /// </summary>
    public string RejectionReason { get; set; }

    #endregion

    #region Tracking

    /// <summary>
    /// Gets or sets the IP address of the applicant
    /// </summary>
    public string IpAddress { get; set; }

    /// <summary>
    /// Gets or sets the created vendor identifier (after approval)
    /// </summary>
    public int? CreatedVendorId { get; set; }

    /// <summary>
    /// Gets or sets the date and time of application creation (UTC)
    /// </summary>
    public DateTime CreatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the date and time of last update (UTC)
    /// </summary>
    public DateTime UpdatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the application was reviewed (UTC)
    /// </summary>
    public DateTime? ReviewedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the customer identifier who reviewed the application
    /// </summary>
    public int? ReviewedByCustomerId { get; set; }

    #endregion
}
