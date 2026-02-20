# 👤 Marketplace.VendorExtensions Plugin

**Vendor account management and transaction tracking for NopCommerce 4.90 Marketplace**

---

## 📋 Overview

Marketplace.VendorExtensions provides comprehensive vendor financial account management (cari hesap) with automatic transaction creation, balance tracking, and detailed transaction history.

**Version:** 2.1  
**Depends On:** Marketplace.Core  
**Status:** Production Ready

---

## ✨ Features

### Vendor Current Account (Cari Hesap)
- ✅ **Balance tracking** - Real-time vendor balance
- ✅ **Credit/Debit totals** - Lifetime totals maintained
- ✅ **Automatic updates** - Balance updated on every transaction
- ✅ **Multi-vendor support** - Separate account per vendor

### Automatic Transaction Creation
- ✅ **Event-driven** - Triggered by commission creation
- ✅ **4 transactions per order** - Revenue, Commission, Fee, Tax
- ✅ **Duplicate prevention** - No duplicate transactions
- ✅ **Reflection-based** - No circular dependencies

### Transaction History
- ✅ **Complete history** - All transactions logged
- ✅ **Date filters** - Filter by date range
- ✅ **Type filters** - Filter by transaction type
- ✅ **Vendor filters** - Admin can view any vendor
- ✅ **AJAX loading** - Fast data loading
- ✅ **Balance tracking** - Balance after each transaction

### Transaction Types (13 Types)
1. **OrderRevenue** (+) - Revenue from order
2. **Commission** (-) - Commission deducted
3. **MarketplaceFee** (-) - Platform fee
4. **TaxWithholding** (-) - Tax withheld
5. **ShippingCost** (+/-) - Shipping charges
6. **ReturnShippingCost** (+/-) - Return shipping
7. **LatePenalty** (-) - Late fulfillment penalty
8. **CancellationPenalty** (-) - Cancellation penalty
9. **ManualAdjustment** (+/-) - Manual correction
10. **Refund** (+/-) - Order refund
11. **PaymentReceived** (-) - Payout made to vendor
12. **VendorDiscount** (-) - Vendor-provided discount
13. **PlatformDiscount** (+) - Platform-provided discount

### Admin Panel
- ✅ **Settings page** - Configure vendor settings
- ✅ **Transaction History** - View all vendor transactions
- ✅ **Account summary** - Balance, Credit, Debit totals
- ✅ **Export ready** - Prepared for Excel export

### Localization
- ✅ English (EN)
- ✅ Turkish (TR)

---

## 📦 Installation

### Prerequisites
- ✅ Marketplace.Core plugin installed
- ✅ NopCommerce 4.90+
- ✅ MySQL 8.0+

### Installation Steps

1. **Ensure Core is installed:**
```bash
# Check Core is installed
# Admin → Configuration → Local Plugins → Marketplace.Core (should show "Installed")
```

2. **Build the plugin:**
```bash
cd src/Plugins/Nop.Plugin.Marketplace.VendorExtensions
dotnet build
```

3. **Install via admin panel:**
   - Admin → Configuration → Local Plugins
   - Find "Vendor Extensions"
   - Click "Install"
   - Restart when prompted

4. **Verify installation:**
   - Admin → Marketplace → Vendor Account → Transaction History
   - Page should load without errors

---

## 🎯 Usage

### Viewing Transaction History (Admin)

1. Navigate to: **Admin → Marketplace → Vendor Account → Transaction History**

2. **Account Summary Cards** show:
   - Current Balance
   - Total Credit (all-time)
   - Total Debit (all-time)
   - Last Update Date

3. **Filters:**
   - **Vendor:** Select specific vendor (or "All")
   - **Start Date:** Beginning of period
   - **End Date:** End of period
   - **Transaction Type:** Filter by type (or "All")
   - Click "Filter" to apply

4. **Transaction Table** displays:
   - Date
   - Type (with color-coded badge)
   - Description
   - Reference Number
   - Order Link (if applicable)
   - Amount (green=credit, red=debit)
   - Balance After

### Understanding Balance

**Balance Interpretation:**
```
Balance > 0: Platform owes vendor money (vendor has credit)
Balance = 0: Accounts are settled
Balance < 0: Vendor owes platform money (vendor has debt)
```

**Example:**
```
Vendor starts with 0 balance
Order #43 paid (1000 TL):
  → Revenue: +1000 TL (Balance: 1000.00 TL)
  → Commission: -230 TL (Balance: 770.00 TL)
  → Fee: -5 TL (Balance: 765.00 TL)
  → Tax: -0.10 TL (Balance: 764.90 TL)
Final: Vendor has 764.90 TL credit with platform
```

