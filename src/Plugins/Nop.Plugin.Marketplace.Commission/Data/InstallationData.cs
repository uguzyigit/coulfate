namespace Nop.Plugin.Marketplace.Commission.Data;

public static class InstallationData
{
    public const string CreateTablesScript = @"
CREATE TABLE IF NOT EXISTS `MarketplaceCategoryCommission` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `CategoryId` int NOT NULL,
    `Rate` decimal(18, 4) NOT NULL,
    `IsActive` tinyint(1) NOT NULL DEFAULT 1,
    `CreatedOnUtc` datetime(6) NOT NULL,
    `UpdatedOnUtc` datetime(6) NULL,
    `CreatedBy` varchar(200) NULL,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `IX_MarketplaceCategoryCommission_CategoryId` (`CategoryId`),
    CONSTRAINT `FK_MarketplaceCategoryCommission_Category` FOREIGN KEY (`CategoryId`) REFERENCES `Category` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `MarketplaceProductCommission` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `ProductId` int NOT NULL,
    `Rate` decimal(18, 4) NOT NULL,
    `IsActive` tinyint(1) NOT NULL DEFAULT 1,
    `Reason` varchar(500) NULL,
    `CreatedOnUtc` datetime(6) NOT NULL,
    `ExpiresOnUtc` datetime(6) NULL,
    `CreatedBy` varchar(200) NULL,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `IX_MarketplaceProductCommission_ProductId` (`ProductId`),
    CONSTRAINT `FK_MarketplaceProductCommission_Product` FOREIGN KEY (`ProductId`) REFERENCES `Product` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `MarketplaceOrderCommission` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `OrderId` int NOT NULL,
    `OrderItemId` int NOT NULL,
    `VendorId` int NOT NULL,
    `ProductId` int NOT NULL,
    `CategoryId` int NOT NULL,
    `Quantity` int NOT NULL,
    `ProductPrice` decimal(18, 4) NOT NULL,
    `DiscountAmount` decimal(18, 4) NOT NULL DEFAULT 0.00,
    `DiscountSource` varchar(50) NULL,
    `ShippingCost` decimal(18, 4) NOT NULL DEFAULT 0.00,
    `NetPrice` decimal(18, 4) NOT NULL,
    `CommissionRate` decimal(18, 4) NOT NULL,
    `CommissionAmount` decimal(18, 4) NOT NULL,
    `MarketplaceFee` decimal(18, 4) NOT NULL,
    `TaxWithholding` decimal(18, 4) NOT NULL,
    `VendorNetAmount` decimal(18, 4) NOT NULL,
    `IsInvoiced` tinyint(1) NOT NULL DEFAULT 0,
    `InvoiceId` varchar(100) NULL,
    `IsPaymentApproved` tinyint(1) NOT NULL DEFAULT 0,
    `IsPaid` tinyint(1) NOT NULL DEFAULT 0,
    `PaidOnUtc` datetime(6) NULL,
    `CreatedOnUtc` datetime(6) NOT NULL,
    PRIMARY KEY (`Id`),
    KEY `IX_MarketplaceOrderCommission_OrderId` (`OrderId`),
    KEY `IX_MarketplaceOrderCommission_VendorId` (`VendorId`),
    KEY `IX_MarketplaceOrderCommission_OrderItemId` (`OrderItemId`),
    CONSTRAINT `FK_MarketplaceOrderCommission_Order` FOREIGN KEY (`OrderId`) REFERENCES `Order` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_MarketplaceOrderCommission_Vendor` FOREIGN KEY (`VendorId`) REFERENCES `Vendor` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
";

    public const string DropTablesScript = @"
DROP TABLE IF EXISTS `MarketplaceOrderCommission`;
DROP TABLE IF EXISTS `MarketplaceProductCommission`;
DROP TABLE IF EXISTS `MarketplaceCategoryCommission`;
";
}
