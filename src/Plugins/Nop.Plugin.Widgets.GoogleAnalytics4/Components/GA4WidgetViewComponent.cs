using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Widgets.GoogleAnalytics4.Domain;
using Nop.Plugin.Widgets.GoogleAnalytics4.Services;
using Nop.Services.Configuration;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Widgets.GoogleAnalytics4.Components;

/// <summary>
/// GA4 widget view component - injects tracking script into page head
/// </summary>
public class GA4WidgetViewComponent : NopViewComponent
{
    private readonly IGA4TrackingService _ga4TrackingService;
    private readonly ISettingService _settingService;
    private readonly IStoreContext _storeContext;

    public GA4WidgetViewComponent(
        IGA4TrackingService ga4TrackingService,
        ISettingService settingService,
        IStoreContext storeContext)
    {
        _ga4TrackingService = ga4TrackingService;
        _settingService = settingService;
        _storeContext = storeContext;
    }

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        var settings = await _settingService.LoadSettingAsync<GA4Settings>(store.Id);

        if (!settings.Enabled || string.IsNullOrEmpty(settings.MeasurementId))
            return Content(string.Empty);

        var script = await _ga4TrackingService.GetGA4ScriptAsync();
        return View("~/Plugins/Widgets.GoogleAnalytics4/Views/Components/GA4Script.cshtml", script);
    }
}
