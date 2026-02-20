using System;
using System.ComponentModel.DataAnnotations.Schema;
using Nop.Core;
using Nop.Core.Configuration;

namespace Nop.Plugin.Marketplace.Commission.Domain;

[Table("MarketplaceCategoryCommission")]
public partial class CategoryCommission : BaseEntity
{
    public int CategoryId { get; set; }
    public decimal Rate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }
    public string? CreatedBy { get; set; }
}

[Table("MarketplaceProductCommission")]
public partial class ProductCommission : BaseEntity
{
    public int ProductId { get; set; }
    public decimal Rate { get; set; }
    public bool IsActive { get; set; }
    public string? Reason { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public DateTime? ExpiresOnUtc { get; set; }
    public string? CreatedBy { get; set; }
}

[Table("MarketplaceOrderCommission")]
public partial class OrderCommission : BaseEntity
{
    public int OrderId { get; set; }
    public int OrderItemId { get; set; }
    public int VendorId { get; set; }
    public int ProductId { get; set; }
    public int CategoryId { get; set; }
    public int Quantity { get; set; }
    public decimal ProductPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public string? DiscountSource { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal NetPrice { get; set; }
    public decimal CommissionRate { get; set; }
    public decimal CommissionAmount { get; set; }
    public decimal MarketplaceFee { get; set; }
    public decimal TaxWithholding { get; set; }
    public decimal VendorNetAmount { get; set; }
    public bool IsInvoiced { get; set; }
    public string? InvoiceId { get; set; }
    public bool IsPaymentApproved { get; set; }
    public bool IsPaid { get; set; }
    public DateTime? PaidOnUtc { get; set; }
    public DateTime CreatedOnUtc { get; set; }
}

public class CommissionSettings : ISettings
{
    public decimal DefaultCommissionRate { get; set; } = 10.0m;
    public decimal MarketplaceFeePerOrder { get; set; } = 5.0m;
    public decimal TaxWithholdingRate { get; set; } = 0.01m;
    public bool CalculateOnDiscountedPrice { get; set; } = true;
    public bool IncludeShippingInCommission { get; set; } = false;
}
