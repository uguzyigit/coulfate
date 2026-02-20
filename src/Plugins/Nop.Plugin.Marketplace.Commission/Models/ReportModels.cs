using System;

namespace Nop.Plugin.Marketplace.Commission.Models;

public record CommissionReportSummary
{
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalCommission { get; set; }
    public decimal TotalMarketplaceFee { get; set; }
    public decimal TotalTaxWithholding { get; set; }
    public decimal TotalVendorPayout { get; set; }
    public decimal AverageCommissionRate { get; set; }
}

public record VendorCommissionSummary
{
    public int VendorId { get; set; }
    public string VendorName { get; set; }
    public int OrderCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalCommission { get; set; }
    public decimal NetPayout { get; set; }
}

public record ProductCommissionSummary
{
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public int OrderCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalCommission { get; set; }
    public decimal CommissionRate { get; set; }
}

// NEW: ReportSummary (used by controller)
public record ReportSummary
{
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalCommission { get; set; }
    public decimal TotalFee { get; set; }
    public decimal TotalTax { get; set; }
    public decimal TotalVendorNet { get; set; }
}

// NEW: CommissionDetailModel (used by controller)
public record CommissionDetailModel
{
    public int OrderId { get; set; }
    public int VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal NetPrice { get; set; }
    public decimal CommissionRate { get; set; }
    public decimal CommissionAmount { get; set; }
    public decimal MarketplaceFee { get; set; }
    public decimal TaxWithholding { get; set; }
    public decimal VendorNetAmount { get; set; }
    public DateTime CreatedOnUtc { get; set; }
}
