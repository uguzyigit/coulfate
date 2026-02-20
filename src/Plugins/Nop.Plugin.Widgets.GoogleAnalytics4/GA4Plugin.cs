using Nop.Core.Domain.Cms;
using Nop.Plugin.Widgets.GoogleAnalytics4.Components;
using Nop.Plugin.Widgets.GoogleAnalytics4.Domain;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Widgets.GoogleAnalytics4;

/// <summary>
/// Google Analytics 4 Enhanced plugin
/// </summary>
public class GA4Plugin : BasePlugin, IWidgetPlugin
{
    private readonly ILocalizationService _localizationService;
    private readonly INopUrlHelper _nopUrlHelper;
    private readonly ISettingService _settingService;
    private readonly WidgetSettings _widgetSettings;

    public GA4Plugin(
        ILocalizationService localizationService,
        INopUrlHelper nopUrlHelper,
        ISettingService settingService,
        WidgetSettings widgetSettings)
    {
        _localizationService = localizationService;
        _nopUrlHelper = nopUrlHelper;
        _settingService = settingService;
        _widgetSettings = widgetSettings;
    }

    /// <summary>
    /// Gets widget zones where this widget should be rendered
    /// </summary>
    public Task<IList<string>> GetWidgetZonesAsync()
    {
        return Task.FromResult<IList<string>>(new List<string>
        {
            PublicWidgetZones.HeadHtmlTag,
            PublicWidgetZones.ProductDetailsTop,
            PublicWidgetZones.CategoryDetailsTop,
            PublicWidgetZones.ProductSearchPageBeforeResults,
            PublicWidgetZones.OrderSummaryContentBefore,
            PublicWidgetZones.OpcContentBefore
        });
    }

    /// <summary>
    /// Gets a configuration page URL
    /// </summary>
    public override string GetConfigurationPageUrl()
    {
        return _nopUrlHelper.RouteUrl(GA4Defaults.ConfigurationRouteName);
    }

    /// <summary>
    /// Gets a type of a view component for displaying widget
    /// </summary>
    public Type GetWidgetViewComponent(string widgetZone)
    {
        // Base script in head
        if (widgetZone.Equals(PublicWidgetZones.HeadHtmlTag))
            return typeof(GA4WidgetViewComponent);

        // Product detail page - view_item event
        if (widgetZone.Equals(PublicWidgetZones.ProductDetailsTop))
            return typeof(GA4ProductViewComponent);

        // Category page - view_item_list event
        if (widgetZone.Equals(PublicWidgetZones.CategoryDetailsTop))
            return typeof(GA4CategoryViewComponent);

        // Search results page - search event
        if (widgetZone.Equals(PublicWidgetZones.ProductSearchPageBeforeResults))
            return typeof(GA4SearchViewComponent);

        // Cart page - view_cart event
        if (widgetZone.Equals(PublicWidgetZones.OrderSummaryContentBefore))
            return typeof(GA4CartViewComponent);

        // Checkout page - begin_checkout event
        if (widgetZone.Equals(PublicWidgetZones.OpcContentBefore))
            return typeof(GA4CartViewComponent);

        return null;
    }

