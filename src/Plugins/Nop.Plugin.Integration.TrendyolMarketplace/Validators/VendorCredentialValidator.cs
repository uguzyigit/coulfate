using FluentValidation;
using Nop.Plugin.Integration.TrendyolMarketplace.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.Integration.TrendyolMarketplace.Validators;

/// <summary>
/// Represents vendor credential model validator
/// </summary>
public class VendorCredentialValidator : BaseNopValidator<VendorCredentialModel>
{
    public VendorCredentialValidator(ILocalizationService localizationService)
    {
        RuleFor(x => x.TrendyolSupplierId)
            .GreaterThan(0)
            .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Integration.TrendyolMarketplace.Validation.SupplierIdRequired"));

        RuleFor(x => x.ApiKey)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Integration.TrendyolMarketplace.Validation.ApiKeyRequired"));

        // ApiSecret is only required when creating new credentials
        RuleFor(x => x.ApiSecret)
            .NotEmpty()
            .When(x => x.Id == 0) // Only for new records
            .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Integration.TrendyolMarketplace.Validation.ApiSecretRequired"));

        RuleFor(x => x.SyncIntervalMinutes)
            .GreaterThan(0)
            .LessThanOrEqualTo(1440) // Max 24 hours
            .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Integration.TrendyolMarketplace.Validation.SyncIntervalRange"));
    }
}
