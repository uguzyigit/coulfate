namespace Nop.Plugin.Accounting.Parasut.Data;

/// <summary>
/// Installation data for Paraşüt plugin (MySQL syntax)
/// </summary>
public static class InstallationData
{
    /// <summary>
    /// SQL script to create plugin tables
    /// </summary>
    public const string CreateTablesScript = @"
-- Vendor to Paraşüt contact mapping
CREATE TABLE IF NOT EXISTS `ParasutVendorMapping` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `VendorId` int NOT NULL,
    `ParasutContactId` varchar(50) NOT NULL,
    `ContactName` varchar(200) NULL,
    `TaxOffice` varchar(200) NULL,
    `TaxNumber` varchar(50) NULL,
    `Email` varchar(200) NULL,
    `IsActive` tinyint(1) NOT NULL DEFAULT 1,
    `LastSyncedOnUtc` datetime(6) NOT NULL,
    `CreatedOnUtc` datetime(6) NOT NULL,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `IX_ParasutVendorMapping_VendorId` (`VendorId`),
    KEY `IX_ParasutVendorMapping_ParasutContactId` (`ParasutContactId`),
    CONSTRAINT `FK_ParasutVendorMapping_Vendor` 
        FOREIGN KEY (`VendorId`) REFERENCES `Vendor` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Paraşüt product mappings (charge types)
CREATE TABLE IF NOT EXISTS `ParasutProductMapping` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `ChargeType` int NOT NULL,
    `ParasutProductId` varchar(50) NOT NULL,
    `ProductName` varchar(200) NOT NULL,
    `ProductCode` varchar(100) NULL,
    `IsActive` tinyint(1) NOT NULL DEFAULT 1,
    `CreatedOnUtc` datetime(6) NOT NULL,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `IX_ParasutProductMapping_ChargeType` (`ChargeType`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Invoice records
CREATE TABLE IF NOT EXISTS `ParasutInvoiceRecord` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `VendorId` int NOT NULL,
    `OrderId` int NULL,
    `InvoiceType` int NOT NULL,
    `ParasutSalesInvoiceId` varchar(50) NULL,
    `ParasutInvoiceNo` varchar(100) NULL,
    `TotalAmount` decimal(18, 4) NOT NULL,
    `VatAmount` decimal(18, 4) NOT NULL,
    `GrossAmount` decimal(18, 4) NOT NULL,
    `EDocumentType` varchar(20) NULL,
    `EDocumentId` varchar(50) NULL,
    `TrackableJobId` varchar(50) NULL,
    `Status` int NOT NULL,
    `StatusMessage` text NULL,
    `ErrorMessage` text NULL,
    `PdfStorageId` varchar(100) NULL,
    `PdfLocalPath` varchar(500) NULL,
    `PdfDownloadedOnUtc` datetime(6) NULL,
    `CreatedOnUtc` datetime(6) NOT NULL,
    `UpdatedOnUtc` datetime(6) NULL,
    PRIMARY KEY (`Id`),
    KEY `IX_ParasutInvoiceRecord_VendorId` (`VendorId`),
    KEY `IX_ParasutInvoiceRecord_OrderId` (`OrderId`),
    KEY `IX_ParasutInvoiceRecord_Status` (`Status`),
    KEY `IX_ParasutInvoiceRecord_TrackableJobId` (`TrackableJobId`),
    UNIQUE KEY `IX_ParasutInvoiceRecord_Unique` (`OrderId`, `VendorId`, `InvoiceType`),
    CONSTRAINT `FK_ParasutInvoiceRecord_Vendor` 
        FOREIGN KEY (`VendorId`) REFERENCES `Vendor` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- API call logs
CREATE TABLE IF NOT EXISTS `ParasutApiLog` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `RequestType` varchar(100) NOT NULL,
    `Endpoint` varchar(500) NOT NULL,
    `Method` varchar(10) NOT NULL,
    `RequestPayload` text NULL,
    `ResponsePayload` text NULL,
    `StatusCode` int NULL,
    `IsSuccess` tinyint(1) NOT NULL,
    `ErrorMessage` text NULL,
    `DurationMs` int NULL,
    `CreatedOnUtc` datetime(6) NOT NULL,
    PRIMARY KEY (`Id`),
    KEY `IX_ParasutApiLog_RequestType` (`RequestType`),
    KEY `IX_ParasutApiLog_IsSuccess` (`IsSuccess`),
    KEY `IX_ParasutApiLog_CreatedOnUtc` (`CreatedOnUtc`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
";

    /// <summary>
    /// SQL script to drop plugin tables
    /// </summary>
    public const string DropTablesScript = @"
DROP TABLE IF EXISTS `ParasutApiLog`;
DROP TABLE IF EXISTS `ParasutInvoiceRecord`;
DROP TABLE IF EXISTS `ParasutProductMapping`;
DROP TABLE IF EXISTS `ParasutVendorMapping`;
";
}
