namespace Nop.Plugin.Payments.Iyzico.Data;

/// <summary>
/// Installation data for MySQL database
/// </summary>
public static class InstallationData
{
    /// <summary>
    /// SQL script to create iyzico tables
    /// </summary>
    public const string CreateTablesScript = @"
CREATE TABLE IF NOT EXISTS `IyzicoPaymentTransaction` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `OrderId` int NOT NULL,
    `OrderItemId` int NOT NULL,
    `PaymentTransactionId` varchar(200) NOT NULL,
    `ConversationId` varchar(200) NOT NULL,
    `PaymentId` varchar(200) NULL,
    `SubMerchantKey` varchar(200) NOT NULL,
    `SubMerchantPrice` decimal(18,4) NOT NULL,
    `WithholdingTax` decimal(18,4) NOT NULL DEFAULT 0,
    `VendorId` int NOT NULL,
    `ProductId` int NOT NULL,
    `Status` varchar(50) NOT NULL DEFAULT 'Pending',
    `IsApproved` tinyint(1) NOT NULL DEFAULT 0,
    `ApprovedOnUtc` datetime(6) NULL,
    `IsDisapproved` tinyint(1) NOT NULL DEFAULT 0,
    `DisapprovedOnUtc` datetime(6) NULL,
    `DisapprovalReason` varchar(500) NULL,
    `CreatedOnUtc` datetime(6) NOT NULL,
    `RawResponse` text NULL,
    PRIMARY KEY (`Id`),
    KEY `IX_IyzicoPaymentTransaction_OrderId` (`OrderId`),
    KEY `IX_IyzicoPaymentTransaction_OrderItemId` (`OrderItemId`),
    KEY `IX_IyzicoPaymentTransaction_VendorId` (`VendorId`),
    UNIQUE KEY `IX_IyzicoPaymentTransaction_PaymentTransactionId` (`PaymentTransactionId`),
    CONSTRAINT `FK_IyzicoPaymentTransaction_Order` 
        FOREIGN KEY (`OrderId`) REFERENCES `Order` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
";

    /// <summary>
    /// SQL script to update VendorExtensions MarketplaceVendorSettings table
    /// </summary>
    public const string UpdateVendorSettingsScript = @"
-- Check if columns exist before adding
SET @dbname = DATABASE();
SET @tablename = 'MarketplaceVendorSettings';

-- IBAN
SET @columnname = 'Iban';
SET @preparedStatement = (SELECT IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
   WHERE TABLE_SCHEMA = @dbname
   AND TABLE_NAME = @tablename
   AND COLUMN_NAME = @columnname) > 0,
  'SELECT ''Column exists'';',
  CONCAT('ALTER TABLE `', @tablename, '` ADD COLUMN `', @columnname, '` varchar(50) NULL;')
));
PREPARE alterStatement FROM @preparedStatement;
EXECUTE alterStatement;
DEALLOCATE PREPARE alterStatement;

-- IdentityNumber
SET @columnname = 'IdentityNumber';
SET @preparedStatement = (SELECT IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
   WHERE TABLE_SCHEMA = @dbname
   AND TABLE_NAME = @tablename
   AND COLUMN_NAME = @columnname) > 0,
  'SELECT ''Column exists'';',
  CONCAT('ALTER TABLE `', @tablename, '` ADD COLUMN `', @columnname, '` varchar(11) NULL;')
));
PREPARE alterStatement FROM @preparedStatement;
EXECUTE alterStatement;
DEALLOCATE PREPARE alterStatement;

-- TaxNumber
SET @columnname = 'TaxNumber';
SET @preparedStatement = (SELECT IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
   WHERE TABLE_SCHEMA = @dbname
   AND TABLE_NAME = @tablename
   AND COLUMN_NAME = @columnname) > 0,
  'SELECT ''Column exists'';',
  CONCAT('ALTER TABLE `', @tablename, '` ADD COLUMN `', @columnname, '` varchar(10) NULL;')
));
PREPARE alterStatement FROM @preparedStatement;
EXECUTE alterStatement;
DEALLOCATE PREPARE alterStatement;

