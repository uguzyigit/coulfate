# 🗄️ Database Schema Documentation

**Complete database schema for NopCommerce 4.90 Marketplace Platform**

---

## 📋 Table of Contents

1. [Overview](#overview)
2. [Database Configuration](#database-configuration)
3. [Table Categories](#table-categories)
4. [Complete Schema](#complete-schema)
5. [Relationships](#relationships)
6. [Indexes](#indexes)
7. [Sample Queries](#sample-queries)

---

## 🎯 Overview

The marketplace platform uses **12 custom tables** managed by the `Marketplace.Core` plugin, all stored in a MySQL 8.0+ database.

**Key Statistics (as of January 13, 2026):**
- Total Tables: 12
- Total Records: 39+
- Database Engine: InnoDB
- Character Set: utf8mb4

---

## ⚙️ Database Configuration

### Connection String
```json
{
  "ConnectionStrings": {
    "ConnectionString": "Server=127.0.0.1;Port=3306;Database=nopcommerce490;Uid=nopuser;Pwd=Nop123456!;",
    "DataProvider": "mysql"
  }
}
```

### MySQL Version
```bash
# Check MySQL version
mysql --version
# Required: MySQL 8.0+ or MariaDB 10.6+
```

### Character Set
```sql
ALTER DATABASE nopcommerce490 
CHARACTER SET = utf8mb4 
COLLATE = utf8mb4_general_ci;
```

---

## 📊 Table Categories

### 1. Vendor Management (6 tables)
Manage vendors, their products, orders, balances, and payouts.

- `MarketplaceVendor`
- `MarketplaceVendorProduct`
- `MarketplaceVendorOrderLine`
- `MarketplaceVendorBalance`
- `MarketplaceVendorShippingAccount`
- `MarketplaceVendorPayout`

### 2. Commission Management (3 tables)
Track commission rates and calculations.

- `MarketplaceCategoryCommission`
- `MarketplaceProductCommission`
- `MarketplaceOrderCommission`

### 3. Vendor Accounting (3 tables)
Manage vendor financial accounts and transactions.

- `MarketplaceVendorSettings`
- `MarketplaceVendorCurrentAccount`
- `MarketplaceVendorTransaction`

---

## 📝 Complete Schema

### Vendor Management Tables

#### 1. MarketplaceVendor
**Purpose:** Core vendor information
```sql
CREATE TABLE IF NOT EXISTS `MarketplaceVendor` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Name` varchar(400) NOT NULL,
    `Code` varchar(50) NOT NULL,
    `Status` int NOT NULL,
    `ContactEmail` varchar(255) NULL,
    `Phone` varchar(50) NULL,
    `BankAccountName` varchar(255) NULL,
    `BankIban` varchar(100) NULL,
    `CreatedOnUtc` datetime(6) NOT NULL,
    `UpdatedOnUtc` datetime(6) NOT NULL,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `IX_MarketplaceVendor_Code` (`Code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```

**Status Enum:**
```csharp
public enum VendorStatus
{
    Pending = 0,      // Awaiting approval
    Approved = 10,    // Approved but not active
    Active = 20,      // Actively selling
    Rejected = 30,    // Application rejected
    Suspended = 40    // Temporarily suspended
}
```

**Current Records:** 0

---

#### 2. MarketplaceVendorProduct
**Purpose:** Vendor-product relationships
```sql
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
    KEY `IX_MarketplaceVendorProduct_ProductId` (`ProductId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```

**Current Records:** 0

---

#### 3. MarketplaceVendorOrderLine
**Purpose:** Track vendor order items with commission snapshots
```sql
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
```

**Purpose of Snapshots:**
- Commission rates may change over time
- Snapshots preserve the rate at order time
- Ensures historical accuracy

**Current Records:** 0

---

#### 4. MarketplaceVendorBalance
**Purpose:** Current balance tracking
```sql
CREATE TABLE IF NOT EXISTS `MarketplaceVendorBalance` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `VendorId` int NOT NULL,
    `CurrentBalance` decimal(18, 4) NOT NULL,
    `Pending` decimal(18, 4) NOT NULL,
    `Hold` decimal(18, 4) NOT NULL,
    `UpdatedOnUtc` datetime(6) NOT NULL,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `IX_MarketplaceVendorBalance_VendorId` (`VendorId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```

**Balance Types:**
- `CurrentBalance`: Available for payout
- `Pending`: Orders not yet completed
- `Hold`: Held for disputes/chargebacks

**Current Records:** 0

---

#### 5. MarketplaceVendorShippingAccount
**Purpose:** Vendor shipping integrations
```sql
CREATE TABLE IF NOT EXISTS `MarketplaceVendorShippingAccount` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `VendorId` int NOT NULL,
    `Carrier` varchar(100) NOT NULL,
    `ApiKey` varchar(500) NULL,
    `IsActive` tinyint(1) NOT NULL DEFAULT 1,
    `CreatedOnUtc` datetime(6) NOT NULL,
    PRIMARY KEY (`Id`),
    KEY `IX_MarketplaceVendorShippingAccount_VendorId` (`VendorId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```

**Supported Carriers:**
- Aras Kargo
- Yurtiçi Kargo
- PTT Kargo
- MNG Kargo
- Custom integrations

**Current Records:** 0

---

#### 6. MarketplaceVendorPayout
**Purpose:** Payout requests and history
```sql
CREATE TABLE IF NOT EXISTS `MarketplaceVendorPayout` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `VendorId` int NOT NULL,
    `Amount` decimal(18, 4) NOT NULL,
    `Status` int NOT NULL,
    `RequestedOnUtc` datetime(6) NOT NULL,
    `ProcessedOnUtc` datetime(6) NULL,
    `ExternalRef` varchar(200) NULL,
    PRIMARY KEY (`Id`),
    KEY `IX_MarketplaceVendorPayout_VendorId` (`VendorId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```

**Status Enum:**
```csharp
public enum VendorPayoutStatus
{
    Requested = 0,   // Vendor requested payout
    Processed = 10,  // Payment completed
    Cancelled = 20   // Request cancelled
}
```

**Current Records:** 0

---

### Commission Management Tables

#### 7. MarketplaceCategoryCommission
**Purpose:** Category-based commission rates
```sql
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
```

**Current Records:** 4

**Sample Data:**
| CategoryId | Rate  | IsActive | CreatedBy |
|------------|-------|----------|-----------|
| 5          | 23.00 | 1        | admin     |
| 12         | 15.00 | 1        | admin     |
| 18         | 20.00 | 1        | admin     |
| 25         | 18.00 | 1        | admin     |

---

#### 8. MarketplaceProductCommission
**Purpose:** Product-specific commission overrides
```sql
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
```

**Use Cases:**
- Promotional periods (temporary rate reduction)
- Special deals with specific vendors
- High-value items (custom rates)
- Trial products (reduced commission)

**Current Records:** 3

---

#### 9. MarketplaceOrderCommission
**Purpose:** Actual commission records per order item
```sql
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
```

**Calculation Formula:**
```
NetPrice = ProductPrice - DiscountAmount + ShippingCost
CommissionAmount = NetPrice × CommissionRate
VendorNetAmount = NetPrice - CommissionAmount - MarketplaceFee - TaxWithholding
```

**Current Records:** 27

**Sample Record:**
```
OrderId: 43
ProductPrice: 1000.00 TL
CommissionRate: 23.00%
CommissionAmount: 230.00 TL
MarketplaceFee: 5.00 TL
TaxWithholding: 0.10 TL
VendorNetAmount: 764.90 TL
```

---

### Vendor Accounting Tables

#### 10. MarketplaceVendorSettings
**Purpose:** Vendor-specific configuration
```sql
CREATE TABLE IF NOT EXISTS `MarketplaceVendorSettings` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `VendorId` int NOT NULL,
    `Code` varchar(100) NULL,
    `BankAccountName` varchar(200) NULL,
    `BankIban` varchar(50) NULL,
    `RequiresProductApproval` tinyint(1) NOT NULL DEFAULT 0,
    `MinimumPayoutAmount` decimal(18, 4) NOT NULL DEFAULT 0.00,
    `CreatedOnUtc` datetime(6) NOT NULL,
    `UpdatedOnUtc` datetime(6) NULL,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `IX_MarketplaceVendorSettings_VendorId` (`VendorId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```

**Current Records:** 0

---

#### 11. MarketplaceVendorCurrentAccount
**Purpose:** Vendor account balance (cari hesap)
```sql
CREATE TABLE IF NOT EXISTS `MarketplaceVendorCurrentAccount` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `VendorId` int NOT NULL,
    `Balance` decimal(18, 4) NOT NULL DEFAULT 0.00,
    `TotalCredit` decimal(18, 4) NOT NULL DEFAULT 0.00,
    `TotalDebit` decimal(18, 4) NOT NULL DEFAULT 0.00,
    `LastUpdatedUtc` datetime(6) NOT NULL,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `IX_MarketplaceVendorCurrentAccount_VendorId` (`VendorId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```

**Balance Calculation:**
```
Balance = TotalCredit - TotalDebit
```

**Interpretation:**
- Positive Balance: Platform owes vendor
- Negative Balance: Vendor owes platform
- Zero Balance: Settled

**Current Records:** 2

---

#### 12. MarketplaceVendorTransaction
**Purpose:** Complete transaction history
```sql
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
    KEY `IX_MarketplaceVendorTransaction_OrderId` (`OrderId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```

**Transaction Types:**
```csharp
public enum VendorTransactionType
{
    OrderRevenue = 1,          // (+) Revenue from order
    Commission = 2,            // (-) Commission deducted
    MarketplaceFee = 3,        // (-) Platform fee
    TaxWithholding = 4,        // (-) Tax withheld
    ShippingCost = 5,          // (+/-) Shipping
    ReturnShippingCost = 6,    // (+/-) Return shipping
    LatePenalty = 7,           // (-) Late fulfillment penalty
    CancellationPenalty = 8,   // (-) Cancellation penalty
    ManualAdjustment = 9,      // (+/-) Manual correction
    Refund = 10,               // (+/-) Order refund
    PaymentReceived = 11,      // (-) Payout made to vendor
    VendorDiscount = 12,       // (-) Vendor-provided discount
    PlatformDiscount = 13      // (+) Platform-provided discount
}
```

**Transaction Flow Example:**
```
Order #43 Paid (1000 TL):
    1. OrderRevenue: +1000.00 TL (Balance: 1000.00 TL)
    2. Commission: -230.00 TL (Balance: 770.00 TL)
    3. MarketplaceFee: -5.00 TL (Balance: 765.00 TL)
    4. TaxWithholding: -0.10 TL (Balance: 764.90 TL)
```

**Current Records:** 0 (will grow as orders are processed)

---

## 🔗 Relationships

### Entity Relationship Diagram
```
┌──────────────────┐
│     Category     │
└────────┬─────────┘
         │
         │ 1:1
         ↓
┌──────────────────────────────┐
│ MarketplaceCategoryCommission│
└──────────────────────────────┘

┌──────────────────┐
│     Product      │
└────────┬─────────┘
         │
         │ 1:1
         ↓
┌──────────────────────────────┐
│ MarketplaceProductCommission │
└──────────────────────────────┘

┌──────────────────┐
│      Order       │
└────────┬─────────┘
         │
         │ 1:N
         ↓
┌──────────────────────────────┐
│  MarketplaceOrderCommission  │◄────┐
└────────┬─────────────────────┘     │
         │                            │
         │ triggers event             │
         ↓                            │
┌──────────────────────────────┐     │
│ MarketplaceVendorTransaction │─────┘
└────────┬─────────────────────┘
         │
         │ updates
         ↓
┌──────────────────────────────┐
│MarketplaceVendorCurrentAccount│
└──────────────────────────────┘

┌──────────────────┐
│     Vendor       │◄───────────┐
└────────┬─────────┘            │
         │                      │
         │ 1:1                  │ N:1
         ↓                      │
┌──────────────────────────────┐│
│ MarketplaceVendorSettings    ││
└──────────────────────────────┘│
                                │
┌──────────────────────────────┐│
│MarketplaceVendorCurrentAccount││
└──────────────────────────────┘│
                                │
┌──────────────────────────────┐│
│ MarketplaceVendorTransaction │┘
└──────────────────────────────┘
```

---

## 📇 Indexes

### Performance-Critical Indexes

**Foreign Key Indexes:**
```sql
-- Category Commission
IX_MarketplaceCategoryCommission_CategoryId (UNIQUE)

-- Product Commission
IX_MarketplaceProductCommission_ProductId (UNIQUE)

-- Order Commission
IX_MarketplaceOrderCommission_OrderId
IX_MarketplaceOrderCommission_VendorId
IX_MarketplaceOrderCommission_OrderItemId

-- Vendor Transaction
IX_MarketplaceVendorTransaction_VendorId
IX_MarketplaceVendorTransaction_OrderId

-- Vendor Settings
IX_MarketplaceVendorSettings_VendorId (UNIQUE)

-- Vendor Current Account
IX_MarketplaceVendorCurrentAccount_VendorId (UNIQUE)
```

**Query Optimization:**
```sql
-- Fast vendor transaction lookup
SELECT * FROM MarketplaceVendorTransaction 
WHERE VendorId = 1 
ORDER BY CreatedOnUtc DESC;
-- Uses: IX_MarketplaceVendorTransaction_VendorId

-- Fast commission report
SELECT * FROM MarketplaceOrderCommission 
WHERE VendorId = 1 AND CreatedOnUtc >= '2026-01-01';
-- Uses: IX_MarketplaceOrderCommission_VendorId
```

---

## 💻 Sample Queries

### 1. Get Vendor Commission Summary
```sql
SELECT 
    v.Id as VendorId,
    v.Name as VendorName,
    COUNT(DISTINCT oc.OrderId) as TotalOrders,
    IFNULL(SUM(oc.NetPrice), 0) as TotalRevenue,
    IFNULL(SUM(oc.CommissionAmount), 0) as TotalCommission,
    IFNULL(SUM(oc.MarketplaceFee), 0) as TotalFees,
    IFNULL(SUM(oc.VendorNetAmount), 0) as TotalVendorNet
FROM `Vendor` v
LEFT JOIN `MarketplaceOrderCommission` oc ON v.Id = oc.VendorId
WHERE v.Deleted = 0
GROUP BY v.Id, v.Name
ORDER BY TotalRevenue DESC;
```

---

### 2. Get Vendor Current Balance
```sql
SELECT 
    v.Name as VendorName,
    vca.Balance,
    vca.TotalCredit,
    vca.TotalDebit,
    vca.LastUpdatedUtc
FROM `MarketplaceVendorCurrentAccount` vca
INNER JOIN `Vendor` v ON vca.VendorId = v.Id
WHERE v.Deleted = 0
ORDER BY vca.Balance DESC;
```

---

### 3. Get Transaction History with Details
```sql
SELECT 
    vt.Id,
    vt.CreatedOnUtc,
    v.Name as VendorName,
    vt.Type,
    CASE vt.Type
        WHEN 1 THEN 'Order Revenue'
        WHEN 2 THEN 'Commission'
        WHEN 3 THEN 'Marketplace Fee'
        WHEN 4 THEN 'Tax Withholding'
        ELSE 'Other'
    END as TypeName,
    vt.Amount,
    vt.BalanceAfter,
    vt.Description,
    vt.OrderId
FROM `MarketplaceVendorTransaction` vt
INNER JOIN `Vendor` v ON vt.VendorId = v.Id
WHERE vt.VendorId = 1
ORDER BY vt.CreatedOnUtc DESC
LIMIT 100;
```

---

### 4. Commission Rate Hierarchy
```sql
-- Get effective commission rate for a product
SELECT 
    p.Id as ProductId,
    p.Name as ProductName,
    COALESCE(
        pc.Rate,           -- Product-specific rate (highest priority)
        cc.Rate,           -- Category rate (medium priority)
        15.00              -- Default rate (lowest priority)
    ) as EffectiveRate,
    CASE
        WHEN pc.Rate IS NOT NULL THEN 'Product Override'
        WHEN cc.Rate IS NOT NULL THEN 'Category Rate'
        ELSE 'Default Rate'
    END as RateSource
FROM `Product` p
LEFT JOIN `MarketplaceProductCommission` pc 
    ON p.Id = pc.ProductId AND pc.IsActive = 1
LEFT JOIN `Product_Category_Mapping` pcm 
    ON p.Id = pcm.ProductId
LEFT JOIN `MarketplaceCategoryCommission` cc 
    ON pcm.CategoryId = cc.CategoryId AND cc.IsActive = 1
WHERE p.Deleted = 0
LIMIT 10;
```

---

### 5. Verify Balance Integrity
```sql
-- Check if calculated balance matches stored balance
SELECT 
    vca.VendorId,
    v.Name as VendorName,
    vca.Balance as StoredBalance,
    (vca.TotalCredit - vca.TotalDebit) as CalculatedBalance,
    CASE 
        WHEN ABS(vca.Balance - (vca.TotalCredit - vca.TotalDebit)) < 0.01 
        THEN 'OK' 
        ELSE 'MISMATCH' 
    END as Status
FROM `MarketplaceVendorCurrentAccount` vca
INNER JOIN `Vendor` v ON vca.VendorId = v.Id
WHERE v.Deleted = 0;
```

---

### 6. Top Revenue Products
```sql
SELECT 
    p.Id,
    p.Name,
    COUNT(oc.Id) as OrderCount,
    SUM(oc.Quantity) as TotalQuantity,
    SUM(oc.NetPrice) as TotalRevenue,
    SUM(oc.CommissionAmount) as TotalCommission,
    AVG(oc.CommissionRate) as AvgCommissionRate
FROM `MarketplaceOrderCommission` oc
INNER JOIN `Product` p ON oc.ProductId = p.Id
WHERE oc.CreatedOnUtc >= DATE_SUB(NOW(), INTERVAL 30 DAY)
GROUP BY p.Id, p.Name
ORDER BY TotalRevenue DESC
LIMIT 10;
```

---

## 🔧 Maintenance Queries

### Rebuild Vendor Balance (If Corrupted)
```sql
-- Recalculate vendor balance from transactions
UPDATE `MarketplaceVendorCurrentAccount` vca
INNER JOIN (
    SELECT 
        VendorId,
        SUM(CASE WHEN Amount > 0 THEN Amount ELSE 0 END) as TotalCredit,
        SUM(CASE WHEN Amount < 0 THEN ABS(Amount) ELSE 0 END) as TotalDebit,
        SUM(Amount) as Balance
    FROM `MarketplaceVendorTransaction`
    GROUP BY VendorId
) calc ON vca.VendorId = calc.VendorId
SET 
    vca.Balance = calc.Balance,
    vca.TotalCredit = calc.TotalCredit,
    vca.TotalDebit = calc.TotalDebit,
    vca.LastUpdatedUtc = UTC_TIMESTAMP();
```

---

### Find Orders Without Commission Records
```sql
SELECT 
    o.Id as OrderId,
    o.OrderTotal,
    o.CreatedOnUtc,
    ps.Name as PaymentStatus
FROM `Order` o
INNER JOIN `PaymentStatus` ps ON o.PaymentStatusId = ps.Id
LEFT JOIN `MarketplaceOrderCommission` oc ON o.Id = oc.OrderId
WHERE o.PaymentStatusId = 30  -- Paid status
  AND oc.Id IS NULL
  AND o.CreatedOnUtc >= '2026-01-01';
```

---

## 📈 Database Statistics

### Current Table Sizes
```sql
SELECT 
    TABLE_NAME,
    TABLE_ROWS,
    ROUND((DATA_LENGTH + INDEX_LENGTH) / 1024 / 1024, 2) as SizeMB,
    ROUND(DATA_LENGTH / 1024 / 1024, 2) as DataMB,
    ROUND(INDEX_LENGTH / 1024 / 1024, 2) as IndexMB
FROM information_schema.TABLES
WHERE TABLE_SCHEMA = 'nopcommerce490'
  AND TABLE_NAME LIKE 'Marketplace%'
ORDER BY (DATA_LENGTH + INDEX_LENGTH) DESC;
```

---

## 🔐 Backup & Restore

### Backup Marketplace Tables Only
```bash
mysqldump -u nopuser -p'Nop123456!' nopcommerce490 \
    MarketplaceVendor \
    MarketplaceVendorProduct \
    MarketplaceVendorOrderLine \
    MarketplaceVendorBalance \
    MarketplaceVendorShippingAccount \
    MarketplaceVendorPayout \
    MarketplaceCategoryCommission \
    MarketplaceProductCommission \
    MarketplaceOrderCommission \
    MarketplaceVendorSettings \
    MarketplaceVendorCurrentAccount \
    MarketplaceVendorTransaction \
    > marketplace_backup_$(date +%Y%m%d_%H%M).sql
```

### Restore
```bash
mysql -u nopuser -p'Nop123456!' nopcommerce490 < marketplace_backup_20260113.sql
```

---

## 📚 Related Documentation

- [Architecture Guide](ARCHITECTURE.md) - System architecture
- [Development Guide](DEVELOPMENT_GUIDE.md) - Plugin development
- [Core Plugin README](../src/Plugins/Nop.Plugin.Marketplace.Core/README.md)

---

**Last Updated:** January 13, 2026  
**Database Version:** MySQL 8.0  
**Total Tables:** 12  
**Total Records:** 39+
