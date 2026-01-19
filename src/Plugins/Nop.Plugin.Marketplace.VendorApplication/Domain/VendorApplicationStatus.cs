namespace Nop.Plugin.Marketplace.VendorApplication.Domain;

/// <summary>
/// Represents vendor application status
/// </summary>
public enum VendorApplicationStatus
{
    /// <summary>
    /// Pending - Initial state, awaiting review
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Under Review - Application is being reviewed by admin
    /// </summary>
    UnderReview = 10,

    /// <summary>
    /// Documents Requested - Additional documents requested from applicant
    /// </summary>
    DocumentsRequested = 20,

    /// <summary>
    /// Approved - Application approved, vendor account created
    /// </summary>
    Approved = 30,

    /// <summary>
    /// Rejected - Application rejected
    /// </summary>
    Rejected = 40,

    /// <summary>
    /// Cancelled - Application cancelled by applicant
    /// </summary>
    Cancelled = 50
}
