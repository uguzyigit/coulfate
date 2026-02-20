using System;
using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Marketplace.Commission.Models;

public record ProductCommissionModel : BaseNopEntityModel
{
    [NopResourceDisplayName("Plugins.Marketplace.Commission.Fields.ProductId")]
    public int ProductId { get; set; }

    [NopResourceDisplayName("Plugins.Marketplace.Commission.Fields.ProductName")]
    public string ProductName { get; set; } = string.Empty;

    [NopResourceDisplayName("Plugins.Marketplace.Commission.Fields.CommissionRate")]
    [Range(0, 100)]
    public decimal Rate { get; set; }

    [NopResourceDisplayName("Plugins.Marketplace.Commission.Fields.IsActive")]
    public bool IsActive { get; set; }

    [NopResourceDisplayName("Plugins.Marketplace.Commission.Fields.ExpiresOn")]
    public DateTime? ExpiresOnUtc { get; set; }

    [NopResourceDisplayName("Plugins.Marketplace.Commission.Fields.Reason")]
    public string? Reason { get; set; }

    [NopResourceDisplayName("Plugins.Marketplace.Commission.Fields.CreatedOn")]
    public DateTime CreatedOnUtc { get; set; }
}

public record ProductCommissionListModel : BaseNopModel
{
    public ProductCommissionListModel()
    {
        Data = new List<ProductCommissionModel>();
    }

    public IList<ProductCommissionModel> Data { get; set; }
    public int Total { get; set; }
}

public record ProductCommissionSearchModel : BaseSearchModel
{
    [NopResourceDisplayName("Plugins.Marketplace.Commission.Fields.SearchProductName")]
    public string SearchProductName { get; set; } = string.Empty;

    [NopResourceDisplayName("Plugins.Marketplace.Commission.Fields.SearchVendorId")]
    public int SearchVendorId { get; set; }
}

public record SetProductCommissionModel : BaseNopModel
{
    public int ProductId { get; set; }

    [Required]
    [Range(0, 100)]
    public decimal Rate { get; set; }

    public DateTime? ExpiresOnUtc { get; set; }

    [MaxLength(500)]
    public string? Reason { get; set; }
}