    /// <summary>
    /// Install plugin
    /// </summary>
    public override async Task InstallAsync()
    {
        // Default settings
        var settings = new GA4Settings
        {
            MeasurementId = "G-XXXXXXXXXX",
            ApiSecret = string.Empty,
            Enabled = true,
            EnableEcommerce = true,
            IncludeTax = true,
            IncludeCustomerId = true,
            EnableDebugMode = false,
            TrackViewItemList = true,
            TrackViewItem = true,
            TrackAddToCart = true,
            TrackRemoveFromCart = true,
            TrackViewCart = true,
            TrackBeginCheckout = true,
            TrackAddShippingInfo = true,
            TrackAddPaymentInfo = true,
            TrackPurchase = true,
            TrackRefund = true,
            TrackSearch = true
        };
        await _settingService.SaveSettingAsync(settings);

        // Add widget to active widgets
        if (!_widgetSettings.ActiveWidgetSystemNames.Contains(GA4Defaults.SystemName))
        {
            _widgetSettings.ActiveWidgetSystemNames.Add(GA4Defaults.SystemName);
            await _settingService.SaveSettingAsync(_widgetSettings);
        }

        // Localization resources
        await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
        {
            // Turkish
            ["Plugins.Widgets.GoogleAnalytics4.MeasurementId"] = "Measurement ID",
            ["Plugins.Widgets.GoogleAnalytics4.MeasurementId.Hint"] = "Google Analytics 4 Measurement ID (G-XXXXXXXXXX formatinda)",
            ["Plugins.Widgets.GoogleAnalytics4.ApiSecret"] = "API Secret",
            ["Plugins.Widgets.GoogleAnalytics4.ApiSecret.Hint"] = "Measurement Protocol API Secret (sunucu tarafi izleme icin gerekli)",
            ["Plugins.Widgets.GoogleAnalytics4.Enabled"] = "Aktif",
            ["Plugins.Widgets.GoogleAnalytics4.Enabled.Hint"] = "Google Analytics 4 izlemeyi aktif et",
            ["Plugins.Widgets.GoogleAnalytics4.EnableEcommerce"] = "E-ticaret Izleme",
            ["Plugins.Widgets.GoogleAnalytics4.EnableEcommerce.Hint"] = "E-ticaret eventlerini izle (purchase, refund, add_to_cart vb.)",
            ["Plugins.Widgets.GoogleAnalytics4.IncludeTax"] = "Vergi Dahil",
            ["Plugins.Widgets.GoogleAnalytics4.IncludeTax.Hint"] = "Fiyatlara vergi dahil edilsin mi?",
            ["Plugins.Widgets.GoogleAnalytics4.IncludeCustomerId"] = "Musteri ID Ekle",
            ["Plugins.Widgets.GoogleAnalytics4.IncludeCustomerId.Hint"] = "Izleme koduna musteri ID ekle (cross-device tracking icin)",
            ["Plugins.Widgets.GoogleAnalytics4.EnableDebugMode"] = "Debug Modu",
            ["Plugins.Widgets.GoogleAnalytics4.EnableDebugMode.Hint"] = "Tarayici konsolunda GA4 eventlerini logla",

            // Event tracking options
            ["Plugins.Widgets.GoogleAnalytics4.TrackViewItemList"] = "Urun Listesi (view_item_list)",
            ["Plugins.Widgets.GoogleAnalytics4.TrackViewItemList.Hint"] = "Kategori ve arama sayfalarinda urun listesi goruntulemelerini izle",
            ["Plugins.Widgets.GoogleAnalytics4.TrackViewItem"] = "Urun Detay (view_item)",
            ["Plugins.Widgets.GoogleAnalytics4.TrackViewItem.Hint"] = "Urun detay sayfasi goruntulemelerini izle",
            ["Plugins.Widgets.GoogleAnalytics4.TrackAddToCart"] = "Sepete Ekle (add_to_cart)",
            ["Plugins.Widgets.GoogleAnalytics4.TrackAddToCart.Hint"] = "Sepete ekleme islemlerini izle",
            ["Plugins.Widgets.GoogleAnalytics4.TrackRemoveFromCart"] = "Sepetten Cikar (remove_from_cart)",
            ["Plugins.Widgets.GoogleAnalytics4.TrackRemoveFromCart.Hint"] = "Sepetten cikarma islemlerini izle",
            ["Plugins.Widgets.GoogleAnalytics4.TrackViewCart"] = "Sepet Goruntule (view_cart)",
            ["Plugins.Widgets.GoogleAnalytics4.TrackViewCart.Hint"] = "Sepet sayfasi goruntulemelerini izle",
            ["Plugins.Widgets.GoogleAnalytics4.TrackBeginCheckout"] = "Odeme Baslat (begin_checkout)",
            ["Plugins.Widgets.GoogleAnalytics4.TrackBeginCheckout.Hint"] = "Odeme sayfasina gecisi izle",
            ["Plugins.Widgets.GoogleAnalytics4.TrackAddShippingInfo"] = "Kargo Bilgisi (add_shipping_info)",
            ["Plugins.Widgets.GoogleAnalytics4.TrackAddShippingInfo.Hint"] = "Kargo yontemi secimini izle",
            ["Plugins.Widgets.GoogleAnalytics4.TrackAddPaymentInfo"] = "Odeme Bilgisi (add_payment_info)",
            ["Plugins.Widgets.GoogleAnalytics4.TrackAddPaymentInfo.Hint"] = "Odeme yontemi secimini izle",
            ["Plugins.Widgets.GoogleAnalytics4.TrackPurchase"] = "Satin Alma (purchase)",
            ["Plugins.Widgets.GoogleAnalytics4.TrackPurchase.Hint"] = "Tamamlanan siparisleri izle (sunucu tarafi)",
            ["Plugins.Widgets.GoogleAnalytics4.TrackRefund"] = "Iade (refund)",
            ["Plugins.Widgets.GoogleAnalytics4.TrackRefund.Hint"] = "Iade islemlerini izle (sunucu tarafi)",
            ["Plugins.Widgets.GoogleAnalytics4.TrackSearch"] = "Arama (search)",
            ["Plugins.Widgets.GoogleAnalytics4.TrackSearch.Hint"] = "Site ici arama islemlerini izle",

            // Instructions
            ["Plugins.Widgets.GoogleAnalytics4.Instructions"] = @"<div class='alert alert-info'>
                <h5>Google Analytics 4 Enhanced E-commerce</h5>
                <p>Bu plugin, GA4 Enhanced E-commerce izlemesi saglar. Kurulum icin:</p>
                <ol>
                    <li><a href='https://analytics.google.com/' target='_blank'>Google Analytics</a>'e gidin</li>
                    <li>Admin > Data Streams > Web stream'inizi secin</li>
                    <li><b>Measurement ID</b>'yi kopyalayin (G-XXXXXXXXXX)</li>
                    <li>Ayni sayfada <b>Measurement Protocol API secrets</b>'a tiklayin</li>
                    <li>Yeni bir API Secret olusturun ve kopyalayin</li>
                    <li>Her iki degeri asagiya girin ve kaydedin</li>
                </ol>
                <p><b>Not:</b> Debug modunu acarak tarayici konsolunda (F12) eventleri gorebilirsiniz.</p>
            </div>",

            ["Plugins.Widgets.GoogleAnalytics4.BasicSettings"] = "Temel Ayarlar",
            ["Plugins.Widgets.GoogleAnalytics4.EventSettings"] = "Event Ayarlari",
            ["Plugins.Widgets.GoogleAnalytics4.ClientSideEvents"] = "Client-Side Eventler (JavaScript)",
            ["Plugins.Widgets.GoogleAnalytics4.ServerSideEvents"] = "Server-Side Eventler (Measurement Protocol)"
        });

        await base.InstallAsync();
    }

    /// <summary>
    /// Uninstall plugin
    /// </summary>
    public override async Task UninstallAsync()
    {
        // Remove from active widgets
        if (_widgetSettings.ActiveWidgetSystemNames.Contains(GA4Defaults.SystemName))
        {
            _widgetSettings.ActiveWidgetSystemNames.Remove(GA4Defaults.SystemName);
            await _settingService.SaveSettingAsync(_widgetSettings);
        }

        // Remove settings
        await _settingService.DeleteSettingAsync<GA4Settings>();

        // Remove localization
        await _localizationService.DeleteLocaleResourcesAsync("Plugins.Widgets.GoogleAnalytics4");

        await base.UninstallAsync();
    }

    /// <summary>
    /// Gets a value indicating whether to hide this plugin on the widget list page in the admin area
    /// </summary>
    public bool HideInWidgetList => false;
}