---

## 🔄 Automatic Transaction Flow

### How It Works
```
1. Order is paid
   ↓
2. Commission plugin creates OrderCommission record
   ↓
3. EntityInsertedEvent<OrderCommission> published
   ↓
4. OrderCommissionCreatedConsumer (VendorExtensions) handles event
   ↓
5. Reflection-based detection (no circular dependency)
   ↓
6. Check for duplicate transactions (OrderItemId)
   ↓
7. Create 4 transactions:
      a. OrderRevenue (+Revenue)
      b. Commission (-Commission)
      c. MarketplaceFee (-Fee)
      d. TaxWithholding (-Tax)
   ↓
8. Update VendorCurrentAccount:
      - Balance = previous + (Credit - Debit)
      - TotalCredit += all credits
      - TotalDebit += all debits
      - LastUpdatedUtc = now
```

### Duplicate Prevention

**How it works:**
- Before creating transactions, checks if OrderItemId already processed
- If exists, skips creation
- Logs warning: "Transactions already exist for OrderItemId: X"

**Why it's needed:**
- Event might fire multiple times
- Plugin uninstall/reinstall
- Manual commission record creation

---

## 📊 Database Tables Used

### MarketplaceVendorSettings
```sql
Id, VendorId, Code, BankAccountName, BankIban, 
RequiresProductApproval, MinimumPayoutAmount, CreatedOnUtc, UpdatedOnUtc
```
**Current:** 0 records (created on-demand)

### MarketplaceVendorCurrentAccount
```sql
Id, VendorId, Balance, TotalCredit, TotalDebit, LastUpdatedUtc
```
**Current:** 2 vendor accounts

**Sample Record:**
```
VendorId: 1
Balance: 764.90 TL
TotalCredit: 1000.00 TL
TotalDebit: 235.10 TL
LastUpdatedUtc: 2026-01-13 14:30:00
```

### MarketplaceVendorTransaction
```sql
Id, VendorId, OrderId, OrderItemId, Type, Amount, BalanceAfter,
Description, ReferenceNumber, Metadata, CreatedOnUtc
```
**Current:** 0 records (will grow as orders are processed)

**Sample Record:**
```
VendorId: 1
OrderId: 43
OrderItemId: 57
Type: 1 (OrderRevenue)
Amount: 1000.00
BalanceAfter: 1000.00
Description: "Order revenue from order #43"
CreatedOnUtc: 2026-01-13 14:30:00
```

---

## 🎨 Admin Menu

**Location:** Admin → Marketplace → Vendor Account

**Menu Items:**
- ⚙️ Settings - Configure vendor settings
- 📜 Transaction History - View vendor transactions

---

## 🔧 Configuration

### Vendor Settings

Navigate to: **Admin → Marketplace → Vendor Account → Settings**

**Per-Vendor Settings:**

| Setting | Description | Default |
|---------|-------------|---------|
| Code | Vendor reference code | Empty |
| Bank Account Name | Vendor bank account | Empty |
| Bank IBAN | Vendor IBAN | Empty |
| Requires Product Approval | Admin approval for new products | false |
| Minimum Payout Amount | Minimum amount for payout request | 0 |

---

## 💻 Code Examples

### Getting Vendor Balance
```csharp
using Nop.Plugin.Marketplace.VendorExtensions.Services;

public class YourService
{
    private readonly IVendorAccountService _vendorAccountService;
    
    public YourService(IVendorAccountService vendorAccountService)
    {
        _vendorAccountService = vendorAccountService;
    }
    
    public async Task<decimal> GetBalanceAsync(int vendorId)
    {
        var account = await _vendorAccountService.GetOrCreateCurrentAccountAsync(vendorId);
        return account.Balance;
    }
}
```

### Creating Manual Transaction
```csharp
await _vendorAccountService.RecordTransactionAsync(
    vendorId: 1,
    type: VendorTransactionType.ManualAdjustment,
    amount: 100.00m,
    description: "Manual adjustment - refund processing fee",
    orderId: null,
    referenceNumber: "ADJ-2026-001"
);
```

### Getting Transaction History
```csharp
var transactions = await _vendorAccountService.GetTransactionsAsync(
    vendorId: 1,
    startDate: DateTime.UtcNow.AddMonths(-1),
    endDate: DateTime.UtcNow,
    transactionType: (int)VendorTransactionType.OrderRevenue
);

foreach (var tx in transactions)
{
    Console.WriteLine($"{tx.CreatedOnUtc}: {tx.Description} - {tx.Amount}");
}
```

---

