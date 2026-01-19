using Nop.Core;
using Nop.Plugin.Marketplace.VendorApplication.Domain;

namespace Nop.Plugin.Marketplace.VendorApplication.Services;

/// <summary>
/// Vendor application service interface
/// </summary>
public interface IVendorApplicationService
{
    #region Applications

    /// <summary>
    /// Gets an application by identifier
    /// </summary>
    Task<Domain.VendorApplication> GetApplicationByIdAsync(int id);

    /// <summary>
    /// Gets an application by application number
    /// </summary>
    Task<Domain.VendorApplication> GetApplicationByNumberAsync(string applicationNumber);

    /// <summary>
    /// Gets an application by email
    /// </summary>
    Task<Domain.VendorApplication> GetApplicationByEmailAsync(string email);

    /// <summary>
    /// Gets all applications
    /// </summary>
    Task<IPagedList<Domain.VendorApplication>> GetAllApplicationsAsync(
        VendorApplicationStatus? status = null,
        string searchTerm = null,
        DateTime? createdFrom = null,
        DateTime? createdTo = null,
        int pageIndex = 0,
        int pageSize = int.MaxValue);

    /// <summary>
    /// Inserts an application
    /// </summary>
    Task InsertApplicationAsync(Domain.VendorApplication application);

    /// <summary>
    /// Updates an application
    /// </summary>
    Task UpdateApplicationAsync(Domain.VendorApplication application);

    /// <summary>
    /// Deletes an application
    /// </summary>
    Task DeleteApplicationAsync(Domain.VendorApplication application);

    /// <summary>
    /// Approves an application and creates a vendor
    /// </summary>
    Task<(bool Success, int? VendorId, string ErrorMessage)> ApproveApplicationAsync(int applicationId, int reviewedByCustomerId);

    /// <summary>
    /// Rejects an application
    /// </summary>
    Task<bool> RejectApplicationAsync(int applicationId, string reason, int reviewedByCustomerId);

    /// <summary>
    /// Changes application status
    /// </summary>
    Task ChangeStatusAsync(int applicationId, VendorApplicationStatus newStatus);

    /// <summary>
    /// Generates a unique application number
    /// </summary>
    Task<string> GenerateApplicationNumberAsync();

    #endregion

    #region Document Types

    /// <summary>
    /// Gets a document type by identifier
    /// </summary>
    Task<VendorDocumentType> GetDocumentTypeByIdAsync(int id);

    /// <summary>
    /// Gets all document types
    /// </summary>
    Task<IList<VendorDocumentType>> GetAllDocumentTypesAsync(bool activeOnly = true);

    /// <summary>
    /// Inserts a document type
    /// </summary>
    Task InsertDocumentTypeAsync(VendorDocumentType documentType);

    /// <summary>
    /// Updates a document type
    /// </summary>
    Task UpdateDocumentTypeAsync(VendorDocumentType documentType);

    /// <summary>
    /// Deletes a document type
    /// </summary>
    Task DeleteDocumentTypeAsync(VendorDocumentType documentType);

    #endregion

    #region Application Documents

    /// <summary>
    /// Gets a document by identifier
    /// </summary>
    Task<VendorApplicationDocument> GetDocumentByIdAsync(int id);

    /// <summary>
    /// Gets documents by application identifier
    /// </summary>
    Task<IList<VendorApplicationDocument>> GetDocumentsByApplicationIdAsync(int applicationId);

    /// <summary>
    /// Inserts a document
    /// </summary>
    Task InsertDocumentAsync(VendorApplicationDocument document);

    /// <summary>
    /// Deletes a document
    /// </summary>
    Task DeleteDocumentAsync(VendorApplicationDocument document);

    /// <summary>
    /// Deletes all documents for an application
    /// </summary>
    Task DeleteDocumentsByApplicationIdAsync(int applicationId);

    #endregion
}
