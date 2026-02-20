# 🚀 Native Vendor Entegrasyonu - Komple Kılavuz

## 📋 Genel Bakış

**Eski Yaklaşım (v1.x):**
```
❌ Kendi Vendor entity'miz
❌ Kendi Vendor tablosu
❌ NopCommerce ile çakışma
❌ Upgrade sorunları
```

**Yeni Yaklaşım (v2.0):**
```
✅ Native Nop.Core.Domain.Vendors.Vendor kullanımı
✅ Extension entities (VendorBalance, VendorPayout, vb.)
✅ NopCommerce ekosistemi ile tam entegrasyon
✅ Upgrade güvenli
```

---

## 🏗️ Yeni Mimari

### Entity Yapısı

```
NopCommerce Native:
└── Vendor (Nop.Core.Domain.Vendors)
    ├── Id
    ├── Name
    ├── Email
    ├── Description
    ├── AdminComment
    ├── Active
    ├── Deleted
    └── DisplayOrder

Bizim Extension Entities:
├── VendorMarketplaceSettings (1:1 with Vendor)
│   ├── VendorId (FK)
│   ├── Code (unique)
│   ├── BankAccountName
│   ├── BankIban
│   ├── CommissionRate
│   ├── RequiresProductApproval
│   └── MinimumPayoutAmount
│
├── VendorBalance (1:1 with Vendor)
│   ├── VendorId (FK)
│   ├── CurrentBalance
│   ├── PendingAmount
│   ├── HoldAmount
│   ├── TotalEarned
│   └── TotalWithdrawn
│
├── VendorPayout (1:many with Vendor)
│   ├── VendorId (FK)
│   ├── Amount
│   ├── Status
│   ├── PaymentMethod
│   ├── ExternalReference
│   └── RequestedOnUtc
│
└── VendorOrderCommission (1:many with Vendor)
    ├── VendorId (FK)
    ├── OrderId
    ├── OrderItemId
    ├── CommissionRate
    ├── CommissionAmount
    └── VendorNetAmount
```

---

## 📁 Dosya Yapısı

```
Nop.Plugin.Marketplace.VendorExtensions/
│
├── plugin.json ✅ (hazır)
│
├── Domain/
│   └── Entities.cs ✅ (hazır)
│       ├── VendorMarketplaceSettings
│       ├── VendorBalance
│       ├── VendorPayout
│       └── VendorOrderCommission
│
├── Services/
│   ├── IVendorMarketplaceService.cs ✅ (hazır)
│   └── VendorMarketplaceService.cs ⏳ (yapılacak)
│
├── Controllers/
│   ├── VendorBalanceController.cs ⏳
│   ├── VendorPayoutController.cs ⏳
│   └── VendorCommissionController.cs ⏳
│
├── Models/
│   ├── VendorBalanceModel.cs ⏳
│   ├── VendorPayoutModel.cs ⏳
│   └── VendorCommissionModel.cs ⏳
│
├── Infrastructure/
│   ├── NopStartup.cs ⏳
│   ├── RouteProvider.cs ⏳
│   └── EventConsumers/
│       ├── OrderPlacedEventConsumer.cs ⏳ (commission tracking)
│       └── AdminMenuEventConsumer.cs ⏳
│
├── Views/
│   ├── _ViewImports.cshtml ⏳
│   ├── Configure.cshtml ⏳
│   ├── VendorBalance/
│   ├── VendorPayout/
│   └── VendorCommission/
│
└── VendorExtensionsPlugin.cs ⏳
```

---

## 💻 Kod Örnekleri

### 1. Native Vendor Kullanımı

```csharp
using Nop.Core.Domain.Vendors;
using Nop.Services.Vendors;

public class VendorBalanceController : BasePluginController
{
    private readonly IVendorService _vendorService;  // ← Native service
    private readonly IVendorMarketplaceService _marketplaceService;

    public async Task<IActionResult> Index()
    {
        // Native vendor service kullan
        var vendors = await _vendorService.GetAllVendorsAsync(
            showHidden: false);

        var models = new List<VendorBalanceModel>();
        foreach (var vendor in vendors)
        {
            // Her vendor için balance bilgisi al
            var balance = await _marketplaceService
                .GetVendorBalanceAsync(vendor.Id);
            
            models.Add(new VendorBalanceModel
            {
                VendorId = vendor.Id,
                VendorName = vendor.Name,  // ← Native Vendor property
                VendorEmail = vendor.Email,  // ← Native Vendor property
                CurrentBalance = balance?.CurrentBalance ?? 0,
                PendingAmount = balance?.PendingAmount ?? 0
            });
        }

        return View(models);
    }
}
```

