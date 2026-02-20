# 🏗️ System Architecture

**NopCommerce 4.90 Marketplace Platform - Architecture Documentation**

---

## 📋 Table of Contents

1. [Overview](#overview)
2. [Architecture Principles](#architecture-principles)
3. [Core-Based Modular Design](#core-based-modular-design)
4. [Plugin Dependency Graph](#plugin-dependency-graph)
5. [Database Architecture](#database-architecture)
6. [Event-Driven Architecture](#event-driven-architecture)
7. [Data Flow](#data-flow)
8. [Security Architecture](#security-architecture)
9. [Scalability Considerations](#scalability-considerations)

---

## 🎯 Overview

The platform is built on a **Core-based modular architecture** where a central Core plugin manages all shared entities and database tables, with UI plugins depending on it for functionality.

This design ensures:
- ✅ **Data Integrity:** Uninstalling UI plugins doesn't delete data
- ✅ **Single Source of Truth:** All entities defined once in Core
- ✅ **Dependency Management:** NopCommerce enforces plugin dependencies
- ✅ **Maintainability:** Centralized schema management

---

## 🎨 Architecture Principles

### 1. Separation of Concerns
```
Core Layer (Data + Business Logic)
    ↓
UI Layer (Controllers + Views)
    ↓
Presentation Layer (User Interface)
```

### 2. Dependency Inversion
```
High-Level Modules (UI Plugins)
    ↓ depend on
Low-Level Modules (Core Plugin)
    ↓ depend on
Abstractions (Interfaces)
```

### 3. Single Responsibility
- **Core Plugin:** Data management only
- **Commission Plugin:** Commission logic only
- **VendorExtensions Plugin:** Vendor accounts only

### 4. Open/Closed Principle
- Core is **closed for modification**
- New plugins **extend** functionality without changing Core

---

## 🔷 Core-Based Modular Design

### Architecture Diagram
```
┌─────────────────────────────────────────────────────────────┐
│                    NopCommerce 4.90 Core                    │
│              (Framework, Services, Data Access)             │
└─────────────────────────────────────────────────────────────┘
                              ↑
                              │
┌─────────────────────────────────────────────────────────────┐
│              Marketplace.Core Plugin (Level 0)              │
│                                                             │
│  📊 Entities:                                               │
│    - Vendor, VendorProduct, VendorOrderLine                │
│    - VendorBalance, VendorPayout                           │
│    - CategoryCommission, ProductCommission                 │
│    - OrderCommission                                        │
│    - VendorSettings, VendorCurrentAccount                  │
│    - VendorTransaction                                      │
│                                                             │
│  🗄️  Database: 12 Tables (MySQL)                            │
│  🔧 Services: Base functionality                            │
│  📦 Installation: All table creation                        │
└─────────────────────────────────────────────────────────────┘
          ↑                    ↑                    ↑
          │                    │                    │
┌─────────┴──────────┐  ┌──────┴───────┐  ┌────────┴──────────┐
│   Commission       │  │   Vendor     │  │   Integration     │
│   Management       │  │  Extensions  │  │    Plugins        │
│   (Level 1)        │  │  (Level 1)   │  │   (Level 1+)      │
│                    │  │              │  │                   │
│ 🎯 Commission      │  │ 💰 Current   │  │ 💳 İyzico        │
│    Calculation     │  │    Account   │  │    Payment       │
│ 📊 Reports         │  │ 📜 Trans.    │  │                   │
│ ⚙️  Settings       │  │    History   │  │ 📋 Paraşüt       │
│                    │  │ 👤 Vendor    │  │    Accounting    │
│                    │  │    Panel     │  │                   │
└────────────────────┘  └──────────────┘  └───────────────────┘
```

### Plugin Layers

**Level 0: Foundation**
- `Marketplace.Core`
- Cannot be uninstalled while dependencies exist
- Manages all database tables
- Provides base entities and services

**Level 1: Core Features**
- `Marketplace.Commission`
- `Marketplace.VendorExtensions`
- Depend on Core
- Can be uninstalled without data loss

**Level 1+: Integrations**
- `Payments.Iyzico` (depends on Core + Commission + VendorExtensions)
- `Accounting.Parasut` (independent)

---

## 🔗 Plugin Dependency Graph

### Dependency Tree
```
Marketplace.Core (Level 0)
    │
    ├─→ Marketplace.Commission (Level 1)
    │       └─→ Used by: İyzico
    │
    ├─→ Marketplace.VendorExtensions (Level 1)
    │       └─→ Used by: İyzico
    │
    └─→ Payments.Iyzico (Level 1+)
            └─→ Depends on: Core, Commission, VendorExtensions

Accounting.Parasut (Independent)
    └─→ No dependencies
```

### Installation Order

**Required Order:**
1. ✅ Marketplace.Core (Install first!)
2. ✅ Marketplace.Commission
3. ✅ Marketplace.VendorExtensions
4. ✅ Payments.Iyzico
5. ✅ Accounting.Parasut (anytime)

### Uninstallation Rules

**Safe to Uninstall (Data Preserved):**
- ✅ Accounting.Parasut (independent)
- ✅ Payments.Iyzico (after checking no active payments)
- ✅ Marketplace.Commission (data stays in Core)
- ✅ Marketplace.VendorExtensions (data stays in Core)

**Cannot Uninstall:**
- ❌ Marketplace.Core (while dependencies exist)
  - Error: "These plugins depend on this: Commission, VendorExtensions, İyzico"

---

## 🗄️ Database Architecture

### Table Organization

All 12 marketplace tables are managed by **Marketplace.Core**:

#### Vendor Management (6 tables)
```sql
MarketplaceVendor
MarketplaceVendorProduct
MarketplaceVendorOrderLine
MarketplaceVendorBalance
MarketplaceVendorShippingAccount
MarketplaceVendorPayout
```

#### Commission Management (3 tables)
```sql
MarketplaceCategoryCommission
MarketplaceProductCommission
MarketplaceOrderCommission
```

#### Vendor Accounting (3 tables)
```sql
MarketplaceVendorSettings
MarketplaceVendorCurrentAccount
MarketplaceVendorTransaction
```

### Database Engine
- **Engine:** InnoDB (MySQL 8.0+)
- **Character Set:** utf8mb4
- **Collation:** utf8mb4_general_ci
- **Transaction Support:** Yes (ACID compliant)

### Key Design Decisions

**1. Single Database Schema in Core**
- ✅ All tables created by Core plugin
- ✅ `CREATE TABLE IF NOT EXISTS` for safe reinstall
- ✅ Foreign keys for referential integrity

**2. MySQL Compatibility**
- ✅ `AUTO_INCREMENT` instead of `IDENTITY`
- ✅ `tinyint(1)` for boolean
- ✅ `datetime(6)` for timestamps
- ✅ Backticks (`) for table/column names

**3. Soft Deletes**
- ✅ `IsActive` flags instead of hard deletes
- ✅ Historical data preservation

See [DATABASE.md](DATABASE.md) for complete schema documentation.

---

## 🔄 Event-Driven Architecture

### Event Flow
```
Order Paid Event (NopCommerce)
    ↓
OrderPaidEventConsumer (Commission Plugin)
    ↓
Calculate Commission
    ↓
Insert OrderCommission Record
    ↓
Publish EntityInsertedEvent<BaseEntity>
    ↓
OrderCommissionCreatedConsumer (VendorExtensions Plugin)
    ↓
Create 4 Vendor Transactions:
    1. OrderRevenue (+)
    2. Commission (-)
    3. MarketplaceFee (-)
    4. TaxWithholding (-)
    ↓
Update VendorCurrentAccount Balance
```

### Event Consumers

**Commission Plugin:**
```csharp
OrderPaidEventConsumer
    ├─ Triggers: OrderPaidEvent
    ├─ Action: Calculate and record commission
    └─ Publishes: EntityInsertedEvent<OrderCommission>
```

**VendorExtensions Plugin:**
```csharp
OrderCommissionCreatedConsumer
    ├─ Triggers: EntityInsertedEvent<BaseEntity>
    ├─ Filter: Type name = "OrderCommission"
    ├─ Action: Create vendor transactions
    └─ Updates: VendorCurrentAccount balance
```

### Event Communication Pattern

**Reflection-Based Loose Coupling:**
```csharp
// VendorExtensions doesn't reference Commission plugin
// Uses reflection to detect OrderCommission events

public async Task HandleEventAsync(EntityInsertedEvent<BaseEntity> eventMessage)
{
    var entity = eventMessage.Entity;
    var entityType = entity.GetType();
    
    // Check by type name (no circular dependency!)
    if (entityType.Name != "OrderCommission")
        return;
        
    // Use reflection to read properties
    var orderIdProp = entityType.GetProperty("OrderId");
    var orderId = (int)orderIdProp.GetValue(entity);
    
    // Process...
}
```

**Benefits:**
- ✅ No circular dependencies
- ✅ Loose coupling between plugins
- ✅ Easy to add new event consumers

---

## 💧 Data Flow

### Commission Calculation Flow
```
1. Customer completes order
   ↓
2. Payment gateway processes payment
   ↓
3. Order status → Paid
   ↓
4. OrderPaidEvent triggered
   ↓
5. Commission Plugin:
   - Get product category
   - Check product-specific rate (if exists)
   - Else check category rate
   - Else use default rate
   - Calculate: Commission, MarketplaceFee, TaxWithholding
   - Calculate VendorNet = Revenue - Commission - Fee - Tax
   - Insert MarketplaceOrderCommission record
   ↓
6. VendorExtensions Plugin:
   - Detect OrderCommission inserted
   - Create 4 transactions (Revenue, Commission, Fee, Tax)
   - Update vendor balance
   - Record transaction history
   ↓
7. Admin can view:
   - Commission Reports
   - Vendor Transaction History
```

### Vendor Transaction Flow
```
VendorCurrentAccount
    ├─ Balance: 0 TL (initial)
    │
    ├─ Transaction 1: OrderRevenue (+1000 TL)
    │  └─ Balance After: 1000 TL
    │
    ├─ Transaction 2: Commission (-230 TL)
    │  └─ Balance After: 770 TL
    │
    ├─ Transaction 3: MarketplaceFee (-5 TL)
    │  └─ Balance After: 765 TL
    │
    └─ Transaction 4: TaxWithholding (-0.10 TL)
       └─ Balance After: 764.90 TL (Final)
```

---

## 🔐 Security Architecture

### Access Control

**Admin Panel:**
- ✅ `[AuthorizeAdmin]` attribute on all controllers
- ✅ Permission: `ManagePlugins` (automatically checked)
- ✅ Vendor users blocked from admin commission pages

**Vendor Panel:**
- ✅ `[AuthorizeAdmin]` attribute (vendors are admin-role users)
- ✅ Vendor check: `await _workContext.GetCurrentVendorAsync()`
- ✅ Data filtered by VendorId
- ✅ Cannot access other vendors' data

### Data Protection

**Anti-Forgery Tokens:**
```javascript
// All AJAX POST requests include token
$.post(url, {
    data: value,
    __RequestVerificationToken: getToken()
});
```

**SQL Injection Prevention:**
```csharp
// Parameterized queries
await _dataProvider.QueryAsync<T>(
    "SELECT * FROM `Table` WHERE Id = @Id",
    new DataParameter("@Id", id)
);
```

**XSS Prevention:**
- ✅ Razor automatic HTML encoding
- ✅ Input validation with `[Required]`, `[MaxLength]`
- ✅ Model state validation

---

## 📈 Scalability Considerations

### Horizontal Scaling

**Web Tier:**
- ✅ Stateless design (no session state in plugins)
- ✅ Load balancer ready
- ✅ Multiple NopCommerce instances can run

**Database Tier:**
- ✅ Read replicas for reports
- ✅ Indexed foreign keys
- ✅ Query optimization

### Performance Optimizations

**1. Database Indexing:**
```sql
-- Frequently queried columns
CREATE INDEX IX_OrderCommission_VendorId ON MarketplaceOrderCommission(VendorId);
CREATE INDEX IX_OrderCommission_OrderId ON MarketplaceOrderCommission(OrderId);
CREATE INDEX IX_Transaction_VendorId ON MarketplaceVendorTransaction(VendorId);
```

**2. AJAX Lazy Loading:**
- Reports load data via AJAX (not on page load)
- Transaction history paginated
- Filters applied server-side

**3. Event Processing:**
- Async event handlers (`async Task HandleEventAsync`)
- Non-blocking database operations

### Future Considerations

**Caching Strategy:**
```csharp
// Category commission rates (rarely change)
var cacheKey = $"marketplace.commission.category.{categoryId}";
var rate = await _cacheManager.GetAsync(cacheKey, async () => 
{
    return await _commissionService.GetCategoryRateAsync(categoryId);
});
```

**Message Queue:**
```
Order Paid Event
    ↓
Message Queue (RabbitMQ/Azure Service Bus)
    ↓
Commission Worker (background service)
    ↓
Transaction Worker (background service)
```

---

## 🎯 Design Patterns Used

### 1. Repository Pattern
```csharp
IRepository<OrderCommission>
    └─ CRUD operations
    └─ LinqToDB queries
```

### 2. Service Layer Pattern
```csharp
ICommissionService
    ├─ GetCategoryRateAsync()
    ├─ GetProductRateAsync()
    └─ CalculateCommissionAsync()
```

### 3. Event Sourcing (Partial)
```csharp
VendorTransaction (immutable log)
    └─ Every balance change recorded
    └─ Audit trail
    └─ Balance recalculation possible
```

### 4. Plugin Pattern
```csharp
BasePlugin
    ├─ InstallAsync()
    ├─ UninstallAsync()
    └─ UpdateAsync()
```

### 5. Dependency Injection
```csharp
// Infrastructure/NopStartup.cs
services.AddScoped<ICommissionService, CommissionService>();
services.AddScoped<IVendorAccountService, VendorAccountService>();
```

---

## 📊 Component Diagram
```
┌─────────────────────────────────────────────────────────┐
│                    Presentation Layer                   │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  │
│  │   Admin UI   │  │  Vendor UI   │  │  Public UI   │  │
│  └──────────────┘  └──────────────┘  └──────────────┘  │
└─────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────┐
│                    Controller Layer                     │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  │
│  │  Commission  │  │VendorExtens. │  │   Payment    │  │
│  │  Controller  │  │  Controller  │  │  Controller  │  │
│  └──────────────┘  └──────────────┘  └──────────────┘  │
└─────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────┐
│                     Service Layer                       │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  │
│  │ Commission   │  │VendorAccount │  │   Payment    │  │
│  │   Service    │  │   Service    │  │   Service    │  │
│  └──────────────┘  └──────────────┘  └──────────────┘  │
└─────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────┐
│                    Data Access Layer                    │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  │
│  │ Repository   │  │  Repository  │  │  Repository  │  │
│  │<OrderComm.>  │  │<VendorTrans>│  │  <Vendor>    │  │
│  └──────────────┘  └──────────────┘  └──────────────┘  │
└─────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────┐
│                    Database (MySQL)                     │
│                      12 Tables                          │
└─────────────────────────────────────────────────────────┘
```

---

## 🔍 Key Architectural Decisions

### Decision 1: Core-Based Architecture
**Rationale:** 
- Data safety during plugin uninstall
- Single source of truth for entities
- Easier maintenance

**Trade-offs:**
- Core plugin cannot be uninstalled
- More initial setup complexity

**Outcome:** ✅ Improved data integrity and safety

---

### Decision 2: Event-Driven Commission Processing
**Rationale:**
- Loose coupling between plugins
- Async processing
- Easy to add new event handlers

**Trade-offs:**
- More complex debugging
- Event order matters

**Outcome:** ✅ Flexible, extensible system

---

### Decision 3: Reflection-Based Event Communication
**Rationale:**
- Avoid circular dependencies
- VendorExtensions doesn't need to reference Commission

**Trade-offs:**
- No compile-time type safety
- Slightly slower (reflection overhead)

**Outcome:** ✅ Clean plugin separation

---

### Decision 4: MySQL Instead of SQL Server
**Rationale:**
- Lower licensing costs
- Better performance for read-heavy workloads
- Cross-platform compatibility

**Trade-offs:**
- Migration effort from SQL Server syntax
- Different tooling

**Outcome:** ✅ Cost-effective, performant

---

## 📚 Related Documentation

- [Database Schema](DATABASE.md) - Complete table definitions
- [Development Guide](DEVELOPMENT_GUIDE.md) - How to add new plugins
- [Deployment Guide](DEPLOYMENT.md) - Production deployment
- [Troubleshooting](TROUBLESHOOTING.md) - Common issues

---

**Last Updated:** January 13, 2026  
**Version:** 1.0  
**Author:** Development Team
