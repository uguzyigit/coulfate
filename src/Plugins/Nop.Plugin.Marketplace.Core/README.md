# 🔷 Marketplace.Core Plugin

**Foundation plugin for NopCommerce 4.90 Marketplace Platform**

---

## 📋 Overview

Marketplace.Core is the **foundation plugin** that manages all marketplace-related database tables and entities. This plugin MUST be installed before any other marketplace plugin.

**Key Responsibility:** Data management and base functionality

---

## ✨ Features

### Database Management
- ✅ Creates and manages 12 marketplace tables
- ✅ Handles all marketplace entities
- ✅ MySQL 8.0+ compatible schema
- ✅ Foreign key relationships
- ✅ Optimized indexes

### Dependency Protection
- ✅ Cannot be uninstalled while other plugins depend on it
- ✅ Prevents accidental data loss
- ✅ Safe uninstall of UI plugins

### Entity Definitions
- ✅ All marketplace domain entities
- ✅ Shared across all plugins
- ✅ Single source of truth

---

## 📦 Installation

### Prerequisites
- NopCommerce 4.90+
- .NET 9.0 SDK
- MySQL 8.0+

### Installation Steps

1. **Build the plugin:**
```bash
cd src/Plugins/Nop.Plugin.Marketplace.Core
dotnet build
```

2. **Access admin panel:**
```
http://localhost:5000/Admin
```

3. **Install plugin:**
   - Configuration → Local Plugins
   - Find "Marketplace Core"
   - Click "Install"
   - Restart application when prompted

4. **Verify installation:**
```sql
-- Check tables created
SHOW TABLES LIKE 'Marketplace%';
-- Should show 12 tables
```

---

## 🗄️ Database Tables

### Vendor Management (6 tables)

#### 1. MarketplaceVendor
Core vendor information
- Id, Name, Code, Status
- Contact details
- Bank account info

#### 2. MarketplaceVendorProduct
Vendor-product relationships
- VendorId, ProductId
- Vendor SKU, price
- Stock quantity

#### 3. MarketplaceVendorOrderLine
Order items with commission snapshots
- OrderId, OrderItemId
- VendorId, ProductId
- Commission snapshots (rate, amount, net)

#### 4. MarketplaceVendorBalance
Current balance tracking
- VendorId
- CurrentBalance, Pending, Hold

#### 5. MarketplaceVendorShippingAccount
Shipping integrations
- VendorId
- Carrier, ApiKey

#### 6. MarketplaceVendorPayout
Payout requests and history
- VendorId
- Amount, Status
- Request/Process dates

### Commission Management (3 tables)

#### 7. MarketplaceCategoryCommission
Category-based commission rates
- CategoryId (unique)
- Rate (decimal 18,4)
- IsActive, CreatedBy

#### 8. MarketplaceProductCommission
Product-specific commission overrides
- ProductId (unique)
- Rate, Reason
- ExpiresOnUtc

#### 9. MarketplaceOrderCommission
Actual commission records
- OrderId, OrderItemId
- VendorId, ProductId
- NetPrice, CommissionRate
- CommissionAmount, MarketplaceFee
- TaxWithholding, VendorNetAmount

### Vendor Accounting (3 tables)

#### 10. MarketplaceVendorSettings
Vendor-specific configuration
- VendorId (unique)
- Code, Bank info
- RequiresProductApproval
- MinimumPayoutAmount

#### 11. MarketplaceVendorCurrentAccount
Vendor account balance (cari hesap)
- VendorId (unique)
- Balance = TotalCredit - TotalDebit
- LastUpdatedUtc

#### 12. MarketplaceVendorTransaction
Complete transaction history
- VendorId, OrderId
- Type (13 types)
- Amount, BalanceAfter
- Description, ReferenceNumber

---

## 🔗 Entity Relationships
```
Category ←1:1→ MarketplaceCategoryCommission
Product ←1:1→ MarketplaceProductCommission
Order ←1:N→ MarketplaceOrderCommission
Vendor ←1:1→ MarketplaceVendorSettings
Vendor ←1:1→ MarketplaceVendorCurrentAccount
Vendor ←1:N→ MarketplaceVendorTransaction
```

---

## 🔌 Dependent Plugins

The following plugins depend on Marketplace.Core:

1. **Marketplace.Commission** - Commission management UI
2. **Marketplace.VendorExtensions** - Vendor account management UI
3. **Payments.Iyzico** - Payment gateway integration

**⚠️ Important:** Core cannot be uninstalled while these plugins are installed.

---

## 💻 Usage in Other Plugins

