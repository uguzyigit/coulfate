using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Accounting.Parasut.Domain;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Accounting.Parasut.Models.Configuration;

public record ConfigurationModel : BaseNopModel
{
    // ========== API Credentials ==========
    [NopResourceDisplayName("Plugins.Accounting.Parasut.Fields.Enabled")]
    public bool Enabled { get; set; }

    [NopResourceDisplayName("Plugins.Accounting.Parasut.Fields.CompanyId")]
    public string CompanyId { get; set; }

    [NopResourceDisplayName("Plugins.Accounting.Parasut.Fields.ClientId")]
    public string ClientId { get; set; }

    [NopResourceDisplayName("Plugins.Accounting.Parasut.Fields.ClientSecret")]
    public string ClientSecret { get; set; }

    [NopResourceDisplayName("Plugins.Accounting.Parasut.Fields.Username")]
    public string Username { get; set; }

    [NopResourceDisplayName("Plugins.Accounting.Parasut.Fields.Password")]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    // ========== Invoice Settings ==========
    [NopResourceDisplayName("Plugins.Accounting.Parasut.Fields.DefaultVatRate")]
    public decimal DefaultVatRate { get; set; }

    // ========== Invoice Creation Strategy (YENİ) ==========
    
    [NopResourceDisplayName("Plugins.Accounting.Parasut.Fields.InvoiceStrategy")]
    public int InvoiceStrategyId { get; set; }
    public IList<SelectListItem> AvailableInvoiceStrategies { get; set; } = new List<SelectListItem>();

    [NopResourceDisplayName("Plugins.Accounting.Parasut.Fields.ReturnPeriodDays")]
    public int ReturnPeriodDays { get; set; }

    [NopResourceDisplayName("Plugins.Accounting.Parasut.Fields.StartFrom")]
    public int StartFromId { get; set; }
    public IList<SelectListItem> AvailableStartFromOptions { get; set; } = new List<SelectListItem>();

    [NopResourceDisplayName("Plugins.Accounting.Parasut.Fields.AutoProcessExpiredReturnPeriods")]
    public bool AutoProcessExpiredReturnPeriods { get; set; }

    // ========== Auto-Create Flags ==========
    [NopResourceDisplayName("Plugins.Accounting.Parasut.Fields.AutoCreateCommissionInvoice")]
    public bool AutoCreateCommissionInvoice { get; set; }

    [NopResourceDisplayName("Plugins.Accounting.Parasut.Fields.AutoCreateShippingInvoice")]
    public bool AutoCreateShippingInvoice { get; set; }

    [NopResourceDisplayName("Plugins.Accounting.Parasut.Fields.AutoCreatePenaltyInvoice")]
    public bool AutoCreatePenaltyInvoice { get; set; }
}
