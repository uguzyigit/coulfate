using Nop.Plugin.Integration.TrendyolMarketplace.Domain;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Integration.TrendyolMarketplace.Models;

/// <summary>
/// Represents the Trendyol product list item model
/// </summary>
public record TrendyolProductModel : BaseNopEntityModel
{
    [NopResourceDisplayName("Plugins.Integration.TrendyolMarketplace.Products.Barcode")]
    public string TrendyolBarcode { get; set; }

    [NopResourceDisplayName("Plugins.Integration.TrendyolMarketplace.Products.ProductCode")]
    public string TrendyolProductCode { get; set; }

    public string TrendyolStockCode { get; set; }

    [NopResourceDisplayName("Plugins.Integration.TrendyolMarketplace.Products.Title.Column")]
    public string TrendyolTitle { get; set; }

    public long? TrendyolCategoryId { get; set; }

    public string TrendyolCategoryName { get; set; }

    public long? TrendyolBrandId { get; set; }

    public string TrendyolBrandName { get; set; }

    [NopResourceDisplayName("Plugins.Integration.TrendyolMarketplace.Products.NopProductId")]
    public int? NopProductId { get; set; }

    public string NopProductName { get; set; }

    public int VendorId { get; set; }

    public string VendorName { get; set; }

    public bool IsVariant { get; set; }

    [NopResourceDisplayName("Plugins.Integration.TrendyolMarketplace.Products.TrendyolPrice")]
    public decimal? LastTrendyolPrice { get; set; }

    [NopResourceDisplayName("Plugins.Integration.TrendyolMarketplace.Products.TrendyolStock")]
    public int? LastTrendyolStock { get; set; }

    [NopResourceDisplayName("Plugins.Integration.TrendyolMarketplace.Products.LastSyncOnUtc")]
    public DateTime? LastSyncOnUtc { get; set; }

    public string LastSyncOnDisplay { get; set; }

    [NopResourceDisplayName("Plugins.Integration.TrendyolMarketplace.Products.ImportStatus")]
    public ImportStatus ImportStatus { get; set; }

    public string ImportStatusDisplay { get; set; }

    public string ImportMessage { get; set; }

    [NopResourceDisplayName("Plugins.Integration.TrendyolMarketplace.Products.TrendyolOnSale")]
    public bool TrendyolOnSale { get; set; }
}

/// <summary>
/// Represents Trendyol product list model
/// </summary>
public record TrendyolProductListModel : BasePagedListModel<TrendyolProductModel>
{
}

/// <summary>
/// Represents Trendyol product search model
/// </summary>
public record TrendyolProductSearchModel : BaseSearchModel
{
    public int? VendorId { get; set; }

    public string SearchTerm { get; set; }

    public ImportStatus? ImportStatus { get; set; }
}
