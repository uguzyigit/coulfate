namespace Nop.Plugin.Marketplace.Core.Data;

/// <summary>
/// Database installation/uninstallation scripts for MySQL
/// Note: Uses NopCommerce native Vendor table, VendorExtension adds marketplace-specific fields
/// </summary>
public static class InstallationData
{
    public const string CreateTablesScript = @"
-- Vendor Extension Table (extends NopCommerce Vendor with marketplace-specific fields)
CREATE TABLE IF NOT EXISTS `MarketplaceVendorExtension` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `VendorId` int NOT NULL,
    `Code` varchar(50) NOT NULL,
    `MarketplaceStatus` int NOT NULL DEFAULT 0,
    `Phone` varchar(50) NULL,
    `BankAccountName` varchar(255) NULL,
    `BankIban` varchar(100) NULL,
    `TaxNumber` varchar(50) NULL,
    `TradeRegistryNumber` varchar(100) NULL,
    `RequiresProductApproval` tinyint(1) NOT NULL DEFAULT 0,
    `MinimumPayoutAmount` decimal(18, 4) NOT NULL DEFAULT 0.00,
    `CreatedOnUtc` datetime(6) NOT NULL,
    `UpdatedOnUtc` datetime(6) NULL,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `IX_MarketplaceVendorExtension_VendorId` (`VendorId`),
    UNIQUE KEY `IX_MarketplaceVendorExtension_Code` (`Code`),
    CONSTRAINT `FK_MarketplaceVendorExtension_Vendor`
        FOREIGN KEY (`VendorId`) REFERENCES `Vendor` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Vendor Product Table
CREATE TABLE IF NOT EXISTS `MarketplaceVendorProduct` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `VendorId` int NOT NULL,
    `ProductId` int NOT NULL,
    `VendorSku` varchar(100) NULL,
    `VendorPrice` decimal(18, 4) NOT NULL,
    `StockQuantity` int NOT NULL,
    `IsApproved` tinyint(1) NOT NULL DEFAULT 0,
    `CreatedOnUtc` datetime(6) NOT NULL,
    `UpdatedOnUtc` datetime(6) NOT NULL,
    PRIMARY KEY (`Id`),
    KEY `IX_MarketplaceVendorProduct_VendorId` (`VendorId`),
    KEY `IX_MarketplaceVendorProduct_ProductId` (`ProductId`),
    CONSTRAINT `FK_MarketplaceVendorProduct_Vendor`
        FOREIGN KEY (`VendorId`) REFERENCES `Vendor` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Vendor Order Line Table
CREATE TABLE IF NOT EXISTS `MarketplaceVendorOrderLine` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `OrderItemId` int NOT NULL,
    `OrderId` int NOT NULL,
    `VendorId` int NOT NULL,
    `ProductId` int NOT NULL,
    `Quantity` int NOT NULL,
    `UnitPrice` decimal(18, 4) NOT NULL,
    `CommissionRateSnapshot` decimal(18, 4) NOT NULL,
    `CommissionAmountSnapshot` decimal(18, 4) NOT NULL,
    `NetAmountSnapshot` decimal(18, 4) NOT NULL,
    `CreatedOnUtc` datetime(6) NOT NULL,
    PRIMARY KEY (`Id`),
    KEY `IX_MarketplaceVendorOrderLine_OrderId` (`OrderId`),
    KEY `IX_MarketplaceVendorOrderLine_VendorId` (`VendorId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Vendor Balance Table
CREATE TABLE IF NOT EXISTS `MarketplaceVendorBalance` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `VendorId` int NOT NULL,
    `CurrentBalance` decimal(18, 4) NOT NULL,
    `Pending` decimal(18, 4) NOT NULL,
    `Hold` decimal(18, 4) NOT NULL,
    `UpdatedOnUtc` datetime(6) NOT NULL,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `IX_MarketplaceVendorBalance_VendorId` (`VendorId`),
    CONSTRAINT `FK_MarketplaceVendorBalance_Vendor`
        FOREIGN KEY (`VendorId`) REFERENCES `Vendor` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Vendor Shipping Account Table
CREATE TABLE IF NOT EXISTS `MarketplaceVendorShippingAccount` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `VendorId` int NOT NULL,
    `Carrier` varchar(100) NOT NULL,
    `ApiKey` varchar(500) NULL,
    `IsActive` tinyint(1) NOT NULL DEFAULT 1,
    `CreatedOnUtc` datetime(6) NOT NULL,
    PRIMARY KEY (`Id`),
    KEY `IX_MarketplaceVendorShippingAccount_VendorId` (`VendorId`),
    CONSTRAINT `FK_MarketplaceVendorShippingAccount_Vendor`
        FOREIGN KEY (`VendorId`) REFERENCES `Vendor` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Vendor Payout Table
CREATE TABLE IF NOT EXISTS `MarketplaceVendorPayout` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `VendorId` int NOT NULL,
    `Amount` decimal(18, 4) NOT NULL,
    `Status` int NOT NULL,
    `RequestedOnUtc` datetime(6) NOT NULL,
    `ProcessedOnUtc` datetime(6) NULL,
    `ExternalRef` varchar(200) NULL,
    PRIMARY KEY (`Id`),
    KEY `IX_MarketplaceVendorPayout_VendorId` (`VendorId`),
    CONSTRAINT `FK_MarketplaceVendorPayout_Vendor`
        FOREIGN KEY (`VendorId`) REFERENCES `Vendor` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Commission Tables
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
    CONSTRAINT `FK_MarketplaceCategoryCommission_Category`
        FOREIGN KEY (`CategoryId`) REFERENCES `Category` (`Id`) ON DELETE CASCADE
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
    CONSTRAINT `FK_MarketplaceProductCommission_Product`
        FOREIGN KEY (`ProductId`) REFERENCES `Product` (`Id`) ON DELETE CASCADE
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
    CONSTRAINT `FK_MarketplaceOrderCommission_Order`
        FOREIGN KEY (`OrderId`) REFERENCES `Order` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Vendor Account Tables
CREATE TABLE IF NOT EXISTS `MarketplaceVendorCurrentAccount` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `VendorId` int NOT NULL,
    `Balance` decimal(18, 4) NOT NULL DEFAULT 0.00,
    `TotalCredit` decimal(18, 4) NOT NULL DEFAULT 0.00,
    `TotalDebit` decimal(18, 4) NOT NULL DEFAULT 0.00,
    `LastUpdatedUtc` datetime(6) NOT NULL,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `IX_MarketplaceVendorCurrentAccount_VendorId` (`VendorId`),
    CONSTRAINT `FK_MarketplaceVendorCurrentAccount_Vendor`
        FOREIGN KEY (`VendorId`) REFERENCES `Vendor` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `MarketplaceVendorTransaction` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `VendorId` int NOT NULL,
    `OrderId` int NULL,
    `OrderItemId` int NULL,
    `Type` int NOT NULL,
    `Amount` decimal(18, 4) NOT NULL,
    `BalanceAfter` decimal(18, 4) NOT NULL,
    `Description` varchar(500) NULL,
    `ReferenceNumber` varchar(100) NULL,
    `Metadata` text NULL,
    `CreatedOnUtc` datetime(6) NOT NULL,
    PRIMARY KEY (`Id`),
    KEY `IX_MarketplaceVendorTransaction_VendorId` (`VendorId`),
    KEY `IX_MarketplaceVendorTransaction_OrderId` (`OrderId`),
    CONSTRAINT `FK_MarketplaceVendorTransaction_Vendor`
        FOREIGN KEY (`VendorId`) REFERENCES `Vendor` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Shipping Provider Table (PTT Kargo, Aras Kargo, etc.)
CREATE TABLE IF NOT EXISTS `MarketplaceShippingProvider` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Name` varchar(200) NOT NULL,
    `SystemName` varchar(100) NOT NULL,
    `SupportsMarketplaceContract` tinyint(1) NOT NULL DEFAULT 1,
    `SupportsVendorContract` tinyint(1) NOT NULL DEFAULT 1,
    `LogoUrl` varchar(500) NULL,
    `DisplayOrder` int NOT NULL DEFAULT 0,
    `IsActive` tinyint(1) NOT NULL DEFAULT 1,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `IX_MarketplaceShippingProvider_SystemName` (`SystemName`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Vendor Shipping Preference Table
CREATE TABLE IF NOT EXISTS `MarketplaceVendorShippingPreference` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `VendorId` int NOT NULL,
    `ShippingProviderId` int NOT NULL,
    `UseMarketplaceContract` tinyint(1) NOT NULL DEFAULT 1,
    `IsActive` tinyint(1) NOT NULL DEFAULT 1,
    `CreatedOnUtc` datetime(6) NOT NULL,
    `UpdatedOnUtc` datetime(6) NULL,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `IX_MarketplaceVendorShippingPreference_VendorId` (`VendorId`),
    KEY `IX_MarketplaceVendorShippingPreference_ShippingProviderId` (`ShippingProviderId`),
    CONSTRAINT `FK_MarketplaceVendorShippingPreference_Vendor`
        FOREIGN KEY (`VendorId`) REFERENCES `Vendor` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_MarketplaceVendorShippingPreference_Provider`
        FOREIGN KEY (`ShippingProviderId`) REFERENCES `MarketplaceShippingProvider` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Vendor Shipping Credential Table (for vendor's own contracts)
CREATE TABLE IF NOT EXISTS `MarketplaceVendorShippingCredential` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `VendorId` int NOT NULL,
    `ShippingProviderId` int NOT NULL,
    `CredentialKey` varchar(100) NOT NULL,
    `CredentialValue` varchar(500) NULL,
    `CreatedOnUtc` datetime(6) NOT NULL,
    PRIMARY KEY (`Id`),
    KEY `IX_MarketplaceVendorShippingCredential_VendorId` (`VendorId`),
    KEY `IX_MarketplaceVendorShippingCredential_ProviderId` (`ShippingProviderId`),
    CONSTRAINT `FK_MarketplaceVendorShippingCredential_Vendor`
        FOREIGN KEY (`VendorId`) REFERENCES `Vendor` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_MarketplaceVendorShippingCredential_Provider`
        FOREIGN KEY (`ShippingProviderId`) REFERENCES `MarketplaceShippingProvider` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Marketplace Shipment Table (links to NopCommerce Shipment)
CREATE TABLE IF NOT EXISTS `MarketplaceShipment` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `NopShipmentId` int NOT NULL,
    `VendorId` int NOT NULL,
    `ShippingProviderId` int NOT NULL,
    `TrackingNumber` varchar(50) NULL,
    `BarcodeNumber` varchar(50) NULL,
    `ExternalStatus` varchar(100) NULL,
    `LastStatusUpdate` datetime(6) NULL,
    `RawResponse` text NULL,
    `IsReturn` tinyint(1) NOT NULL DEFAULT 0,
    `CreatedOnUtc` datetime(6) NOT NULL,
    PRIMARY KEY (`Id`),
    KEY `IX_MarketplaceShipment_NopShipmentId` (`NopShipmentId`),
    KEY `IX_MarketplaceShipment_VendorId` (`VendorId`),
    KEY `IX_MarketplaceShipment_TrackingNumber` (`TrackingNumber`),
    CONSTRAINT `FK_MarketplaceShipment_Vendor`
        FOREIGN KEY (`VendorId`) REFERENCES `Vendor` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_MarketplaceShipment_Provider`
        FOREIGN KEY (`ShippingProviderId`) REFERENCES `MarketplaceShippingProvider` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Insert default shipping providers
INSERT IGNORE INTO `MarketplaceShippingProvider` (`Name`, `SystemName`, `SupportsMarketplaceContract`, `SupportsVendorContract`, `DisplayOrder`, `IsActive`)
VALUES ('PTT Kargo', 'Shipping.PttKargo', 1, 1, 1, 1);
";

    public const string DropTablesScript = @"
-- Shipping tables (must be dropped first due to FK constraints)
DROP TABLE IF EXISTS `MarketplaceShipment`;
DROP TABLE IF EXISTS `MarketplaceVendorShippingCredential`;
DROP TABLE IF EXISTS `MarketplaceVendorShippingPreference`;
DROP TABLE IF EXISTS `MarketplaceShippingProvider`;

DROP TABLE IF EXISTS `MarketplaceVendorTransaction`;
DROP TABLE IF EXISTS `MarketplaceVendorCurrentAccount`;
DROP TABLE IF EXISTS `MarketplaceOrderCommission`;
DROP TABLE IF EXISTS `MarketplaceProductCommission`;
DROP TABLE IF EXISTS `MarketplaceCategoryCommission`;
DROP TABLE IF EXISTS `MarketplaceVendorPayout`;
DROP TABLE IF EXISTS `MarketplaceVendorShippingAccount`;
DROP TABLE IF EXISTS `MarketplaceVendorBalance`;
DROP TABLE IF EXISTS `MarketplaceVendorOrderLine`;
DROP TABLE IF EXISTS `MarketplaceVendorProduct`;
DROP TABLE IF EXISTS `MarketplaceVendorExtension`;
";
}
