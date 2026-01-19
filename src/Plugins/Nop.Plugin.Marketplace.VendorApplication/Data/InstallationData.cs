namespace Nop.Plugin.Marketplace.VendorApplication.Data;

/// <summary>
/// Database installation/uninstallation scripts for MySQL
/// </summary>
public static class InstallationData
{
    public const string CreateTablesScript = @"
-- Vendor Application Tables

-- Document Types (admin-configurable)
CREATE TABLE IF NOT EXISTS `MarketplaceVendorDocumentType` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Name` varchar(200) NOT NULL,
    `Description` varchar(500) NULL,
    `IsRequired` tinyint(1) NOT NULL DEFAULT 1,
    `DisplayOrder` int NOT NULL DEFAULT 0,
    `IsActive` tinyint(1) NOT NULL DEFAULT 1,
    `AllowedExtensions` varchar(200) NULL,
    `MaxFileSizeKb` int NOT NULL DEFAULT 5120,
    `CreatedOnUtc` datetime(6) NOT NULL,
    `UpdatedOnUtc` datetime(6) NULL,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Vendor Applications
CREATE TABLE IF NOT EXISTS `MarketplaceVendorApplication` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `ApplicationNumber` varchar(50) NOT NULL,
    `Status` int NOT NULL DEFAULT 0,

    -- Company Information
    `CompanyName` varchar(400) NOT NULL,
    `ContactPerson` varchar(200) NOT NULL,
    `Email` varchar(255) NOT NULL,
    `Phone` varchar(50) NOT NULL,

    -- Legal Information
    `TaxNumber` varchar(50) NULL,
    `TradeRegistryNumber` varchar(100) NULL,

    -- Address
    `Address` varchar(1000) NULL,
    `City` varchar(100) NULL,
    `District` varchar(100) NULL,
    `PostalCode` varchar(20) NULL,
    `CountryId` int NULL,
    `StateProvinceId` int NULL,

    -- Bank Information
    `BankAccountHolder` varchar(200) NULL,
    `BankIban` varchar(50) NULL,
    `BankName` varchar(200) NULL,

    -- Additional
    `Description` text NULL,
    `AdminNotes` text NULL,
    `RejectionReason` text NULL,

    -- Tracking
    `IpAddress` varchar(45) NULL,
    `CreatedVendorId` int NULL,
    `CreatedOnUtc` datetime(6) NOT NULL,
    `UpdatedOnUtc` datetime(6) NOT NULL,
    `ReviewedOnUtc` datetime(6) NULL,
    `ReviewedByCustomerId` int NULL,

    PRIMARY KEY (`Id`),
    UNIQUE KEY `IX_MarketplaceVendorApplication_ApplicationNumber` (`ApplicationNumber`),
    KEY `IX_MarketplaceVendorApplication_Status` (`Status`),
    KEY `IX_MarketplaceVendorApplication_Email` (`Email`),
    KEY `IX_MarketplaceVendorApplication_CreatedOnUtc` (`CreatedOnUtc` DESC)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Application Documents
CREATE TABLE IF NOT EXISTS `MarketplaceVendorApplicationDocument` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `VendorApplicationId` int NOT NULL,
    `DocumentTypeId` int NOT NULL,
    `DownloadId` int NOT NULL,
    `FileName` varchar(500) NULL,
    `ContentType` varchar(100) NULL,
    `FileSize` bigint NOT NULL DEFAULT 0,
    `CreatedOnUtc` datetime(6) NOT NULL,
    PRIMARY KEY (`Id`),
    KEY `IX_MarketplaceVendorApplicationDocument_ApplicationId` (`VendorApplicationId`),
    KEY `IX_MarketplaceVendorApplicationDocument_DocumentTypeId` (`DocumentTypeId`),
    CONSTRAINT `FK_VendorApplicationDocument_Application`
        FOREIGN KEY (`VendorApplicationId`) REFERENCES `MarketplaceVendorApplication` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_VendorApplicationDocument_DocumentType`
        FOREIGN KEY (`DocumentTypeId`) REFERENCES `MarketplaceVendorDocumentType` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Insert default document types
INSERT INTO `MarketplaceVendorDocumentType` (`Name`, `Description`, `IsRequired`, `DisplayOrder`, `IsActive`, `AllowedExtensions`, `MaxFileSizeKb`, `CreatedOnUtc`)
VALUES
    ('Vergi Levhasi', 'Guncel vergi levhasi belgesi', 1, 1, 1, '.pdf,.jpg,.jpeg,.png', 5120, UTC_TIMESTAMP()),
    ('Ticaret Sicil Gazetesi', 'Ticaret sicil gazetesi veya faaliyet belgesi', 1, 2, 1, '.pdf,.jpg,.jpeg,.png', 5120, UTC_TIMESTAMP()),
    ('Imza Sirkuleri', 'Noter onayli imza sirkuleri', 0, 3, 1, '.pdf,.jpg,.jpeg,.png', 5120, UTC_TIMESTAMP());
";

    public const string DropTablesScript = @"
DROP TABLE IF EXISTS `MarketplaceVendorApplicationDocument`;
DROP TABLE IF EXISTS `MarketplaceVendorApplication`;
DROP TABLE IF EXISTS `MarketplaceVendorDocumentType`;
";
}