### Referencing Core Plugin
```xml
<!-- YourPlugin.csproj -->
<ItemGroup>
  <ProjectReference Include="..\Nop.Plugin.Marketplace.Core\Nop.Plugin.Marketplace.Core.csproj" />
</ItemGroup>
```

### Using Core Entities
```csharp
using Nop.Plugin.Marketplace.Core.Domain;

public class YourService
{
    private readonly IRepository<OrderCommission> _orderCommissionRepository;
    
    public YourService(IRepository<OrderCommission> orderCommissionRepository)
    {
        _orderCommissionRepository = orderCommissionRepository;
    }
    
    public async Task<decimal> GetTotalCommissionAsync(int vendorId)
    {
        return await _orderCommissionRepository.Table
            .Where(oc => oc.VendorId == vendorId)
            .SumAsync(oc => oc.CommissionAmount);
    }
}
```

---

## 📊 Database Statistics

**Current State (as of Jan 13, 2026):**

| Table | Records |
|-------|---------|
| MarketplaceVendor | 0 |
| MarketplaceVendorProduct | 0 |
| MarketplaceVendorOrderLine | 0 |
| MarketplaceVendorBalance | 0 |
| MarketplaceVendorShippingAccount | 0 |
| MarketplaceVendorPayout | 0 |
| MarketplaceCategoryCommission | 4 |
| MarketplaceProductCommission | 3 |
| MarketplaceOrderCommission | 27 |
| MarketplaceVendorSettings | 0 |
| MarketplaceVendorCurrentAccount | 2 |
| MarketplaceVendorTransaction | 0 |

---

## 🔧 Maintenance

### Backup Tables
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
    > marketplace_core_backup_$(date +%Y%m%d).sql
```

### Verify Integrity
```sql
-- Check foreign keys
SELECT 
    TABLE_NAME,
    CONSTRAINT_NAME,
    REFERENCED_TABLE_NAME
FROM information_schema.KEY_COLUMN_USAGE
WHERE TABLE_SCHEMA = 'nopcommerce490'
  AND REFERENCED_TABLE_NAME IS NOT NULL
  AND TABLE_NAME LIKE 'Marketplace%';

-- Check indexes
SELECT 
    TABLE_NAME,
    INDEX_NAME,
    COLUMN_NAME
FROM information_schema.STATISTICS
WHERE TABLE_SCHEMA = 'nopcommerce490'
  AND TABLE_NAME LIKE 'Marketplace%'
ORDER BY TABLE_NAME, INDEX_NAME;
```

---

## ⚠️ Important Notes

### Cannot Uninstall While Dependencies Exist

If you try to uninstall Core while other plugins depend on it:
```
Error: Cannot uninstall. These plugins depend on this:
  - Marketplace.Commission
  - Marketplace.VendorExtensions
  - Payments.Iyzico
```

**Solution:** Uninstall dependent plugins first.

### Data Preservation

When dependent plugins are uninstalled:
- ✅ Their UI is removed
- ✅ Their settings are deleted
- ✅ **Data in Core tables is preserved**

This ensures you never lose commission records, transactions, or vendor data.

---

## 🔐 Security

### Table-Level Security
- All tables use InnoDB engine (transaction support)
- Foreign keys enforce referential integrity
- Cascade deletes configured appropriately

### Access Control
- No direct UI (Core is foundation only)
- Access controlled by dependent plugins
- Admin-only via AuthorizeAdmin attribute

---

## 🐛 Troubleshooting

### Tables Not Created After Install
```sql
-- Check if tables exist
SHOW TABLES LIKE 'Marketplace%';
```

If tables missing:
1. Check logs: `App_Data/Logs/nopcommerce-*.txt`
2. Manually execute: `Data/InstallationData.cs` SQL
3. Restart application

### Foreign Key Errors

If you get foreign key constraint errors:
1. Check referenced records exist
2. Verify cascade rules
3. Check database engine is InnoDB

---

## 📚 Related Documentation

- [Architecture Guide](../../../docs/ARCHITECTURE.md)
- [Database Schema](../../../docs/DATABASE.md)
- [Commission Plugin](../Nop.Plugin.Marketplace.Commission/README.md)
- [VendorExtensions Plugin](../Nop.Plugin.Marketplace.VendorExtensions/README.md)

---

## 📝 Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0.0 | 2026-01-13 | Initial release with 12 tables |

---

## 👥 Support

For issues or questions:
- Check [Troubleshooting Guide](../../../docs/TROUBLESHOOTING.md)
- Email: support@yourcompany.com

---

**Plugin Type:** Foundation  
**Dependencies:** None  
**Dependent Plugins:** Commission, VendorExtensions, İyzico  
**Database Tables:** 12  
**Status:** Production Ready
