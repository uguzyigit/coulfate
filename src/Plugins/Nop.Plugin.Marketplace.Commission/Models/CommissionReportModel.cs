using System;
using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Marketplace.Commission.Models;

public record CommissionReportModel : BaseNopModel
{
    public CommissionReportModel()
    {
        VendorReports = new List<VendorCommissionReport>();
    }

    [NopResourceDisplayName("Plugins.Marketplace.Commission.Reports.StartDate")]
    public DateTime? StartDate { get; set; }

    [NopResourceDisplayName("Plugins.Marketplace.Commission.Reports.EndDate")]
    public DateTime? EndDate { get; set; }

    [NopResourceDisplayName("Plugins.Marketplace.Commission.Reports.VendorId")]
    public int VendorId { get; set; }

    public string VendorName { get; set; } = string.Empty;

    // Summary
    public decimal TotalRevenue { get; set; }
    public decimal TotalCommission { get; set; }
    public decimal TotalMarketplaceFee { get; set; }
    public decimal TotalTaxWithholding { get; set; }
    public decimal TotalVendorNet { get; set; }
    public int TotalOrders { get; set; }

    // Vendor breakdown
    public IList<VendorCommissionReport> VendorReports { get; set; }
}

public record VendorCommissionReport
{
    public int VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public int OrderCount { get; set; }
    public decimal Revenue { get; set; }
    public decimal Commission { get; set; }
    public decimal MarketplaceFee { get; set; }
    public decimal TaxWithholding { get; set; }
    public decimal VendorNet { get; set; }
    public decimal AverageCommissionRate { get; set; }
}

public record OrderCommissionDetailModel : BaseNopModel
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int OrderItemId { get; set; }
    public int VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal ProductPrice { get; set; }
    public decimal NetPrice { get; set; }
    public decimal CommissionRate { get; set; }
    public decimal CommissionAmount { get; set; }
    public decimal MarketplaceFee { get; set; }
    public decimal TaxWithholding { get; set; }
    public decimal VendorNetAmount { get; set; }
    public DateTime CreatedOnUtc { get; set; }
}