### 2. Service Implementation Örneği

```csharp
public class VendorMarketplaceService : IVendorMarketplaceService
{
    private readonly IRepository<VendorBalance> _balanceRepository;
    private readonly IRepository<VendorPayout> _payoutRepository;
    private readonly IRepository<VendorOrderCommission> _commissionRepository;
    private readonly IVendorService _vendorService;  // ← Native service
    private readonly IOrderService _orderService;

    public async Task<VendorBalance> GetVendorBalanceAsync(int vendorId)
    {
        // Vendor exists mi kontrol et (native service ile)
        var vendor = await _vendorService.GetVendorByIdAsync(vendorId);
        if (vendor == null || vendor.Deleted)
            return null;

        return await _balanceRepository.Table
            .FirstOrDefaultAsync(b => b.VendorId == vendorId);
    }

    public async Task ProcessOrderItemCommissionAsync(int orderItemId)
    {
        var orderItem = await _orderService.GetOrderItemByIdAsync(orderItemId);
        if (orderItem == null)
            return;

        var product = await _productService
            .GetProductByIdAsync(orderItem.ProductId);
        
        // Native Vendor property kullan
        if (product.VendorId == 0)
            return;  // Not a vendor product

        var vendor = await _vendorService
            .GetVendorByIdAsync(product.VendorId);
        if (vendor == null || !vendor.Active)
            return;

        // Commission settings al
        var settings = await GetVendorSettingsByVendorIdAsync(vendor.Id);
        var commissionRate = settings?.CommissionRate ?? 10; // default %10

        // Commission hesapla
        var totalPrice = orderItem.PriceInclTax;
        var commissionAmount = totalPrice * (commissionRate / 100);
        var vendorNetAmount = totalPrice - commissionAmount;

        // Commission kaydı oluştur
        var commission = new VendorOrderCommission
        {
            VendorId = vendor.Id,
            OrderId = orderItem.OrderId,
            OrderItemId = orderItem.Id,
            ProductId = orderItem.ProductId,
            Quantity = orderItem.Quantity,
            UnitPrice = orderItem.UnitPriceInclTax,
            TotalPrice = totalPrice,
            CommissionRate = commissionRate,
            CommissionAmount = commissionAmount,
            VendorNetAmount = vendorNetAmount,
            IsPaid = false,
            CreatedOnUtc = DateTime.UtcNow
        };

        await InsertVendorOrderCommissionAsync(commission);

        // Vendor balance'ı güncelle
        await UpdateVendorBalanceAsync(vendor.Id, vendorNetAmount, isPending: true);
    }
}
```

### 3. Event Consumer Örneği

```csharp
public class OrderPlacedEventConsumer : IConsumer<OrderPlacedEvent>
{
    private readonly IVendorMarketplaceService _marketplaceService;
    private readonly IOrderService _orderService;

    public async Task HandleEventAsync(OrderPlacedEvent eventMessage)
    {
        var order = eventMessage.Order;
        
        // Her order item için commission hesapla
        var orderItems = await _orderService.GetOrderItemsAsync(order.Id);
        
        foreach (var orderItem in orderItems)
        {
            await _marketplaceService
                .ProcessOrderItemCommissionAsync(orderItem.Id);
        }
    }
}
```

---

## 🗄️ Database Schema

### Install Script

