using System;
using LinqToDB.Mapping;
using Nop.Core;

namespace Nop.Plugin.Marketplace.Core.Domain;

/// <summary>
/// Represents marketplace vendor status enumeration (extends NopCommerce Vendor)
/// </summary>
public enum MarketplaceVendorStatus
{
    Pending = 0,
    Approved = 10,
    Active = 20,
    Rejected = 30,
    Suspended = 40
}

/// <summary>
/// Represents marketplace extension for NopCommerce Vendor
/// Contains additional fields not in the native Vendor entity
/// </summary>
[Table("MarketplaceVendorExtension")]
public partial class VendorExtension : BaseEntity
{
    /// <summary>
    /// Gets or sets the NopCommerce Vendor identifier (foreign key to Vendor table)
    /// </summary>
    [Column] public int VendorId { get; set; }

    /// <summary>
    /// Gets or sets the unique vendor code
    /// </summary>
    [Column] public string Code { get; set; }

    /// <summary>
    /// Gets or sets the marketplace status
    /// </summary>
    [Column] public MarketplaceVendorStatus MarketplaceStatus { get; set; }

    /// <summary>
    /// Gets or sets the phone number
    /// </summary>
    [Column] public string Phone { get; set; }

    /// <summary>
    /// Gets or sets the bank account holder name
    /// </summary>
    [Column] public string BankAccountName { get; set; }

    /// <summary>
    /// Gets or sets the IBAN
    /// </summary>
    [Column] public string BankIban { get; set; }

    /// <summary>
    /// Gets or sets the tax number
    /// </summary>
    [Column] public string TaxNumber { get; set; }

    /// <summary>
    /// Gets or sets the trade registry number
    /// </summary>
    [Column] public string TradeRegistryNumber { get; set; }

    /// <summary>
    /// Gets or sets whether product approval is required
    /// </summary>
    [Column] public bool RequiresProductApproval { get; set; }

    /// <summary>
    /// Gets or sets minimum payout amount
    /// </summary>
    [Column] public decimal MinimumPayoutAmount { get; set; }

    /// <summary>
    /// Gets or sets the date and time of creation (UTC)
    /// </summary>
    [Column] public DateTime CreatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the date and time of last update (UTC)
    /// </summary>
    [Column] public DateTime? UpdatedOnUtc { get; set; }
}

/// <summary>
/// Represents vendor-product relationship
/// </summary>
[Table("MarketplaceVendorProduct")]
public partial class VendorProduct : BaseEntity
{
    [Column] public int VendorId { get; set; }
    [Column] public int ProductId { get; set; }
    [Column] public string VendorSku { get; set; }
    [Column] public decimal VendorPrice { get; set; }
    [Column] public int StockQuantity { get; set; }
    [Column] public bool IsApproved { get; set; }
    [Column] public DateTime CreatedOnUtc { get; set; }
    [Column] public DateTime UpdatedOnUtc { get; set; }
}

/// <summary>
/// Represents a vendor order line
/// </summary>
[Table("MarketplaceVendorOrderLine")]
public partial class VendorOrderLine : BaseEntity
{
    [Column] public int OrderItemId { get; set; }
    [Column] public int OrderId { get; set; }
    [Column] public int VendorId { get; set; }
    [Column] public int ProductId { get; set; }
    [Column] public int Quantity { get; set; }
    [Column] public decimal UnitPrice { get; set; }
    [Column] public decimal CommissionRateSnapshot { get; set; }
    [Column] public decimal CommissionAmountSnapshot { get; set; }
    [Column] public decimal NetAmountSnapshot { get; set; }
    [Column] public DateTime CreatedOnUtc { get; set; }
}

/// <summary>
/// Represents vendor balance
/// </summary>
[Table("MarketplaceVendorBalance")]
public partial class VendorBalance : BaseEntity
{
    [Column] public int VendorId { get; set; }
    [Column] public decimal CurrentBalance { get; set; }
    [Column] public decimal Pending { get; set; }
    [Column] public decimal Hold { get; set; }
    [Column] public DateTime UpdatedOnUtc { get; set; }
}

/// <summary>
/// Represents vendor shipping account
/// </summary>
[Table("MarketplaceVendorShippingAccount")]
public partial class VendorShippingAccount : BaseEntity
{
    [Column] public int VendorId { get; set; }
    [Column] public string Carrier { get; set; }
    [Column] public string ApiKey { get; set; }
    [Column] public bool IsActive { get; set; }
    [Column] public DateTime CreatedOnUtc { get; set; }
}

/// <summary>
/// Represents vendor payout status
/// </summary>
public enum VendorPayoutStatus
{
    Requested = 0,
    Processed = 10,
    Cancelled = 20
}

