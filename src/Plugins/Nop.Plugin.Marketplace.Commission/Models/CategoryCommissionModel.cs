using System;
using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Marketplace.Commission.Models;

public record CategoryCommissionModel : BaseNopEntityModel
{
    [NopResourceDisplayName("Plugins.Marketplace.Commission.Fields.CategoryName")]
    public string CategoryName { get; set; } = string.Empty;

    [NopResourceDisplayName("Plugins.Marketplace.Commission.Fields.CategoryId")]
    public int CategoryId { get; set; }

    [NopResourceDisplayName("Plugins.Marketplace.Commission.Fields.Rate")]
    public decimal Rate { get; set; }

    // Alias for Rate (used by controller)
    public decimal CommissionRate
    {
        get => Rate;
        set => Rate = value;
    }

    [NopResourceDisplayName("Plugins.Marketplace.Commission.Fields.IsActive")]
    public bool IsActive { get; set; }

    // Alias for IsActive (used by controller)
    public bool IsCustomRate
    {
        get => IsActive;
        set => IsActive = value;
    }

    [NopResourceDisplayName("Plugins.Marketplace.Commission.Fields.CreatedOn")]
    public DateTime CreatedOnUtc { get; set; }

    [NopResourceDisplayName("Plugins.Marketplace.Commission.Fields.UpdatedOn")]
    public DateTime? UpdatedOnUtc { get; set; }
}

public record CategoryCommissionListModel : BaseNopModel
{
    public CategoryCommissionListModel()
    {
        Data = new List<CategoryCommissionModel>();
    }

    public IList<CategoryCommissionModel> Data { get; set; }
    public int Total { get; set; }
}

public record CategoryCommissionSearchModel : BaseSearchModel
{
    [NopResourceDisplayName("Plugins.Marketplace.Commission.Fields.SearchCategoryName")]
    public string SearchCategoryName { get; set; } = string.Empty;
}
