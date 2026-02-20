using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Marketplace.VendorExtensions.Domain;

namespace Nop.Plugin.Marketplace.VendorExtensions.Data;

public class VendorCurrentAccountBuilder : NopEntityBuilder<VendorCurrentAccount>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(VendorCurrentAccount.VendorId)).AsInt32().NotNullable()
            .WithColumn(nameof(VendorCurrentAccount.Balance)).AsDecimal(18, 4).NotNullable()
            .WithColumn(nameof(VendorCurrentAccount.TotalCredit)).AsDecimal(18, 4).NotNullable()
            .WithColumn(nameof(VendorCurrentAccount.TotalDebit)).AsDecimal(18, 4).NotNullable()
            .WithColumn(nameof(VendorCurrentAccount.LastUpdatedUtc)).AsDateTime2().NotNullable();
    }
}

public class VendorTransactionBuilder : NopEntityBuilder<VendorTransaction>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(VendorTransaction.VendorId)).AsInt32().NotNullable()
            .WithColumn(nameof(VendorTransaction.OrderId)).AsInt32().Nullable()
            .WithColumn(nameof(VendorTransaction.OrderItemId)).AsInt32().Nullable()
            .WithColumn(nameof(VendorTransaction.Type)).AsInt32().NotNullable()
            .WithColumn(nameof(VendorTransaction.Amount)).AsDecimal(18, 4).NotNullable()
            .WithColumn(nameof(VendorTransaction.BalanceAfter)).AsDecimal(18, 4).NotNullable()
            .WithColumn(nameof(VendorTransaction.Description)).AsString(int.MaxValue).Nullable()
            .WithColumn(nameof(VendorTransaction.ReferenceNumber)).AsString(100).Nullable()
            .WithColumn(nameof(VendorTransaction.Metadata)).AsString(int.MaxValue).Nullable()
            .WithColumn(nameof(VendorTransaction.CreatedOnUtc)).AsDateTime2().NotNullable();
    }
}
