# 💰 Marketplace.Commission Plugin

**Advanced commission management system for NopCommerce 4.90 Marketplace**

---

## 📋 Overview

Marketplace.Commission provides a comprehensive commission management system with category-based rates, product-specific overrides, automatic calculation, and detailed reporting.

**Version:** 1.0.0  
**Depends On:** Marketplace.Core  
**Status:** Production Ready

---

## ✨ Features

### Commission Rate Management
- ✅ **Category-based rates** - Set commission per category
- ✅ **Product-specific overrides** - Override for individual products
- ✅ **Rate hierarchy** - Product → Category → Default
- ✅ **Expiration support** - Temporary overrides with expiry dates
- ✅ **Reason tracking** - Document why overrides exist

### Automatic Commission Calculation
- ✅ **Triggers on order payment** - Calculates when order is paid
- ✅ **Marketplace fee** - Fixed fee per order
- ✅ **Tax withholding** - Configurable tax rate
- ✅ **Discount handling** - Accounts for order discounts
- ✅ **Shipping costs** - Includes shipping in calculation

### Reporting & Analytics
- ✅ **Date range filters** - Filter by date range
- ✅ **Summary cards** - Key metrics at a glance
- ✅ **Detail table** - Last 100 commission records
- ✅ **Vendor breakdown** - Commission per vendor
- ✅ **Excel export** - Export reports (ready to implement)

### Admin Panel
- ✅ **Settings page** - Configure default rates and fees
- ✅ **Category Rates** - Manage category commission rates
- ✅ **Product Rates** - Manage product-specific overrides
- ✅ **Reports** - View commission analytics

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

1. **Ensure Core is installed first:**
```bash
# Check Core is installed
# Admin → Configuration → Local Plugins → Marketplace.Core (should show "Installed")
```

2. **Build the plugin:**
```bash
cd src/Plugins/Nop.Plugin.Marketplace.Commission
dotnet build
```

3. **Install via admin panel:**
   - Admin → Configuration → Local Plugins
   - Find "Commission Management"
   - Click "Install"
   - Restart when prompted

4. **Configure plugin:**
   - Admin → Marketplace → Commission → Settings
   - Set default commission rate (e.g., 15%)
   - Set marketplace fee (e.g., 5 TL)
   - Set tax withholding rate (e.g., 0.01%)

---

## 🎯 Usage

### Setting Category Commission Rates

1. Navigate to: **Admin → Marketplace → Commission → Category Rates**

2. The page shows all categories with their current rates

3. To set a rate:
   - Find the category in the list
   - Enter rate (e.g., 23 for 23%)
   - Click "Set Rate"
   - Rate is saved immediately

**Example:**
```
Electronics → 15%
Clothing → 20%
Books → 10%
```

### Setting Product Commission Overrides

1. Navigate to: **Admin → Marketplace → Commission → Product Rates**

2. To add an override:
   - Select product from dropdown
   - Enter rate (e.g., 10 for 10%)
   - Enter reason (e.g., "Promotional period")
   - Set expiry date (optional)
   - Click "Set Override"

**Use Cases:**
- Promotional discounts (temporary lower rate)
- High-value items (custom rate)
- Special vendor agreements
- Trial products (reduced commission)

### Viewing Reports

1. Navigate to: **Admin → Marketplace → Commission → Reports**

2. Use date filters:
   - Start Date: Beginning of period
   - End Date: End of period
   - Click "Filter"

3. View summary cards:
   - Total Orders
   - Total Revenue
   - Total Commission
   - Total Marketplace Fees
   - Total Tax Withholding
   - Total Vendor Net

4. Review detail table (last 100 records)

---

## 💡 Commission Calculation

### Calculation Formula
```
Net Price = Product Price - Discount + Shipping
Commission Amount = Net Price × Commission Rate
Marketplace Fee = Fixed amount per order
Tax Withholding = Net Price × Tax Rate
Vendor Net Amount = Net Price - Commission - Fee - Tax
```

### Rate Hierarchy
```
1. Product-specific rate (if exists and not expired)
   ↓
2. Category rate (if exists)
   ↓
3. Default rate (from settings)
```

### Example Calculation

**Product:** MacBook Pro  
**Price:** 50,000 TL  
**Discount:** 5,000 TL  
**Shipping:** 0 TL  

**Commission Settings:**
- Category (Electronics): 15%
- Product Override: None
- Default: 20%
- Marketplace Fee: 5 TL
- Tax Withholding: 0.01%

**Calculation:**
```
Net Price = 50,000 - 5,000 + 0 = 45,000 TL
Commission Rate = 15% (category rate used)
Commission Amount = 45,000 × 0.15 = 6,750 TL
Marketplace Fee = 5 TL
Tax Withholding = 45,000 × 0.0001 = 4.50 TL
Vendor Net = 45,000 - 6,750 - 5 - 4.50 = 38,240.50 TL
```

---

## 🔄 Event Flow

### When Order is Paid
```
1. Order Payment Completed
   ↓
2. OrderPaidEvent fired by NopCommerce
   ↓
3. OrderPaidEventConsumer handles event
   ↓
4. For each order item:
   - Get product
   - Determine commission rate (Product → Category → Default)
   - Calculate commission, fee, tax
   - Create OrderCommission record
   ↓
5. EntityInsertedEvent<OrderCommission> published
   ↓
6. VendorExtensions plugin creates vendor transactions
   (if VendorExtensions is installed)
```

### Duplicate Prevention

