using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Integration.TrendyolMarketplace.Infrastructure;

/// <summary>
/// Represents plugin route provider
/// </summary>
public class RouteProvider : IRouteProvider
{
    /// <summary>
    /// Register routes
    /// </summary>
    public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
    {
        #region Admin Routes

        // Admin Configuration
        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.TrendyolMarketplace.Admin.Configure",
            pattern: "Admin/TrendyolAdmin/Configure",
            defaults: new { controller = "TrendyolAdmin", action = "Configure", area = "Admin" });

        // Admin Dashboard
        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.TrendyolMarketplace.Admin.Dashboard",
            pattern: "Admin/TrendyolAdmin/Dashboard",
            defaults: new { controller = "TrendyolAdmin", action = "Dashboard", area = "Admin" });

        // Category Mapping
        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.TrendyolMarketplace.Admin.CategoryMapping",
            pattern: "Admin/TrendyolAdmin/CategoryMapping",
            defaults: new { controller = "TrendyolAdmin", action = "CategoryMapping", area = "Admin" });

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.TrendyolMarketplace.Admin.CategoryMappingList",
            pattern: "Admin/TrendyolAdmin/CategoryMappingList",
            defaults: new { controller = "TrendyolAdmin", action = "CategoryMappingList", area = "Admin" });

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.TrendyolMarketplace.Admin.UpdateCategoryMapping",
            pattern: "Admin/TrendyolAdmin/UpdateCategoryMapping",
            defaults: new { controller = "TrendyolAdmin", action = "UpdateCategoryMapping", area = "Admin" });

        // Brand Mapping
        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.TrendyolMarketplace.Admin.BrandMapping",
            pattern: "Admin/TrendyolAdmin/BrandMapping",
            defaults: new { controller = "TrendyolAdmin", action = "BrandMapping", area = "Admin" });

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.TrendyolMarketplace.Admin.BrandMappingList",
            pattern: "Admin/TrendyolAdmin/BrandMappingList",
            defaults: new { controller = "TrendyolAdmin", action = "BrandMappingList", area = "Admin" });

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.TrendyolMarketplace.Admin.UpdateBrandMapping",
            pattern: "Admin/TrendyolAdmin/UpdateBrandMapping",
            defaults: new { controller = "TrendyolAdmin", action = "UpdateBrandMapping", area = "Admin" });

        // Attribute Mapping
        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.TrendyolMarketplace.Admin.AttributeMapping",
            pattern: "Admin/TrendyolAdmin/AttributeMapping",
            defaults: new { controller = "TrendyolAdmin", action = "AttributeMapping", area = "Admin" });

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.TrendyolMarketplace.Admin.AttributeMappingList",
            pattern: "Admin/TrendyolAdmin/AttributeMappingList",
            defaults: new { controller = "TrendyolAdmin", action = "AttributeMappingList", area = "Admin" });

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.TrendyolMarketplace.Admin.UpdateAttributeMapping",
            pattern: "Admin/TrendyolAdmin/UpdateAttributeMapping",
            defaults: new { controller = "TrendyolAdmin", action = "UpdateAttributeMapping", area = "Admin" });

        // Sync Categories/Brands
        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.TrendyolMarketplace.Admin.SyncCategories",
            pattern: "Admin/TrendyolAdmin/SyncCategories",
            defaults: new { controller = "TrendyolAdmin", action = "SyncCategories", area = "Admin" });

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.TrendyolMarketplace.Admin.SyncBrands",
            pattern: "Admin/TrendyolAdmin/SyncBrands",
            defaults: new { controller = "TrendyolAdmin", action = "SyncBrands", area = "Admin" });

        // Vendor List (Admin view of all vendors)
        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.TrendyolMarketplace.Admin.VendorList",
            pattern: "Admin/TrendyolAdmin/VendorList",
            defaults: new { controller = "TrendyolAdmin", action = "VendorList", area = "Admin" });

        #endregion

        #region Vendor Routes

        // Vendor Credentials
        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.TrendyolMarketplace.Vendor.Credentials",
            pattern: "Admin/TrendyolVendor/Credentials",
            defaults: new { controller = "TrendyolVendor", action = "Credentials", area = "Admin" });

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.TrendyolMarketplace.Vendor.SaveCredentials",
            pattern: "Admin/TrendyolVendor/SaveCredentials",
            defaults: new { controller = "TrendyolVendor", action = "SaveCredentials", area = "Admin" });

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.TrendyolMarketplace.Vendor.TestConnection",
            pattern: "Admin/TrendyolVendor/TestConnection",
            defaults: new { controller = "TrendyolVendor", action = "TestConnection", area = "Admin" });

        // Vendor Dashboard
        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.TrendyolMarketplace.Vendor.Dashboard",
            pattern: "Admin/TrendyolVendor/Dashboard",
            defaults: new { controller = "TrendyolVendor", action = "Dashboard", area = "Admin" });

        // Vendor Products
        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.TrendyolMarketplace.Vendor.Products",
            pattern: "Admin/TrendyolVendor/Products",
            defaults: new { controller = "TrendyolVendor", action = "Products", area = "Admin" });

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.TrendyolMarketplace.Vendor.ProductList",
            pattern: "Admin/TrendyolVendor/ProductList",
            defaults: new { controller = "TrendyolVendor", action = "ProductList", area = "Admin" });

        // Vendor Sync Logs
        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.TrendyolMarketplace.Vendor.SyncLogs",
            pattern: "Admin/TrendyolVendor/SyncLogs",
            defaults: new { controller = "TrendyolVendor", action = "SyncLogs", area = "Admin" });

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.TrendyolMarketplace.Vendor.SyncLogList",
            pattern: "Admin/TrendyolVendor/SyncLogList",
            defaults: new { controller = "TrendyolVendor", action = "SyncLogList", area = "Admin" });

        // Manual Sync
        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.TrendyolMarketplace.Vendor.ManualSync",
            pattern: "Admin/TrendyolVendor/ManualSync",
            defaults: new { controller = "TrendyolVendor", action = "ManualSync", area = "Admin" });

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.TrendyolMarketplace.Vendor.StartProductSync",
            pattern: "Admin/TrendyolVendor/StartProductSync",
            defaults: new { controller = "TrendyolVendor", action = "StartProductSync", area = "Admin" });

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.TrendyolMarketplace.Vendor.StartStockPriceSync",
            pattern: "Admin/TrendyolVendor/StartStockPriceSync",
            defaults: new { controller = "TrendyolVendor", action = "StartStockPriceSync", area = "Admin" });

        #endregion
    }

    /// <summary>
    /// Priority of route provider
    /// </summary>
    public int Priority => 0;
}
