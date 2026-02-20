namespace Nop.Plugin.Marketplace.VendorExtensions.Data;

public static class InstallationData
{
    public const string CreateTablesScript = @"
CREATE TABLE IF NOT EXISTS `MarketplaceVendorSettings` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `VendorId` int NOT NULL,
    `Code` varchar(100) NULL,
    `BankAccountName` varchar(200) NULL,
    `BankIban` varchar(50) NULL,
    `RequiresProductApproval` tinyint(1) NOT NULL DEFAULT 0,
    `MinimumPayoutAmount` decimal(18, 4) NOT NULL DEFAULT 100.00,
    `CreatedOnUtc` datetime(6) NOT NULL,
    `UpdatedOnUtc` datetime(6) NOT NULL,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `IX_MarketplaceVendorSettings_VendorId` (`VendorId`),
    CONSTRAINT `FK_MarketplaceVendorSettings_Vendor` FOREIGN KEY (`VendorId`) REFERENCES `Vendor` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `MarketplaceVendorCurrentAccount` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `VendorId` int NOT NULL,
    `Balance` decimal(18, 4) NOT NULL DEFAULT 0.00,
    `TotalCredit` decimal(18, 4) NOT NULL DEFAULT 0.00,
    `TotalDebit` decimal(18, 4) NOT NULL DEFAULT 0.00,
    `LastUpdatedUtc` datetime(6) NOT NULL,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `IX_MarketplaceVendorCurrentAccount_VendorId` (`VendorId`),
    CONSTRAINT `FK_MarketplaceVendorCurrentAccount_Vendor` FOREIGN KEY (`VendorId`) REFERENCES `Vendor` (`Id`) ON DELETE CASCADE
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
    KEY `IX_MarketplaceVendorTransaction_Type` (`Type`),
    KEY `IX_MarketplaceVendorTransaction_CreatedOnUtc` (`CreatedOnUtc` DESC),
    CONSTRAINT `FK_MarketplaceVendorTransaction_Vendor` FOREIGN KEY (`VendorId`) REFERENCES `Vendor` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
";

    public const string DropTablesScript = @"
DROP TABLE IF EXISTS `MarketplaceVendorTransaction`;
DROP TABLE IF EXISTS `MarketplaceVendorCurrentAccount`;
DROP TABLE IF EXISTS `MarketplaceVendorSettings`;
";
}
