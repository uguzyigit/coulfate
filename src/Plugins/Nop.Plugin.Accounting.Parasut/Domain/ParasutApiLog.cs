using System.ComponentModel.DataAnnotations.Schema;
using Nop.Core;

namespace Nop.Plugin.Accounting.Parasut.Domain;

/// <summary>
/// Represents a Paraşüt API call log
/// </summary>
[Table("ParasutApiLog")]
public partial class ParasutApiLog : BaseEntity
{
    /// <summary>
    /// Gets or sets the request type (Auth, CreateContact, CreateInvoice, etc.)
    /// </summary>
    public string RequestType { get; set; }

    /// <summary>
    /// Gets or sets the API endpoint
    /// </summary>
    public string Endpoint { get; set; }

    /// <summary>
    /// Gets or sets the HTTP method
    /// </summary>
    public string Method { get; set; }

    /// <summary>
    /// Gets or sets the request payload (JSON)
    /// </summary>
    public string RequestPayload { get; set; }

    /// <summary>
    /// Gets or sets the response payload (JSON)
    /// </summary>
    public string ResponsePayload { get; set; }

    /// <summary>
    /// Gets or sets the HTTP status code
    /// </summary>
    public int? StatusCode { get; set; }

    /// <summary>
    /// Gets or sets whether the request was successful
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Gets or sets the error message (if any)
    /// </summary>
    public string ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the duration in milliseconds
    /// </summary>
    public int? DurationMs { get; set; }

    /// <summary>
    /// Gets or sets when this log was created (UTC)
    /// </summary>
    public DateTime CreatedOnUtc { get; set; }
}
