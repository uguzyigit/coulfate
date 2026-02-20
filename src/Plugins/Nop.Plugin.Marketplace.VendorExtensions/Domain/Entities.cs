using System;
using System.ComponentModel.DataAnnotations.Schema;
using Nop.Core;
using Nop.Core.Domain.Vendors;

namespace Nop.Plugin.Marketplace.VendorExtensions.Domain;

/// <summary>
/// Represents marketplace-specific vendor settings
/// </summary>
[Table("MarketplaceVendorSettings")]
public partial class VendorMarketplaceSettings : BaseEntity
{
    /// <summary>
    /// Gets or sets the vendor identifier
    /// </summary>
    public int VendorId { get; set; }

    /// <summary>
    /// Gets or sets the vendor code (unique identifier)
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// Gets or sets the bank account name
    /// </summary>
    public string BankAccountName { get; set; }

    /// <summary>
    /// Gets or sets the bank IBAN
    /// </summary>
    public string BankIban { get; set; }

    /// <summary>
    /// Gets or sets the commission rate (percentage)
    /// </summary>
    public decimal CommissionRate { get; set; }

    /// <summary>
    /// Gets or sets whether vendor requires approval for products
    /// </summary>
    public bool RequiresProductApproval { get; set; }

    /// <summary>
    /// Gets or sets the minimum payout amount
    /// </summary>
    public decimal MinimumPayoutAmount { get; set; }

    /// <summary>
    /// Gets or sets the date when settings were created
    /// </summary>
    public DateTime CreatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the date when settings were last updated
    /// </summary>
    public DateTime UpdatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the vendor (navigation property)
    /// </summary>
    public virtual Vendor Vendor { get; set; }
}

/// <summary>
/// Represents vendor balance
/// </summary>
[Table("MarketplaceVendorBalance")]
public partial class VendorBalance : BaseEntity
{
    /// <summary>
    /// Gets or sets the vendor identifier
    /// </summary>
    public int VendorId { get; set; }

    /// <summary>
    /// Gets or sets the current balance
    /// </summary>
    public decimal CurrentBalance { get; set; }

    /// <summary>
    /// Gets or sets the pending amount
    /// </summary>
    public decimal PendingAmount { get; set; }

    /// <summary>
    /// Gets or sets the hold amount
    /// </summary>
    public decimal HoldAmount { get; set; }

    /// <summary>
    /// Gets or sets the total earned
    /// </summary>
    public decimal TotalEarned { get; set; }

    /// <summary>
    /// Gets or sets the total withdrawn
    /// </summary>
    public decimal TotalWithdrawn { get; set; }

    /// <summary>
    /// Gets or sets the date when balance was last updated
    /// </summary>
    public DateTime UpdatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the vendor (navigation property)
    /// </summary>
    public virtual Vendor Vendor { get; set; }
}

/// <summary>
/// Represents vendor payout status
/// </summary>
public enum VendorPayoutStatus
{
    /// <summary>
    /// Requested
    /// </summary>
    Requested = 0,

    /// <summary>
    /// Approved
    /// </summary>
    Approved = 10,

    /// <summary>
    /// Processing
    /// </summary>
    Processing = 20,

    /// <summary>
    /// Completed
    /// </summary>
    Completed = 30,

    /// <summary>
    /// Cancelled
    /// </summary>
    Cancelled = 40,

    /// <summary>
    /// Failed
    /// </summary>
    Failed = 50
}

/// <summary>
/// Represents a vendor payout request
/// </summary>
[Table("MarketplaceVendorPayout")]
public partial class VendorPayout : BaseEntity
{
    /// <summary>
    /// Gets or sets the vendor identifier
    /// </summary>
    public int VendorId { get; set; }

    /// <summary>
    /// Gets or sets the payout amount
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the payout status
    /// </summary>
    public VendorPayoutStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the payment method (Bank Transfer, PayPal, etc.)
    /// </summary>
    public string PaymentMethod { get; set; }

    /// <summary>
    /// Gets or sets the external reference (transaction ID, etc.)
    /// </summary>
    public string ExternalReference { get; set; }

    /// <summary>
    /// Gets or sets admin notes
    /// </summary>
    public string AdminNotes { get; set; }

    /// <summary>
    /// Gets or sets the date when payout was requested
    /// </summary>
    public DateTime RequestedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the date when payout was approved
    /// </summary>
    public DateTime? ApprovedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the date when payout was processed
    /// </summary>
    public DateTime? ProcessedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the vendor (navigation property)
    /// </summary>
    public virtual Vendor Vendor { get; set; }
}

/// <summary>
/// Represents vendor order commission tracking
/// </summary>
[Table("MarketplaceVendorOrderCommission")]
public partial class VendorOrderCommission : BaseEntity
{
    /// <summary>
    /// Gets or sets the vendor identifier
    /// </summary>
    public int VendorId { get; set; }

    /// <summary>
    /// Gets or sets the order identifier
    /// </summary>
    public int OrderId { get; set; }

    /// <summary>
    /// Gets or sets the order item identifier
    /// </summary>
    public int OrderItemId { get; set; }

    /// <summary>
    /// Gets or sets the product identifier
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Gets or sets the quantity
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Gets or sets the unit price
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Gets or sets the total price (unit price * quantity)
    /// </summary>
    public decimal TotalPrice { get; set; }

    /// <summary>
    /// Gets or sets the commission rate at time of order
    /// </summary>
    public decimal CommissionRate { get; set; }

    /// <summary>
    /// Gets or sets the commission amount
    /// </summary>
    public decimal CommissionAmount { get; set; }

    /// <summary>
    /// Gets or sets the vendor net amount (total - commission)
    /// </summary>
    public decimal VendorNetAmount { get; set; }

    /// <summary>
    /// Gets or sets whether commission has been paid to vendor
    /// </summary>
    public bool IsPaid { get; set; }

    /// <summary>
    /// Gets or sets the payout identifier (if paid)
    /// </summary>
    public int? PayoutId { get; set; }

    /// <summary>
    /// Gets or sets the date when commission was created
    /// </summary>
    public DateTime CreatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the date when commission was paid
    /// </summary>
    public DateTime? PaidOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the vendor (navigation property)
    /// </summary>
    public virtual Vendor Vendor { get; set; }

    /// <summary>
    /// Gets or sets the payout (navigation property)
    /// </summary>
    public virtual VendorPayout Payout { get; set; }
}