```sql
-- VendorMarketplaceSettings
CREATE TABLE [dbo].[MarketplaceVendorSettings](
    [Id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [VendorId] [int] NOT NULL,
    [Code] [nvarchar](100) NULL,
    [BankAccountName] [nvarchar](255) NULL,
    [BankIban] [nvarchar](100) NULL,
    [CommissionRate] [decimal](18, 4) NOT NULL DEFAULT 10.0,
    [RequiresProductApproval] [bit] NOT NULL DEFAULT 0,
    [MinimumPayoutAmount] [decimal](18, 4) NOT NULL DEFAULT 100.0,
    [CreatedOnUtc] [datetime2](7) NOT NULL,
    [UpdatedOnUtc] [datetime2](7) NOT NULL,
    CONSTRAINT [FK_MarketplaceVendorSettings_Vendor] 
        FOREIGN KEY([VendorId]) REFERENCES [dbo].[Vendor]([Id]) ON DELETE CASCADE
)
CREATE UNIQUE INDEX IX_MarketplaceVendorSettings_VendorId 
    ON [dbo].[MarketplaceVendorSettings] ([VendorId])
CREATE UNIQUE INDEX IX_MarketplaceVendorSettings_Code 
    ON [dbo].[MarketplaceVendorSettings] ([Code]) WHERE [Code] IS NOT NULL

-- VendorBalance
CREATE TABLE [dbo].[MarketplaceVendorBalance](
    [Id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [VendorId] [int] NOT NULL,
    [CurrentBalance] [decimal](18, 4) NOT NULL DEFAULT 0,
    [PendingAmount] [decimal](18, 4) NOT NULL DEFAULT 0,
    [HoldAmount] [decimal](18, 4) NOT NULL DEFAULT 0,
    [TotalEarned] [decimal](18, 4) NOT NULL DEFAULT 0,
    [TotalWithdrawn] [decimal](18, 4) NOT NULL DEFAULT 0,
    [UpdatedOnUtc] [datetime2](7) NOT NULL,
    CONSTRAINT [FK_MarketplaceVendorBalance_Vendor] 
        FOREIGN KEY([VendorId]) REFERENCES [dbo].[Vendor]([Id]) ON DELETE CASCADE
)
CREATE UNIQUE INDEX IX_MarketplaceVendorBalance_VendorId 
    ON [dbo].[MarketplaceVendorBalance] ([VendorId])

-- VendorPayout
CREATE TABLE [dbo].[MarketplaceVendorPayout](
    [Id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [VendorId] [int] NOT NULL,
    [Amount] [decimal](18, 4) NOT NULL,
    [Status] [int] NOT NULL,
    [PaymentMethod] [nvarchar](100) NULL,
    [ExternalReference] [nvarchar](255) NULL,
    [AdminNotes] [nvarchar](max) NULL,
    [RequestedOnUtc] [datetime2](7) NOT NULL,
    [ApprovedOnUtc] [datetime2](7) NULL,
    [ProcessedOnUtc] [datetime2](7) NULL,
    CONSTRAINT [FK_MarketplaceVendorPayout_Vendor] 
        FOREIGN KEY([VendorId]) REFERENCES [dbo].[Vendor]([Id]) ON DELETE CASCADE
)
CREATE INDEX IX_MarketplaceVendorPayout_VendorId 
    ON [dbo].[MarketplaceVendorPayout] ([VendorId])
CREATE INDEX IX_MarketplaceVendorPayout_Status 
    ON [dbo].[MarketplaceVendorPayout] ([Status])

-- VendorOrderCommission
CREATE TABLE [dbo].[MarketplaceVendorOrderCommission](
    [Id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [VendorId] [int] NOT NULL,
    [OrderId] [int] NOT NULL,
    [OrderItemId] [int] NOT NULL,
    [ProductId] [int] NOT NULL,
    [Quantity] [int] NOT NULL,
    [UnitPrice] [decimal](18, 4) NOT NULL,
    [TotalPrice] [decimal](18, 4) NOT NULL,
    [CommissionRate] [decimal](18, 4) NOT NULL,
    [CommissionAmount] [decimal](18, 4) NOT NULL,
    [VendorNetAmount] [decimal](18, 4) NOT NULL,
    [IsPaid] [bit] NOT NULL DEFAULT 0,
    [PayoutId] [int] NULL,
    [CreatedOnUtc] [datetime2](7) NOT NULL,
    [PaidOnUtc] [datetime2](7) NULL,
    CONSTRAINT [FK_MarketplaceVendorOrderCommission_Vendor] 
        FOREIGN KEY([VendorId]) REFERENCES [dbo].[Vendor]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_MarketplaceVendorOrderCommission_Payout] 
        FOREIGN KEY([PayoutId]) REFERENCES [dbo].[MarketplaceVendorPayout]([Id])
)
CREATE INDEX IX_MarketplaceVendorOrderCommission_VendorId 
    ON [dbo].[MarketplaceVendorOrderCommission] ([VendorId])
CREATE INDEX IX_MarketplaceVendorOrderCommission_OrderId 
    ON [dbo].[MarketplaceVendorOrderCommission] ([OrderId])
CREATE INDEX IX_MarketplaceVendorOrderCommission_IsPaid 
    ON [dbo].[MarketplaceVendorOrderCommission] ([IsPaid])
```

