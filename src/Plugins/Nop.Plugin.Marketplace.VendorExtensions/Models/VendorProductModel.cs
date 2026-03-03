using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Marketplace.VendorExtensions.Models;

public record VendorProductModel : BaseNopEntityModel, ILocalizedModel<VendorProductLocalizedModel>
{
    public VendorProductModel()
    {
        Locales = new List<VendorProductLocalizedModel>();
        AvailableCategories = new List<SelectListItem>();
        AvailableManufacturers = new List<SelectListItem>();
        AvailableTaxCategories = new List<SelectListItem>();
        AvailableDeliveryDates = new List<SelectListItem>();
        AvailableWarehouses = new List<SelectListItem>();
        AvailableDiscounts = new List<SelectListItem>();
        AvailableManageInventoryMethods = new List<SelectListItem>();
        SelectedDiscountIds = new List<int>();

        ProductPictureSearchModel = new ProductPictureSearchModel();
        ProductVideoSearchModel = new ProductVideoSearchModel();
        ProductAttributeMappingSearchModel = new ProductAttributeMappingSearchModel();
        ProductAttributeCombinationSearchModel = new ProductAttributeCombinationSearchModel();
        ProductSpecificationAttributeSearchModel = new ProductSpecificationAttributeSearchModel();
        TierPriceSearchModel = new TierPriceSearchModel();
        ProductWarehouseInventoryModels = new List<ProductWarehouseInventoryModel>();
    }

    // Basic info
    [NopResourceDisplayName("Admin.Catalog.Products.Fields.Name")]
    [Required(ErrorMessage = "Ürün adı zorunludur.")]
    public string Name { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Products.Fields.ShortDescription")]
    public string ShortDescription { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Products.Fields.FullDescription")]
    public string FullDescription { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Products.Fields.Sku")]
    [Required(ErrorMessage = "Stok kodu zorunludur.")]
    public string Sku { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Products.Fields.Gtin")]
    [Required(ErrorMessage = "GTIN zorunludur.")]
    public string Gtin { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Products.Fields.ManufacturerPartNumber")]
    [Required(ErrorMessage = "Üretici parti numarası zorunludur.")]
    public string ManufacturerPartNumber { get; set; }

    // Category & Manufacturer (single select)
    [Range(1, int.MaxValue, ErrorMessage = "Kategori seçimi zorunludur.")]
    public int SelectedCategoryId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Üretici seçimi zorunludur.")]
    public int SelectedManufacturerId { get; set; }

    // Pricing
    [NopResourceDisplayName("Admin.Catalog.Products.Fields.Price")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Fiyat zorunludur ve 0'dan büyük olmalıdır.")]
    public decimal Price { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Products.Fields.OldPrice")]
    public decimal OldPrice { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Products.Fields.IsTaxExempt")]
    public bool IsTaxExempt { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Products.Fields.TaxCategory")]
    [Range(1, int.MaxValue, ErrorMessage = "Vergi kategorisi seçimi zorunludur.")]
    public int TaxCategoryId { get; set; }

    public string PrimaryStoreCurrencyCode { get; set; }

    // Shipping
    [NopResourceDisplayName("Admin.Catalog.Products.Fields.DeliveryDate")]
    [Range(1, int.MaxValue, ErrorMessage = "Teslimat tarihi seçimi zorunludur.")]
    public int DeliveryDateId { get; set; }

    // Inventory
    [NopResourceDisplayName("Admin.Catalog.Products.Fields.ManageInventoryMethod")]
    public int ManageInventoryMethodId { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Products.Fields.StockQuantity")]
    [Range(1, int.MaxValue, ErrorMessage = "Stok miktarı zorunludur ve 0'dan büyük olmalıdır.")]
    public int StockQuantity { get; set; }

    public int LastStockQuantity { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Products.Fields.Warehouse")]
    public int WarehouseId { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Products.Fields.UseMultipleWarehouses")]
    public bool UseMultipleWarehouses { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Products.Fields.AllowedQuantities")]
    public string AllowedQuantities { get; set; }

    // Discounts
    public IList<int> SelectedDiscountIds { get; set; }

    // Published status (readonly for vendor)
    public bool Published { get; set; }

    // Dropdown lists
    public IList<SelectListItem> AvailableCategories { get; set; }
    public IList<SelectListItem> AvailableManufacturers { get; set; }
    public IList<SelectListItem> AvailableTaxCategories { get; set; }
    public IList<SelectListItem> AvailableDeliveryDates { get; set; }
    public IList<SelectListItem> AvailableWarehouses { get; set; }
    public IList<SelectListItem> AvailableDiscounts { get; set; }
    public IList<SelectListItem> AvailableManageInventoryMethods { get; set; }

    // Localization
    public IList<VendorProductLocalizedModel> Locales { get; set; }

    // Inner search models for edit
    public ProductPictureSearchModel ProductPictureSearchModel { get; set; }
    public ProductVideoSearchModel ProductVideoSearchModel { get; set; }
    public ProductAttributeMappingSearchModel ProductAttributeMappingSearchModel { get; set; }
    public ProductAttributeCombinationSearchModel ProductAttributeCombinationSearchModel { get; set; }
    public ProductSpecificationAttributeSearchModel ProductSpecificationAttributeSearchModel { get; set; }
    public TierPriceSearchModel TierPriceSearchModel { get; set; }
    public IList<ProductWarehouseInventoryModel> ProductWarehouseInventoryModels { get; set; }
}

public partial record VendorProductLocalizedModel : ILocalizedLocaleModel
{
    public int LanguageId { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Products.Fields.Name")]
    public string Name { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Products.Fields.ShortDescription")]
    public string ShortDescription { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Products.Fields.FullDescription")]
    public string FullDescription { get; set; }
}
