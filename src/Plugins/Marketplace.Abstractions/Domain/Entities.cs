using LinqToDB.Mapping;
using Nop.Core;

namespace Marketplace.Abstractions.Domain;

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
    [Column] public int VendorId { get; set; }
    [Column] public string Code { get; set; }
    [Column] public MarketplaceVendorStatus MarketplaceStatus { get; set; }
    [Column] public string Phone { get; set; }
    [Column] public string BankAccountName { get; set; }
    [Column] public string BankIban { get; set; }
    [Column] public string TaxNumber { get; set; }
    [Column] public string TradeRegistryNumber { get; set; }
    [Column] public bool RequiresProductApproval { get; set; }
    [Column] public decimal MinimumPayoutAmount { get; set; }
    [Column] public DateTime CreatedOnUtc { get; set; }
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

[Table("MarketplaceVendorCurrentAccount")]
public partial class VendorCurrentAccount : BaseEntity
{
    [Column] public int VendorId { get; set; }
    [Column] public decimal Balance { get; set; }
    [Column] public decimal TotalCredit { get; set; }
    [Column] public decimal TotalDebit { get; set; }
    [Column] public DateTime LastUpdatedUtc { get; set; }
}

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

// ============================================================
// PERFORMANCE TRACKING ENTITIES
// ============================================================

public enum ProductInteractionType
{
    View = 1,
    AddToCart = 2,
    Wishlist = 3
}

[Table("MarketplaceProductInteraction")]
public partial class ProductInteraction : BaseEntity
{
    [Column] public int ProductId { get; set; }
    [Column] public int VendorId { get; set; }
    [Column] public int? CustomerId { get; set; }
    [Column] public int InteractionType { get; set; }
    [Column] public DateTime CreatedOnUtc { get; set; }
}

[Table("MarketplaceProductPerformanceSnapshot")]
public partial class ProductPerformanceSnapshot : BaseEntity
{
    [Column] public int ProductId { get; set; }
    [Column] public int VendorId { get; set; }

    // Interaction metrics (30 days)
    [Column] public int Views30d { get; set; }
    [Column] public int AddToCart30d { get; set; }
    [Column] public int Wishlist30d { get; set; }

    // Sales metrics (30 days gross)
    [Column] public int GrossOrders30d { get; set; }
    [Column] public int GrossQty30d { get; set; }
    [Column] public decimal GrossRevenue30d { get; set; }
    [Column] public decimal DiscountAmount30d { get; set; }

    // Sales metrics (7 days for trend)
    [Column] public decimal Revenue7d { get; set; }
    [Column] public int Orders7d { get; set; }

    // Cancel & return metrics (30 days)
    [Column] public int CancelQty30d { get; set; }
    [Column] public int ReturnQty30d { get; set; }
    [Column] public decimal CancelRevenue30d { get; set; }
    [Column] public decimal ReturnRevenue30d { get; set; }

    // Net metrics
    [Column] public int NetQty30d { get; set; }
    [Column] public decimal NetRevenue30d { get; set; }

    // Score components
    [Column] public decimal SalesScore { get; set; }
    [Column] public decimal TrendScore { get; set; }
    [Column] public decimal ConversionScore { get; set; }
    [Column] public decimal QualityPenalty { get; set; }
    [Column] public decimal NewProductBoost { get; set; }
    [Column] public decimal FinalScore { get; set; }

    // Review count (total approved reviews)
    [Column] public int ReviewCount { get; set; }

    [Column] public DateTime CalculatedOnUtc { get; set; }
}

// ============================================================
// SHIPPING ENTITIES
// ============================================================

[Table("MarketplaceShippingProvider")]
public partial class ShippingProvider : BaseEntity
{
    [Column] public string Name { get; set; }
    [Column] public string SystemName { get; set; }
    [Column] public bool SupportsMarketplaceContract { get; set; }
    [Column] public bool SupportsVendorContract { get; set; }
    [Column] public string LogoUrl { get; set; }
    [Column] public int DisplayOrder { get; set; }
    [Column] public bool IsActive { get; set; }
}

[Table("MarketplaceVendorShippingPreference")]
public partial class VendorShippingPreference : BaseEntity
{
    [Column] public int VendorId { get; set; }
    [Column] public int ShippingProviderId { get; set; }
    [Column] public bool UseMarketplaceContract { get; set; }
    [Column] public bool IsActive { get; set; }
    [Column] public DateTime CreatedOnUtc { get; set; }
    [Column] public DateTime? UpdatedOnUtc { get; set; }
}

[Table("MarketplaceVendorShippingCredential")]
public partial class VendorShippingCredential : BaseEntity
{
    [Column] public int VendorId { get; set; }
    [Column] public int ShippingProviderId { get; set; }
    [Column] public string CredentialKey { get; set; }
    [Column] public string CredentialValue { get; set; }
    [Column] public DateTime CreatedOnUtc { get; set; }
}

[Table("MarketplaceShipment")]
public partial class MarketplaceShipment : BaseEntity
{
    [Column] public int NopShipmentId { get; set; }
    [Column] public int VendorId { get; set; }
    [Column] public int ShippingProviderId { get; set; }
    [Column] public string TrackingNumber { get; set; }
    [Column] public string BarcodeNumber { get; set; }
    [Column] public string ExternalStatus { get; set; }
    [Column] public DateTime? LastStatusUpdate { get; set; }
    [Column] public string RawResponse { get; set; }
    [Column] public bool IsReturn { get; set; }
    [Column] public DateTime CreatedOnUtc { get; set; }
}