---

## 🎯 Admin Menü Entegrasyonu

```csharp
public class AdminMenuEventConsumer : IConsumer<AdminMenuCreatedEvent>
{
    public async Task HandleEventAsync(AdminMenuCreatedEvent eventMessage)
    {
        // Native Vendors menüsünü bul
        var vendorsMenuItem = eventMessage.RootMenuItem
            .GetItemBySystemName("Vendors");
        
        if (vendorsMenuItem == null)
            return;

        // Altına marketplace özelliklerini ekle
        vendorsMenuItem.ChildNodes.Add(new AdminMenuItem
        {
            SystemName = "Marketplace.VendorBalance",
            Title = "Vendor Balances",
            Url = eventMessage.GetMenuItemUrl("VendorBalance", "Index"),
            IconClass = "far fa-circle",
            Visible = true
        });

        vendorsMenuItem.ChildNodes.Add(new AdminMenuItem
        {
            SystemName = "Marketplace.VendorPayouts",
            Title = "Vendor Payouts",
            Url = eventMessage.GetMenuItemUrl("VendorPayout", "Index"),
            IconClass = "far fa-circle",
            Visible = true
        });

        vendorsMenuItem.ChildNodes.Add(new AdminMenuItem
        {
            SystemName = "Marketplace.VendorCommissions",
            Title = "Vendor Commissions",
            Url = eventMessage.GetMenuItemUrl("VendorCommission", "Index"),
            IconClass = "far fa-circle",
            Visible = true
        });
    }
}
```

**Sonuç:**
```
📂 Vendors (Native)
   ├─ All vendors (Native)
   ├─ Vendor attributes (Native)
   ├─ Vendor Balances ← YENİ
   ├─ Vendor Payouts ← YENİ
   └─ Vendor Commissions ← YENİ
```

---

## ✅ Avantajlar

### 1. NopCommerce Entegrasyonu
```
✅ Admin → Vendors menüsü ile tam entegre
✅ Vendor listesi, düzenleme native çalışıyor
✅ Vendor attributes kullanılabilir
✅ Vendor permissions otomatik çalışıyor
```

### 2. Diğer Plugin Uyumu
```
✅ Shipping plugins vendor'ı tanıyor
✅ Tax plugins vendor'ı tanıyor
✅ Report plugins vendor data'sını görüyor
✅ Payment plugins vendor commission'larını biliyor
```

### 3. Upgrade Güvenliği
```
✅ NopCommerce 5.0'a geçişte uyumlu
✅ Entity değişiklikleri otomatik yansıyor
✅ API değişiklikleri merkezi
```

### 4. Development Hızı
```
✅ Native CRUD operations hazır
✅ Validation rules hazır
✅ ACL/Store mapping hazır
✅ Sadece extension features yazıyoruz
```

---

## 🚀 Sonraki Adımlar

### Faz 1: Core Implementation ✅
- [x] Entity'ler hazır
- [x] Service interface hazır
- [ ] Service implementation
- [ ] Database scripts

### Faz 2: Admin UI
- [ ] Controllers
- [ ] Models
- [ ] Views
- [ ] Validation

### Faz 3: Business Logic
- [ ] Order event consumers
- [ ] Commission calculation
- [ ] Payout processing
- [ ] Balance management

### Faz 4: Testing & Polish
- [ ] Unit tests
- [ ] Integration tests
- [ ] Documentation
- [ ] Sample data

---

## 📞 Destek

Bu yeni mimari ile:
- ✅ Professional marketplace solution
- ✅ NopCommerce best practices
- ✅ Maintainable code
- ✅ Scalable architecture

**v2.0 çok daha güçlü ve sürdürülebilir olacak!** 🎉
