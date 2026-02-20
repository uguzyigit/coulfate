-- Trendyol Plugin Tabloları
-- Bu SQL'i nopcommerce490 veritabanında çalıştırın

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
    PRIMARY KEY (`Id`),
    UNIQUE KEY `IX_TrendyolProduct_Barcode_VendorId` (`TrendyolBarcode`, `VendorId`),
    KEY `IX_TrendyolProduct_TrendyolProductCode` (`TrendyolProductCode`),
    KEY `IX_TrendyolProduct_NopProductId` (`NopProductId`),
    KEY `IX_TrendyolProduct_VendorId` (`VendorId`),
    KEY `IX_TrendyolProduct_ImportStatus` (`ImportStatus`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

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
