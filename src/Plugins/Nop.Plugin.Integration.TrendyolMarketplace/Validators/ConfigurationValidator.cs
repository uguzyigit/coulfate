using FluentValidation;
using Nop.Plugin.Integration.TrendyolMarketplace.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.Integration.TrendyolMarketplace.Validators;

/// <summary>
/// Represents configuration model validator
/// </summary>
public class ConfigurationValidator : BaseNopValidator<ConfigurationModel>
{
    public ConfigurationValidator(ILocalizationService localizationService)
    {
        RuleFor(x => x.ApiBaseUrl)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Integration.TrendyolMarketplace.Fields.ApiBaseUrl.Required"));

        RuleFor(x => x.DefaultProductSyncIntervalMinutes)
            .GreaterThan(0)
            .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Integration.TrendyolMarketplace.Fields.DefaultProductSyncIntervalMinutes.Positive"));

        RuleFor(x => x.DefaultStockPriceSyncIntervalMinutes)
            .GreaterThan(0)
            .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Integration.TrendyolMarketplace.Fields.DefaultStockPriceSyncIntervalMinutes.Positive"));

        RuleFor(x => x.ApiRequestDelayMs)
            .GreaterThanOrEqualTo(0)
            .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Integration.TrendyolMarketplace.Fields.ApiRequestDelayMs.NonNegative"));

        RuleFor(x => x.MaxRetryCount)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(10)
            .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Integration.TrendyolMarketplace.Fields.MaxRetryCount.Range"));

        RuleFor(x => x.ApiPageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(200)
            .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Integration.TrendyolMarketplace.Fields.ApiPageSize.Range"));
    }
}