-- TaxOffice
SET @columnname = 'TaxOffice';
SET @preparedStatement = (SELECT IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
   WHERE TABLE_SCHEMA = @dbname
   AND TABLE_NAME = @tablename
   AND COLUMN_NAME = @columnname) > 0,
  'SELECT ''Column exists'';',
  CONCAT('ALTER TABLE `', @tablename, '` ADD COLUMN `', @columnname, '` varchar(100) NULL;')
));
PREPARE alterStatement FROM @preparedStatement;
EXECUTE alterStatement;
DEALLOCATE PREPARE alterStatement;

-- GsmNumber
SET @columnname = 'GsmNumber';
SET @preparedStatement = (SELECT IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
   WHERE TABLE_SCHEMA = @dbname
   AND TABLE_NAME = @tablename
   AND COLUMN_NAME = @columnname) > 0,
  'SELECT ''Column exists'';',
  CONCAT('ALTER TABLE `', @tablename, '` ADD COLUMN `', @columnname, '` varchar(20) NULL;')
));
PREPARE alterStatement FROM @preparedStatement;
EXECUTE alterStatement;
DEALLOCATE PREPARE alterStatement;

-- LegalCompanyTitle
SET @columnname = 'LegalCompanyTitle';
SET @preparedStatement = (SELECT IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
   WHERE TABLE_SCHEMA = @dbname
   AND TABLE_NAME = @tablename
   AND COLUMN_NAME = @columnname) > 0,
  'SELECT ''Column exists'';',
  CONCAT('ALTER TABLE `', @tablename, '` ADD COLUMN `', @columnname, '` varchar(200) NULL;')
));
PREPARE alterStatement FROM @preparedStatement;
EXECUTE alterStatement;
DEALLOCATE PREPARE alterStatement;

-- SubMerchantType
SET @columnname = 'SubMerchantType';
SET @preparedStatement = (SELECT IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
   WHERE TABLE_SCHEMA = @dbname
   AND TABLE_NAME = @tablename
   AND COLUMN_NAME = @columnname) > 0,
  'SELECT ''Column exists'';',
  CONCAT('ALTER TABLE `', @tablename, '` ADD COLUMN `', @columnname, '` varchar(50) NULL;')
));
PREPARE alterStatement FROM @preparedStatement;
EXECUTE alterStatement;
DEALLOCATE PREPARE alterStatement;

-- IyzicoSubMerchantKey
SET @columnname = 'IyzicoSubMerchantKey';
SET @preparedStatement = (SELECT IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
   WHERE TABLE_SCHEMA = @dbname
   AND TABLE_NAME = @tablename
   AND COLUMN_NAME = @columnname) > 0,
  'SELECT ''Column exists'';',
  CONCAT('ALTER TABLE `', @tablename, '` ADD COLUMN `', @columnname, '` varchar(200) NULL;')
));
PREPARE alterStatement FROM @preparedStatement;
EXECUTE alterStatement;
DEALLOCATE PREPARE alterStatement;

-- IyzicoRegisteredOnUtc
SET @columnname = 'IyzicoRegisteredOnUtc';
SET @preparedStatement = (SELECT IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
   WHERE TABLE_SCHEMA = @dbname
   AND TABLE_NAME = @tablename
   AND COLUMN_NAME = @columnname) > 0,
  'SELECT ''Column exists'';',
  CONCAT('ALTER TABLE `', @tablename, '` ADD COLUMN `', @columnname, '` datetime(6) NULL;')
));
PREPARE alterStatement FROM @preparedStatement;
EXECUTE alterStatement;
DEALLOCATE PREPARE alterStatement;
";

    /// <summary>
    /// SQL script to drop iyzico tables
    /// </summary>
    public const string DropTablesScript = @"
DROP TABLE IF EXISTS `IyzicoPaymentTransaction`;
";
}
