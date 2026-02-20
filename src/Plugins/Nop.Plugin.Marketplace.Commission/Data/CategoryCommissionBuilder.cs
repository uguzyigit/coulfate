using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Marketplace.Commission.Domain;

namespace Nop.Plugin.Marketplace.Commission.Data;

public class CategoryCommissionBuilder : NopEntityBuilder<CategoryCommission>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(CategoryCommission.CategoryId)).AsInt32().NotNullable()
            .WithColumn(nameof(CategoryCommission.Rate)).AsDecimal(18, 4).NotNullable()
            .WithColumn(nameof(CategoryCommission.IsActive)).AsBoolean().NotNullable()
            .WithColumn(nameof(CategoryCommission.CreatedOnUtc)).AsDateTime2().NotNullable()
            .WithColumn(nameof(CategoryCommission.UpdatedOnUtc)).AsDateTime2().Nullable()
            .WithColumn(nameof(CategoryCommission.CreatedBy)).AsString(200).Nullable();
    }
}

public class ProductCommissionBuilder : NopEntityBuilder<ProductCommission>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(ProductCommission.ProductId)).AsInt32().NotNullable()
            .WithColumn(nameof(ProductCommission.Rate)).AsDecimal(18, 4).NotNullable()
            .WithColumn(nameof(ProductCommission.IsActive)).AsBoolean().NotNullable()
            .WithColumn(nameof(ProductCommission.Reason)).AsString(500).Nullable()
            .WithColumn(nameof(ProductCommission.CreatedOnUtc)).AsDateTime2().NotNullable()
            .WithColumn(nameof(ProductCommission.ExpiresOnUtc)).AsDateTime2().Nullable()
            .WithColumn(nameof(ProductCommission.CreatedBy)).AsString(200).Nullable();
    }
}

public class OrderCommissionBuilder : NopEntityBuilder<OrderCommission>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(OrderCommission.OrderId)).AsInt32().NotNullable()
            .WithColumn(nameof(OrderCommission.OrderItemId)).AsInt32().NotNullable()
            .WithColumn(nameof(OrderCommission.VendorId)).AsInt32().NotNullable()
            .WithColumn(nameof(OrderCommission.ProductId)).AsInt32().NotNullable()
            .WithColumn(nameof(OrderCommission.CategoryId)).AsInt32().NotNullable()
            .WithColumn(nameof(OrderCommission.Quantity)).AsInt32().NotNullable()
            .WithColumn(nameof(OrderCommission.ProductPrice)).AsDecimal(18, 4).NotNullable()
            .WithColumn(nameof(OrderCommission.DiscountAmount)).AsDecimal(18, 4).NotNullable()
            .WithColumn(nameof(OrderCommission.DiscountSource)).AsString(50).Nullable()
            .WithColumn(nameof(OrderCommission.ShippingCost)).AsDecimal(18, 4).NotNullable()
            .WithColumn(nameof(OrderCommission.NetPrice)).AsDecimal(18, 4).NotNullable()
            .WithColumn(nameof(OrderCommission.CommissionRate)).AsDecimal(18, 4).NotNullable()
            .WithColumn(nameof(OrderCommission.CommissionAmount)).AsDecimal(18, 4).NotNullable()
            .WithColumn(nameof(OrderCommission.MarketplaceFee)).AsDecimal(18, 4).NotNullable()
            .WithColumn(nameof(OrderCommission.TaxWithholding)).AsDecimal(18, 4).NotNullable()
            .WithColumn(nameof(OrderCommission.VendorNetAmount)).AsDecimal(18, 4).NotNullable()
            .WithColumn(nameof(OrderCommission.IsInvoiced)).AsBoolean().NotNullable()
            .WithColumn(nameof(OrderCommission.InvoiceId)).AsString(100).Nullable()
            .WithColumn(nameof(OrderCommission.IsPaymentApproved)).AsBoolean().NotNullable()
            .WithColumn(nameof(OrderCommission.IsPaid)).AsBoolean().NotNullable()
            .WithColumn(nameof(OrderCommission.PaidOnUtc)).AsDateTime2().Nullable()
            .WithColumn(nameof(OrderCommission.CreatedOnUtc)).AsDateTime2().NotNullable();
    }
}