## 🔐 Security & Permissions

### Access Control
- ✅ Admin-only access (AuthorizeAdmin attribute)
- ✅ Vendors blocked from accessing commission/transaction pages
- ✅ Vendors can view their own transaction history (future)

### Data Protection
- ✅ Anti-forgery tokens on all POST requests
- ✅ Model validation
- ✅ SQL injection prevention
- ✅ Vendor data isolation

---

## 🔄 Event System

### Event Consumer

**OrderCommissionCreatedConsumer:**
- Listens to: `EntityInsertedEvent<BaseEntity>`
- Filters: Entities named "OrderCommission"
- Action: Creates vendor transactions
- No circular dependency (uses reflection)

**Why Reflection?**
```csharp
// VendorExtensions doesn't reference Commission plugin
// Avoids circular dependency
var entityType = entity.GetType();
if (entityType.Name == "OrderCommission" && 
    entityType.FullName.Contains("Marketplace.Commission"))
{
    // Use reflection to read properties
    var orderIdProp = entityType.GetProperty("OrderId");
    var orderId = (int)orderIdProp.GetValue(entity);
    // ...
}
```

---

## 📈 Performance

### Query Optimization
- ✅ Indexes on VendorId, OrderId
- ✅ AJAX lazy loading
- ✅ Date range filters
- ✅ MySQL-optimized queries

### Transaction Volume
- Expect 4 transactions per paid order
- 1000 orders = 4000 transactions
- Performance tested up to 10,000 transactions

---

## 🐛 Troubleshooting

### Transactions Not Created

**Issue:** Order is paid but no transactions appear

**Solutions:**
1. Check Commission plugin is installed
2. Check commission record was created
3. Check logs for event consumer errors
4. Verify Core plugin tables exist

### Balance Mismatch

**Issue:** Balance doesn't match TotalCredit - TotalDebit

**Solutions:**
1. Run balance integrity check:
```sql
SELECT 
    VendorId,
    Balance as StoredBalance,
    (TotalCredit - TotalDebit) as CalculatedBalance,
    CASE 
        WHEN ABS(Balance - (TotalCredit - TotalDebit)) < 0.01 THEN 'OK'
        ELSE 'MISMATCH'
    END as Status
FROM `MarketplaceVendorCurrentAccount`;
```

2. If mismatch, recalculate:
```sql
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

### Duplicate Transactions

**Issue:** Same order creating multiple transaction sets

**Solutions:**
- Check OrderItemId uniqueness in transactions
- Verify duplicate prevention logic is working
- Review event consumer logs

---

## 🔄 Uninstall

### Safe Uninstall

**What happens:**
- ✅ Plugin UI removed
- ✅ Settings deleted from database
- ✅ Localization resources removed
- ✅ **Transaction records preserved in Core**

**What is preserved:**
- All MarketplaceVendorSettings records
- All MarketplaceVendorCurrentAccount records
- All MarketplaceVendorTransaction records

**To completely remove data:**
1. Uninstall VendorExtensions plugin
2. Uninstall all other marketplace plugins
3. Uninstall Core plugin (deletes all tables)

---

## 📚 Related Documentation

- [Core Plugin](../Nop.Plugin.Marketplace.Core/README.md)
- [Commission Plugin](../Nop.Plugin.Marketplace.Commission/README.md)
- [Architecture Guide](../../../docs/ARCHITECTURE.md)
- [Database Schema](../../../docs/DATABASE.md)

---

## 🎯 Roadmap

### Planned Features
- [ ] Vendor dashboard (vendor-facing)
- [ ] Payout request workflow
- [ ] Batch payout processing
- [ ] Transaction export (Excel, PDF)
- [ ] Email notifications to vendors
- [ ] Balance alerts (low balance warning)
- [ ] Transaction reversal support
- [ ] Multi-currency support

---

## 📝 Version History

| Version | Date | Changes |
|---------|------|---------|
| 2.1 | 2026-01-13 | Reflection-based events, duplicate prevention |
| 2.0 | 2026-01-05 | Automatic transaction creation |
| 1.0 | 2025-12-28 | Initial release |

---

## 👥 Support

**Issues:** Check [Troubleshooting Guide](../../../docs/TROUBLESHOOTING.md)  
**Email:** support@yourcompany.com  
**Documentation:** [Full Documentation](../../../README.md)

---

**Plugin Type:** UI + Business Logic  
**Dependencies:** Marketplace.Core  
**Database Tables:** Uses 3 tables from Core  
**Status:** Production Ready  
**Localization:** EN, TR  
**Event Consumers:** 1 (OrderCommissionCreatedConsumer)