The system prevents duplicate commission records:
- Checks if commission already exists for OrderItemId
- Skips if already processed
- Logs warning if duplicate detected

---

## 📊 Database Tables Used

### MarketplaceCategoryCommission
```sql
Id, CategoryId, Rate, IsActive, CreatedOnUtc, UpdatedOnUtc, CreatedBy
```
**Current:** 4 active rates

### MarketplaceProductCommission
```sql
Id, ProductId, Rate, IsActive, Reason, CreatedOnUtc, ExpiresOnUtc, CreatedBy
```
**Current:** 3 active overrides

### MarketplaceOrderCommission
```sql
Id, OrderId, OrderItemId, VendorId, ProductId, CategoryId,
Quantity, ProductPrice, DiscountAmount, DiscountSource, ShippingCost,
NetPrice, CommissionRate, CommissionAmount, MarketplaceFee, TaxWithholding,
VendorNetAmount, IsInvoiced, InvoiceId, IsPaymentApproved, IsPaid, PaidOnUtc, CreatedOnUtc
```
**Current:** 27 commission records

---

## 🎨 Admin Menu

**Location:** Admin → Marketplace → Commission

**Menu Items:**
- ⚙️ Settings - Configure default rates and fees
- 📊 Category Rates - Manage category commission rates
- 🏷️ Product Rates - Manage product-specific overrides
- 📈 Reports - View commission analytics

---

## 🔧 Configuration

### Plugin Settings

Navigate to: **Admin → Marketplace → Commission → Settings**

**Available Settings:**

| Setting | Description | Default | Example |
|---------|-------------|---------|---------|
| Default Commission Rate | Used when no category/product rate | 15% | 20% |
| Marketplace Fee | Fixed fee per order | 5 TL | 10 TL |
| Tax Withholding Rate | Tax withheld from vendor | 0.01% | 0.10% |
| Auto Process On Payment | Calculate commission when paid | true | true |

---

## 💻 Code Examples

### Getting Commission Rate Programmatically
```csharp
using Nop.Plugin.Marketplace.Commission.Services;

public class YourService
{
    private readonly ICommissionService _commissionService;
    
    public YourService(ICommissionService commissionService)
    {
        _commissionService = commissionService;
    }
    
    public async Task<decimal> GetRateAsync(int productId)
    {
        // Returns rate based on hierarchy: Product → Category → Default
        return await _commissionService.GetProductCommissionRateAsync(productId);
    }
}
```

### Calculating Commission Manually
```csharp
var calculation = await _commissionService.CalculateCommissionAsync(order, orderItem);

Console.WriteLine($"Revenue: {calculation.Revenue}");
Console.WriteLine($"Rate: {calculation.CommissionRate}%");
Console.WriteLine($"Commission: {calculation.CommissionAmount}");
Console.WriteLine($"Vendor Net: {calculation.VendorNet}");
```

---

## 🔐 Security & Permissions

### Access Control
- ✅ Admin-only access (AuthorizeAdmin attribute)
- ✅ Vendors cannot access commission pages
- ✅ Vendor access check in all controller actions

### Data Protection
- ✅ Anti-forgery tokens on all POST requests
- ✅ Model validation
- ✅ SQL injection prevention (parameterized queries)

---

## 📈 Performance

### Query Optimization
- ✅ Indexes on VendorId, OrderId
- ✅ AJAX lazy loading for reports
- ✅ MySQL query optimization (IFNULL, LIMIT)

### Caching
- Commission rates can be cached (future improvement)
- Reports use database aggregation

---

## 🐛 Troubleshooting

### Commission Not Calculating

**Issue:** Order is paid but no commission record created

**Solutions:**
1. Check if order payment status is "Paid" (not "Pending")
2. Check logs: `App_Data/Logs/nopcommerce-*.txt`
3. Verify OrderPaidEventConsumer is registered
4. Check Core plugin is installed

### Category Rate Not Applying

**Issue:** Product using default rate instead of category rate

**Solutions:**
1. Check product is assigned to category
2. Verify category rate is set and active
3. Check product doesn't have override (higher priority)

### Reports Page Slow

**Issue:** Reports page takes > 5 seconds to load

**Solutions:**
1. Add date range filter (don't load all data)
2. Check database indexes exist
3. Consider pagination for large datasets

---

## 🔄 Uninstall

### Safe Uninstall

**What happens:**
- ✅ Plugin UI removed
- ✅ Settings deleted from database
- ✅ Localization resources removed
- ✅ **Commission records preserved in Core**

**What is preserved:**
- All MarketplaceOrderCommission records
- All MarketplaceCategoryCommission records
- All MarketplaceProductCommission records

**To completely remove data:**
1. Uninstall Commission plugin
2. Uninstall all dependent plugins
3. Uninstall Core plugin (deletes all tables)

---

## 📚 Related Documentation

- [Core Plugin](../Nop.Plugin.Marketplace.Core/README.md)
- [VendorExtensions Plugin](../Nop.Plugin.Marketplace.VendorExtensions/README.md)
- [Architecture Guide](../../../docs/ARCHITECTURE.md)
- [Development Guide](../../../docs/DEVELOPMENT_GUIDE.md)

---

## 🎯 Roadmap

### Planned Features
- [ ] Excel export for reports
- [ ] Commission trend charts
- [ ] Volume-based commission tiers
- [ ] Time-based commission rules (e.g., seasonal rates)
- [ ] Commission approval workflow
- [ ] Email notifications to vendors
- [ ] API endpoints for external systems

---

## 📝 Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0.0 | 2026-01-13 | Initial release with full commission management |

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
