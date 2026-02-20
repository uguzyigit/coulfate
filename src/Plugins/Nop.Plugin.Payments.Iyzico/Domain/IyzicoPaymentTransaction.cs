using System.ComponentModel.DataAnnotations.Schema;
using Nop.Core;

namespace Nop.Plugin.Payments.Iyzico.Domain;

/// <summary>
/// Represents an iyzico payment transaction
/// </summary>
[Table("IyzicoPaymentTransaction")]
public partial class IyzicoPaymentTransaction : BaseEntity
{
    /// <summary>
    /// Gets or sets the order identifier
    /// </summary>
    public int OrderId { get; set; }

    /// <summary>
    /// Gets or sets the order item identifier
    /// </summary>
    public int OrderItemId { get; set; }

    /// <summary>
    /// Gets or sets the payment transaction ID from iyzico
    /// </summary>
    public string PaymentTransactionId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the conversation ID
    /// </summary>
    public string ConversationId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the payment ID from iyzico
    /// </summary>
    public string? PaymentId { get; set; }

    /// <summary>
    /// Gets or sets the submerchant key
    /// </summary>
    public string SubMerchantKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the submerchant price (amount sent to vendor)
    /// </summary>
    public decimal SubMerchantPrice { get; set; }

    /// <summary>
    /// Gets or sets the withholding tax amount
    /// </summary>
    public decimal WithholdingTax { get; set; }

    /// <summary>
    /// Gets or sets the vendor ID
    /// </summary>
    public int VendorId { get; set; }

    /// <summary>
    /// Gets or sets the product ID
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Gets or sets the transaction status
    /// </summary>
    public string Status { get; set; } = "Pending";

    /// <summary>
    /// Gets or sets a value indicating whether this transaction is approved
    /// </summary>
    public bool IsApproved { get; set; }

    /// <summary>
    /// Gets or sets the approved date
    /// </summary>
    public DateTime? ApprovedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this transaction is disapproved (refunded)
    /// </summary>
    public bool IsDisapproved { get; set; }

    /// <summary>
    /// Gets or sets the disapproved date
    /// </summary>
    public DateTime? DisapprovedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the disapproval reason
    /// </summary>
    public string? DisapprovalReason { get; set; }

    /// <summary>
    /// Gets or sets the created date
    /// </summary>
    public DateTime CreatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the raw response from iyzico API
    /// </summary>
    public string? RawResponse { get; set; }
}