/// <summary>
/// Represents a vendor payout
/// </summary>
[Table("MarketplaceVendorPayout")]
public partial class VendorPayout : BaseEntity
{
    [Column] public int VendorId { get; set; }
    [Column] public decimal Amount { get; set; }
    [Column] public VendorPayoutStatus Status { get; set; }
    [Column] public DateTime RequestedOnUtc { get; set; }
    [Column] public DateTime? ProcessedOnUtc { get; set; }
    [Column] public string ExternalRef { get; set; }
}

// ============================================================
// COMMISSION ENTITIES
// ============================================================

/// <summary>
/// Represents a category commission rate
/// </summary>
[Table("MarketplaceCategoryCommission")]
public partial class CategoryCommission : BaseEntity
{
    [Column] public int CategoryId { get; set; }
    [Column] public decimal Rate { get; set; }
    [Column] public bool IsActive { get; set; }
    [Column] public DateTime CreatedOnUtc { get; set; }
    [Column] public DateTime? UpdatedOnUtc { get; set; }
    [Column] public string CreatedBy { get; set; }
}

/// <summary>
/// Represents a product-specific commission rate
/// </summary>
[Table("MarketplaceProductCommission")]
public partial class ProductCommission : BaseEntity
{
    [Column] public int ProductId { get; set; }
    [Column] public decimal Rate { get; set; }
    [Column] public bool IsActive { get; set; }
    [Column] public string Reason { get; set; }
    [Column] public DateTime CreatedOnUtc { get; set; }
    [Column] public DateTime? ExpiresOnUtc { get; set; }
    [Column] public string CreatedBy { get; set; }
}

/// <summary>
/// Represents an order commission record
/// </summary>
[Table("MarketplaceOrderCommission")]
public partial class OrderCommission : BaseEntity
{
    [Column] public int OrderId { get; set; }
    [Column] public int OrderItemId { get; set; }
    [Column] public int VendorId { get; set; }
    [Column] public int ProductId { get; set; }
    [Column] public int CategoryId { get; set; }
    [Column] public int Quantity { get; set; }
    [Column] public decimal ProductPrice { get; set; }
    [Column] public decimal DiscountAmount { get; set; }
    [Column] public string DiscountSource { get; set; }
    [Column] public decimal ShippingCost { get; set; }
    [Column] public decimal NetPrice { get; set; }
    [Column] public decimal CommissionRate { get; set; }
    [Column] public decimal CommissionAmount { get; set; }
    [Column] public decimal MarketplaceFee { get; set; }
    [Column] public decimal TaxWithholding { get; set; }
    [Column] public decimal VendorNetAmount { get; set; }
    [Column] public bool IsInvoiced { get; set; }
    [Column] public string InvoiceId { get; set; }
    [Column] public bool IsPaymentApproved { get; set; }
    [Column] public bool IsPaid { get; set; }
    [Column] public DateTime? PaidOnUtc { get; set; }
    [Column] public DateTime CreatedOnUtc { get; set; }
}

// ============================================================
// VENDOR ACCOUNT ENTITIES
// ============================================================

/// <summary>
/// Represents vendor current account (cari hesap)
/// </summary>
[Table("MarketplaceVendorCurrentAccount")]
public partial class VendorCurrentAccount : BaseEntity
{
    [Column] public int VendorId { get; set; }
    [Column] public decimal Balance { get; set; }
    [Column] public decimal TotalCredit { get; set; }
    [Column] public decimal TotalDebit { get; set; }
    [Column] public DateTime LastUpdatedUtc { get; set; }
}

/// <summary>
/// Transaction type enumeration
/// </summary>
public enum VendorTransactionType
{
    OrderRevenue = 1,
    Commission = 2,
    MarketplaceFee = 3,
    TaxWithholding = 4,
    ShippingCost = 5,
    ReturnShippingCost = 6,
    LatePenalty = 7,
    CancellationPenalty = 8,
    ManualAdjustment = 9,
    Refund = 10,
    PaymentReceived = 11,
    VendorDiscount = 12,
    PlatformDiscount = 13
}

/// <summary>
/// Represents a vendor transaction
/// </summary>
[Table("MarketplaceVendorTransaction")]
public partial class VendorTransaction : BaseEntity
{
    [Column] public int VendorId { get; set; }
    [Column] public int? OrderId { get; set; }
    [Column] public int? OrderItemId { get; set; }
    [Column] public VendorTransactionType Type { get; set; }
    [Column] public decimal Amount { get; set; }
    [Column] public decimal BalanceAfter { get; set; }
    [Column] public string Description { get; set; }
    [Column] public string ReferenceNumber { get; set; }
    [Column] public string Metadata { get; set; }
    [Column] public DateTime CreatedOnUtc { get; set; }
}
