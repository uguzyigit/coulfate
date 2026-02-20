namespace Nop.Plugin.Integration.TrendyolMarketplace.Data;

/// <summary>
/// Contains SQL scripts for plugin installation/uninstallation
/// </summary>
public static class InstallationData
{
    /// <summary>
    /// Individual table creation scripts for robust installation
    /// </summary>
    public static readonly string[] CreateTableScripts = new[]
    {
        // Vendor Credentials Table
        @"CREATE TABLE IF NOT EXISTS `TrendyolVendorCredential` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `VendorId` int NOT NULL,
    `TrendyolSupplierId` bigint NOT NULL,
    `ApiKey` varchar(255) NOT NULL,
    `ApiSecret` varchar(512) NOT NULL,
    `IntegrationReferenceCode` varchar(255) NULL,
    `IsActive` tinyint(1) NOT NULL DEFAULT 1,
    `LastSyncOnUtc` datetime(6) NULL,
    `AutoSyncEnabled` tinyint(1) NOT NULL DEFAULT 1,
    `SyncIntervalMinutes` int NOT NULL DEFAULT 60,
    `CreatedOnUtc` datetime(6) NOT NULL,
    `UpdatedOnUtc` datetime(6) NULL,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `IX_TrendyolVendorCredential_VendorId` (`VendorId`),
    KEY `IX_TrendyolVendorCredential_IsActive` (`IsActive`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4",

        // Category Mapping Table
        @"CREATE TABLE IF NOT EXISTS `TrendyolCategory` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `TrendyolCategoryId` bigint NOT NULL,
    `TrendyolCategoryName` varchar(500) NULL,
    `TrendyolParentId` bigint NULL,
    `TrendyolCategoryPath` varchar(1000) NULL,
    `NopCategoryId` int NULL,
    `IsAutoMapped` tinyint(1) NOT NULL DEFAULT 0,
    `IsLeaf` tinyint(1) NOT NULL DEFAULT 0,
    `CreatedOnUtc` datetime(6) NOT NULL,
    `UpdatedOnUtc` datetime(6) NULL,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `IX_TrendyolCategory_TrendyolCategoryId` (`TrendyolCategoryId`),
    KEY `IX_TrendyolCategory_NopCategoryId` (`NopCategoryId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4",

        // Brand Mapping Table
        @"CREATE TABLE IF NOT EXISTS `TrendyolBrand` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `TrendyolBrandId` bigint NOT NULL,
    `TrendyolBrandName` varchar(255) NULL,
    `NopManufacturerId` int NULL,
    `IsAutoMapped` tinyint(1) NOT NULL DEFAULT 0,
    `AutoCreateIfNotExists` tinyint(1) NOT NULL DEFAULT 1,
    `CreatedOnUtc` datetime(6) NOT NULL,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `IX_TrendyolBrand_TrendyolBrandId` (`TrendyolBrandId`),
    KEY `IX_TrendyolBrand_NopManufacturerId` (`NopManufacturerId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4",

        // Attribute Mapping Table
        @"CREATE TABLE IF NOT EXISTS `TrendyolAttribute` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `TrendyolCategoryId` bigint NOT NULL,
    `TrendyolAttributeId` bigint NOT NULL,
    `TrendyolAttributeName` varchar(255) NULL,
    `IsRequired` tinyint(1) NOT NULL DEFAULT 0,
    `IsVariantAttribute` tinyint(1) NOT NULL DEFAULT 0,
    `AllowCustomValue` tinyint(1) NOT NULL DEFAULT 0,
    `NopSpecificationAttributeId` int NULL,
    `NopProductAttributeId` int NULL,
    `MappingType` int NOT NULL DEFAULT 0,
    `CreatedOnUtc` datetime(6) NOT NULL,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `IX_TrendyolAttribute_CategoryAttribute` (`TrendyolCategoryId`, `TrendyolAttributeId`),
    KEY `IX_TrendyolAttribute_NopSpecificationAttributeId` (`NopSpecificationAttributeId`),
    KEY `IX_TrendyolAttribute_NopProductAttributeId` (`NopProductAttributeId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4",

        // Attribute Value Mapping Table
        @"CREATE TABLE IF NOT EXISTS `TrendyolAttributeValue` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `TrendyolAttributeId` int NOT NULL,
    `TrendyolValueId` bigint NOT NULL,
    `TrendyolValueName` varchar(255) NULL,
    `NopSpecificationAttributeOptionId` int NULL,
    `NopProductAttributeValueId` int NULL,
    `CreatedOnUtc` datetime(6) NOT NULL,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `IX_TrendyolAttributeValue_AttributeValue` (`TrendyolAttributeId`, `TrendyolValueId`),
    KEY `IX_TrendyolAttributeValue_NopSpecificationAttributeOptionId` (`NopSpecificationAttributeOptionId`),
    KEY `IX_TrendyolAttributeValue_NopProductAttributeValueId` (`NopProductAttributeValueId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4",

        // Product Tracking Table
        @"CREATE TABLE IF NOT EXISTS `TrendyolProduct` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `TrendyolBarcode` varchar(100) NOT NULL,
    `TrendyolProductCode` varchar(100) NULL,
    `TrendyolStockCode` varchar(100) NULL,
    `TrendyolTitle` varchar(500) NULL,
    `TrendyolCategoryId` bigint NULL,
    `TrendyolBrandId` bigint NULL,
    `NopProductId` int NULL,
    `NopParentProductId` int NULL,
    `VendorId` int NOT NULL,
    `IsVariant` tinyint(1) NOT NULL DEFAULT 0,
    `LastTrendyolPrice` decimal(18, 4) NULL,
    `LastTrendyolStock` int NULL,
    `LastSyncOnUtc` datetime(6) NULL,
    `ImportStatus` int NOT NULL DEFAULT 0,
    `ImportMessage` text NULL,
    `TrendyolJsonData` longtext NULL,
    `CreatedOnUtc` datetime(6) NOT NULL,
    `UpdatedOnUtc` datetime(6) NULL,
    `TrendyolOnSale` tinyint(1) NOT NULL DEFAULT 1,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `IX_TrendyolProduct_Barcode_VendorId` (`TrendyolBarcode`, `VendorId`),
    KEY `IX_TrendyolProduct_TrendyolProductCode` (`TrendyolProductCode`),
    KEY `IX_TrendyolProduct_NopProductId` (`NopProductId`),
    KEY `IX_TrendyolProduct_VendorId` (`VendorId`),
    KEY `IX_TrendyolProduct_ImportStatus` (`ImportStatus`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4",

        // Sync Log Table
        @"CREATE TABLE IF NOT EXISTS `TrendyolSyncLog` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `SyncType` int NOT NULL,
    `VendorId` int NULL,
    `StartedOnUtc` datetime(6) NOT NULL,
    `CompletedOnUtc` datetime(6) NULL,
    `TotalItems` int NOT NULL DEFAULT 0,
    `SuccessCount` int NOT NULL DEFAULT 0,
    `FailedCount` int NOT NULL DEFAULT 0,
    `SkippedCount` int NOT NULL DEFAULT 0,
    `Status` int NOT NULL DEFAULT 0,
    `ErrorMessage` text NULL,
    `Details` longtext NULL,
    PRIMARY KEY (`Id`),
    KEY `IX_TrendyolSyncLog_SyncType` (`SyncType`),
    KEY `IX_TrendyolSyncLog_VendorId` (`VendorId`),
    KEY `IX_TrendyolSyncLog_StartedOnUtc` (`StartedOnUtc`),
    KEY `IX_TrendyolSyncLog_Status` (`Status`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4"
    };

    /// <summary>
    /// Individual scheduled task INSERT scripts
    /// </summary>
    public static readonly string[] ScheduledTaskScripts = new[]
    {
        @"INSERT INTO `ScheduleTask` (`Name`, `Seconds`, `Type`, `LastEnabledUtc`, `Enabled`, `StopOnError`)
SELECT 'Trendyol - Product Sync', 3600, 'Nop.Plugin.Integration.TrendyolMarketplace.Tasks.ProductSyncTask, Nop.Plugin.Integration.TrendyolMarketplace', UTC_TIMESTAMP(), 1, 0
WHERE NOT EXISTS (SELECT 1 FROM `ScheduleTask` WHERE `Type` = 'Nop.Plugin.Integration.TrendyolMarketplace.Tasks.ProductSyncTask, Nop.Plugin.Integration.TrendyolMarketplace')",

        @"INSERT INTO `ScheduleTask` (`Name`, `Seconds`, `Type`, `LastEnabledUtc`, `Enabled`, `StopOnError`)
SELECT 'Trendyol - Stock/Price Sync', 900, 'Nop.Plugin.Integration.TrendyolMarketplace.Tasks.StockPriceSyncTask, Nop.Plugin.Integration.TrendyolMarketplace', UTC_TIMESTAMP(), 1, 0
WHERE NOT EXISTS (SELECT 1 FROM `ScheduleTask` WHERE `Type` = 'Nop.Plugin.Integration.TrendyolMarketplace.Tasks.StockPriceSyncTask, Nop.Plugin.Integration.TrendyolMarketplace')",

        @"INSERT INTO `ScheduleTask` (`Name`, `Seconds`, `Type`, `LastEnabledUtc`, `Enabled`, `StopOnError`)
SELECT 'Trendyol - Category/Brand Sync', 86400, 'Nop.Plugin.Integration.TrendyolMarketplace.Tasks.CategoryBrandSyncTask, Nop.Plugin.Integration.TrendyolMarketplace', UTC_TIMESTAMP(), 1, 0
WHERE NOT EXISTS (SELECT 1 FROM `ScheduleTask` WHERE `Type` = 'Nop.Plugin.Integration.TrendyolMarketplace.Tasks.CategoryBrandSyncTask, Nop.Plugin.Integration.TrendyolMarketplace')"
    };

    /// <summary>
    /// SQL script to create all required tables (legacy - kept for reference)
    /// </summary>
    public const string CreateTablesScript = @"
-- Vendor Credentials Table
CREATE TABLE IF NOT EXISTS `TrendyolVendorCredential` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `VendorId` int NOT NULL,
    `TrendyolSupplierId` bigint NOT NULL,
    `ApiKey` varchar(255) NOT NULL,
    `ApiSecret` varchar(512) NOT NULL,
    `IntegrationReferenceCode` varchar(255) NULL,
    `IsActive` tinyint(1) NOT NULL DEFAULT 1,
    `LastSyncOnUtc` datetime(6) NULL,
    `AutoSyncEnabled` tinyint(1) NOT NULL DEFAULT 1,
    `SyncIntervalMinutes` int NOT NULL DEFAULT 60,
    `CreatedOnUtc` datetime(6) NOT NULL,
    `UpdatedOnUtc` datetime(6) NULL,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `IX_TrendyolVendorCredential_VendorId` (`VendorId`),
    KEY `IX_TrendyolVendorCredential_IsActive` (`IsActive`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Category Mapping Table
CREATE TABLE IF NOT EXISTS `TrendyolCategory` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `TrendyolCategoryId` bigint NOT NULL,
    `TrendyolCategoryName` varchar(500) NULL,
    `TrendyolParentId` bigint NULL,
    `TrendyolCategoryPath` varchar(1000) NULL,
    `NopCategoryId` int NULL,
    `IsAutoMapped` tinyint(1) NOT NULL DEFAULT 0,
    `IsLeaf` tinyint(1) NOT NULL DEFAULT 0,
    `CreatedOnUtc` datetime(6) NOT NULL,
    `UpdatedOnUtc` datetime(6) NULL,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `IX_TrendyolCategory_TrendyolCategoryId` (`TrendyolCategoryId`),
    KEY `IX_TrendyolCategory_NopCategoryId` (`NopCategoryId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Brand Mapping Table
CREATE TABLE IF NOT EXISTS `TrendyolBrand` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `TrendyolBrandId` bigint NOT NULL,
    `TrendyolBrandName` varchar(255) NULL,
    `NopManufacturerId` int NULL,
    `IsAutoMapped` tinyint(1) NOT NULL DEFAULT 0,
    `AutoCreateIfNotExists` tinyint(1) NOT NULL DEFAULT 1,
    `CreatedOnUtc` datetime(6) NOT NULL,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `IX_TrendyolBrand_TrendyolBrandId` (`TrendyolBrandId`),
    KEY `IX_TrendyolBrand_NopManufacturerId` (`NopManufacturerId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Attribute Mapping Table
CREATE TABLE IF NOT EXISTS `TrendyolAttribute` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `TrendyolCategoryId` bigint NOT NULL,
    `TrendyolAttributeId` bigint NOT NULL,
    `TrendyolAttributeName` varchar(255) NULL,
    `IsRequired` tinyint(1) NOT NULL DEFAULT 0,
    `IsVariantAttribute` tinyint(1) NOT NULL DEFAULT 0,
    `AllowCustomValue` tinyint(1) NOT NULL DEFAULT 0,
    `NopSpecificationAttributeId` int NULL,
    `NopProductAttributeId` int NULL,
    `MappingType` int NOT NULL DEFAULT 0,
    `CreatedOnUtc` datetime(6) NOT NULL,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `IX_TrendyolAttribute_CategoryAttribute` (`TrendyolCategoryId`, `TrendyolAttributeId`),
    KEY `IX_TrendyolAttribute_NopSpecificationAttributeId` (`NopSpecificationAttributeId`),
    KEY `IX_TrendyolAttribute_NopProductAttributeId` (`NopProductAttributeId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Attribute Value Mapping Table
CREATE TABLE IF NOT EXISTS `TrendyolAttributeValue` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `TrendyolAttributeId` int NOT NULL,
    `TrendyolValueId` bigint NOT NULL,
    `TrendyolValueName` varchar(255) NULL,
    `NopSpecificationAttributeOptionId` int NULL,
    `NopProductAttributeValueId` int NULL,
    `CreatedOnUtc` datetime(6) NOT NULL,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `IX_TrendyolAttributeValue_AttributeValue` (`TrendyolAttributeId`, `TrendyolValueId`),
    KEY `IX_TrendyolAttributeValue_NopSpecificationAttributeOptionId` (`NopSpecificationAttributeOptionId`),
    KEY `IX_TrendyolAttributeValue_NopProductAttributeValueId` (`NopProductAttributeValueId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Product Tracking Table
CREATE TABLE IF NOT EXISTS `TrendyolProduct` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `TrendyolBarcode` varchar(100) NOT NULL,
    `TrendyolProductCode` varchar(100) NULL,
    `TrendyolStockCode` varchar(100) NULL,
    `TrendyolTitle` varchar(500) NULL,
    `TrendyolCategoryId` bigint NULL,
    `TrendyolBrandId` bigint NULL,
    `NopProductId` int NULL,
    `NopParentProductId` int NULL,
    `VendorId` int NOT NULL,
    `IsVariant` tinyint(1) NOT NULL DEFAULT 0,
    `LastTrendyolPrice` decimal(18, 4) NULL,
    `LastTrendyolStock` int NULL,
    `LastSyncOnUtc` datetime(6) NULL,
    `ImportStatus` int NOT NULL DEFAULT 0,
    `ImportMessage` text NULL,
    `TrendyolJsonData` longtext NULL,
    `CreatedOnUtc` datetime(6) NOT NULL,
    `UpdatedOnUtc` datetime(6) NULL,
    `TrendyolOnSale` tinyint(1) NOT NULL DEFAULT 1,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `IX_TrendyolProduct_Barcode_VendorId` (`TrendyolBarcode`, `VendorId`),
    KEY `IX_TrendyolProduct_TrendyolProductCode` (`TrendyolProductCode`),
    KEY `IX_TrendyolProduct_NopProductId` (`NopProductId`),
    KEY `IX_TrendyolProduct_VendorId` (`VendorId`),
    KEY `IX_TrendyolProduct_ImportStatus` (`ImportStatus`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Sync Log Table
CREATE TABLE IF NOT EXISTS `TrendyolSyncLog` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `SyncType` int NOT NULL,
    `VendorId` int NULL,
    `StartedOnUtc` datetime(6) NOT NULL,
    `CompletedOnUtc` datetime(6) NULL,
    `TotalItems` int NOT NULL DEFAULT 0,
    `SuccessCount` int NOT NULL DEFAULT 0,
    `FailedCount` int NOT NULL DEFAULT 0,
    `SkippedCount` int NOT NULL DEFAULT 0,
    `Status` int NOT NULL DEFAULT 0,
    `ErrorMessage` text NULL,
    `Details` longtext NULL,
    PRIMARY KEY (`Id`),
    KEY `IX_TrendyolSyncLog_SyncType` (`SyncType`),
    KEY `IX_TrendyolSyncLog_VendorId` (`VendorId`),
    KEY `IX_TrendyolSyncLog_StartedOnUtc` (`StartedOnUtc`),
    KEY `IX_TrendyolSyncLog_Status` (`Status`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
";

    /// <summary>
    /// Migration scripts for existing installations (add TrendyolOnSale column)
    /// Each script is executed in a try/catch so "duplicate column" errors are safely ignored
    /// </summary>
    public static readonly string[] MigrationScripts = new[]
    {
        @"ALTER TABLE `TrendyolProduct` ADD COLUMN `TrendyolOnSale` tinyint(1) NOT NULL DEFAULT 1"
    };

    /// <summary>
    /// SQL script to drop all tables (for uninstall)
    /// </summary>
    public const string DropTablesScript = @"
DROP TABLE IF EXISTS `TrendyolSyncLog`;
DROP TABLE IF EXISTS `TrendyolProduct`;
DROP TABLE IF EXISTS `TrendyolAttributeValue`;
DROP TABLE IF EXISTS `TrendyolAttribute`;
DROP TABLE IF EXISTS `TrendyolBrand`;
DROP TABLE IF EXISTS `TrendyolCategory`;
DROP TABLE IF EXISTS `TrendyolVendorCredential`;
";

    /// <summary>
    /// SQL script to create scheduled tasks
    /// </summary>
    public const string CreateScheduledTasksScript = @"
INSERT INTO `ScheduleTask` (`Name`, `Seconds`, `Type`, `LastEnabledUtc`, `Enabled`, `StopOnError`)
SELECT 'Trendyol - Product Sync', 3600, 'Nop.Plugin.Integration.TrendyolMarketplace.Tasks.ProductSyncTask, Nop.Plugin.Integration.TrendyolMarketplace', UTC_TIMESTAMP(), 1, 0
WHERE NOT EXISTS (SELECT 1 FROM `ScheduleTask` WHERE `Type` = 'Nop.Plugin.Integration.TrendyolMarketplace.Tasks.ProductSyncTask, Nop.Plugin.Integration.TrendyolMarketplace');

INSERT INTO `ScheduleTask` (`Name`, `Seconds`, `Type`, `LastEnabledUtc`, `Enabled`, `StopOnError`)
SELECT 'Trendyol - Stock/Price Sync', 900, 'Nop.Plugin.Integration.TrendyolMarketplace.Tasks.StockPriceSyncTask, Nop.Plugin.Integration.TrendyolMarketplace', UTC_TIMESTAMP(), 1, 0
WHERE NOT EXISTS (SELECT 1 FROM `ScheduleTask` WHERE `Type` = 'Nop.Plugin.Integration.TrendyolMarketplace.Tasks.StockPriceSyncTask, Nop.Plugin.Integration.TrendyolMarketplace');

INSERT INTO `ScheduleTask` (`Name`, `Seconds`, `Type`, `LastEnabledUtc`, `Enabled`, `StopOnError`)
SELECT 'Trendyol - Category/Brand Sync', 86400, 'Nop.Plugin.Integration.TrendyolMarketplace.Tasks.CategoryBrandSyncTask, Nop.Plugin.Integration.TrendyolMarketplace', UTC_TIMESTAMP(), 1, 0
WHERE NOT EXISTS (SELECT 1 FROM `ScheduleTask` WHERE `Type` = 'Nop.Plugin.Integration.TrendyolMarketplace.Tasks.CategoryBrandSyncTask, Nop.Plugin.Integration.TrendyolMarketplace');
";

    /// <summary>
    /// SQL script to delete scheduled tasks
    /// </summary>
    public const string DeleteScheduledTasksScript = @"
DELETE FROM `ScheduleTask` WHERE `Type` LIKE 'Nop.Plugin.Integration.TrendyolMarketplace.Tasks.%';
";
}
