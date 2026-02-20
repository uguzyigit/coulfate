using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Marketplace.VendorExtensions.Models;

public record VendorTransactionHistoryModel : BaseNopModel
{
    public VendorTransactionHistoryModel()
    {
        Transactions = new List<TransactionItemModel>();
        AvailableVendors = new List<SelectListItem>();
    }

    public decimal CurrentBalance { get; set; }
    public decimal TotalCredit { get; set; }
    public decimal TotalDebit { get; set; }
    public DateTime? LastTransactionDate { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? TransactionType { get; set; }

    public IList<TransactionItemModel> Transactions { get; set; }
    public IList<SelectListItem> AvailableVendors { get; set; }
    public int TotalRecords { get; set; }
}

public record TransactionItemModel
{
    public int Id { get; set; }
    public int VendorId { get; set; }
    public int Type { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public int TypeId { get; set; }
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ReferenceNumber { get; set; } = string.Empty;
    public int? OrderId { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public string CreatedOnStr { get; set; } = string.Empty;
}
