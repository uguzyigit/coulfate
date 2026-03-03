using FluentMigrator.Builders.Create.Table;
using Marketplace.Abstractions.Domain;
using Nop.Data.Mapping.Builders;

namespace Nop.Plugin.Marketplace.VendorExtensions.Data;

public class CategorySpecificationAttributeBuilder : NopEntityBuilder<CategorySpecificationAttribute>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(CategorySpecificationAttribute.CategoryId)).AsInt32().NotNullable()
            .WithColumn(nameof(CategorySpecificationAttribute.SpecificationAttributeId)).AsInt32().NotNullable()
            .WithColumn(nameof(CategorySpecificationAttribute.DisplayOrder)).AsInt32().NotNullable().WithDefaultValue(0);
    }
}
