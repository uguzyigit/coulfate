namespace Nop.Plugin.Marketplace.Performance.Data;

public static class InstallationData
{
    public const string CreateTablesScript = @"
-- Product Interaction Table (performance tracking)
CREATE TABLE IF NOT EXISTS `MarketplaceProductInteraction` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `ProductId` int NOT NULL,
    `VendorId` int NOT NULL,
    `CustomerId` int NULL,
    `InteractionType` int NOT NULL,
    `CreatedOnUtc` datetime(6) NOT NULL,
    PRIMARY KEY (`Id`),
    KEY `IX_MarketplaceProductInteraction_TypeProductDate` (`InteractionType`, `ProductId`, `CreatedOnUtc`),
    KEY `IX_MarketplaceProductInteraction_VendorId` (`VendorId`),
    KEY `IX_MarketplaceProductInteraction_CreatedOnUtc` (`CreatedOnUtc`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Product Performance Snapshot Table (calculated scores)
CREATE TABLE IF NOT EXISTS `MarketplaceProductPerformanceSnapshot` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `ProductId` int NOT NULL,
    `VendorId` int NOT NULL,
    `Views30d` int NOT NULL DEFAULT 0,
    `AddToCart30d` int NOT NULL DEFAULT 0,
    `Wishlist30d` int NOT NULL DEFAULT 0,
    `GrossOrders30d` int NOT NULL DEFAULT 0,
    `GrossQty30d` int NOT NULL DEFAULT 0,
    `GrossRevenue30d` decimal(18, 4) NOT NULL DEFAULT 0.00,
    `DiscountAmount30d` decimal(18, 4) NOT NULL DEFAULT 0.00,
    `Revenue7d` decimal(18, 4) NOT NULL DEFAULT 0.00,
    `Orders7d` int NOT NULL DEFAULT 0,
    `CancelQty30d` int NOT NULL DEFAULT 0,
    `ReturnQty30d` int NOT NULL DEFAULT 0,
    `CancelRevenue30d` decimal(18, 4) NOT NULL DEFAULT 0.00,
    `ReturnRevenue30d` decimal(18, 4) NOT NULL DEFAULT 0.00,
    `NetQty30d` int NOT NULL DEFAULT 0,
    `NetRevenue30d` decimal(18, 4) NOT NULL DEFAULT 0.00,
    `SalesScore` decimal(18, 4) NOT NULL DEFAULT 0.00,
    `TrendScore` decimal(18, 4) NOT NULL DEFAULT 0.00,
    `ConversionScore` decimal(18, 4) NOT NULL DEFAULT 0.00,
    `QualityPenalty` decimal(18, 4) NOT NULL DEFAULT 0.00,
    `NewProductBoost` decimal(18, 4) NOT NULL DEFAULT 0.00,
    `FinalScore` decimal(18, 4) NOT NULL DEFAULT 0.00,
    `ReviewCount` int NOT NULL DEFAULT 0,
    `CalculatedOnUtc` datetime(6) NOT NULL,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `IX_MarketplaceProductPerformanceSnapshot_ProductId` (`ProductId`),
    KEY `IX_MarketplaceProductPerformanceSnapshot_FinalScore` (`FinalScore` DESC),
    KEY `IX_MarketplaceProductPerformanceSnapshot_VendorId` (`VendorId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
";

    // Migration v1.1.0: Check if ReviewCount column exists
    public const string CheckReviewCountColumnScript = @"
SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME = 'MarketplaceProductPerformanceSnapshot'
  AND COLUMN_NAME = 'ReviewCount';
";

    // Migration v1.1.0: Add ReviewCount column
    public const string AddReviewCountColumnScript = @"
ALTER TABLE `MarketplaceProductPerformanceSnapshot` ADD COLUMN `ReviewCount` int NOT NULL DEFAULT 0 AFTER `FinalScore`;
";

    public const string DropTablesScript = @"
DROP TABLE IF EXISTS `MarketplaceProductPerformanceSnapshot`;
DROP TABLE IF EXISTS `MarketplaceProductInteraction`;
";
}
